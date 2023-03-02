## Features

- Allows RC drones to automatically hover in place when a player (who has permission) disconnects control at a computer station.
- Does not apply if the drone is on the ground when the player disconnects control.

Without this plugin, RC drones will fall to the ground when the controlling player disconnects.

## Permissions

- `dronehover.use` -- When a player has this permission, disconnecting from a drone at a computer station will cause that drone to hover in place.

## Recommended compatible plugins

Drone balance:
- [Drone Settings](https://umod.org/plugins/drone-settings) -- Allows changing speed, toughness and other properties of RC drones.
- [Targetable Drones](https://umod.org/plugins/targetable-drones) -- Allows RC drones to be targeted by Auto Turrets and SAM Sites.
- [Limited Drone Range](https://umod.org/plugins/limited-drone-range) -- Limits how far RC drones can be controlled from computer stations.

Drone fixes and improvements:
- [Better Drone Collision](https://umod.org/plugins/better-drone-collision) -- Overhauls RC drone collision damage so it's more intuitive.
- [Auto Flip Drones](https://umod.org/plugins/auto-flip-drones) -- Auto flips upside-down RC drones when a player takes control.
- [Drone Hover](https://umod.org/plugins/drone-hover) (This plugin) -- Allows RC drones to hover in place while not being controlled.

Drone attachments:
- [Drone Lights](https://umod.org/plugins/drone-lights) -- Adds controllable search lights to RC drones.
- [Drone Turrets](https://umod.org/plugins/drone-turrets) -- Allows players to deploy auto turrets to RC drones.
- [Drone Storage](https://umod.org/plugins/drone-storage) -- Allows players to deploy a small stash to RC drones.
- [Ridable Drones](https://umod.org/plugins/ridable-drones) -- Allows players to ride RC drones by standing on them or mounting a chair.

## Developer Hooks

#### OnDroneHoverStart

- Called when a drone is about to start hovering
- Returning `false` will prevent the drone from hovering
- Returning `null` will result in the default behavior
- The `BasePlayer` argument will be `null` if the drone is resuming hovering after a server restart

```csharp
bool? OnDroneHoverStart(Drone drone, BasePlayer optionalPilot)
```

#### OnDroneHoverStarted

- Called after a drone has started hovering
- No return behavior
- The `BasePlayer` argument will be `null` if the drone resumed hovering after a server restart

```csharp
void OnDroneHoverStarted(Drone drone, BasePlayer optionalPilot)
```
