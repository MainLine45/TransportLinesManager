﻿using ColossalFramework;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class TransportToolOverrides : MonoBehaviour, IRedirectable
    {
        
        #region Hooking
        private static bool preventDefault() => false;

        public void Awake()
        {
            MethodInfo preventDefault = typeof(TransportToolOverrides).GetMethod("preventDefault", allFlags);

            #region Automation Hooks
            MethodInfo onEnable = typeof(TransportToolOverrides).GetMethod("OnEnable", allFlags);
            MethodInfo onDisable = typeof(TransportToolOverrides).GetMethod("OnDisable", allFlags);
            MethodInfo OnToolGUIPos = typeof(TransportToolOverrides).GetMethod("OnToolGUIPos", allFlags);
            MethodInfo SimulationStepPos = typeof(TransportToolOverrides).GetMethod("SimulationStepPos", allFlags);

            TLMUtils.doLog("Loading TransportToolOverrides Hook");
            try
            {
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), onEnable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnToolGUI", allFlags), null, OnToolGUIPos);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("SimulationStep", allFlags), null, SimulationStepPos);
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("ERRO AO CARREGAR HOOKS: {0}", e.StackTrace);
            }
            #endregion


        }
        #endregion
        private static FieldInfo tt_lineCurrent = typeof(TransportTool).GetField("m_line", allFlags);
        private static FieldInfo tt_lineTemp = typeof(TransportTool).GetField("m_tempLine", allFlags);
        private static FieldInfo tt_mode = typeof(TransportTool).GetField("m_mode", allFlags);


        private static void OnEnable()
        {
            TLMUtils.doLog("OnEnableTransportTool");
            TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
            TLMController.instance.LinearMapCreatingLine?.setVisible(true);
            TLMController.instance.LineCreationToolbox?.setVisible(true);
            TLMController.instance.setCurrentSelectedId(0);
        }

        private static void OnDisable()
        {
            TLMUtils.doLog("OnDisableTransportTool");
            TLMController.instance.setCurrentSelectedId(0);
            TLMController.instance.LinearMapCreatingLine?.setVisible(false);
            TLMController.instance.LineCreationToolbox?.setVisible(false);
        }

        private static ToolStatus lastState = new ToolStatus();
        private static float lastLength = 0;
        private static bool needsUpdate = false;
        private static TransportTool ttInstance;

        public TransportToolOverrides()
        {
        }

        private static bool isInsideUI => Singleton<ToolController>.instance.IsInsideUI;

        private static bool HasInputFocus => Singleton<ToolController>.instance.HasInputFocus;

        public Redirector RedirectorInstance => new Redirector();

        private static void OnToolGUIPos(ref TransportTool __instance, ref Event e)
        {
            lock (__instance)
            {
                if (e.type == EventType.MouseUp && !isInsideUI)
                {
                    ttInstance = __instance;
                    needsUpdate = true;
                }
            }
        }

        private static void SimulationStepPos(ref TransportTool __instance)
        {
            if (lastState.m_lineCurrent > 0 && Math.Abs(lastLength - Singleton<TransportManager>.instance.m_lines.m_buffer[lastState.m_lineCurrent].m_totalLength) > 0.001f)
            {
                ttInstance = __instance;
                needsUpdate = true;
            }
        }

        private static void redrawMap(ToolStatus __state)
        {
            if (__state.m_lineCurrent > 0 || (Singleton<TransportManager>.instance.m_lines.m_buffer[TLMController.instance.CurrentSelectedId].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                TLMController.instance.setCurrentSelectedId(__state.m_lineCurrent);
                if (__state.m_lineCurrent > 0 && TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                {
                    TLMController.instance.AutoColor(__state.m_lineCurrent, true, true);
                }
                TLMController.instance.LinearMapCreatingLine.redrawLine();
                lastLength = Singleton<TransportManager>.instance.m_lines.m_buffer[lastState.m_lineCurrent].m_totalLength;
            }
        }

        private struct ToolStatus
        {
            public Mode m_mode;
            public ushort m_lineTemp;
            public ushort m_lineCurrent;

            public override string ToString() => $"mode={m_mode};lineTemp={m_lineTemp};lineCurrent={m_lineCurrent}";

        }
        private enum Mode
        {
            NewLine,
            AddStops,
            MoveStops
        }

        private void Update()
        {
            if (needsUpdate && ttInstance != null)
            {
                TLMUtils.doLog("OnToolGUIPostTransportTool");
                var currentState = new ToolStatus();
                TLMUtils.doLog("__state => {0} | tt_mode=> {1} | tt_lineCurrent => {2}", currentState, tt_mode, tt_lineCurrent);
                currentState.m_mode = (Mode) tt_mode.GetValue(ttInstance);
                currentState.m_lineCurrent = (ushort) tt_lineCurrent.GetValue(ttInstance);
                currentState.m_lineTemp = (ushort) tt_lineTemp.GetValue(ttInstance);
                TLMUtils.doLog("__state = {0} => {1}, newMode = {2}", lastState, currentState, currentState.m_mode);
                lastState = currentState;
                redrawMap(currentState);
                needsUpdate = false;
            }
        }
    }
}
