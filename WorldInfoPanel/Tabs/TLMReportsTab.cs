﻿using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMReportsTab : UICustomControl, IUVMPTWIPChild
    {
        private UIPanel m_bg;
        private UITabstrip m_reportTabstrip;
        private Dictionary<string, ITLMReportChild> m_childControls = new Dictionary<string, ITLMReportChild>();
        private bool m_showDayTime = false;

        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = true;
            m_bg.autoLayoutDirection = LayoutDirection.Vertical;
            m_bg.clipChildren = true;
        }

        public void Update()
        {
            if (m_childControls.Count == 0 && m_bg.isVisible)
            {
                float heightCheck = 0f;
                if (!TLMController.IsRealTimeEnabled)
                {
                    var uiHelper = new UIHelperExtension(m_bg);
                    UICheckBox m_checkChangeDateLabel = uiHelper.AddCheckboxLocale("K45_TLM_SHOW_DAYTIME_INSTEAD_DATE", false, (x) => m_showDayTime = x && SimulationManager.instance.m_enableDayNight);
                    KlyteMonoUtils.LimitWidthAndBox(m_checkChangeDateLabel.label, m_bg.width - 50);
                    heightCheck = m_checkChangeDateLabel.height;
                }
                KlyteMonoUtils.CreateTabsComponent(out m_reportTabstrip, out _, m_bg.transform, "LineConfig", new Vector4(0, 0, m_bg.width, 30), new Vector4(0, 30, m_bg.width, m_bg.height - heightCheck - 30));
                m_childControls.Add("FinanceReport", TabCommons.CreateTabLocalized<TLMLineFinanceReportTab>(m_reportTabstrip, "InfoPanelIconCurrency", "K45_TLM_WIP_FINANCE_REPORT_TAB", "FinanceReport", false));
                m_childControls.Add("PassengerAgeReport", TabCommons.CreateTabLocalized<TLMLinePassengerAgeReportTab>(m_reportTabstrip, "InfoIconAge", "K45_TLM_WIP_PASSENGER_AGE_REPORT_TAB", "PassengerAgeReport", false));
                m_childControls.Add("PassengerStudentTouristReport", TabCommons.CreateTabLocalized<TLMLinePassengerStudentTouristsReportTab>(m_reportTabstrip, "InfoIconTourism", "K45_TLM_WIP_PASSENGER_REPORT_TAB", "PassengerStudentTouristReport", false));
                m_childControls.Add("PassengerWealthReport", TabCommons.CreateTabLocalized<TLMLinePassengerWealthReportTab>(m_reportTabstrip, "InfoIconLandValue", "K45_TLM_WIP_PASSENGER_WEALTH_REPORT_TAB", "PassengerWealthReport", false));
                m_childControls.Add("PassengerGenderReport", TabCommons.CreateTabLocalized<TLMLinePassengerGenderReportTab>(m_reportTabstrip, "InfoIconPopulation", "K45_TLM_WIP_PASSENGER_GENDER_REPORT_TAB", "PassengerGenderReport", false));
            }
        }

        public void UpdateBindings()
        {
            foreach (KeyValuePair<string, ITLMReportChild> tab in m_childControls)
            {
                if (tab.Value.MayBeVisible())
                {
                    tab.Value.UpdateBindings(m_showDayTime);
                }
            }
        }
        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGotFocus() { }
        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID() > 0;
        public void Hide() => m_bg.isVisible = false;
        public void OnSetTarget(Type source)
        {

        }


        public interface ITLMReportChild
        {
            void UpdateBindings(bool showDayTime);
            bool MayBeVisible();
        }
    }
}
