using Newtonsoft.Json;
using Oxide.Core;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Drone Hover", "WhiteThunder", "1.0.3")]
    [Description("Allows RC drones to hover in place when a player disconnects control at a computer station.")]
    internal class DroneHover : CovalencePlugin
    {
        #region Fields

        private const string PermissionUse = "dronehover.use";

        private readonly InputState EmptyInputState = new InputState();

        private StoredData _pluginData;

        #endregion

        #region Hooks

        private void Init()
        {
            _pluginData = StoredData.Load();

            permission.RegisterPermission(PermissionUse, this);
        }

        private void Unload()
        {
            OnServerSave();
        }

        private void OnServerInitialized()
        {
            if (_pluginData.HoveringDrones == null)
                return;

            foreach (var entity in RemoteControlEntity.allControllables)
            {
                var drone = entity as Drone;
                if (drone != null && _pluginData.HoveringDrones.Contains(drone.net.ID))
                {
                    MaybeStartDroneHover(drone, null);
                }
            }
        }

        private void OnServerSave()
        {
            _pluginData.Save();
        }

        private void OnNewSave()
        {
            _pluginData = StoredData.Clear();
        }

        private void OnBookmarkControlStarted(ComputerStation station, BasePlayer player, string bookmarkName, Drone drone)
        {
            if (!drone.ControllingViewerId.HasValue)
                return;

            var viewerId = drone.ControllingViewerId.Value;
            if (viewerId.SteamId != 0)
                return;

            drone.StopControl(viewerId);
            drone.InitializeControl(new CameraViewerId(player.userID, 0));
            station.SetFlag(BaseEntity.Flags.Reserved2, true);
        }

        private void OnBookmarkControlEnded(ComputerStation station, BasePlayer player, Drone drone)
        {
            OnDroneControlEnded(drone, player);
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
            // Null check since somebody reported this method was somehow throwing NREs.
            if (drone == null)
                return null;

            if (drone.IsBeingControlled)
                return false;

            return null;
        }

        // This hook is exposed by plugin: Ridable Drones (RidableDrones).
        private void OnDroneControlEnded(Drone drone, BasePlayer player)
        {
            if (drone == null)
                return;

            MaybeStartDroneHover(drone, player);
        }

        #endregion

        #region Helper Methods

        private bool HoverWasBlocked(Drone drone, BasePlayer formerPilot)
        {
            var hookResult = Interface.CallHook("OnDroneHoverStart", drone, formerPilot);
            return hookResult is bool && (bool)hookResult == false;
        }

        private bool ShouldHover(Drone drone, BasePlayer formerPilot)
        {
            if (drone.IsBeingControlled || drone.isGrounded)
                return false;

            if (formerPilot != null && !permission.UserHasPermission(formerPilot.UserIDString, PermissionUse))
                return false;

            if (HoverWasBlocked(drone, formerPilot))
                return false;

            return true;
        }

        private void MaybeStartDroneHover(Drone drone, BasePlayer formerPilot)
        {
            if (!ShouldHover(drone, formerPilot))
            {
                _pluginData.HoveringDrones.Remove(drone.net.ID);
                return;
            }

            var cameraViewerId = new CameraViewerId(0, 0);
            drone.InitializeControl(cameraViewerId);
            drone.UserInput(EmptyInputState, cameraViewerId);
            _pluginData.HoveringDrones.Add(drone.net.ID);
            Interface.CallHook("OnDroneHoverStarted", drone, formerPilot);
        }

        #endregion

        #region Data

        private class StoredData
        {
            [JsonProperty("HoveringDrones")]
            public HashSet<uint> HoveringDrones = new HashSet<uint>();

            public static StoredData Load() =>
                Interface.Oxide.DataFileSystem.ReadObject<StoredData>(nameof(DroneHover)) ?? new StoredData();

            public static StoredData Clear() => new StoredData().Save();

            public StoredData Save()
            {
                Interface.Oxide.DataFileSystem.WriteObject(nameof(DroneHover), this);
                return this;
            }
        }

        #endregion
    }
}
