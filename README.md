## Features

- Allows RC drones to automatically hover in place when a player (who has permission) disconnects control at a computer station.
- Does not apply if the drone is on the ground when the player disconnects control.

Without this plugin, RC drones will fall to the ground when the controlling player disconnects.

## Recommended compatible plugins

- [Drone Lights](https://umod.org/plugins/drone-lights) -- Adds controllable search lights to RC drones.
- [Drone Storage](https://umod.org/plugins/drone-storage) -- Allows players to deploy a small stash to RC drones.

## Permissions

- `dronehover.use` -- When a player has this permission, disconnecting from a drone at a computer station will cause that drone to hover in place.

## FAQ

#### How do I get a drone?

As of this writing (March 2021), RC drones are a deployable item named `drone`, but they do not appear naturally in any loot table, nor are they craftable. However, since they are simply an item, you can use plugins to add them to loot tables, kits, GUI shops, etc. Admins can also get them with the command `inventory.give drone 1`, or spawn one in directly with `spawn drone.deployed`.

#### How do I remote-control a drone?

If a player has building privilege, they can pull out a hammer and set the ID of the drone. They can then enter that ID at a computer station and select it to start controlling the drone. Controls are `W`/`A`/`S`/`D` to move, `shift` (sprint) to go up, `ctrl` (duck) to go down, and mouse to steer.

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
