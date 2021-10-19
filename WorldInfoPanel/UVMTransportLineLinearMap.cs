﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Overrides;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.TransportLinesManager.UI.UVMPublicTransportWorldInfoPanel.UVMPublicTransportWorldInfoPanelObject;

namespace Klyte.TransportLinesManager.UI
{

    public class UVMTransportLineLinearMap : UICustomControl, IUVMPTWIPChild
    {
        private UIScrollablePanel m_bg;
        private UIScrollbar m_bgScrollbar;
        private UICheckBox m_unscaledCheck;
        private MapMode m_currentMode = MapMode.NONE;
        private bool m_unscaledMode = true;
        private bool m_cachedUnscaledMode = true;
        private static bool m_dirty;
        private static bool m_dirtyNames;
        private static bool m_dirtyTerminal;

        private UILabel m_labelLineIncomplete;
        internal UISprite m_stopsLineSprite;

        internal UISprite m_lineEnd;


        internal float m_uILineLength;

        internal float m_uILineOffset;

        internal bool m_vehicleCountMismatch;
        private UIPanel m_stopsContainer;
        internal UITemplateList<UIPanel> m_stopButtons;

        internal UITemplateList<UIButton> m_vehicleButtons;

        internal float m_kstopsX = 170;
        internal float m_kstopsXForWalkingTours = 170;
        internal float m_kvehiclesX = 130;
        internal float m_kminStopDistance = 50f;
        internal float m_kvehicleButtonHeight = 36f;
        internal float m_kminUILineLength = 370f;
        internal float m_kmaxUILineLength = 10000f;

        internal float m_actualStopsX;
        internal Vector2 m_kLineSSpritePosition = new Vector2(175f, 20f);
        internal Vector2 m_kLineSSpritePositionForWalkingTours = new Vector2(175f, 20f);
        internal int m_kMaxConnectionsLine = 4;

        internal UILabel m_stopsLabel;

        internal UILabel m_vehiclesLabel;

        internal UILabel m_connectionLabel;
        internal TLMLineItemButtonControl m_lineTitleBtnCtrl;

        public static UIScrollablePanel m_scrollPanel;

        internal static Vector2 m_cachedScrollPosition;
        private UIDropDown m_mapModeDropDown;
        private UIPanel m_panelModeSelector;
        private ushort[] m_cachedStopOrder;

        #region Overridable



        public void Awake()
        {
            m_bg = component as UIScrollablePanel;

            PublicTransportWorldInfoPanel ptwip = UVMPublicTransportWorldInfoPanel.m_obj.origInstance;

            AddNewStopTemplate();

            ptwip.component.width = 800;

            BindComponents(ptwip);
            AdjustLineStopsPanel(ptwip);

            KlyteMonoUtils.CreateUIElement(out m_panelModeSelector, m_bg.parent.transform);
            m_panelModeSelector.autoFitChildrenHorizontally = true;
            m_panelModeSelector.autoFitChildrenVertically = true;
            m_panelModeSelector.autoLayout = true;
            m_panelModeSelector.autoLayoutDirection = LayoutDirection.Horizontal;
            m_mapModeDropDown = UIHelperExtension.CloneBasicDropDownNoLabel(Enum.GetNames(typeof(MapMode)).Select(x => Locale.Get("K45_TLM_LINEAR_MAP_VIEW_MODE", x)).ToArray(), (int idx) =>
               {
                   m_currentMode = (MapMode)idx;
                   RefreshVehicleButtons(GetLineID());
                   MarkDirty();
               }, m_panelModeSelector);
            m_mapModeDropDown.textScale = 0.75f;
            m_mapModeDropDown.size = new Vector2(200, 25);
            m_mapModeDropDown.itemHeight = 16;

            m_unscaledCheck = UIHelperExtension.AddCheckboxLocale(m_panelModeSelector, "K45_TLM_LINEAR_MAP_SHOW_UNSCALED", m_unscaledMode, (val) =>
            {
                m_unscaledMode = val;
                MarkDirty();
            });
            KlyteMonoUtils.LimitWidthAndBox(m_unscaledCheck.label, 165);

            InstanceManagerOverrides.EventOnBuildingRenamed += (x) => m_dirtyNames = true;
        }

        private static void AddNewStopTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(36, 36);
            UIButton button = UITemplateManager.Get<UIButton>("StopButton");
            panel.AttachUIComponent(button.gameObject).transform.localScale = Vector3.one;
            button.relativePosition = Vector2.zero;
            button.name = "StopButton";
            button.scaleFactor = 1f;
            button.spritePadding.top = 2;
            button.isTooltipLocalized = true;
            KlyteMonoUtils.InitButtonFg(button, false, "DistrictOptionBrushMedium");
            KlyteMonoUtils.InitButtonSameSprite(button, "");

            UILabel uilabel = button.Find<UILabel>("PassengerCount");
            panel.AttachUIComponent(uilabel.gameObject).transform.localScale = Vector3.one;
            uilabel.relativePosition = new Vector3(38, 12);
            uilabel.processMarkup = true;
            uilabel.isVisible = true;
            uilabel.minimumSize = new Vector2(175, 50);
            uilabel.verticalAlignment = UIVerticalAlignment.Middle;
            KlyteMonoUtils.LimitWidthAndBox(uilabel, 175, true);


            UIPanel connectionPanel = panel.AddUIComponent<UIPanel>();
            connectionPanel.name = "ConnectionPanel";
            connectionPanel.relativePosition = new Vector3(-50, 5);
            connectionPanel.size = new Vector3(50, 40);
            connectionPanel.autoLayout = true;
            connectionPanel.wrapLayout = true;
            connectionPanel.autoLayoutDirection = LayoutDirection.Vertical;
            connectionPanel.autoLayoutStart = LayoutStart.TopRight;
            TLMLineItemButtonControl.EnsureTemplate();
            connectionPanel.objectUserData = new UITemplateList<UIButton>(connectionPanel, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);



            UILabel distLabel = panel.AddUIComponent<UILabel>();

            distLabel.name = "Distance";
            distLabel.relativePosition = new Vector3(-12, 37);
            distLabel.textAlignment = UIHorizontalAlignment.Center;
            distLabel.textScale = 0.65f;
            distLabel.suffix = "m";
            distLabel.useOutline = true;
            distLabel.minimumSize = new Vector2(60, 0);
            distLabel.outlineColor = Color.black;

            KlyteMonoUtils.CreateUIElement(out UITextField lineNameField, panel.transform, "StopNameField", new Vector4(38, -6, 175, 50));
            lineNameField.maxLength = 256;
            lineNameField.isVisible = false;
            lineNameField.verticalAlignment = UIVerticalAlignment.Middle;
            lineNameField.horizontalAlignment = UIHorizontalAlignment.Left;
            lineNameField.selectionSprite = "EmptySprite";
            lineNameField.builtinKeyNavigation = true;
            lineNameField.textScale = uilabel.textScale;
            lineNameField.padding.top = 18;
            lineNameField.padding.left = 5;
            lineNameField.padding.bottom = 14;
            KlyteMonoUtils.InitButtonFull(lineNameField, false, "TextFieldPanel");


            TLMUiTemplateUtils.GetTemplateDict()["StopButtonPanel"] = panel;
        }

        private void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            //STOPS
            m_stopsContainer = __instance.Find<UIPanel>("StopsPanel");
            m_stopButtons = new UITemplateList<UIPanel>(m_stopsContainer, "StopButtonPanel");
            m_vehicleButtons = new UITemplateList<UIButton>(m_stopsContainer, "VehicleButton");
            m_stopsLineSprite = __instance.Find<UISprite>("StopsLineSprite");
            m_lineEnd = __instance.Find<UISprite>("LineEnd");
            m_stopsLabel = __instance.Find<UILabel>("StopsLabel");
            m_vehiclesLabel = __instance.Find<UILabel>("VehiclesLabel");
            m_labelLineIncomplete = __instance.Find<UILabel>("LabelLineIncomplete");
            m_bgScrollbar = __instance.Find<UIScrollbar>("Scrollbar");


            UISprite lineStart = __instance.Find<UISprite>("LineStart");
            lineStart.relativePosition = new Vector3(4, -8);

            m_vehiclesLabel.relativePosition = new Vector3(100, 12);

            m_stopsLineSprite.spriteName = "PlainWhite";
            m_stopsLineSprite.width = 25;

            m_connectionLabel = Instantiate(m_vehiclesLabel);
            m_connectionLabel.transform.SetParent(m_vehiclesLabel.transform.parent);
            m_connectionLabel.absolutePosition = m_vehiclesLabel.absolutePosition;
            m_connectionLabel.localeID = "K45_TLM_CONNECTIONS";

            var lineStringButton = m_vehiclesLabel.parent.AttachUIComponent(UITemplateManager.GetAsGameObject(TLMLineItemButtonControl.LINE_ITEM_TEMPLATE)) as UIButton;
            m_lineTitleBtnCtrl = lineStringButton.GetComponent<TLMLineItemButtonControl>();
            m_lineTitleBtnCtrl.Resize(36);
            lineStringButton.relativePosition = new Vector3(170, 4);
            lineStringButton.Disable();

        }

        private void AdjustLineStopsPanel(PublicTransportWorldInfoPanel __instance)
        {
            m_scrollPanel = __instance.Find<UIScrollablePanel>("ScrollablePanel");
            m_scrollPanel.eventGotFocus += OnGotFocusBind;
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }
        private uint m_lastDrawTick;
        public void UpdateBindings()
        {
            if (component.isVisible && (m_lastDrawTick + 23 < SimulationManager.instance.m_referenceFrameIndex || m_dirty))
            {
                ushort lineID = GetLineID();
                if (lineID != 0)
                {
                    if (m_cachedUnscaledMode != m_unscaledMode || m_dirty)
                    {
                        OnSetTarget(null);
                        m_cachedUnscaledMode = m_unscaledMode;
                        m_dirty = false;
                    }
                    UpdateVehicleButtons(lineID);
                    UpdateStopButtons(lineID);
                    m_panelModeSelector.relativePosition = new Vector3(405, 45);
                }
                m_lastDrawTick = SimulationManager.instance.m_referenceFrameIndex;
            }
        }

        public static void MarkDirty() => m_dirty = true;

        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(UVMPublicTransportWorldInfoPanel.GetLineID()));
            return false;
        }

        public static bool ResetScrollPosition()
        {
            m_scrollPanel.scrollPosition = m_cachedScrollPosition;
            return false;
        }


        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                m_bg.isVisible = true;
                m_bgScrollbar.isVisible = true;            
                m_unscaledCheck.isVisible = true;
                m_mapModeDropDown.isVisible = true;
                LineType lineType = GetLineType(lineID);
                bool isTour = (lineType == LineType.WalkingTour);
                m_mapModeDropDown.isVisible = !isTour;
                m_vehiclesLabel.isVisible = !isTour && m_currentMode != MapMode.CONNECTIONS;
                m_connectionLabel.isVisible = !isTour || m_currentMode == MapMode.CONNECTIONS;


                if (isTour)
                {
                    m_currentMode = MapMode.CONNECTIONS;
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePositionForWalkingTours;
                    m_actualStopsX = m_kstopsXForWalkingTours;
                }
                else
                {
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePosition;
                    m_actualStopsX = m_kstopsX;

                }
                m_lineTitleBtnCtrl.ResetData(lineID, Vector3.zero);
                m_stopsLineSprite.color = Singleton<TransportManager>.instance.GetLineColor(lineID);
                NetManager instance = Singleton<NetManager>.instance;
                int stopsCount = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountStops(lineID);
                float[] stopPositions = new float[stopsCount];
                m_cachedStopOrder = new ushort[stopsCount];
                float minDistance = float.MaxValue;
                float lineLength = 0f;
                UIPanel[] stopsButtons = m_stopButtons.SetItemCount(stopsCount);
                ushort firstStop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                ushort currentStop = firstStop;
                int idx = 0;
                while (currentStop != 0 && idx < stopsButtons.Length)
                {
                    stopsButtons[idx].GetComponentInChildren<UIButton>().objectUserData = currentStop;

                    m_cachedStopOrder[idx] = currentStop;
                    UILabel uilabel = stopsButtons[idx].Find<UILabel>("PassengerCount");

                    uilabel.prefix = TLMStationUtils.GetFullStationName(currentStop, lineID, TransportSystemDefinition.GetDefinitionForLine(lineID).SubService);
                    uilabel.text = "";

                    UILabel dist = stopsButtons[idx].Find<UILabel>("Distance");
                    dist.text = "(???)";


                    CreateConnectionPanel(instance, stopsButtons[idx], currentStop);
                    UIButton button = stopsButtons[idx].GetComponentInChildren<UIButton>();
                    UpdateTerminalStatus(lineID, currentStop, button);
                    button.tooltipLocaleID
                        = !TransportSystemDefinition.From(lineID).CanHaveTerminals() ? ""
                        : currentStop == firstStop ? "K45_TLM_FIRSTSTOPALWAYSTERMINAL"
                        : "K45_TLM_RIGHTCLICKSETTERMINAL";

                    if (uilabel.objectUserData == null)
                    {
                        UITextField stopNameField = stopsButtons[idx].Find<UITextField>("StopNameField");
                        uilabel.eventMouseEnter += (c, r) => uilabel.backgroundSprite = "TextFieldPanelHovered";
                        uilabel.eventMouseLeave += (c, r) => uilabel.backgroundSprite = string.Empty;
                        uilabel.eventClick += (c, r) =>
                        {
                            uilabel.Hide();
                            stopNameField.Show();
                            stopNameField.text = TLMStationUtils.GetStationName((ushort)button.objectUserData, GetLineID(), TransportSystemDefinition.GetDefinitionForLine(GetLineID()).SubService);
                            stopNameField.Focus();
                        };
                        stopNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
                        {
                            stopNameField.Hide();
                            uilabel.Show();
                        };
                        stopNameField.eventTextSubmitted += (x, y) => TLMStationUtils.SetStopName(y.Trim(), (ushort)button.objectUserData, GetLineID(), () => uilabel.prefix = $"<color white>{TLMStationUtils.GetFullStationName((ushort)button.GetComponentInChildren<UIButton>().objectUserData, GetLineID(), TransportSystemDefinition.GetDefinitionForLine(GetLineID()).SubService)}</color>");
                        button.eventMouseUp += (x, y) =>
                        {
                            var stop = (ushort)x.objectUserData;
                            var lineId = GetLineID();
                            if ((y.buttons & UIMouseButton.Right) != 0 && TransportSystemDefinition.From(lineId).CanHaveTerminals() && stop != Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_stops)
                            {
                                var newVal = TLMStopDataContainer.Instance.SafeGet(stop).IsTerminal;
                                TLMStopDataContainer.Instance.SafeGet(stop).IsTerminal = !newVal;
                                NetProperties properties = NetManager.instance.m_properties;
                                if (!(properties is null) && !(properties.m_drawSound is null))
                                {
                                    AudioManager.instance.DefaultGroup.AddPlayer(0, properties.m_drawSound, 1f);
                                }
                                m_dirtyTerminal = true;
                                MarkDirty();
                            }
                        };

                        uilabel.objectUserData = true;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segmentId = instance.m_nodes.m_buffer[currentStop].GetSegment(i);
                        if (segmentId != 0 && instance.m_segments.m_buffer[segmentId].m_startNode == currentStop)
                        {
                            currentStop = instance.m_segments.m_buffer[segmentId].m_endNode;
                            dist.text = (instance.m_segments.m_buffer[segmentId].m_averageLength).ToString("0");
                            float segmentSize = m_unscaledMode ? m_kminStopDistance : instance.m_segments.m_buffer[segmentId].m_averageLength;
                            if (segmentSize == 0f)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Two transport line stops have zero distance");
                                segmentSize = 100f;
                            }
                            stopPositions[idx] = segmentSize;
                            if (segmentSize < minDistance)
                            {
                                minDistance = segmentSize;
                            }
                            lineLength += stopPositions[idx];
                            break;
                        }
                    }

                    if (stopsCount > 2 && currentStop == firstStop)
                    {
                        break;
                    }
                    if (stopsCount == 2 && idx > 0)
                    {
                        break;
                    }
                    if (stopsCount == 1)
                    {
                        break;
                    }
                    if (++idx >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                float stopDistanceFactor = m_kminStopDistance / minDistance;
                m_uILineLength = stopDistanceFactor * lineLength;
                if (m_uILineLength < m_kminUILineLength)
                {
                    m_uILineLength = m_kminUILineLength;
                    stopDistanceFactor = m_uILineLength / lineLength;
                }
                else if (m_uILineLength > m_kmaxUILineLength)
                {
                    m_uILineLength = m_kmaxUILineLength;
                    stopDistanceFactor = m_uILineLength / lineLength;
                }
                if (stopsCount <= 2)
                {
                    m_uILineOffset = (stopDistanceFactor * stopPositions[stopPositions.Length - 1]) - 30f;
                }
                if (m_uILineOffset < 20f || stopsCount > 2)
                {
                    m_uILineOffset = stopDistanceFactor * stopPositions[stopPositions.Length - 1] / 2f;
                }
                m_uILineLength += 20;
                m_stopsLineSprite.height = m_uILineLength;
                m_stopsContainer.height = m_uILineLength + m_kvehicleButtonHeight;
                Vector3 relativePosition = m_lineEnd.relativePosition;
                relativePosition.y = m_uILineLength + 13f;
                relativePosition.x = m_stopsLineSprite.relativePosition.x + 4f;
                m_lineEnd.relativePosition = relativePosition;
                float num8 = 0f;
                for (int j = 0; j < stopsCount; j++)
                {
                    Vector3 relativePosition2 = m_stopButtons.items[j].relativePosition;
                    relativePosition2.x = m_actualStopsX;
                    relativePosition2.y = ShiftVerticalPosition(num8);
                    m_stopButtons.items[j].relativePosition = relativePosition2;
                    num8 += stopPositions[j] * stopDistanceFactor;
                }
                RefreshVehicleButtons(lineID);
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None)
                {
                    m_labelLineIncomplete.isVisible = false;
                    m_stopsContainer.isVisible = true;
                }
                else
                {
                    m_labelLineIncomplete.isVisible = true;
                    m_stopsContainer.isVisible = false;
                }

                MarkDirty();
            }
        }

        private void UpdateTerminalStatus(ushort lineID, ushort currentStop, UIButton button) => button.normalBgSprite =
                                TransportSystemDefinition.From(lineID).CanHaveTerminals() && (currentStop == Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops || TLMStopDataContainer.Instance.SafeGet(currentStop).IsTerminal)
                                ? KlyteResourceLoader.GetDefaultSpriteNameFor(LineIconSpriteNames.K45_S05StarIcon, true)
                                : "";
        private void CreateConnectionPanel(NetManager instance, UIPanel basePanel, ushort currentStop)
        {
            ushort lineID = GetLineID();
            var linesFound = new List<ushort>();
            var targetPos = instance.m_nodes.m_buffer[currentStop].m_position;
            TLMLineUtils.GetNearLines(targetPos, 150f, ref linesFound);
            linesFound.Remove(lineID);
            UIPanel connectionPanel = basePanel.Find<UIPanel>("ConnectionPanel");
            if (connectionPanel.objectUserData is null)
            {
                connectionPanel.objectUserData = new UITemplateList<UIButton>(connectionPanel, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);
            }
            var templateList = connectionPanel.objectUserData as UITemplateList<UIButton>;

            int newSize = linesFound.Count > m_kMaxConnectionsLine ? 18 : 36;

            var itemsEntries = templateList.SetItemCount(linesFound.Count);
            for (int idx = 0; idx < linesFound.Count; idx++)
            {
                ushort lineId = linesFound[idx];
                var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                itemControl.Resize(newSize);
                itemControl.ResetData(lineId, targetPos);
            }
            connectionPanel.isVisible = m_currentMode == MapMode.CONNECTIONS;
        }
        #endregion

        private void UpdateVehicleButtons(ushort lineID)
        {
            if (m_vehicleCountMismatch)
            {
                RefreshVehicleButtons(lineID);
                m_vehicleCountMismatch = false;
            }
            if (m_currentMode == MapMode.CONNECTIONS)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleId = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
            int idx = 0;
            while (vehicleId != 0)
            {
                if (idx > m_vehicleButtons.items.Count - 1)
                {
                    m_vehicleCountMismatch = true;
                    break;
                }
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info;


                if (m_unscaledMode)
                {
                    ushort nextStop = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding;
                    float prevStationIdx = Array.IndexOf(m_cachedStopOrder, TransportLine.GetPrevStop(nextStop));
                    float nextStationIdx = Array.IndexOf(m_cachedStopOrder, nextStop);
                    if (nextStationIdx < prevStationIdx)
                    {
                        nextStationIdx += m_cachedStopOrder.Length;
                    }
                    Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                    relativePosition.y
                        = (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Leaving)) != 0 ? (prevStationIdx * 0.75f) + (nextStationIdx * 0.25f)
                        : (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Arriving)) != 0 ? (prevStationIdx * 0.25f) + (nextStationIdx * 0.75f)
                        : (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Stopped)) != 0 ? prevStationIdx
                        : (prevStationIdx * 0.5f) + (nextStationIdx * 0.5f);
                    relativePosition.y = ShiftVerticalPosition(relativePosition.y * m_kminStopDistance);
                    m_vehicleButtons.items[idx].relativePosition = relativePosition;
                }
                else
                {
                    if (info.m_vehicleAI.GetProgressStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out float current, out float max))
                    {
                        float percent = current / max;
                        float y = m_uILineLength * percent;
                        Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                        relativePosition.y = ShiftVerticalPosition(y);
                        m_vehicleButtons.items[idx].relativePosition = relativePosition;
                    }
                }

                info.m_vehicleAI.GetBufferStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out _, out int passengerQuantity, out int passengerCapacity);
                UILabel labelVehicle = m_vehicleButtons.items[idx].Find<UILabel>("PassengerCount");
                labelVehicle.prefix = passengerQuantity.ToString() + "/" + passengerCapacity.ToString();
                labelVehicle.processMarkup = true;
                labelVehicle.textAlignment = UIHorizontalAlignment.Right;
                switch (m_currentMode)
                {
                    case MapMode.WAITING:
                    case MapMode.NONE:
                    case MapMode.CONNECTIONS:
                        labelVehicle.text = "";
                        labelVehicle.suffix = "";
                        break;
                    case MapMode.EARNINGS_ALL_TIME:
                        TLMTransportLineStatusesManager.instance.GetIncomeAndExpensesForVehicle(vehicleId, out long income, out long expense);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income, expense, 100);
                        break;
                    case MapMode.EARNINGS_LAST_WEEK:
                        TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForVehicles(vehicleId, out long income2, out long expense2);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income2, expense2, 8);
                        break;
                    case MapMode.EARNINGS_CURRENT_WEEK:
                        TLMTransportLineStatusesManager.instance.GetCurrentIncomeAndExpensesForVehicles(vehicleId, out long income3, out long expense3);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income3, expense3, 8);
                        break;
                }

                vehicleId = instance.m_vehicles.m_buffer[vehicleId].m_nextLineVehicle;
                if (++idx >= 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (idx < m_vehicleButtons.items.Count - 1)
            {
                m_vehicleCountMismatch = true;
            }
        }

        private void PrintIncomeExpenseVehicle(ushort lineID, int idx, UILabel labelVehicle, long income, long expense, float scale)
        {
            var tsd = TransportSystemDefinition.From(lineID);
            m_vehicleButtons.items[idx].color = Color.Lerp(Color.white, income > expense ? Color.green : Color.red, Mathf.Max(income, expense) / scale * TLMLineUtils.GetTicketPriceForLine(tsd, lineID).First.Value);
            labelVehicle.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            labelVehicle.suffix = $"\n<color #ff0000>{(expense / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
        }



        public void OnGotFocus() => m_cachedScrollPosition = m_scrollPanel.scrollPosition;
        private void OnGotFocusBind(UIComponent component, UIFocusEventParameter eventParam) => m_cachedScrollPosition = m_scrollPanel.scrollPosition;

        internal LineType GetLineType(ushort lineID) => UVMPublicTransportWorldInfoPanel.GetLineType(lineID);

        private float ShiftVerticalPosition(float y)
        {
            y += m_uILineOffset;
            if (y > m_uILineLength + 5f)
            {
                y -= m_uILineLength;
            }
            return y;
        }

        private void RefreshVehicleButtons(ushort lineID)
        {
            if (m_currentMode == MapMode.CONNECTIONS)
            {
                m_vehiclesLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(0);
                m_connectionLabel.isVisible = true;
            }
            else
            {
                m_vehiclesLabel.isVisible = true;
                m_connectionLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID));
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort num = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
                int num2 = 0;
                while (num != 0)
                {
                    Vector3 relativePosition = m_vehicleButtons.items[num2].relativePosition;
                    relativePosition.x = m_kvehiclesX;
                    m_vehicleButtons.items[num2].relativePosition = relativePosition;
                    m_vehicleButtons.items[num2].objectUserData = num;
                    num = instance.m_vehicles.m_buffer[num].m_nextLineVehicle;
                    if (++num2 >= 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        public void Hide()
        {
            m_bg.isVisible = false;
            m_bgScrollbar.isVisible = false;
            m_unscaledCheck.isVisible = false;
            m_mapModeDropDown.isVisible = false;
            m_labelLineIncomplete.isVisible = false;
        }

        internal ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        private void UpdateStopButtons(ushort lineID)
        {
            if (GetLineType(lineID) != LineType.WalkingTour || m_dirtyNames)
            {
                ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                foreach (UIPanel uiPanel in m_stopButtons.items)
                {
                    UILabel uilabel = uiPanel.Find<UILabel>("PassengerCount");
                    UIButton uibutton = uiPanel.Find<UIButton>("StopButton");
                    if (m_dirtyNames)
                    {
                        uilabel.prefix = TLMStationUtils.GetFullStationName((ushort)uibutton.objectUserData, lineID, TransportSystemDefinition.GetDefinitionForLine(lineID).SubService);
                    }
                    if (m_dirtyTerminal)
                    {
                        UpdateTerminalStatus(lineID, stop, uibutton);
                    }
                    if (GetLineType(lineID) == LineType.WalkingTour)
                    {
                        continue;
                    }


                    UIPanel connectionPanel = uiPanel.Find<UIPanel>("ConnectionPanel");
                    if (connectionPanel != null)
                    {
                        connectionPanel.isVisible = m_currentMode == MapMode.CONNECTIONS;
                    }


                    switch (m_currentMode)
                    {
                        case MapMode.WAITING:
                            TLMLineUtils.GetQuantityPassengerWaiting(stop, out int residents, out int tourists, out int timeTillBored);
                            uilabel.text = "\n" + string.Format(Locale.Get("K45_TLM_WAITING_PASSENGERS_RESIDENT_TOURSTS"), residents + tourists, residents, tourists) + "\n";
                            uibutton.color = Color.Lerp(Color.red, Color.white, timeTillBored / 255f);
                            uilabel.suffix = string.Format(Locale.Get("K45_TLM_TIME_TILL_BORED_TEMPLATE_STATION_MAP"), uibutton.color.ToRGB(), timeTillBored);
                            break;
                        case MapMode.NONE:
                            uibutton.color = Color.white;
                            uilabel.text = "";
                            uilabel.suffix = "";
                            uibutton.tooltip = "";
                            break;
                        case MapMode.CONNECTIONS:
                            uibutton.color = Color.white;
                            uilabel.text = "";
                            uilabel.suffix = "";
                            uibutton.tooltip = "";
                            break;
                        case MapMode.EARNINGS_ALL_TIME:
                            TLMTransportLineStatusesManager.instance.GetStopIncome(stop, out long income);
                            PrintIncomeStop(lineID, uibutton, uilabel, income);
                            break;
                        case MapMode.EARNINGS_CURRENT_WEEK:
                            TLMTransportLineStatusesManager.instance.GetCurrentStopIncome(stop, out long income2);
                            PrintIncomeStop(lineID, uibutton, uilabel, income2);
                            break;
                        case MapMode.EARNINGS_LAST_WEEK:
                            TLMTransportLineStatusesManager.instance.GetLastWeekStopIncome(stop, out long income3);
                            PrintIncomeStop(lineID, uibutton, uilabel, income3);
                            break;
                    }
                    stop = TransportLine.GetNextStop(stop);
                }
                m_dirtyNames = false;
                m_dirtyTerminal = false;
            }
        }

        private static void PrintIncomeStop(ushort lineID, UIButton uibutton, UILabel uilabel, long income)
        {
            uibutton.color = Color.Lerp(Color.white, Color.green, income / (1000f * Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info.m_ticketPrice));
            uilabel.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            uibutton.tooltip = "";
            uilabel.suffix = "";
        }

        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID() > 0;

        private enum MapMode
        {
            NONE,
            WAITING,
            CONNECTIONS,
            EARNINGS_CURRENT_WEEK,
            EARNINGS_LAST_WEEK,
            EARNINGS_ALL_TIME,
        }
    }
}