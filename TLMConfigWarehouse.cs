﻿using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static Klyte.TransportLinesManager.TLMConfigWarehouse.ConfigIndex;

namespace Klyte.TransportLinesManager
{
    public class TLMConfigWarehouse : ConfigWarehouseBase<TLMConfigWarehouse.ConfigIndex, TLMConfigWarehouse>
    {

        protected override string ID { get; } = "K45_TLM_ConfigWarehouse";
        [XmlIgnore]
        public static readonly ConfigIndex[] PALETTES_INDEXES = new ConfigIndex[] {
          SHIP_PALETTE_MAIN,
          TRAIN_PALETTE_MAIN,
          TRAM_PALETTE_MAIN,
          METRO_PALETTE_MAIN ,
          BUS_PALETTE_MAIN ,
          PLANE_PALETTE_MAIN ,
          MONORAIL_PALETTE_MAIN ,
          TOUR_BUS_CONFIG_PALETTE_MAIN ,
          TOUR_PED_CONFIG_PALETTE_MAIN,
          TROLLEY_CONFIG_PALETTE_MAIN,
          HELICOPTER_CONFIG_PALETTE_MAIN
       };
        protected bool unsafeMode = false;
        public TLMConfigWarehouse() { }

        public override void SetBool(ConfigIndex idx, bool? newVal)
        {
            base.SetBool(idx, newVal);
            CheckEvents(idx);
        }

        public override void SetInt(ConfigIndex idx, int? value)
        {
            base.SetInt(idx, value);
            CheckEvents(idx);
        }

        public override void SetString(ConfigIndex i, string value)
        {
            base.SetString(i, value);
            CheckEvents(i);
        }

        private void CheckEvents(ConfigIndex idx)
        {
            LogUtils.DoLog($"CheckEvents: {idx} | (idx & ADC_DESC_PART) = {(idx & ADC_DESC_PART)} | idx & DESC_DATA = {idx & DESC_DATA}");
            if ((idx & ADC_DESC_PART) == 0)
            {
                switch (idx & (DESC_DATA | ConfigIndex.TYPE_PART))
                {
                    case PREFIX:
                    case SEPARATOR:
                    case SUFFIX:
                    case LEADING_ZEROS:
                    case INVERT_PREFIX_SUFFIX:
                    case NON_PREFIX:
                    case TRANSPORT_ICON_TLM:
                        TLMFacade.Instance?.OnLineSymbolParameterChanged();
                        break;
                    case VEHICLE_NUMBER_FORMAT_FOREIGN:
                    case VEHICLE_NUMBER_FORMAT_LOCAL:
                        TLMFacade.Instance?.OnVehicleIdentifierParameterChanged();
                        break;
                }
            }
            if ((idx & AUTO_NAMING_REF_TEXT) != 0 || (idx & USE_FOR_AUTO_NAMING_REF) != 0)
            {
                TLMFacade.Instance?.OnAutoNameParameterChanged();
            }
        }

        protected override void FallBackDefaultFile()
        {
            /*LogUtils.DoErrorLog("FallBackDefaultFile");
            var legacyConfig = TLMConfigWarehouseLegacy.getConfig(null, null);
            if (legacyConfig != null)
            {
                LogUtils.DoErrorLog("HAS LEGACY");
                foreach (ConfigIndex ci in Enum.GetValues(typeof(ConfigIndex)))
                {
                    try
                    {
                        switch (((int)ci) & TYPE_PART)
                        {
                            case TYPE_BOOL:
                                m_cachedBoolSaved[ci] = legacyConfig.getBool(ci);
                                break;
                            case TYPE_STRING:
                                m_cachedStringSaved[ci] = legacyConfig.getString(ci);
                                break;
                            case TYPE_INT:
                                m_cachedIntSaved[ci] = legacyConfig.getInt(ci);
                                break;
                        }
                    }
                    catch { }
                }
                SaveAsDefault();
            }*/

        }

        public static Color32 getColorForTransportType(ConfigIndex i)
        {
            switch (i & SYSTEM_PART)
            {
                case TRAIN_CONFIG:
                    return new Color32(250, 104, 0, 255);
                case TRAM_CONFIG:
                    return new Color32(73, 27, 137, 255);
                case METRO_CONFIG:
                    return new Color32(58, 224, 50, 255);
                case BUS_CONFIG:
                    return new Color32(53, 121, 188, 255);
                case PLANE_CONFIG:
                    return new Color32(0xa8, 0x01, 0x7a, 255);
                case SHIP_CONFIG:
                    return new Color32(0xa3, 0xb0, 0, 255);
                case BLIMP_CONFIG:
                    return new Color32(0xd8, 0x01, 0xaa, 255);
                case FERRY_CONFIG:
                    return new Color32(0xe3, 0xf0, 0, 255);
                case MONORAIL_CONFIG:
                    return new Color32(217, 51, 89, 255);
                case CABLE_CAR_CONFIG:
                    return new Color32(31, 96, 225, 255);
                case TAXI_CONFIG:
                    return new Color32(60, 184, 120, 255);
                case EVAC_BUS_CONFIG:
                    return new Color32(202, 162, 31, 255);
                case TOUR_BUS_CONFIG:
                    return new Color32(110, 152, 251, 255);
                case TOUR_PED_CONFIG:
                    return new Color32(83, 157, 48, 255);
                case TROLLEY_CONFIG:
                    return new Color(1, .517f, 0, 1);
                case HELICOPTER_CONFIG:
                    return new Color(.671f, .333f, .604f, 1);
                default:
                    return new Color();

            }
        }


        public static string GetNameForServiceType(ConfigIndex i) => Locale.Get(GetLocaleIdForIndex(i, out string key, out int index), key, index);

        private static string GetLocaleIdForIndex(ConfigIndex i, out string key, out int index)
        {
            index = 0;
            switch (i & DESC_DATA)
            {
                case PLAYER_EDUCATION_SERVICE_CONFIG:
                    index = 2;
                    break;
            };
            key = null;

            switch (i & DESC_DATA)
            {
                case BEAUTIFICATION_SERVICE_CONFIG:
                    key = "Beautification";
                    break;
                case ELECTRICITY_SERVICE_CONFIG:
                    key = "Electricity";
                    break;
                case WATER_SERVICE_CONFIG:
                    key = "WaterAndSewage";
                    break;
                case GARBAGE_SERVICE_CONFIG:
                    key = "Garbage";
                    break;
                case ROAD_SERVICE_CONFIG:
                    key = "Roads";
                    break;
                case HEALTHCARE_SERVICE_CONFIG:
                    key = "Healthcare";
                    break;
                case POLICEDEPARTMENT_SERVICE_CONFIG:
                    key = "Police";
                    break;
                case EDUCATION_SERVICE_CONFIG:
                    key = "Education";
                    break;
                case MONUMENT_SERVICE_CONFIG:
                    key = "Monuments";
                    break;
                case FIREDEPARTMENT_SERVICE_CONFIG:
                    key = "FireDepartment";
                    break;
                case PUBLICTRANSPORT_SERVICE_CONFIG:
                    key = "PublicTransport";
                    break;
                case DISASTER_SERVICE_CONFIG:
                    key = "FireDepartment";
                    break;
                case DISTRICT_NAME_CONFIG:
                    key = "District";
                    break;
                case VARSITY_SPORTS_SERVICE_CONFIG:
                    key = "VarsitySports";
                    break;
                case MUSEUMS_SERVICE_CONFIG:
                    key = "CampusAreaMuseums";
                    break;
                case PLAYER_INDUSTRY_SERVICE_CONFIG:
                    key = "Industry";
                    break;
                case PARKAREA_NAME_CONFIG:
                    key = "ParkAreas";
                    break;
                case INDUSTRIAL_AREA_NAME_CONFIG:
                    key = "IndustryAreas";
                    break;
                case CAMPUS_AREA_NAME_CONFIG:
                    key = "CampusAreas";
                    break;
            };
            switch (i & DESC_DATA)
            {
                case RESIDENTIAL_SERVICE_CONFIG:
                    return "DISTRICT_RESIDENTIAL";
                case COMMERCIAL_SERVICE_CONFIG:
                    return "DISTRICT_COMMERCIAL";
                case INDUSTRIAL_SERVICE_CONFIG:
                    return "DISTRICT_INDUSTRIAL";
                case NATURAL_SERVICE_CONFIG:
                    return "NATURAL_SERVICE";
                //case UNUSED2_SERVICE_CONFIG:
                //return "Unused2";
                case CITIZEN_SERVICE_CONFIG:
                    return "INCOME_CITIZEN";
                case TOURISM_SERVICE_CONFIG:
                    return "INCOME_TOURIST";
                case OFFICE_SERVICE_CONFIG:
                    return "DISTRICT_OFFICE";
                case ADDRESS_NAME_CONFIG:
                    return "K45_TLM_ROAD_NAMING_STOP";
                case PARKAREA_NAME_CONFIG:
                case CAMPUS_AREA_NAME_CONFIG:
                case INDUSTRIAL_AREA_NAME_CONFIG:
                    return "FEATURES";
                case ROAD_SERVICE_CONFIG:
                case BEAUTIFICATION_SERVICE_CONFIG:
                case GARBAGE_SERVICE_CONFIG:
                case ELECTRICITY_SERVICE_CONFIG:
                case WATER_SERVICE_CONFIG:
                case HEALTHCARE_SERVICE_CONFIG:
                case POLICEDEPARTMENT_SERVICE_CONFIG:
                case EDUCATION_SERVICE_CONFIG:
                case MONUMENT_SERVICE_CONFIG:
                case FIREDEPARTMENT_SERVICE_CONFIG:
                case PUBLICTRANSPORT_SERVICE_CONFIG:
                case DISTRICT_NAME_CONFIG:
                case VARSITY_SPORTS_SERVICE_CONFIG:
                    return "MAIN_TOOL";
                case MUSEUMS_SERVICE_CONFIG:
                    return "MAIN_CATEGORY";
                case DISASTER_SERVICE_CONFIG:
                    return "MAIN_TOOL_ND";
                case PLAYER_INDUSTRY_SERVICE_CONFIG:
                    return "PARKSOVERVIEW_TOOLTIP";
                case PLAYER_EDUCATION_SERVICE_CONFIG:
                    return "INFO_EDUCATION_BUILDING";
                default:
                    return "???";

            }
        }

        //public static LineIconSpriteNames getBgIconForIndex(TLMConfigWarehouse.ConfigIndex transportType)
        //{
        //    string iconName = GetCurrentConfigString((transportType & SYSTEM_PART) | TRANSPORT_ICON_TLM);
        //    if (iconName == null || !Enum.IsDefined(typeof(LineIconSpriteNames), iconName) || iconName == LineIconSpriteNames.NULL.ToString())
        //    {
        //        return getDefaultBgIconForIndex(transportType);
        //    }
        //    else
        //    {
        //        return ((LineIconSpriteNames)Enum.Parse(typeof(LineIconSpriteNames), iconName));
        //    }
        //}
        //public static int GetSettedTicketPrice(TLMConfigWarehouse.ConfigIndex transportType) => GetCurrentConfigInt((transportType & SYSTEM_PART) | DEFAULT_TICKET_PRICE);
        private static LineIconSpriteNames getDefaultBgIconForIndex(TLMConfigWarehouse.ConfigIndex transportType)
        {
            switch (transportType & SYSTEM_PART)
            {
                case TRAIN_CONFIG:
                    return LineIconSpriteNames.K45_CircleIcon;
                case METRO_CONFIG:
                    return LineIconSpriteNames.K45_SquareIcon;
                case BUS_CONFIG:
                    return LineIconSpriteNames.K45_HexagonIcon;
                case TRAM_CONFIG:
                    return LineIconSpriteNames.K45_TrapezeIcon;
                case SHIP_CONFIG:
                    return LineIconSpriteNames.K45_DiamondIcon;
                case CABLE_CAR_CONFIG:
                    return LineIconSpriteNames.K45_ConeIcon;
                case MONORAIL_CONFIG:
                    return LineIconSpriteNames.K45_RoundedSquareIcon;
                case PLANE_CONFIG:
                    return LineIconSpriteNames.K45_PentagonIcon;
                case FERRY_CONFIG:
                    return LineIconSpriteNames.K45_S08StarIcon;
                case BLIMP_CONFIG:
                    return LineIconSpriteNames.K45_ParachuteIcon;
                case EVAC_BUS_CONFIG:
                    return LineIconSpriteNames.K45_CrossIcon;
                case TOUR_PED_CONFIG:
                    return LineIconSpriteNames.K45_MountainIcon;
                case TOUR_BUS_CONFIG:
                    return LineIconSpriteNames.K45_CameraIcon;
                case TAXI_CONFIG:
                    return LineIconSpriteNames.K45_TriangleIcon;
                case TROLLEY_CONFIG:
                    return LineIconSpriteNames.K45_OvalIcon;
                case HELICOPTER_CONFIG:
                    return LineIconSpriteNames.K45_S05StarIcon;
                default:
                    LogUtils.DoErrorLog($"INVALID TT! {transportType}");
                    return LineIconSpriteNames.K45_S09StarIcon;
            }
        }
        public static string getNameForTransportType(ConfigIndex i)
        {
            switch (i & SYSTEM_PART)
            {
                case TRAIN_CONFIG: return Locale.Get("VEHICLE_TITLE", "Train Engine");
                case TRAM_CONFIG: return Locale.Get("VEHICLE_TITLE", "Tram");
                case METRO_CONFIG: return Locale.Get("VEHICLE_TITLE", "Metro");
                case BUS_CONFIG: return Locale.Get("VEHICLE_TITLE", "Bus");
                case PLANE_CONFIG: return Locale.Get("VEHICLE_TITLE", "Aircraft Passenger");
                case SHIP_CONFIG: return Locale.Get("VEHICLE_TITLE", "Ship Passenger");
                case BLIMP_CONFIG: return Locale.Get("VEHICLE_TITLE", "Blimp");
                case FERRY_CONFIG: return Locale.Get("VEHICLE_TITLE", "Ferry");
                case MONORAIL_CONFIG: return Locale.Get("VEHICLE_TITLE", "Monorail Front");
                case EVAC_BUS_CONFIG: return Locale.Get("VEHICLE_TITLE", "Evacuation Bus");
                case TOUR_BUS_CONFIG: return Locale.Get("TOOLTIP_TOURISTBUSLINES");
                case TOUR_PED_CONFIG: return Locale.Get("TOOLTIP_WALKINGTOURS");
                case CABLE_CAR_CONFIG: return Locale.Get("VEHICLE_TITLE", "Cable Car");
                case TAXI_CONFIG: return Locale.Get("VEHICLE_TITLE", "Taxi");
                case HELICOPTER_CONFIG: return Locale.Get("VEHICLE_TITLE", "Passenger Helicopter");
                case TROLLEY_CONFIG: return Locale.Get("VEHICLE_TITLE", "Trolleybus 01");
                default: return "???";
            }
        }

        public override bool GetDefaultBoolValueForProperty(ConfigIndex i) => defaultTrueBoolProperties.Contains(i);

        public override int GetDefaultIntValueForProperty(ConfigIndex i)
        {
            switch (i)
            {
                case MAX_VEHICLES_SPECIFIC_CONFIG:
                    return 50;
                default:
                    return 0;
            }
        }
        public override string GetDefaultStringValueForProperty(ConfigIndex i)
        {
            if ((i & ADC_DESC_PART) == 0)
            {
                switch (i & (DESC_DATA | ConfigIndex.TYPE_PART))
                {
                    case VEHICLE_NUMBER_FORMAT_FOREIGN:
                    case VEHICLE_NUMBER_FORMAT_LOCAL:
                        switch (i & SYSTEM_PART)
                        {
                            case TRAIN_CONFIG:
                            case TRAM_CONFIG:
                            case METRO_CONFIG:
                            case MONORAIL_CONFIG:
                            case CABLE_CAR_CONFIG:
                                return "FOTUL";
                            case PLANE_CONFIG:
                            case BALLOON_CONFIG:
                            case BLIMP_CONFIG:
                            case HELICOPTER_CONFIG:
                                return "NO-XYZ";
                            case SHIP_CONFIG:
                            case FERRY_CONFIG:
                                return "EFNOXYZ";
                            case BUS_CONFIG:
                            case EVAC_BUS_CONFIG:
                            case TOUR_BUS_CONFIG:
                            case TROLLEY_CONFIG:
                                return "PQR EFSTU";
                            default:
                                return "VW.XYZ";
                        }
                }
            }
            return base.GetDefaultStringValueForProperty(i);
        }



        public static ItemClass.SubService getSubserviceFromSystemId(ConfigIndex idx)
        {
            ConfigIndex systemIdx = idx & SYSTEM_PART;
            switch (systemIdx)
            {
                case TRAIN_CONFIG:
                    return ItemClass.SubService.PublicTransportTrain;
                case TRAM_CONFIG:
                    return ItemClass.SubService.PublicTransportTram;
                case METRO_CONFIG:
                    return ItemClass.SubService.PublicTransportMetro;
                case BUS_CONFIG:
                    return ItemClass.SubService.PublicTransportBus;
                case EVAC_BUS_CONFIG:
                    return ItemClass.SubService.PublicTransportBus;
                case PLANE_CONFIG:
                    return ItemClass.SubService.PublicTransportPlane;
                case SHIP_CONFIG:
                    return ItemClass.SubService.PublicTransportShip;
                case MONORAIL_CONFIG:
                    return ItemClass.SubService.PublicTransportMonorail;
                case TAXI_CONFIG:
                    return ItemClass.SubService.PublicTransportTaxi;
                case CABLE_CAR_CONFIG:
                    return ItemClass.SubService.PublicTransportCableCar;
                case TOUR_PED_CONFIG:
                    return ItemClass.SubService.PublicTransportTours;
                case TOUR_BUS_CONFIG:
                    return ItemClass.SubService.PublicTransportTours;
                case BALLOON_CONFIG:
                    return ItemClass.SubService.PublicTransportTours;
                case BLIMP_CONFIG:
                    return ItemClass.SubService.PublicTransportPlane;
                case FERRY_CONFIG:
                    return ItemClass.SubService.PublicTransportShip;
                case TROLLEY_CONFIG:
                    return ItemClass.SubService.PublicTransportTrolleybus;
                case HELICOPTER_CONFIG:
                    return ItemClass.SubService.PublicTransportPlane;
                    ;

                default:
                    return ItemClass.SubService.None;
            }
        }
        public static TransferManager.TransferReason[] getTransferReasonFromSystemId(ConfigIndex idx)
        {



            ConfigIndex systemIdx = idx & SYSTEM_PART;
            switch (systemIdx)
            {
                case TRAIN_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerTrain };
                case TRAM_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Tram };
                case METRO_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.MetroTrain, TransferManager.TransferReason.PassengerTrain };
                case BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Bus };
                case EVAC_BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.EvacuateA, TransferManager.TransferReason.EvacuateB, TransferManager.TransferReason.EvacuateC, TransferManager.TransferReason.EvacuateD, TransferManager.TransferReason.EvacuateVipA, TransferManager.TransferReason.EvacuateVipB, TransferManager.TransferReason.EvacuateVipC, TransferManager.TransferReason.EvacuateVipD };
                case PLANE_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane };
                case SHIP_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerShip };
                case MONORAIL_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Monorail };
                case TAXI_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Taxi };
                case CABLE_CAR_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.CableCar };
                case TOUR_PED_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristA, TransferManager.TransferReason.TouristB, TransferManager.TransferReason.TouristC, TransferManager.TransferReason.TouristD };
                case TOUR_BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristBus };
                case BALLOON_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristA, TransferManager.TransferReason.TouristB, TransferManager.TransferReason.TouristC, TransferManager.TransferReason.TouristD };
                case BLIMP_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Blimp };
                case FERRY_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Ferry };
                case TROLLEY_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Trolleybus };
                case HELICOPTER_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerHelicopter };

                default:
                    return null;
            }
        }

        public static ConfigIndex getConfigTransportSystemForDefinition(TransportSystemDefinition tsd)
        {
            if (tsd == TransportSystemDefinition.BUS)
            {
                return BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TRAIN)
            {
                return TRAIN_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TRAM)
            {
                return TRAM_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.SHIP)
            {
                return SHIP_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.PLANE)
            {
                return PLANE_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.FERRY)
            {
                return FERRY_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.BLIMP)
            {
                return BLIMP_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.MONORAIL)
            {
                return MONORAIL_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.METRO)
            {
                return METRO_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.EVAC_BUS)
            {
                return EVAC_BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TOUR_BUS)
            {
                return TOUR_BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TOUR_PED)
            {
                return TOUR_PED_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.CABLE_CAR)
            {
                return CABLE_CAR_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TAXI)
            {
                return TAXI_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.BALLOON)
            {
                return BALLOON_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.HELICOPTER)
            {
                return HELICOPTER_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TROLLEY)
            {
                return TROLLEY_CONFIG;
            }
            else
            {
                return NIL;
            }
        }
        public static TransportSystemDefinition GetTransportSystemDefinitionForConfigTransport(ConfigIndex idx)
        {
            ConfigIndex systemIdx = idx & SYSTEM_PART;
            switch (systemIdx)
            {
                case BUS_CONFIG:
                    return TransportSystemDefinition.BUS;
                case TRAIN_CONFIG:
                    return TransportSystemDefinition.TRAIN;
                case TRAM_CONFIG:
                    return TransportSystemDefinition.TRAM;
                case SHIP_CONFIG:
                    return TransportSystemDefinition.SHIP;
                case PLANE_CONFIG:
                    return TransportSystemDefinition.PLANE;
                case METRO_CONFIG:
                    return TransportSystemDefinition.METRO;
                case MONORAIL_CONFIG:
                    return TransportSystemDefinition.MONORAIL;
                case BLIMP_CONFIG:
                    return TransportSystemDefinition.BLIMP;
                case FERRY_CONFIG:
                    return TransportSystemDefinition.FERRY;
                case TOUR_BUS_CONFIG:
                    return TransportSystemDefinition.TOUR_BUS;
                case TOUR_PED_CONFIG:
                    return TransportSystemDefinition.TOUR_PED;
                case TROLLEY_CONFIG:
                    return TransportSystemDefinition.TROLLEY;
                case HELICOPTER_CONFIG:
                    return TransportSystemDefinition.HELICOPTER;
                default:
                    return default;
            }
        }


        public static readonly ConfigIndex[] configurableTicketTransportCategories = {
            TRAIN_CONFIG      ,
            TRAM_CONFIG       ,
            METRO_CONFIG      ,
            BUS_CONFIG        ,
            PLANE_CONFIG      ,
            SHIP_CONFIG       ,
            MONORAIL_CONFIG   ,
            FERRY_CONFIG      ,
            BLIMP_CONFIG      ,
            CABLE_CAR_CONFIG  ,
            TOUR_BUS_CONFIG   ,
            TROLLEY_CONFIG       ,
            HELICOPTER_CONFIG       ,
       };

        public static readonly ConfigIndex[] configurableAutoNameTransportCategories = {
           PLANE_CONFIG,
           BLIMP_CONFIG,
           SHIP_CONFIG,
           FERRY_CONFIG,
           TRAIN_CONFIG,
           MONORAIL_CONFIG,
           TRAM_CONFIG,
           METRO_CONFIG,
           BUS_CONFIG,
           TOUR_BUS_CONFIG,
           TOUR_PED_CONFIG,
           TROLLEY_CONFIG,
           HELICOPTER_CONFIG,
       };
        public static readonly ConfigIndex[] configurableAutoNameCategories = {
           MONUMENT_SERVICE_CONFIG,
           BEAUTIFICATION_SERVICE_CONFIG,
           HEALTHCARE_SERVICE_CONFIG,
           POLICEDEPARTMENT_SERVICE_CONFIG,
           FIREDEPARTMENT_SERVICE_CONFIG,
           EDUCATION_SERVICE_CONFIG,
           DISASTER_SERVICE_CONFIG,
           GARBAGE_SERVICE_CONFIG,
           PLAYER_INDUSTRY_SERVICE_CONFIG,
           PLAYER_EDUCATION_SERVICE_CONFIG,
           VARSITY_SPORTS_SERVICE_CONFIG,
           MUSEUMS_SERVICE_CONFIG,
       };
        public static readonly ConfigIndex[] extraAutoNameCategories = {
           PARKAREA_NAME_CONFIG       ,
           CAMPUS_AREA_NAME_CONFIG       ,
           INDUSTRIAL_AREA_NAME_CONFIG       ,
           DISTRICT_NAME_CONFIG       ,
           ADDRESS_NAME_CONFIG
       };
        public static readonly ConfigIndex[] defaultTrueBoolProperties = {
            MONUMENT_USE_FOR_AUTO_NAMING_REF,
            BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF,
            TRAIN_USE_FOR_AUTO_NAMING_REF,
            METRO_USE_FOR_AUTO_NAMING_REF,
            BUS_USE_FOR_AUTO_NAMING_REF,
            PLANE_USE_FOR_AUTO_NAMING_REF,
            SHIP_USE_FOR_AUTO_NAMING_REF,
            PARKAREA_USE_FOR_AUTO_NAMING_REF,
            DISTRICT_USE_FOR_AUTO_NAMING_REF,
            ADD_LINE_NUMBER_IN_AUTONAME,
            TRAIN_SHOW_IN_LINEAR_MAP ,
            METRO_SHOW_IN_LINEAR_MAP ,
            BUS_SHOW_IN_LINEAR_MAP ,
            MONORAIL_SHOW_IN_LINEAR_MAP ,
            CABLE_CAR_SHOW_IN_LINEAR_MAP ,
            PLANE_SHOW_IN_LINEAR_MAP ,
            SHIP_SHOW_IN_LINEAR_MAP ,
            TOUR_PED_CONFIG_SHOW_IN_LINEAR_MAP ,
            TOUR_BUS_CONFIG_SHOW_IN_LINEAR_MAP ,
            HELICOPTER_CONFIG_SHOW_IN_LINEAR_MAP ,
            TROLLEY_CONFIG_SHOW_IN_LINEAR_MAP ,
       };
        public static readonly ConfigIndex[] namingOrder =
        {
           PLANE_CONFIG ,
           SHIP_CONFIG  ,
           TRAIN_CONFIG ,
           HELICOPTER_CONFIG  ,
           BLIMP_CONFIG  ,
           FERRY_CONFIG  ,
           MONORAIL_CONFIG ,
           METRO_CONFIG ,
           CABLE_CAR_CONFIG ,
           TRAM_CONFIG ,
           TROLLEY_CONFIG ,
           BUS_CONFIG   ,
           TAXI_CONFIG  ,
           NATURAL_SERVICE_CONFIG   ,
           VARSITY_SPORTS_SERVICE_CONFIG   ,
           MUSEUMS_SERVICE_CONFIG   ,
           PLAYER_EDUCATION_SERVICE_CONFIG   ,
           PLAYER_INDUSTRY_SERVICE_CONFIG  ,
           BEAUTIFICATION_SERVICE_CONFIG    ,
           MONUMENT_SERVICE_CONFIG  ,
           HEALTHCARE_SERVICE_CONFIG    ,
           EDUCATION_SERVICE_CONFIG ,
           DISASTER_SERVICE_CONFIG    ,
           POLICEDEPARTMENT_SERVICE_CONFIG  ,
           FIREDEPARTMENT_SERVICE_CONFIG    ,
           GARBAGE_SERVICE_CONFIG   ,
           WATER_SERVICE_CONFIG ,
           ELECTRICITY_SERVICE_CONFIG   ,
           ROAD_SERVICE_CONFIG  ,
           OFFICE_SERVICE_CONFIG    ,
           TOURISM_SERVICE_CONFIG   ,
           CITIZEN_SERVICE_CONFIG   ,
           INDUSTRIAL_SERVICE_CONFIG    ,
           COMMERCIAL_SERVICE_CONFIG    ,
           RESIDENTIAL_SERVICE_CONFIG   ,
       };

        public static TLMSpecialNamingClass ToSpecial(ConfigIndex idx)
        {
            if ((idx & AUTO_NAMING_REF_TEXT) != 0 || (idx & USE_FOR_AUTO_NAMING_REF) != 0)
            {
                switch (idx & DESC_DATA)
                {
                    case CAMPUS_AREA_NAME_CONFIG: return TLMSpecialNamingClass.Campus;
                    case INDUSTRIAL_AREA_NAME_CONFIG: return TLMSpecialNamingClass.Industrial;
                    case PARKAREA_NAME_CONFIG: return TLMSpecialNamingClass.ParkArea;
                    case DISTRICT_NAME_CONFIG: return TLMSpecialNamingClass.District;
                    case ADDRESS_NAME_CONFIG: return TLMSpecialNamingClass.Address;
                }
            }
            return default;
        }

        public static ItemClass.Service ToService(ConfigIndex idx)
        {
            if ((idx & AUTO_NAMING_REF_TEXT) != 0 || (idx & USE_FOR_AUTO_NAMING_REF) != 0)
            {
                switch (idx & DESC_DATA)
                {
                    case RESIDENTIAL_SERVICE_CONFIG: return ItemClass.Service.Residential;
                    case COMMERCIAL_SERVICE_CONFIG: return ItemClass.Service.Commercial;
                    case INDUSTRIAL_SERVICE_CONFIG: return ItemClass.Service.Industrial;
                    case NATURAL_SERVICE_CONFIG: return ItemClass.Service.Natural;
                    case CITIZEN_SERVICE_CONFIG: return ItemClass.Service.Citizen;
                    case TOURISM_SERVICE_CONFIG: return ItemClass.Service.Tourism;
                    case OFFICE_SERVICE_CONFIG: return ItemClass.Service.Office;
                    case ROAD_SERVICE_CONFIG: return ItemClass.Service.Road;
                    case ELECTRICITY_SERVICE_CONFIG: return ItemClass.Service.Electricity;
                    case WATER_SERVICE_CONFIG: return ItemClass.Service.Water;
                    case BEAUTIFICATION_SERVICE_CONFIG: return ItemClass.Service.Beautification;
                    case GARBAGE_SERVICE_CONFIG: return ItemClass.Service.Garbage;
                    case HEALTHCARE_SERVICE_CONFIG: return ItemClass.Service.HealthCare;
                    case POLICEDEPARTMENT_SERVICE_CONFIG: return ItemClass.Service.PoliceDepartment;
                    case EDUCATION_SERVICE_CONFIG: return ItemClass.Service.Education;
                    case MONUMENT_SERVICE_CONFIG: return ItemClass.Service.Monument;
                    case FIREDEPARTMENT_SERVICE_CONFIG: return ItemClass.Service.FireDepartment;
                    case PUBLICTRANSPORT_SERVICE_CONFIG: return ItemClass.Service.PublicTransport;
                    case DISASTER_SERVICE_CONFIG: return ItemClass.Service.Disaster;
                    case PLAYER_INDUSTRY_SERVICE_CONFIG: return ItemClass.Service.PlayerIndustry;
                    case PLAYER_EDUCATION_SERVICE_CONFIG: return ItemClass.Service.PlayerEducation;
                    case MUSEUMS_SERVICE_CONFIG: return ItemClass.Service.Museums;
                    case VARSITY_SPORTS_SERVICE_CONFIG: return ItemClass.Service.VarsitySports;

                }
            }
            return default;
        }


        public enum ConfigIndex
        {
            NIL = -1,
            ADC_DESC_PART = 0x7F000000,
            SYSTEM_PART = 0xFF0000,
            UNDEF_PART = 0xF000,
            TYPE_PART = TLMConfigWarehouse.TYPE_PART,
            DESC_DATA = 0xFF,

            GLOBAL_CONFIG = 0x1000000,
            USE_FOR_AUTO_NAMING_REF = 0x2000000 | TYPE_BOOL,
            AUTO_NAMING_REF_TEXT = 0x3000000 | TYPE_STRING,

            TYPE_STRING = TLMConfigWarehouse.TYPE_STRING,
            TYPE_INT = TLMConfigWarehouse.TYPE_INT,
            TYPE_BOOL = TLMConfigWarehouse.TYPE_BOOL,
            TYPE_DICTIONARY = TLMConfigWarehouse.TYPE_DICTIONARY,

            AUTO_COLOR_ENABLED = GLOBAL_CONFIG | 0x2 | TYPE_BOOL,
            CIRCULAR_IN_SINGLE_DISTRICT_LINE = GLOBAL_CONFIG | 0x3 | TYPE_BOOL,
            AUTO_NAME_ENABLED = GLOBAL_CONFIG | 0x4 | TYPE_BOOL,
            ADD_LINE_NUMBER_IN_AUTONAME = GLOBAL_CONFIG | 0x9 | TYPE_BOOL,
            MAX_VEHICLES_SPECIFIC_CONFIG = GLOBAL_CONFIG | 0x23 | TYPE_INT,


            TRAIN_CONFIG = TransportInfo.TransportType.Train << 16,
            TRAM_CONFIG = TransportInfo.TransportType.Tram << 16,
            METRO_CONFIG = TransportInfo.TransportType.Metro << 16,
            BUS_CONFIG = (TransportInfo.TransportType.Bus << 16) | 0x800000,
            EVAC_BUS_CONFIG = TransportInfo.TransportType.EvacuationBus << 16,
            PLANE_CONFIG = TransportInfo.TransportType.Airplane << 16,
            SHIP_CONFIG = TransportInfo.TransportType.Ship << 16,
            MONORAIL_CONFIG = TransportInfo.TransportType.Monorail << 16,
            TAXI_CONFIG = TransportInfo.TransportType.Taxi << 16,
            CABLE_CAR_CONFIG = TransportInfo.TransportType.CableCar << 16,
            TOUR_PED_CONFIG = TransportInfo.TransportType.Pedestrian << 16,
            TOUR_BUS_CONFIG = TransportInfo.TransportType.TouristBus << 16,
            BALLOON_CONFIG = TransportInfo.TransportType.HotAirBalloon << 16,
            BLIMP_CONFIG = (TransportInfo.TransportType.Airplane << 16) | 0x800000,
            FERRY_CONFIG = (TransportInfo.TransportType.Ship << 16) | 0x800000,
            TROLLEY_CONFIG = TransportInfo.TransportType.Trolleybus << 16,
            HELICOPTER_CONFIG = TransportInfo.TransportType.Helicopter << 16,



            RESIDENTIAL_SERVICE_CONFIG = ItemClass.Service.Residential,
            COMMERCIAL_SERVICE_CONFIG = ItemClass.Service.Commercial,
            INDUSTRIAL_SERVICE_CONFIG = ItemClass.Service.Industrial,
            NATURAL_SERVICE_CONFIG = ItemClass.Service.Natural,
            //UNUSED2_SERVICE_CONFIG = ItemClass.Service.Unused2,
            CITIZEN_SERVICE_CONFIG = ItemClass.Service.Citizen,
            TOURISM_SERVICE_CONFIG = ItemClass.Service.Tourism,
            OFFICE_SERVICE_CONFIG = ItemClass.Service.Office,
            ROAD_SERVICE_CONFIG = ItemClass.Service.Road,
            ELECTRICITY_SERVICE_CONFIG = ItemClass.Service.Electricity,
            WATER_SERVICE_CONFIG = ItemClass.Service.Water,
            BEAUTIFICATION_SERVICE_CONFIG = ItemClass.Service.Beautification,
            GARBAGE_SERVICE_CONFIG = ItemClass.Service.Garbage,
            HEALTHCARE_SERVICE_CONFIG = ItemClass.Service.HealthCare,
            POLICEDEPARTMENT_SERVICE_CONFIG = ItemClass.Service.PoliceDepartment,
            EDUCATION_SERVICE_CONFIG = ItemClass.Service.Education,
            MONUMENT_SERVICE_CONFIG = ItemClass.Service.Monument,
            FIREDEPARTMENT_SERVICE_CONFIG = ItemClass.Service.FireDepartment,
            PUBLICTRANSPORT_SERVICE_CONFIG = ItemClass.Service.PublicTransport,
            DISASTER_SERVICE_CONFIG = ItemClass.Service.Disaster,
            PLAYER_INDUSTRY_SERVICE_CONFIG = ItemClass.Service.PlayerIndustry,
            PLAYER_EDUCATION_SERVICE_CONFIG = ItemClass.Service.PlayerEducation,
            MUSEUMS_SERVICE_CONFIG = ItemClass.Service.Museums,
            VARSITY_SPORTS_SERVICE_CONFIG = ItemClass.Service.VarsitySports,
            CAMPUS_AREA_NAME_CONFIG = 0xfb,
            INDUSTRIAL_AREA_NAME_CONFIG = 0xfc,
            PARKAREA_NAME_CONFIG = 0xfd,
            DISTRICT_NAME_CONFIG = 0xfe,
            ADDRESS_NAME_CONFIG = 0xff,



            PREFIX = 0x1 | TYPE_INT,
            SEPARATOR = 0x2 | TYPE_INT,
            SUFFIX = 0x3 | TYPE_INT,
            LEADING_ZEROS = 0x4 | TYPE_BOOL,
            PALETTE_MAIN = 0x5 | TYPE_STRING,
            // PALETTE_SUBLINE = 0x6 | TYPE_STRING,
            PALETTE_RANDOM_ON_OVERFLOW = 0x7 | TYPE_BOOL,
            PALETTE_PREFIX_BASED = 0x8 | TYPE_BOOL,
            SHOW_IN_LINEAR_MAP = 0x9 | TYPE_BOOL,
            INVERT_PREFIX_SUFFIX = 0xA | TYPE_BOOL,
            DEFAULT_COST_PER_PASSENGER_CAPACITY = 0xB | TYPE_INT,
            NON_PREFIX = 0xC | TYPE_INT,
            PREFIX_INCREMENT = 0xD | TYPE_BOOL,
            DEFAULT_TICKET_PRICE = 0xE | TYPE_INT,
            TRANSPORT_ICON_TLM = 0Xf | TYPE_STRING,
            VEHICLE_NUMBER_FORMAT_LOCAL = 0X10 | TYPE_STRING,
            VEHICLE_NUMBER_FORMAT_FOREIGN = 0X11 | TYPE_STRING,

            TRAIN_PREFIX = TRAIN_CONFIG | PREFIX,
            TRAM_PREFIX = TRAM_CONFIG | PREFIX,
            METRO_PREFIX = METRO_CONFIG | PREFIX,
            BUS_PREFIX = BUS_CONFIG | PREFIX,
            SHIP_PREFIX = SHIP_CONFIG | PREFIX,
            PLANE_PREFIX = PLANE_CONFIG | PREFIX,
            MONORAIL_PREFIX = MONORAIL_CONFIG | PREFIX,
            FERRY_PREFIX = FERRY_CONFIG | PREFIX,
            BLIMP_PREFIX = BLIMP_CONFIG | PREFIX,
            TOUR_PED_CONFIG_PREFIX = TOUR_PED_CONFIG | PREFIX,
            TOUR_BUS_CONFIG_PREFIX = TOUR_BUS_CONFIG | PREFIX,
            TROLLEY_CONFIG_PREFIX = TROLLEY_CONFIG | PREFIX,
            HELICOPTER_CONFIG_PREFIX = HELICOPTER_CONFIG | PREFIX,

            TRAIN_SEPARATOR = TRAIN_CONFIG | SEPARATOR,
            TRAM_SEPARATOR = TRAM_CONFIG | SEPARATOR,
            METRO_SEPARATOR = METRO_CONFIG | SEPARATOR,
            BUS_SEPARATOR = BUS_CONFIG | SEPARATOR,
            SHIP_SEPARATOR = SHIP_CONFIG | SEPARATOR,
            PLANE_SEPARATOR = PLANE_CONFIG | SEPARATOR,
            MONORAIL_SEPARATOR = MONORAIL_CONFIG | SEPARATOR,
            FERRY_SEPARATOR = FERRY_CONFIG | SEPARATOR,
            BLIMP_SEPARATOR = BLIMP_CONFIG | SEPARATOR,
            TOUR_PED_CONFIG_SEPARATOR = TOUR_PED_CONFIG | SEPARATOR,
            TOUR_BUS_CONFIG_SEPARATOR = TOUR_BUS_CONFIG | SEPARATOR,
            TROLLEY_CONFIG_SEPARATOR = TROLLEY_CONFIG | SEPARATOR,
            HELICOPTER_CONFIG_SEPARATOR = HELICOPTER_CONFIG | SEPARATOR,

            TRAIN_SUFFIX = TRAIN_CONFIG | SUFFIX,
            TRAM_SUFFIX = TRAM_CONFIG | SUFFIX,
            METRO_SUFFIX = METRO_CONFIG | SUFFIX,
            BUS_SUFFIX = BUS_CONFIG | SUFFIX,
            SHIP_SUFFIX = SHIP_CONFIG | SUFFIX,
            PLANE_SUFFIX = PLANE_CONFIG | SUFFIX,
            MONORAIL_SUFFIX = MONORAIL_CONFIG | SUFFIX,
            FERRY_SUFFIX = FERRY_CONFIG | SUFFIX,
            BLIMP_SUFFIX = BLIMP_CONFIG | SUFFIX,
            TOUR_PED_CONFIG_SUFFIX = TOUR_PED_CONFIG | SUFFIX,
            TOUR_BUS_CONFIG_SUFFIX = TOUR_BUS_CONFIG | SUFFIX,
            TROLLEY_CONFIG_SUFFIX = TROLLEY_CONFIG | SUFFIX,
            HELICOPTER_CONFIG_SUFFIX = HELICOPTER_CONFIG | SUFFIX,

            TRAIN_NON_PREFIX = TRAIN_CONFIG | NON_PREFIX,
            TRAM_NON_PREFIX = TRAM_CONFIG | NON_PREFIX,
            METRO_NON_PREFIX = METRO_CONFIG | NON_PREFIX,
            BUS_NON_PREFIX = BUS_CONFIG | NON_PREFIX,
            SHIP_NON_PREFIX = SHIP_CONFIG | NON_PREFIX,
            PLANE_NON_PREFIX = PLANE_CONFIG | NON_PREFIX,
            MONORAIL_NON_PREFIX = MONORAIL_CONFIG | NON_PREFIX,
            FERRY_NON_PREFIX = FERRY_CONFIG | NON_PREFIX,
            BLIMP_NON_PREFIX = BLIMP_CONFIG | NON_PREFIX,
            TOUR_PED_CONFIG_NON_PREFIX = TOUR_PED_CONFIG | NON_PREFIX,
            TOUR_BUS_CONFIG_NON_PREFIX = TOUR_BUS_CONFIG | NON_PREFIX,
            TROLLEY_CONFIG_NON_PREFIX = TROLLEY_CONFIG | NON_PREFIX,
            HELICOPTER_CONFIG_NON_PREFIX = HELICOPTER_CONFIG | NON_PREFIX,

            TRAIN_LEADING_ZEROS = TRAIN_CONFIG | LEADING_ZEROS,
            TRAM_LEADING_ZEROS = TRAM_CONFIG | LEADING_ZEROS,
            METRO_LEADING_ZEROS = METRO_CONFIG | LEADING_ZEROS,
            BUS_LEADING_ZEROS = BUS_CONFIG | LEADING_ZEROS,
            SHIP_LEADING_ZEROS = SHIP_CONFIG | LEADING_ZEROS,
            PLANE_LEADING_ZEROS = PLANE_CONFIG | LEADING_ZEROS,
            MONORAIL_LEADING_ZEROS = MONORAIL_CONFIG | LEADING_ZEROS,
            FERRY_LEADING_ZEROS = FERRY_CONFIG | LEADING_ZEROS,
            BLIMP_LEADING_ZEROS = BLIMP_CONFIG | LEADING_ZEROS,
            TOUR_PED_CONFIG_LEADING_ZEROS = TOUR_PED_CONFIG | LEADING_ZEROS,
            TOUR_BUS_CONFIG_LEADING_ZEROS = TOUR_BUS_CONFIG | LEADING_ZEROS,
            TROLLEY_CONFIG_LEADING_ZEROS = TROLLEY_CONFIG | LEADING_ZEROS,
            HELICOPTER_CONFIG_LEADING_ZEROS = HELICOPTER_CONFIG | LEADING_ZEROS,

            TRAIN_INVERT_PREFIX_SUFFIX = TRAIN_CONFIG | INVERT_PREFIX_SUFFIX,
            TRAM_INVERT_PREFIX_SUFFIX = TRAM_CONFIG | INVERT_PREFIX_SUFFIX,
            METRO_INVERT_PREFIX_SUFFIX = METRO_CONFIG | INVERT_PREFIX_SUFFIX,
            BUS_INVERT_PREFIX_SUFFIX = BUS_CONFIG | INVERT_PREFIX_SUFFIX,
            SHIP_INVERT_PREFIX_SUFFIX = SHIP_CONFIG | INVERT_PREFIX_SUFFIX,
            PLANE_INVERT_PREFIX_SUFFIX = PLANE_CONFIG | INVERT_PREFIX_SUFFIX,
            MONORAIL_INVERT_PREFIX_SUFFIX = MONORAIL_CONFIG | INVERT_PREFIX_SUFFIX,
            FERRY_INVERT_PREFIX_SUFFIX = FERRY_CONFIG | INVERT_PREFIX_SUFFIX,
            BLIMP_INVERT_PREFIX_SUFFIX = BLIMP_CONFIG | INVERT_PREFIX_SUFFIX,
            TOUR_PED_CONFIG_INVERT_PREFIX_SUFFIX = TOUR_PED_CONFIG | INVERT_PREFIX_SUFFIX,
            TOUR_BUS_CONFIG_INVERT_PREFIX_SUFFIX = TOUR_BUS_CONFIG | INVERT_PREFIX_SUFFIX,
            TROLLEY_CONFIG_INVERT_PREFIX_SUFFIX = TROLLEY_CONFIG | INVERT_PREFIX_SUFFIX,
            HELICOPTER_CONFIG_INVERT_PREFIX_SUFFIX = HELICOPTER_CONFIG | INVERT_PREFIX_SUFFIX,

            TRAIN_PALETTE_MAIN = TRAIN_CONFIG | PALETTE_MAIN,
            TRAM_PALETTE_MAIN = TRAM_CONFIG | PALETTE_MAIN,
            METRO_PALETTE_MAIN = METRO_CONFIG | PALETTE_MAIN,
            BUS_PALETTE_MAIN = BUS_CONFIG | PALETTE_MAIN,
            SHIP_PALETTE_MAIN = SHIP_CONFIG | PALETTE_MAIN,
            PLANE_PALETTE_MAIN = PLANE_CONFIG | PALETTE_MAIN,
            MONORAIL_PALETTE_MAIN = MONORAIL_CONFIG | PALETTE_MAIN,
            FERRY_PALETTE_MAIN = FERRY_CONFIG | PALETTE_MAIN,
            BLIMP_PALETTE_MAIN = BLIMP_CONFIG | PALETTE_MAIN,
            TOUR_PED_CONFIG_PALETTE_MAIN = TOUR_PED_CONFIG | PALETTE_MAIN,
            TOUR_BUS_CONFIG_PALETTE_MAIN = TOUR_BUS_CONFIG | PALETTE_MAIN,
            TROLLEY_CONFIG_PALETTE_MAIN = TROLLEY_CONFIG | PALETTE_MAIN,
            HELICOPTER_CONFIG_PALETTE_MAIN = HELICOPTER_CONFIG | PALETTE_MAIN,

            //TRAIN_PALETTE_SUBLINE = TRAIN_CONFIG | PALETTE_SUBLINE,
            //TRAM_PALETTE_SUBLINE = TRAM_CONFIG | PALETTE_SUBLINE,
            //METRO_PALETTE_SUBLINE = METRO_CONFIG | PALETTE_SUBLINE,
            //BUS_PALETTE_SUBLINE = BUS_CONFIG | PALETTE_SUBLINE,
            //SHIP_PALETTE_SUBLINE = SHIP_CONFIG | PALETTE_SUBLINE,
            //PLANE_PALETTE_SUBLINE = PLANE_CONFIG | PALETTE_SUBLINE,
            //MONORAIL_PALETTE_SUBLINE = MONORAIL_CONFIG | PALETTE_SUBLINE,
            //FERRY_PALETTE_SUBLINE = FERRY_CONFIG | PALETTE_SUBLINE,
            //BLIMP_PALETTE_SUBLINE = BLIMP_CONFIG | PALETTE_SUBLINE,
            //TOUR_PED_CONFIG_PALETTE_SUBLINE = TOUR_PED_CONFIG | PALETTE_SUBLINE,
            //TOUR_BUS_CONFIG_PALETTE_SUBLINE = TOUR_BUS_CONFIG | PALETTE_SUBLINE,

            TRAIN_PALETTE_RANDOM_ON_OVERFLOW = TRAIN_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TRAM_PALETTE_RANDOM_ON_OVERFLOW = TRAM_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            METRO_PALETTE_RANDOM_ON_OVERFLOW = METRO_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            BUS_PALETTE_RANDOM_ON_OVERFLOW = BUS_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            SHIP_PALETTE_RANDOM_ON_OVERFLOW = SHIP_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            PLANE_PALETTE_RANDOM_ON_OVERFLOW = PLANE_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            MONORAIL_PALETTE_RANDOM_ON_OVERFLOW = MONORAIL_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            FERRY_PALETTE_RANDOM_ON_OVERFLOW = FERRY_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            BLIMP_PALETTE_RANDOM_ON_OVERFLOW = BLIMP_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TOUR_PED_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = TOUR_PED_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TOUR_BUS_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = TOUR_BUS_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TROLLEY_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = TROLLEY_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            HELICOPTER_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = HELICOPTER_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,

            TRAIN_PALETTE_PREFIX_BASED = TRAIN_CONFIG | PALETTE_PREFIX_BASED,
            TRAM_PALETTE_PREFIX_BASED = TRAM_CONFIG | PALETTE_PREFIX_BASED,
            METRO_PALETTE_PREFIX_BASED = METRO_CONFIG | PALETTE_PREFIX_BASED,
            BUS_PALETTE_PREFIX_BASED = BUS_CONFIG | PALETTE_PREFIX_BASED,
            SHIP_PALETTE_PREFIX_BASED = SHIP_CONFIG | PALETTE_PREFIX_BASED,
            PLANE_PALETTE_PREFIX_BASED = PLANE_CONFIG | PALETTE_PREFIX_BASED,
            MONORAIL_PALETTE_PREFIX_BASED = MONORAIL_CONFIG | PALETTE_PREFIX_BASED,
            FERRY_PALETTE_PREFIX_BASED = FERRY_CONFIG | PALETTE_PREFIX_BASED,
            BLIMP_PALETTE_PREFIX_BASED = BLIMP_CONFIG | PALETTE_PREFIX_BASED,
            TOUR_PED_CONFIG_PALETTE_PREFIX_BASED = TOUR_PED_CONFIG | PALETTE_PREFIX_BASED,
            TOUR_BUS_CONFIG_PALETTE_PREFIX_BASED = TOUR_BUS_CONFIG | PALETTE_PREFIX_BASED,
            TROLLEY_CONFIG_PALETTE_PREFIX_BASED = TROLLEY_CONFIG | PALETTE_PREFIX_BASED,
            HELICOPTER_CONFIG_PALETTE_PREFIX_BASED = HELICOPTER_CONFIG | PALETTE_PREFIX_BASED,

            TRAIN_SHOW_IN_LINEAR_MAP = TRAIN_CONFIG | SHOW_IN_LINEAR_MAP,
            TRAM_SHOW_IN_LINEAR_MAP = TRAM_CONFIG | SHOW_IN_LINEAR_MAP,
            METRO_SHOW_IN_LINEAR_MAP = METRO_CONFIG | SHOW_IN_LINEAR_MAP,
            BUS_SHOW_IN_LINEAR_MAP = BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            PLANE_SHOW_IN_LINEAR_MAP = PLANE_CONFIG | SHOW_IN_LINEAR_MAP,
            SHIP_SHOW_IN_LINEAR_MAP = SHIP_CONFIG | SHOW_IN_LINEAR_MAP,
            MONORAIL_SHOW_IN_LINEAR_MAP = MONORAIL_CONFIG | SHOW_IN_LINEAR_MAP,
            FERRY_SHOW_IN_LINEAR_MAP = FERRY_CONFIG | SHOW_IN_LINEAR_MAP,
            BLIMP_SHOW_IN_LINEAR_MAP = BLIMP_CONFIG | SHOW_IN_LINEAR_MAP,
            CABLE_CAR_SHOW_IN_LINEAR_MAP = CABLE_CAR_CONFIG | SHOW_IN_LINEAR_MAP,
            TAXI_SHOW_IN_LINEAR_MAP = TAXI_CONFIG | SHOW_IN_LINEAR_MAP,
            EVAC_BUS_SHOW_IN_LINEAR_MAP = EVAC_BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            TOUR_PED_CONFIG_SHOW_IN_LINEAR_MAP = TOUR_PED_CONFIG | SHOW_IN_LINEAR_MAP,
            TOUR_BUS_CONFIG_SHOW_IN_LINEAR_MAP = TOUR_BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            TROLLEY_CONFIG_SHOW_IN_LINEAR_MAP = TROLLEY_CONFIG | SHOW_IN_LINEAR_MAP,
            HELICOPTER_CONFIG_SHOW_IN_LINEAR_MAP = HELICOPTER_CONFIG | SHOW_IN_LINEAR_MAP,


            TRAIN_DEFAULT_TICKET_PRICE = TRAIN_CONFIG | DEFAULT_TICKET_PRICE,
            TRAM_DEFAULT_TICKET_PRICE = TRAM_CONFIG | DEFAULT_TICKET_PRICE,
            METRO_DEFAULT_TICKET_PRICE = METRO_CONFIG | DEFAULT_TICKET_PRICE,
            BUS_DEFAULT_TICKET_PRICE = BUS_CONFIG | DEFAULT_TICKET_PRICE,
            PLANE_DEFAULT_TICKET_PRICE = PLANE_CONFIG | DEFAULT_TICKET_PRICE,
            SHIP_DEFAULT_TICKET_PRICE = SHIP_CONFIG | DEFAULT_TICKET_PRICE,
            MONORAIL_DEFAULT_TICKET_PRICE = MONORAIL_CONFIG | DEFAULT_TICKET_PRICE,
            FERRY_DEFAULT_TICKET_PRICE = FERRY_CONFIG | DEFAULT_TICKET_PRICE,
            BLIMP_DEFAULT_TICKET_PRICE = BLIMP_CONFIG | DEFAULT_TICKET_PRICE,
            CABLE_CAR_DEFAULT_TICKET_PRICE = CABLE_CAR_CONFIG | DEFAULT_TICKET_PRICE,
            TAXI_DEFAULT_TICKET_PRICE = TAXI_CONFIG | DEFAULT_TICKET_PRICE,
            TOUR_BUS_CONFIG_DEFAULT_TICKET_PRICE = TOUR_BUS_CONFIG | DEFAULT_TICKET_PRICE,
            TROLLEY_CONFIG_DEFAULT_TICKET_PRICE = TROLLEY_CONFIG | DEFAULT_TICKET_PRICE,
            HELICOPTER_CONFIG_DEFAULT_TICKET_PRICE = HELICOPTER_CONFIG | DEFAULT_TICKET_PRICE,

            TRAIN_DEFAULT_TRANSPORT_ICON_TLM = TRAIN_CONFIG | TRANSPORT_ICON_TLM,
            TRAM_DEFAULT_TRANSPORT_ICON_TLM = TRAM_CONFIG | TRANSPORT_ICON_TLM,
            METRO_DEFAULT_TRANSPORT_ICON_TLM = METRO_CONFIG | TRANSPORT_ICON_TLM,
            BUS_DEFAULT_TRANSPORT_ICON_TLM = BUS_CONFIG | TRANSPORT_ICON_TLM,
            PLANE_DEFAULT_TRANSPORT_ICON_TLM = PLANE_CONFIG | TRANSPORT_ICON_TLM,
            SHIP_DEFAULT_TRANSPORT_ICON_TLM = SHIP_CONFIG | TRANSPORT_ICON_TLM,
            MONORAIL_DEFAULT_TRANSPORT_ICON_TLM = MONORAIL_CONFIG | TRANSPORT_ICON_TLM,
            FERRY_DEFAULT_TRANSPORT_ICON_TLM = FERRY_CONFIG | TRANSPORT_ICON_TLM,
            BLIMP_DEFAULT_TRANSPORT_ICON_TLM = BLIMP_CONFIG | TRANSPORT_ICON_TLM,
            CABLE_CAR_DEFAULT_TRANSPORT_ICON_TLM = CABLE_CAR_CONFIG | TRANSPORT_ICON_TLM,
            TAXI_DEFAULT_TRANSPORT_ICON_TLM = TAXI_CONFIG | TRANSPORT_ICON_TLM,
            TOUR_BUS_CONFIG_DEFAULT_TRANSPORT_ICON_TLM = TOUR_BUS_CONFIG | TRANSPORT_ICON_TLM,
            TROLLEY_CONFIG_TRANSPORT_ICON_TLM = TROLLEY_CONFIG | TRANSPORT_ICON_TLM,
            HELICOPTER_CONFIG_TRANSPORT_ICON_TLM = HELICOPTER_CONFIG | TRANSPORT_ICON_TLM,

            TRAIN_DEFAULT_COST_PER_PASSENGER_CAPACITY = TRAIN_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            TRAM_DEFAULT_COST_PER_PASSENGER_CAPACITY = TRAM_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            METRO_DEFAULT_COST_PER_PASSENGER_CAPACITY = METRO_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            BUS_DEFAULT_COST_PER_PASSENGER_CAPACITY = BUS_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            SHIP_DEFAULT_COST_PER_PASSENGER_CAPACITY = SHIP_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            PLANE_DEFAULT_COST_PER_PASSENGER_CAPACITY = PLANE_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            MONORAIL_DEFAULT_COST_PER_PASSENGER_CAPACITY = MONORAIL_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            FERRY_DEFAULT_COST_PER_PASSENGER_CAPACITY = FERRY_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            BLIMP_DEFAULT_COST_PER_PASSENGER_CAPACITY = BLIMP_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //  TOUR_PED_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = TOUR_PED_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            TOUR_BUS_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = TOUR_BUS_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            TROLLEY_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = TROLLEY_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            HELICOPTER_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = HELICOPTER_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,

            TRAIN_PREFIX_INCREMENT = TRAIN_CONFIG | PREFIX_INCREMENT,
            METRO_PREFIX_INCREMENT = METRO_CONFIG | PREFIX_INCREMENT,
            BUS_PREFIX_INCREMENT = BUS_CONFIG | PREFIX_INCREMENT,
            TRAM_PREFIX_INCREMENT = TRAM_CONFIG | PREFIX_INCREMENT,
            PLANE_PREFIX_INCREMENT = PLANE_CONFIG | PREFIX_INCREMENT,
            SHIP_PREFIX_INCREMENT = SHIP_CONFIG | PREFIX_INCREMENT,
            MONORAIL_PREFIX_INCREMENT = MONORAIL_CONFIG | PREFIX_INCREMENT,
            FERRY_PREFIX_INCREMENT = FERRY_CONFIG | PREFIX_INCREMENT,
            BLIMP_PREFIX_INCREMENT = BLIMP_CONFIG | PREFIX_INCREMENT,
            TOUR_PED_CONFIG_PREFIX_INCREMENT = TOUR_PED_CONFIG | PREFIX_INCREMENT,
            TOUR_BUS_CONFIG_PREFIX_INCREMENT = TOUR_BUS_CONFIG | PREFIX_INCREMENT,
            TROLLEY_CONFIG_PREFIX_INCREMENT = TROLLEY_CONFIG | PREFIX_INCREMENT,
            HELICOPTER_CONFIG_PREFIX_INCREMENT = HELICOPTER_CONFIG | PREFIX_INCREMENT,

            RESIDENTIAL_USE_FOR_AUTO_NAMING_REF = RESIDENTIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            COMMERCIAL_USE_FOR_AUTO_NAMING_REF = COMMERCIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            INDUSTRIAL_USE_FOR_AUTO_NAMING_REF = INDUSTRIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            NATURAL_USE_FOR_AUTO_NAMING_REF = NATURAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            //UNUSED2_USE_FOR_AUTO_NAMING_REF = UNUSED2_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            CITIZEN_USE_FOR_AUTO_NAMING_REF = CITIZEN_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            TOURISM_USE_FOR_AUTO_NAMING_REF = TOURISM_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            OFFICE_USE_FOR_AUTO_NAMING_REF = OFFICE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ROAD_USE_FOR_AUTO_NAMING_REF = ROAD_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ELECTRICITY_USE_FOR_AUTO_NAMING_REF = ELECTRICITY_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            WATER_USE_FOR_AUTO_NAMING_REF = WATER_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF = BEAUTIFICATION_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            GARBAGE_USE_FOR_AUTO_NAMING_REF = GARBAGE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            HEALTHCARE_USE_FOR_AUTO_NAMING_REF = HEALTHCARE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            POLICEDEPARTMENT_USE_FOR_AUTO_NAMING_REF = POLICEDEPARTMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            EDUCATION_USE_FOR_AUTO_NAMING_REF = EDUCATION_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            MONUMENT_USE_FOR_AUTO_NAMING_REF = MONUMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            FIREDEPARTMENT_USE_FOR_AUTO_NAMING_REF = FIREDEPARTMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF = PUBLICTRANSPORT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            DISASTER_USE_FOR_AUTO_NAMING_REF = DISASTER_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            PLAYER_INDUSTRY_USE_FOR_AUTO_NAMING_REF = PLAYER_INDUSTRY_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            PLAYER_EDUCATION_USE_FOR_AUTO_NAMING_REF = PLAYER_EDUCATION_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            MUSEUMS_USE_FOR_AUTO_NAMING_REF = MUSEUMS_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            VARSITY_SPORTS_USE_FOR_AUTO_NAMING_REF = VARSITY_SPORTS_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,


            CAMPUS_AREA_USE_FOR_AUTO_NAMING_REF = CAMPUS_AREA_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            INDUSTRIAL_AREA_USE_FOR_AUTO_NAMING_REF = INDUSTRIAL_AREA_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            PARKAREA_USE_FOR_AUTO_NAMING_REF = PARKAREA_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            DISTRICT_USE_FOR_AUTO_NAMING_REF = DISTRICT_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ADDRESS_USE_FOR_AUTO_NAMING_REF = ADDRESS_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,


            TRAIN_USE_FOR_AUTO_NAMING_REF = TRAIN_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TRAM_USE_FOR_AUTO_NAMING_REF = TRAM_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            METRO_USE_FOR_AUTO_NAMING_REF = METRO_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BUS_USE_FOR_AUTO_NAMING_REF = BUS_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            PLANE_USE_FOR_AUTO_NAMING_REF = PLANE_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TAXI_USE_FOR_AUTO_NAMING_REF = TAXI_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            SHIP_USE_FOR_AUTO_NAMING_REF = SHIP_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            CABLE_CAR_USE_FOR_AUTO_NAMING_REF = CABLE_CAR_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            MONORAIL_USE_FOR_AUTO_NAMING_REF = MONORAIL_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            FERRY_USE_FOR_AUTO_NAMING_REF = FERRY_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BLIMP_USE_FOR_AUTO_NAMING_REF = BLIMP_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TOUR_PED_USE_FOR_AUTO_NAMING_REF = TOUR_PED_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TOUR_BUS_USE_FOR_AUTO_NAMING_REF = TOUR_BUS_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BALOON_USE_FOR_AUTO_NAMING_REF = BALLOON_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TROLLEY_CONFIG_USE_FOR_AUTO_NAMING_REF = TROLLEY_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            HELICOPTER_CONFIG_USE_FOR_AUTO_NAMING_REF = HELICOPTER_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,

            RESIDENTIAL_AUTO_NAMING_REF_TEXT = RESIDENTIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            COMMERCIAL_AUTO_NAMING_REF_TEXT = COMMERCIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            INDUSTRIAL_AUTO_NAMING_REF_TEXT = INDUSTRIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            UNUSED1_AUTO_NAMING_REF_TEXT = NATURAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            //UNUSED2_AUTO_NAMING_REF_TEXT = UNUSED2_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            CITIZEN_AUTO_NAMING_REF_TEXT = CITIZEN_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            TOURISM_AUTO_NAMING_REF_TEXT = TOURISM_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            OFFICE_AUTO_NAMING_REF_TEXT = OFFICE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            ROAD_AUTO_NAMING_REF_TEXT = ROAD_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            ELECTRICITY_AUTO_NAMING_REF_TEXT = ELECTRICITY_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            WATER_AUTO_NAMING_REF_TEXT = WATER_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            BEAUTIFICATION_AUTO_NAMING_REF_TEXT = BEAUTIFICATION_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            GARBAGE_AUTO_NAMING_REF_TEXT = GARBAGE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            HEALTHCARE_AUTO_NAMING_REF_TEXT = HEALTHCARE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            POLICEDEPARTMENT_AUTO_NAMING_REF_TEXT = POLICEDEPARTMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            EDUCATION_AUTO_NAMING_REF_TEXT = EDUCATION_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            MONUMENT_AUTO_NAMING_REF_TEXT = MONUMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            FIREDEPARTMENT_AUTO_NAMING_REF_TEXT = FIREDEPARTMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT = PUBLICTRANSPORT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            DISASTER_AUTO_NAMING_REF_TEXT = DISASTER_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            PLAYER_INDUSTRY_AUTO_NAMING_REF_TEXT = PLAYER_INDUSTRY_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            PLAYER_EDUCATION_AUTO_NAMING_REF_TEXT = PLAYER_EDUCATION_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            MUSEUMS_AUTO_NAMING_REF_TEXT = MUSEUMS_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            VARSITY_SPORTS_AUTO_NAMING_REF_TEXT = VARSITY_SPORTS_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,

            CAMPUS_AREA_NAMING_REF_TEXT = CAMPUS_AREA_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            INDUSTRIAL_AREA_NAMING_REF_TEXT = INDUSTRIAL_AREA_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            PARKAREA_NAMING_REF_TEXT = PARKAREA_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            DISTRICT_NAMING_REF_TEXT = DISTRICT_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            ADDRESS_NAMING_REF_TEXT = ADDRESS_NAME_CONFIG | AUTO_NAMING_REF_TEXT,

            TRAIN_AUTO_NAMING_REF_TEXT = TRAIN_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            METRO_AUTO_NAMING_REF_TEXT = METRO_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BUS_AUTO_NAMING_REF_TEXT = BUS_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TRAM_AUTO_NAMING_REF_TEXT = TRAM_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            PLANE_AUTO_NAMING_REF_TEXT = PLANE_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TAXI_AUTO_NAMING_REF_TEXT = TAXI_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            SHIP_AUTO_NAMING_REF_TEXT = SHIP_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            CABLE_CAR_AUTO_NAMING_REF_TEXT = CABLE_CAR_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            MONORAIL_AUTO_NAMING_REF_TEXT = MONORAIL_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            FERRY_AUTO_NAMING_REF_TEXT = FERRY_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BLIMP_AUTO_NAMING_REF_TEXT = BLIMP_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TOUR_PED_AUTO_NAMING_REF_TEXT = TOUR_PED_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TOUR_BUS_AUTO_NAMING_REF_TEXT = TOUR_BUS_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BALOON_AUTO_NAMING_REF_TEXT = BALLOON_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TROLLEY_CONFIG_AUTO_NAMING_REF_TEXT = TROLLEY_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            HELICOPTER_CONFIG_AUTO_NAMING_REF_TEXT = HELICOPTER_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,



            TRAIN_VEHICLE_NUMBER_FORMAT_LOCAL = TRAIN_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            TRAM_VEHICLE_NUMBER_FORMAT_LOCAL = TRAM_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            METRO_VEHICLE_NUMBER_FORMAT_LOCAL = METRO_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            BUS_VEHICLE_NUMBER_FORMAT_LOCAL = BUS_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            PLANE_VEHICLE_NUMBER_FORMAT_LOCAL = PLANE_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            SHIP_VEHICLE_NUMBER_FORMAT_LOCAL = SHIP_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            MONORAIL_VEHICLE_NUMBER_FORMAT_LOCAL = MONORAIL_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            FERRY_VEHICLE_NUMBER_FORMAT_LOCAL = FERRY_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            BLIMP_VEHICLE_NUMBER_FORMAT_LOCAL = BLIMP_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            CABLE_CAR_VEHICLE_NUMBER_FORMAT_LOCAL = CABLE_CAR_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            TAXI_VEHICLE_NUMBER_FORMAT_LOCAL = TAXI_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            TOUR_BUS_CONFIG_VEHICLE_NUMBER_FORMAT_LOCAL = TOUR_BUS_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            TROLLEY_CONFIG_VEHICLE_NUMBER_FORMAT_LOCAL = TROLLEY_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,
            HELICOPTER_CONFIG_VEHICLE_NUMBER_FORMAT_LOCAL = HELICOPTER_CONFIG | VEHICLE_NUMBER_FORMAT_LOCAL,




            TRAIN_VEHICLE_NUMBER_FORMAT_FOREIGN = TRAIN_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            TRAM_VEHICLE_NUMBER_FORMAT_FOREIGN = TRAM_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            METRO_VEHICLE_NUMBER_FORMAT_FOREIGN = METRO_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            BUS_VEHICLE_NUMBER_FORMAT_FOREIGN = BUS_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            PLANE_VEHICLE_NUMBER_FORMAT_FOREIGN = PLANE_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            SHIP_VEHICLE_NUMBER_FORMAT_FOREIGN = SHIP_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            MONORAIL_VEHICLE_NUMBER_FORMAT_FOREIGN = MONORAIL_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            FERRY_VEHICLE_NUMBER_FORMAT_FOREIGN = FERRY_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            BLIMP_VEHICLE_NUMBER_FORMAT_FOREIGN = BLIMP_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            CABLE_CAR_VEHICLE_NUMBER_FORMAT_FOREIGN = CABLE_CAR_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            TAXI_VEHICLE_NUMBER_FORMAT_FOREIGN = TAXI_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            TOUR_BUS_CONFIG_VEHICLE_NUMBER_FORMAT_FOREIGN = TOUR_BUS_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            TROLLEY_CONFIG_VEHICLE_NUMBER_FORMAT_FOREIGN = TROLLEY_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
            HELICOPTER_CONFIG_VEHICLE_NUMBER_FORMAT_FOREIGN = HELICOPTER_CONFIG | VEHICLE_NUMBER_FORMAT_FOREIGN,
        }


        public void WriteToBaseConfigXML(TLMBaseConfigXML target)
        {
            foreach (var entry in Enum.GetValues(typeof(ConfigIndex)).OfType<ConfigIndex>().Where(x => x > UNDEF_PART && (x & ConfigIndex.TYPE_PART) != 0))
            {
                switch (entry & ADC_DESC_PART)
                {
                    case 0:
                        var tsd = GetTransportSystemDefinitionForConfigTransport(entry);
                        if (!(tsd is null))
                        {
                            switch (entry & (DESC_DATA | ConfigIndex.TYPE_PART))
                            {
                                case PREFIX: target.GetTransportData(tsd).Prefix = (NamingMode)GetInt(entry); continue;
                                case SEPARATOR:
                                    var valueSep = GetInt(entry);
                                    if (valueSep == 5)// old new line option
                                    {
                                        valueSep = 4;//change to blank space
                                    }
                                    target.GetTransportData(tsd).Separator = (Separator)valueSep;
                                    continue;
                                case SUFFIX: target.GetTransportData(tsd).Suffix = (NamingMode)GetInt(entry); continue;
                                case LEADING_ZEROS: target.GetTransportData(tsd).UseLeadingZeros = GetBool(entry); continue;
                                case PALETTE_MAIN: target.GetTransportData(tsd).Palette = GetString(entry); continue;
                                case PALETTE_RANDOM_ON_OVERFLOW: target.GetTransportData(tsd).PaletteRandomOnOverflow = GetBool(entry); continue;
                                case PALETTE_PREFIX_BASED: target.GetTransportData(tsd).PalettePrefixBased = GetBool(entry); continue;
                                case SHOW_IN_LINEAR_MAP: target.GetTransportData(tsd).ShowInLinearMap = GetBool(entry); continue;
                                case INVERT_PREFIX_SUFFIX: target.GetTransportData(tsd).InvertPrefixSuffix = GetBool(entry); continue;
                                case DEFAULT_COST_PER_PASSENGER_CAPACITY: target.GetTransportData(tsd).DefaultCostPerPassenger = GetInt(entry); continue;
                                case NON_PREFIX: target.GetTransportData(tsd).NonPrefixedNaming = (NamingMode)GetInt(entry); continue;
                                case PREFIX_INCREMENT: target.GetTransportData(tsd).IncrementPrefixOnNewLine = GetBool(entry); continue;
                                case DEFAULT_TICKET_PRICE: target.GetTransportData(tsd).DefaultTicketPrice = GetInt(entry); continue;
                                case TRANSPORT_ICON_TLM: target.GetTransportData(tsd).DefaultLineIcon = (LineIconSpriteNames)GetInt(entry); continue;
                                case VEHICLE_NUMBER_FORMAT_LOCAL: target.GetTransportData(tsd).VehicleIdentifierFormatLocal = GetString(entry); continue;
                                case VEHICLE_NUMBER_FORMAT_FOREIGN: target.GetTransportData(tsd).VehicleIdentifierFormatForeign = GetString(entry); continue;
                            }
                        }
                        break;
                    case GLOBAL_CONFIG & ADC_DESC_PART:
                        switch (entry & DESC_DATA)
                        {
                            case AUTO_COLOR_ENABLED & DESC_DATA:
                                target.UseAutoColor = GetBool(entry);
                                continue;
                            case CIRCULAR_IN_SINGLE_DISTRICT_LINE & DESC_DATA:
                                target.CircularIfSingleDistrictLine = GetBool(entry);
                                continue;
                            case AUTO_NAME_ENABLED & DESC_DATA:
                                target.UseAutoName = GetBool(entry);
                                continue;
                            case ADD_LINE_NUMBER_IN_AUTONAME & DESC_DATA:
                                target.AddLineCodeInAutoname = GetBool(entry);
                                continue;
                        }
                        break;
                    case USE_FOR_AUTO_NAMING_REF & ADC_DESC_PART:
                        var tsd3 = GetTransportSystemDefinitionForConfigTransport(entry);
                        if (!(tsd3 is null))
                        {
                            target.GetTransportData(tsd3).UseInAutoName = GetBool(entry); continue;
                        }
                        var serv = ToService(entry);
                        if (serv != default)
                        {
                            target.GetAutoNameData(serv).UseInAutoName = GetBool(entry); continue;
                        }
                        var special = ToSpecial(entry);
                        if (special != default)
                        {
                            target.GetAutoNameData(special).UseInAutoName = GetBool(entry); continue;
                        }
                        break;
                    case AUTO_NAMING_REF_TEXT & ADC_DESC_PART:
                        var tsd2 = GetTransportSystemDefinitionForConfigTransport(entry);
                        if (!(tsd2 is null))
                        {
                            target.GetTransportData(tsd2).NamingPrefix = GetString(entry); continue;
                        }
                        var serv2 = ToService(entry);
                        if (serv2 != default)
                        {
                            target.GetAutoNameData(serv2).NamingPrefix = GetString(entry); continue;
                        }
                        var special2 = ToSpecial(entry);
                        if (special2 != default)
                        {
                            target.GetAutoNameData(special2).NamingPrefix = GetString(entry); continue;
                        }
                        break;
                }
                LogUtils.DoWarnLog($"Invalid entry found - not copied to base XML: {entry} ({((int)entry).ToString("X8")})");
            }
        }
    }

}
