using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class UVMBudgetEditorLine : MonoBehaviour
    {
        private UIPanel m_container;
        //private UILabel m_identifier;
        private UITextField m_timeInput;
        private UILabel m_value;
        private UISlider m_slider;
        private UIButton m_die;

        private bool m_loading = false;

        public int zOrder
        {
            get => m_container.zOrder;
            set => m_container.zOrder = value;
        }

        public BudgetEntryXml Entry
        {
            get => m_entry; set {
                m_entry = value;
                FillData();
            }
        }
        public event Action<BudgetEntryXml> OnDie;
        public event Action<BudgetEntryXml, int> OnTimeChanged;
        public event Action<BudgetEntryXml, float> OnBudgetChanged;
        private int m_id;
        private BudgetEntryXml m_entry;

        public void SetLegendInfo(Color c, int id)
        {
            m_timeInput.eventTextChanged -= SendText;
            m_id = id - 1;
            //m_identifier.text = "";
            //m_identifier.color = c;
            ((UISprite) m_slider.thumbObject).color = c;
            m_timeInput.eventTextChanged += SendText;
        }

        private void FillData()
        {
            m_loading = true;
            try
            {
                m_timeInput.text = Entry.HourOfDay.ToString();
                m_slider.value = Entry.Value;
                m_value.text = $"{Entry.Value}%";
            }
            finally
            {
                m_loading = false;
            }
        }

        public string GetCurrentVal() => m_timeInput.text;

        public void SetTextColor(Color c) => m_timeInput.color = c;

        public void Awake()
        {
            m_container = transform.gameObject.AddComponent<UIPanel>();
            m_container.width = transform.parent.gameObject.GetComponent<UIComponent>().width;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "AzimuthInputLine";

            //KlyteMonoUtils.CreateUIElement(out m_identifier, m_container.transform, "CityId");
            //m_identifier.autoSize = false;
            //m_identifier.relativePosition = new Vector3(0, 0);
            //m_identifier.backgroundSprite = "EmptySprite";
            //m_identifier.width = 30;
            //m_identifier.height = 30;
            //m_identifier.textScale = 1.3f;
            //m_identifier.padding = new RectOffset(3, 3, 4, 3);
            //m_identifier.useOutline = true;
            //m_identifier.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out m_timeInput, m_container.transform, "StartAzimuth");
            KlyteMonoUtils.UiTextFieldDefaults(m_timeInput);
            m_timeInput.normalBgSprite = "OptionsDropboxListbox";
            m_timeInput.width = 50;
            m_timeInput.height = 28;
            m_timeInput.textScale = 1.6f;
            m_timeInput.maxLength = 2;
            m_timeInput.numericalOnly = true;
            m_timeInput.text = "0";
            m_timeInput.eventTextChanged += SendText;

            KlyteMonoUtils.CreateUIElement(out m_value, m_container.transform, "Direction");
            m_value.autoSize = false;
            m_value.width = 60;
            m_value.height = 30;
            m_value.textScale = 1.125f;
            m_value.textAlignment = UIHorizontalAlignment.Center;
            m_value.padding = new RectOffset(3, 3, 5, 3);

            m_slider = GenerateBudgetMultiplierField(m_container);

            KlyteMonoUtils.CreateUIElement(out m_die, m_container.transform, "Delete");
            m_die.textScale = 1f;
            m_die.width = 30;
            m_die.height = 30;
            m_die.tooltip = Locale.Get("K45_ADR_DELETE_STOP_NEIGHBOR");
            KlyteMonoUtils.InitButton(m_die, true, "ButtonMenu");
            m_die.isVisible = true;
            m_die.text = "X";
            m_die.eventClick += (component, eventParam) => OnDie?.Invoke(Entry);

        }

        private UISlider GenerateBudgetMultiplierField(UIComponent container)
        {
            UISlider bugdetSlider = UIHelperExtension.AddSlider(container, null, 0, 500, 5, -1,
                (x) =>
                {

                });
            Destroy(bugdetSlider.transform.parent.GetComponentInChildren<UILabel>());
            UIPanel budgetSliderPanel = bugdetSlider.GetComponentInParent<UIPanel>();

            budgetSliderPanel.width = 205;
            budgetSliderPanel.height = 20;
            budgetSliderPanel.autoLayout = true;

            bugdetSlider.size = new Vector2(200, 20);
            bugdetSlider.scrollWheelAmount = 0;
            bugdetSlider.clipChildren = true;
            bugdetSlider.thumbOffset = new Vector2(-200, 0);
            bugdetSlider.color = new Color32(128, 128, 128, 128);

            bugdetSlider.thumbObject.width = 400;
            bugdetSlider.thumbObject.height = 20;
            ((UISprite) bugdetSlider.thumbObject).spriteName = "PlainWhite";
            ((UISprite) bugdetSlider.thumbObject).color = new Color32(1, 140, 46, 255);

            bugdetSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                SetBudgetHour(val);
            };

            return bugdetSlider;
        }

        private void SetBudgetHour(float val)
        {
            if (m_loading)
            {
                return;
            }

            OnBudgetChanged?.Invoke(Entry, val);
            FillData();
        }

        private void SendText(UIComponent x, string y)
        {
            if (m_loading)
            {
                return;
            }
            if (int.TryParse(y, out int time))
            {
                OnTimeChanged?.Invoke(Entry, time);
            }
        }
    }

}

