﻿using ColossalFramework.Math;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Klyte.Commons.Extensions.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    public class OutsideConnectionOverrides : Redirector, IRedirectable
    {
        #region Overrides

        private static readonly TransferManager.TransferReason[] m_managedReasons = new TransferManager.TransferReason[]   {
                TransferManager.TransferReason.DummyCar,
                TransferManager.TransferReason.DummyTrain,
                TransferManager.TransferReason.DummyShip,
                TransferManager.TransferReason.DummyPlane
            };

        public static VehicleInfo GetRandomVehicle(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, TransferManager.TransferReason reason)
        {
            if (m_managedReasons.Contains(reason))
            {
                LogUtils.DoLog("START TRANSFER OutsideConnectionAI!!!!!!!!");
                return TryGetRandomVehicle(vm, ref r, service, subService, level);
            }
            return vm.GetRandomVehicleInfo(ref r, service, subService, level);

        }
        private static VehicleInfo TryGetRandomVehicleStation(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {

            LogUtils.DoLog("START TRANSFER StationAI!!!!!!!!");
            return TryGetRandomVehicle(vm, ref r, service, subService, level);
        }
        private static void SetRegionalLine(ushort vehicleId, ushort stopId)
        {
            ref Vehicle veh = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
            if (TransportSystemDefinition.From(veh.Info) == TransportSystemDefinition.TRAIN)
            {
                if (TLMStationUtils.GetStationBuilding(stopId, 0, false) != veh.m_sourceBuilding)
                {
                    veh.m_custom = NetManager.instance.m_segments.m_buffer[NetManager.instance.m_nodes.m_buffer[stopId].GetSegment(0)].GetOtherNode(stopId);
                }
                else
                {
                    veh.m_custom = stopId;
                }
            }
        }

        private static VehicleInfo TryGetRandomVehicle(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
            var tsd = TransportSystemDefinition.FromOutsideConnection(subService, level);
            if (!(tsd is null))
            {
                VehicleInfo randomVehicleInfo = tsd.GetTransportExtension().GetAModel(0);
                if (randomVehicleInfo != null)
                {
                    return randomVehicleInfo;
                }
            }
            return vm.GetRandomVehicleInfo(ref r, service, subService, level);
        }

        private static IEnumerable<CodeInstruction> TranspileStationAISpawnVehicle(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo TryGetRandomVehicle = typeof(OutsideConnectionOverrides).GetMethod("TryGetRandomVehicleStation", allFlags);

            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetRandomVehicleInfo")
                {
                    inst.RemoveAt(i);
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Call, TryGetRandomVehicle),
                    });
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        private static IEnumerable<CodeInstruction> TranspileStartConnectionTransferImpl(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo GetRandomVehicle = typeof(OutsideConnectionOverrides).GetMethod("GetRandomVehicle", allFlags);

            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetRandomVehicleInfo")
                {
                    inst.RemoveAt(i);
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call, GetRandomVehicle),
                    });
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        private static IEnumerable<CodeInstruction> TranspileIncomingVehicleSetLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(TranspileStationAISpawnVehicle(instructions));
            MethodInfo SetRegionalLine = typeof(OutsideConnectionOverrides).GetMethod("SetRegionalLine", allFlags);

            for (int i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i + 1].opcode == OpCodes.Ret
                    && inst[i].opcode == OpCodes.Ldc_I4_1)
                {
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Call, SetRegionalLine),
                    });
                    break;
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        private static IEnumerable<CodeInstruction> TranspileOutgoingVehicleSetLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(TranspileStationAISpawnVehicle(instructions));
            MethodInfo SetRegionalLine = typeof(OutsideConnectionOverrides).GetMethod("SetRegionalLine", allFlags);

            for (int i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i + 1].opcode == OpCodes.Ret
                    && inst[i].opcode == OpCodes.Ldc_I4_1)
                {
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Call, SetRegionalLine),
                    });
                    break;
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        private static IEnumerable<CodeInstruction> TranspileUpdateBindingsCSWIP(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo CanAllowRegionalLines = typeof(OutsideConnectionOverrides).GetMethod("CanAllowVanillaRegionalLines", allFlags);

            for (int i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i + 1].opcode == OpCodes.Ldnull
                    && inst[i].opcode == OpCodes.Ldloc_S
                    && inst[i].operand is LocalBuilder lb
                    && lb.LocalIndex == 5
                    )
                {
                    inst.RemoveAt(i + 1);
                    inst.RemoveAt(i + 1);
                    inst.InsertRange(i + 1, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Call, CanAllowRegionalLines),
                    });
                    break;
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        private static bool CanAllowVanillaRegionalLines(TransportStationAI stationAI, ushort buildingId) => !(stationAI is null) && !TLMBuildingDataContainer.Instance.SafeGet(buildingId).TlmManagedRegionalLines;

        public Redirector RedirectorInstance => this;

        #endregion

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading OutsideConnectionAI Hooks!");
            RedirectorInstance.AddRedirect(typeof(OutsideConnectionAI).GetMethod("StartConnectionTransferImpl", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileStartConnectionTransferImpl", allFlags));
            RedirectorInstance.AddRedirect(typeof(TransportStationAI).GetMethod("CreateOutgoingVehicle", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileOutgoingVehicleSetLine", allFlags));
            RedirectorInstance.AddRedirect(typeof(TransportStationAI).GetMethod("CreateIncomingVehicle", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileIncomingVehicleSetLine", allFlags));
            RedirectorInstance.AddRedirect(typeof(CityServiceWorldInfoPanel).GetMethod("UpdateBindings", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileUpdateBindingsCSWIP", allFlags));
        }

        #endregion

    }
}
