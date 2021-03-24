using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Oxide.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Drone Hover", "WhiteThunder", "1.0.0")]
    [Description("Allows drones to stay hovering after a player stops controlling them at a computer station.")]
    internal class DroneHover : CovalencePlugin
    {
        #region Fields

        private static DroneHover _pluginInstance;

        private const string PermissionUse = "dronehover.use";

        private readonly InputState EmptyInputState = new InputState();

        private StoredData _pluginData;

        #endregion

        #region Hooks

        private void Init()
        {
            _pluginInstance = this;
            _pluginData = StoredData.Load();

            permission.RegisterPermission(PermissionUse, this);
        }

        private void Unload()
        {
            OnServerSave();
            _pluginInstance = null;
        }

        private void OnServerInitialized()
        {
            RestoreHoveringDrones();
        }

        private void OnServerSave()
        {
            _pluginData.Save();
        }

        private void OnNewSave()
        {
            _pluginData = StoredData.Clear();
        }

        private void OnBookmarkControl(ComputerStation computerStation, BasePlayer player, string bookmarkName, IRemoteControllable entity)
        {
            var currentDrone = GetControlledDrone(computerStation);
            if (currentDrone == null)
                return;

            // Must delay since the drone hasn't stopped being controlled yet.
            NextTick(() =>
            {
                // Ignore if the drone is still being controlled, since that indicates the hook was blocked.
                if (currentDrone == null
                    || computerStation == null
                    || computerStation.currentlyControllingEnt.uid == currentDrone.net.ID)
                    return;

                TryStartDroneHover(currentDrone, player);
            });
        }

        private void OnBookmarkControlEnded(ComputerStation station, BasePlayer player, Drone drone)
        {
            if (drone == null || drone.IsBeingControlled)
                return;

            TryStartDroneHover(drone, player);
        }

        private void OnEntityKill(Drone drone)
        {
            _pluginData.HoveringDrones.Remove(drone.net.ID);
        }

        private bool? CanPickupEntity(BasePlayer player, Drone drone)
        {
            // Prevent pickup while the drone is being controlled.
            // This fixes the issue with vanilla drones, but the purpose is for the ones that are hovering.
            if (drone.IsBeingControlled)
                return false;

            return null;
        }

        // This hook is exposed by plugin: Remover Tool (RemoverTool).
        private bool? canRemove(BasePlayer player, Drone drone)
        {
            if (drone.IsBeingControlled)
                return false;

            return null;
        }

        #endregion

        #region Helper Methods

        private static bool DeployHoverWasBlocked(Drone drone, BasePlayer controller)
        {
            object hookResult = Interface.CallHook("OnDroneHoverStart", drone, controller);
            return hookResult is bool && (bool)hookResult == false;
        }

        private static Drone GetControlledDrone(BasePlayer player)
        {
            var computerStation = player.GetMounted() as ComputerStation;
            if (computerStation == null)
                return null;

            return GetControlledDrone(computerStation);
        }

        private static Drone GetControlledDrone(ComputerStation computerStation) =>
            computerStation.currentlyControllingEnt.Get(serverside: true) as Drone;

        private void TryStartDroneHover(Drone drone, BasePlayer player)
        {
            if (player != null && !permission.UserHasPermission(player.UserIDString, PermissionUse))
                return;

            RaycastHit hit;
            var isGrounded = drone.body.SweepTest(-drone.transform.up, out hit, drone.groundTraceDist);
            if (isGrounded || DeployHoverWasBlocked(drone, player))
            {
                _pluginData.HoveringDrones.Remove(drone.net.ID);
                return;
            }

            drone.InitializeControl(player);
            drone.UserInput(EmptyInputState, player);
            _pluginData.HoveringDrones.Add(drone.net.ID);
            Interface.CallHook("OnDroneHoverStarted", drone, player);
        }

        private void RestoreHoveringDrones()
        {
            if (_pluginData.HoveringDrones == null)
                return;

            foreach (var entity in RemoteControlEntity.allControllables)
            {
                var drone = entity as Drone;
                if (drone != null && !drone.IsBeingControlled && _pluginData.HoveringDrones.Contains(drone.net.ID))
                    TryStartDroneHover(drone, null);
            }
        }

        #endregion

        #region Data

        private class StoredData
        {
            [JsonProperty("HoveringDrones")]
            public HashSet<uint> HoveringDrones = new HashSet<uint>();

            public static StoredData Load() =>
                Interface.Oxide.DataFileSystem.ReadObject<StoredData>(_pluginInstance.Name) ?? new StoredData();

            public static StoredData Clear() => new StoredData().Save();

            public StoredData Save()
            {
                Interface.Oxide.DataFileSystem.WriteObject(_pluginInstance.Name, this);
                return this;
            }
        }

        #endregion
    }
}
