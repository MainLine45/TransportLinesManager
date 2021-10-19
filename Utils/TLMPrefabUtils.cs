﻿using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TLMPrefabUtils
    {

        internal static List<string> LoadBasicAssets(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();

            LogUtils.DoLog("LoadBasicAssets: pre prefab read");
            foreach (var prefabEntry in VehiclesIndexes.instance.PrefabsLoaded)
            {
                VehicleInfo prefab = prefabEntry.Value;
                if (!(prefab is null) && definition.IsFromSystem(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
        internal static List<string> LoadBasicAssetsIntercity(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();
            if (definition.LevelIntercity is null)
            {
                return basicAssetsList;
            }
            LogUtils.DoLog("LoadBasicAssetsIntercity: pre prefab read");
            foreach (var prefabEntry in VehiclesIndexes.instance.PrefabsLoaded)
            {
                VehicleInfo prefab = prefabEntry.Value;
                if (!(prefab is null) && definition.IsFromSystemIntercity(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }


}

