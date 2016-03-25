using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Extensions;
using System;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;
using Klyte.TransportLinesManager.Extensors;
using System.Collections.Generic;
using ColossalFramework.Globalization;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMDepotInfoPanel
    {
        private TLMController m_controller;

        //line info	
        private UIPanel depotInfoPanel;
        private InstanceID m_buildingIdSelecionado;
        private CameraController m_CameraController;
        private string lastDepotName;
        private UILabel vehiclesInUseLabel;
        private UILabel lineTransportIconTypeLabel;
        private UILabel upkeepCost;
        private UILabel prefixesSpawned;
        private UILabel passengersLastWeek;
        //private UILabel generalDebugLabel;
        private UITextField depotNameField;
        private TLMWorkerChartPanel workerChart;
        private Dictionary<uint, UICheckBox> prefixesCheckboxes;
        private bool isLoading = false;

        public Transform transform
        {
            get
            {
                return depotInfoPanel.transform;
            }
        }

        public GameObject gameObject
        {
            get
            {
                try
                {
                    return depotInfoPanel.gameObject;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    return null;
                }
            }
        }

        public bool isVisible
        {
            get
            {
                return depotInfoPanel.isVisible;
            }
        }

        public TLMController controller
        {
            get
            {
                return m_controller;
            }
        }


        public InstanceID lineIdSelecionado
        {
            get
            {
                return m_buildingIdSelecionado;
            }
        }

        public CameraController cameraController
        {
            get
            {
                return m_CameraController;
            }
        }

        public TLMDepotInfoPanel(TLMController controller)
        {
            this.m_controller = controller;
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
                m_CameraController = gameObject.GetComponent<CameraController>();
            }
            createInfoView();
        }

        public void Show()
        {
            if (!GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible)
            {
                GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible = true;
            }
            depotInfoPanel.Show();
        }

        public void Hide()
        {
            depotInfoPanel.Hide();
        }




        //ACOES
        private void saveDepotName(UITextField u)
        {
            string value = u.text;

            TLMUtils.setBuildingName(m_buildingIdSelecionado.Building, value);
        }

        private void createInfoView()
        {
            //line info painel

            TLMUtils.createUIElement<UIPanel>(ref depotInfoPanel, m_controller.mainRef.transform);
            depotInfoPanel.Hide();
            depotInfoPanel.relativePosition = new Vector3(394.0f, 0.0f);
            depotInfoPanel.width = 650;
            depotInfoPanel.height = 290;
            depotInfoPanel.zOrder = 50;
            depotInfoPanel.color = new Color32(255, 255, 255, 255);
            depotInfoPanel.backgroundSprite = "MenuPanel2";
            depotInfoPanel.name = "DepotInfoPanel";
            depotInfoPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            depotInfoPanel.autoLayout = false;
            depotInfoPanel.useCenter = true;
            depotInfoPanel.wrapLayout = false;
            depotInfoPanel.canFocus = true;
            TLMUtils.createDragHandle(depotInfoPanel, depotInfoPanel, 35f);



            TLMUtils.createUIElement<UILabel>(ref lineTransportIconTypeLabel, depotInfoPanel.transform);
            lineTransportIconTypeLabel.autoSize = false;
            lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            lineTransportIconTypeLabel.width = 30;
            lineTransportIconTypeLabel.height = 20;
            lineTransportIconTypeLabel.name = "DepotTransportIcon";
            lineTransportIconTypeLabel.clipChildren = true;
            TLMUtils.createDragHandle(lineTransportIconTypeLabel, depotInfoPanel);


            TLMUtils.createUIElement<UITextField>(ref depotNameField, depotInfoPanel.transform);
            depotNameField.autoSize = false;
            depotNameField.relativePosition = new Vector3(160f, 10f);
            depotNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            depotNameField.text = "NOME";
            depotNameField.width = 450;
            depotNameField.height = 25;
            depotNameField.name = "DepotNameLabel";
            depotNameField.maxLength = 256;
            depotNameField.textScale = 1.5f;
            TLMUtils.uiTextFieldDefaults(depotNameField);
            depotNameField.eventGotFocus += (component, eventParam) =>
            {
                lastDepotName = depotNameField.text;
            };
            depotNameField.eventLostFocus += (component, eventParam) =>
            {
                if (lastDepotName != depotNameField.text)
                {
                    saveDepotName(depotNameField);
                }
                depotNameField.text = Singleton<BuildingManager>.instance.GetBuildingName(m_buildingIdSelecionado.Building, default(InstanceID));
            };

            TLMUtils.createUIElement<UILabel>(ref vehiclesInUseLabel, depotInfoPanel.transform);
            vehiclesInUseLabel.autoSize = false;
            vehiclesInUseLabel.relativePosition = new Vector3(10f, 60f);
            vehiclesInUseLabel.textAlignment = UIHorizontalAlignment.Left;
            vehiclesInUseLabel.text = "";
            vehiclesInUseLabel.width = 550;
            vehiclesInUseLabel.height = 25;
            vehiclesInUseLabel.prefix = "Vehicles in use: ";
            vehiclesInUseLabel.suffix = "";
            vehiclesInUseLabel.name = "VehiclesInUseLabel";
            vehiclesInUseLabel.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref passengersLastWeek, depotInfoPanel.transform);
            passengersLastWeek.autoSize = false;
            passengersLastWeek.relativePosition = new Vector3(10f, 90);
            passengersLastWeek.textAlignment = UIHorizontalAlignment.Left;
            passengersLastWeek.text = "";
            passengersLastWeek.width = 550;
            passengersLastWeek.height = 25;
            passengersLastWeek.name = "PassengersLastWeek";
            passengersLastWeek.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref upkeepCost, depotInfoPanel.transform);
            upkeepCost.autoSize = false;
            upkeepCost.relativePosition = new Vector3(10f, 75);
            upkeepCost.textAlignment = UIHorizontalAlignment.Left;
            upkeepCost.width = 250;
            upkeepCost.height = 25;
            upkeepCost.name = "AvoidedTravelsLabel";
            upkeepCost.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref prefixesSpawned, depotInfoPanel.transform);
            prefixesSpawned.autoSize = false;
            prefixesSpawned.relativePosition = new Vector3(10f, 120f);
            prefixesSpawned.textAlignment = UIHorizontalAlignment.Left;
            prefixesSpawned.text = "Spawned prefixes in this depot:";
            prefixesSpawned.width = 350;
            prefixesSpawned.height = 25;
            prefixesSpawned.name = "TouristAndPassagersLabel";
            prefixesSpawned.textScale = 0.8f;

            UIPanel prefixesPanel = null;
            TLMUtils.createUIElement<UIPanel>(ref prefixesPanel, depotInfoPanel.transform);
            prefixesPanel.autoSize = false;
            prefixesPanel.relativePosition = new Vector3(10f, 135f);
            prefixesPanel.width = 630;
            prefixesPanel.height = 100;
            prefixesPanel.name = "Prefixes Panel";
            prefixesPanel.autoLayout = true;
            prefixesPanel.wrapLayout = true;

            prefixesCheckboxes = new Dictionary<uint, UICheckBox>();
            for (uint i = 0; i <= 65; i++)
            {
                prefixesCheckboxes[i] = prefixesPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;
                prefixesCheckboxes[i].text = i == 0 ? "Unprefixed" : i == 65 ? "Regional" : i.ToString();
                prefixesCheckboxes[i].width = 50;
                prefixesCheckboxes[i].GetComponentInChildren<UILabel>().relativePosition = new Vector3(20, 2);
                uint j = i;
                prefixesCheckboxes[i].eventCheckChanged += (x, y) =>
                {
                     if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("prefixesCheckboxes[i].eventCheckChanged; j = {0}; check = {1}; loading = {2}", j, y, isLoading);
                    if (!isLoading)
                    {
                        togglePrefix(j, y);
                    }
                };
            }

            prefixesCheckboxes[0].width = prefixesPanel.width / 2.1f;
            prefixesCheckboxes[65].width = prefixesPanel.width / 2.1f;
            prefixesCheckboxes[65].zOrder = 0;

            UIButton voltarButton2 = null;
            TLMUtils.createUIElement<UIButton>(ref voltarButton2, depotInfoPanel.transform);
            voltarButton2.relativePosition = new Vector3(depotInfoPanel.width - 33f, 5f);
            voltarButton2.width = 28;
            voltarButton2.height = 28;
            TLMUtils.initButton(voltarButton2, true, "DeleteLineButton");
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeDepotInfo;

            workerChart = new TLMWorkerChartPanel(this, new Vector3(400f, 60f));

            UIButton addAllPrefixesButton = null;
            TLMUtils.createUIElement<UIButton>(ref addAllPrefixesButton, transform);
            addAllPrefixesButton.relativePosition = new Vector3(200, 120f);
            addAllPrefixesButton.text = "Add All";
            addAllPrefixesButton.textScale = 0.6f;
            addAllPrefixesButton.width = 80;
            addAllPrefixesButton.height = 15;
            addAllPrefixesButton.tooltip = "Add all prefixes to this Depot";
            TLMUtils.initButton(addAllPrefixesButton, true, "ButtonMenu");
            addAllPrefixesButton.name = "AddAll";
            addAllPrefixesButton.isVisible = true;
            addAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.addAllPrefixesToDepot(m_buildingIdSelecionado.Building);
                updateCheckboxes();
            };

            UIButton removeAllPrefixesButton = null;
            TLMUtils.createUIElement<UIButton>(ref removeAllPrefixesButton, transform);
            removeAllPrefixesButton.relativePosition = new Vector3(300, 120f);
            removeAllPrefixesButton.text = "Remove All";
            removeAllPrefixesButton.textScale = 0.6f;
            removeAllPrefixesButton.width = 80;
            removeAllPrefixesButton.height = 15;
            removeAllPrefixesButton.tooltip = "Add this prefix to served prefixes list of this Depot.";
            TLMUtils.initButton(removeAllPrefixesButton, true, "ButtonMenu");
            removeAllPrefixesButton.name = "RemoveAll";
            removeAllPrefixesButton.isVisible = true;
            removeAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.removeAllPrefixesFromDepot(m_buildingIdSelecionado.Building);
                updateCheckboxes();
            };
        }

        private void togglePrefix(uint prefix, bool value)
        {
            if (value)
            {
                TLMDepotAI.addPrefixToDepot(m_buildingIdSelecionado.Building, prefix);
            }
            else
            {
                TLMDepotAI.removePrefixFromDepot(m_buildingIdSelecionado.Building, prefix);
            }
        }

        public void updateBidings()
        {
            BuildingInfo basicInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingIdSelecionado.Building].Info;
            DepotAI basicAI = basicInfo.GetAI() as DepotAI;

            if (basicAI == null)
            {
                closeDepotInfo(null, null);
                return;
            }

            TransportStationAI stationAI = basicInfo.GetAI() as TransportStationAI;
            HarborAI harborAI = basicInfo.GetAI() as HarborAI;


            vehiclesInUseLabel.text = basicAI.GetVehicleCount(m_buildingIdSelecionado.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingIdSelecionado.Building]).ToString();
            if (stationAI != null)
            {
                passengersLastWeek.isVisible = true;
                int passengerCount = stationAI.GetPassengerCount(m_buildingIdSelecionado.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingIdSelecionado.Building]);
                passengersLastWeek.text = LocaleFormatter.FormatGeneric("AIINFO_PASSENGERS_SERVICED", new object[] { passengerCount });
            }
            else
            {
                passengersLastWeek.isVisible = false;
            }

            upkeepCost.text = LocaleFormatter.FormatUpkeep(basicAI.GetResourceRate(m_buildingIdSelecionado.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)m_buildingIdSelecionado.Building], EconomyManager.Resource.Maintenance), false);

            uint num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingIdSelecionado.Building].m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            int unskill = 0;
            int oneSchool = 0;
            int twoSchool = 0;
            int threeSchool = 0;

            CitizenManager instance = Singleton<CitizenManager>.instance;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                        if (citizen != 0u && !instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].Dead && (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
                        {
                            num3++;
                            switch (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].EducationLevel)
                            {
                                case Citizen.Education.Uneducated:
                                    unskill++;
                                    break;
                                case Citizen.Education.OneSchool:
                                    oneSchool++;
                                    break;
                                case Citizen.Education.TwoSchools:
                                    twoSchool++;
                                    break;
                                case Citizen.Education.ThreeSchools:
                                    threeSchool++;
                                    break;
                            }
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            workerChart.SetValues(new int[] { unskill, oneSchool, twoSchool, threeSchool }, new int[] { basicAI.m_workPlaceCount0, basicAI.m_workPlaceCount1, basicAI.m_workPlaceCount2, basicAI.m_workPlaceCount3 });
        }



        public void closeDepotInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)m_buildingIdSelecionado.TransportLine];
            Hide();
            m_controller.defaultListingLinesPanel.Show();
        }

        public void openDepotInfo(ushort buildingID)
        {
            isLoading = true;
            WorldInfoPanel.HideAllWorldInfoPanels();

            m_buildingIdSelecionado = default(InstanceID);
            m_buildingIdSelecionado.Building = buildingID;

            DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (depotAI == null) return;
            depotNameField.text = Singleton<BuildingManager>.instance.GetBuildingName(buildingID, default(InstanceID));

            lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            lineTransportIconTypeLabel.height = 20;
            lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(depotAI.m_transportInfo.m_transportType);

            Show();
            m_controller.defaultListingLinesPanel.Hide();

            updateCheckboxes();

            isLoading = false;
        }

        private void updateCheckboxes()
        {
            bool oldIsLoading = isLoading;
            isLoading = true;
            DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingIdSelecionado.Building].Info.GetAI() as DepotAI;
            List<string> prefixOptions = TLMUtils.getDepotPrefixesOptions(TLMCW.getConfigIndexForTransportType(depotAI.m_transportInfo.m_transportType));
            var prefixesServedList = TLMDepotAI.getPrefixesServedByDepot(m_buildingIdSelecionado.Building);
            for (uint i = 0; i <= 64; i++)
            {
                if (i < prefixOptions.Count)
                {
                    prefixesCheckboxes[i].isVisible = true;
                    prefixesCheckboxes[i].isChecked = prefixesServedList.Contains(i);
                    prefixesCheckboxes[i].text = prefixOptions[(int)i];
                }
                else
                {
                    prefixesCheckboxes[i].isVisible = false;
                }
            }
            prefixesCheckboxes[65].isChecked = prefixesServedList.Contains(65);
            isLoading = oldIsLoading;
        }
    }
}

