﻿using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    public class TLMTransportLineExtension : DataExtensionBase<TLMTransportLineExtension>, ISafeGettable<TLMTransportLineConfiguration>, IBasicExtension
    {
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMTransportLineConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<TLMTransportLineConfiguration>();
        internal void SafeCleanEntry(ushort lineID) => Configurations[lineID] = new TLMTransportLineConfiguration();
        public TLMTransportLineConfiguration SafeGet(uint lineId)
        {
            if (!Configurations.ContainsKey(lineId))
            {
                Configurations[lineId] = new TLMTransportLineConfiguration();
            }
            return Configurations[lineId];
        }
        IAssetSelectorStorage ISafeGettable<IAssetSelectorStorage>.SafeGet(uint index) => SafeGet(index);
        IBudgetStorage ISafeGettable<IBudgetStorage>.SafeGet(uint index) => SafeGet(index);
        ITicketPriceStorage ISafeGettable<ITicketPriceStorage>.SafeGet(uint index) => SafeGet(index);
        IDepotSelectionStorage ISafeGettable<IDepotSelectionStorage>.SafeGet(uint index) => SafeGet(index);
        IBasicExtensionStorage ISafeGettable<IBasicExtensionStorage>.SafeGet(uint index) => SafeGet(index);

        public override string SaveId => $"K45_TLM_TLMTransportLineExtension";

        private readonly Dictionary<TransportSystemDefinition, List<string>> m_basicAssetsList = new Dictionary<TransportSystemDefinition, List<string>>();

        public void SetUseCustomConfig(ushort lineId, bool value)
        {
            SafeGet(lineId).IsCustom = value;
            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        public bool IsUsingCustomConfig(ushort lineId) => SafeGet(lineId).IsCustom;

        public void SetDisplayAbsoluteValues(ushort lineId, bool value) => SafeGet(lineId).DisplayAbsoluteValues = value;
        public bool IsDisplayAbsoluteValues(ushort lineId) => SafeGet(lineId).DisplayAbsoluteValues;
        #region Asset List
        public List<string> GetBasicAssetListForLine(ushort lineId)
        {

            var tsd = TransportSystemDefinition.From(lineId);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMPrefabUtils.LoadBasicAssets(tsd);
            }
            return m_basicAssetsList[tsd];
        }
        public Dictionary<string, string> GetSelectedBasicAssetsForLine(ushort lineId) => this.GetAssetListForLine(lineId).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
        public Dictionary<string, string> GetAllBasicAssetsForLine(ushort lineId)
        {
            var tsd = TransportSystemDefinition.From(lineId);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMPrefabUtils.LoadBasicAssets(tsd);
            }

            return m_basicAssetsList[tsd].ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
        }
        public VehicleInfo GetAModel(ushort lineId)
        {
            VehicleInfo info = null;
            List<string> assetList = ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineId);
            while (info == null && assetList.Count > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    ExtensionStaticExtensionMethods.RemoveAssetFromLine(this, lineId, modelName);
                    assetList = ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineId);
                }
            }
            return info;
        }

        #endregion

        #region Ticket Price

        public uint GetDefaultTicketPrice(uint lineId = 0)
        {
            var tsd = TransportSystemDefinition.From(lineId);
            switch (tsd.SubService)
            {
                case ItemClass.SubService.PublicTransportCableCar:
                case ItemClass.SubService.PublicTransportBus:
                case ItemClass.SubService.PublicTransportMonorail:
                    return 100;
                case ItemClass.SubService.PublicTransportMetro:
                case ItemClass.SubService.PublicTransportTaxi:
                case ItemClass.SubService.PublicTransportTrain:
                case ItemClass.SubService.PublicTransportTram:
                    return 200;
                case ItemClass.SubService.PublicTransportPlane:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        return 100;
                    }
                    else
                    {
                        return 1000;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        return 100;
                    }
                    else
                    {
                        return 500;
                    }
                case ItemClass.SubService.PublicTransportTours:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Car)
                    {
                        return 100;
                    }
                    else if (tsd.VehicleType == VehicleInfo.VehicleType.None)
                    {
                        return 0;
                    }
                    return 102;
                default:
                    LogUtils.DoLog("subservice not found: {0}", tsd.SubService);
                    return 103;
            }

        }
        #endregion
        #region Depot
        public uint LineToIndex(ushort lineId) => lineId > 0 ? lineId : throw new System.Exception("Line 0 cannot have specific configuration!");


        #endregion


    }
}
