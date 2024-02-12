# Utilla

A PC library for Gorilla Tag that handles various room-related things (and more?)

## Installation

### Automatic installation
If you don't want to manually install, you can install this mod with the [Monke Mod Manager](https://github.com/DeadlyKitten/MonkeModManager/releases/latest)
### Manual Installation

If your game isn't modded with BepinEx, DO THAT FIRST! Simply go to the [latest BepinEx release](https://github.com/BepInEx/BepInEx/releases) and extract BepinEx_x64_VERSION.zip directly into your game's folder, then run the game once to install BepinEx properly.

Next, go to the [latest release of this mod](https://github.com/legoandmars/Utilla/releases/latest) and extract it directly into your game's folder. Make sure it's extracted directly into your game's folder and not into a subfolder!

## For Developers
### **Important:** Utilla 1.5.0 Update
With the release of Utilla 1.5.0, mods are required to only function in modded gamemodes, instead of in any private lobby. Utilla provides easy to use attributes to make this transition as painless as possible. See [this commit](https://github.com/Graicc/SpaceMonke/commit/85074d5947856f5c8d673b141056d26fcc267115) for an example of updating your mod to work with the new system.

### Enabling your mod

Handling whether the player is in a modded room is a very important part of making sure your mod won't be used to cheat. Utilla makes this easy by providing attributes to trigger when a modded lobby is joined.

The `[ModdedGamemode]` must be applied to the plugin class of your mod to use the other attributes. `[ModdedGamemodeJoin]` and `[ModdedGamemodeLeave]` can be applied to any void method within this class, with an optional string parameter containing the complete gamemode string. These methods are called when a modded room is joined or left, respectively.

```cs
using System;
using BepInEx;
using Utilla;

namespace ExamplePlugin
{
    [BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode] // Enable callbacks in default modded gamemodes
    public class ExamplePlugin : BaseUnityPlugin
    {
        bool inAllowedRoom = false;

        private void Update()
        {
            if (inAllowedRoom)
            {
                // Do mod stuff
            }
        }

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            // The room is modded. Enable mod stuff.
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
        }
    }
}
```

### Custom Gamemodes

Utilla 1.5.0 brings support for custom gamemodes to Gorilla Tag. A mod can register custom gamemodes through the `[ModdedGamemode]` attribute, and will appear next to the default gamemodes in game.

```cs
[BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
[BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
[ModdedGamemode("mygamemodeid", "MY GAMEMODE", Models.BaseGamemode.Casual)] // Enable callbacks in a new casual gamemode called "MY GAMEMODE"
public class ExamplePlugin : BaseUnityPlugin {}
```

Additionally, a completely custom game manager can be used, by creating a class that inherits `GorillaGameManager`. Creating a custom gamemode requires advanced knowledge of Gorilla Tag's networking code. Currently, matchmaking does not work for fully custom gamemodes, but they can still be used through room codes.

```cs
[BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
[BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
[ModdedGamemode("mygamemodeid", "MY GAMEMODE", typeof(MyGameManager))] // Enable callbacks in a new custom gamemode using MyGameManager
public class ExamplePlugin : BaseUnityPlugin {}

public class MyGameManager : GorillaGameManager
{
    // The game calls this when this is the gamemode for the room.
    public override void StartPlaying()
    {
        // Base needs to run for base GorillaGamanger functionality to run.
        base.StartPlaying();
    }

    // Called by game when you leave a room or gamemode is changed.
    public override void StopPlaying()
    {
        // Base needs to run for the game mode stop base GorillaGameManager functionality from running.
        base.StopPlaying();
    }

    // Called by the game when you leave a room after StopPlaying, use this for all important clean up and resetting the state of your game mode.
    public override void Reset()
    {
    }

    // Gamemode names must not have spaces and must not contain "CASUAL", "INFECTION", "HUNT", or "BATTLE".
    // Names that contain the name of other custom gamemodes will confilict.
    public override string GameModeName()
    {
        return "CUSTOM";
    }

    // GameModeType is an enum which is really an int, so any int value will work. 
    // Make sure to use a unique value not taken by other game modes.
    public override GameModeType GameType()
    {
        return (GameModeType)765;
    }

    public override int MyMatIndex(Player forPlayer)
    {
        return 3;
    }
}
```

### Room join events

Utilla provides events to broadcast when any room is joined. They are not recommended to enable and disable your mod, for that use attributes, described above. Utilla provides two events for room joining and leaving, `Utilla.Events.RoomJoined` and `Utilla.Events.RoomLeft` 

```cs
using System;
using BepInEx;
using Utilla;

namespace ExamplePlugin
{
    [BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla as a dependency!
    public class ExamplePlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Utilla.Events.RoomJoined += RoomJoined;
            Utilla.Events.RoomLeft += RoomLeft;
        }

        private void RoomJoined(object sender, Events.RoomJoinedArgs e)
        {
            UnityEngine.Debug.Log($"Private room: {e.isPrivate}, Gamemode: {e.Gamemode}");
        }

        private void RoomLeft(object sender, Events.RoomJoinedArgs e)
        {
            UnityEngine.Debug.Log($"Private room: {e.isPrivate}, Gamemode: {e.Gamemode}");
        }
    }
}
```

### Initialization event

Utilla provides an event that is triggered after Gorilla Tag initializes, use this if you are getting null reference errors on singleton objects such as `GorillaLocomotion.Player.Instance`

```cs
using System;
using BepInEx;
using Utilla;

namespace ExamplePlugin
{
    [BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla as a dependency!
    public class ExamplePlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Utilla.Events.GameInitialized += GameInitialized;
        }

        private void GameInitialized(object sender, EventArgs e)
        {
            // Player instance has been created
            UnityEngine.Debug.Log(GorillaLocomotion.Player.Instance.jumpMultiplier);
        }
    }
}
```

### Manually joining private lobbies
If you'd like to join custom private lobbies with your mod, Utilla implements methods for that too.
```cs
Utilla.Utils.RoomUtils.JoinPrivateLobby() // Joins a private lobby with a random 6 character code
Utilla.Utils.RoomUtils.JoinPrivateLobby("TestLobby") // Joins a private lobby with the code TestLobby
Utilla.Utils.RoomUtils.JoinPrivateLobby("TestLobby", true) // Joins a private casual lobby with the code TestLobby
```

### Using custom queues
If you'd like to use custom queues with your mod, Utilla implements methods for that as well.
```cs
Utilla.Utils.RoomUtils.JoinModdedLobby("TestQueue") // Joins a random room in the queue TestQueue
Utilla.Utils.RoomUtils.JoinModdedLobby("TestQueue", true) // Joins a random casual room in the queue TestQueue
```

## Building
This project is built with C# using .NET Standard.

For references, create a Libs folder in the same folder as the project solution. Inside of this folder you'll need to copy:

```
0Harmony.dll
BepInEx.dll
BepInEx.Harmony.dll
``` 
from `Gorilla Tag\BepInEx\plugins`, and
```
Assembly-CSharp.dll
PhotonRealtime.dll
PhotonUnityNetworking.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
``` 
from `Gorilla Tag\Gorilla Tag_Data\Managed`.

## Disclaimers
This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. ©2021 Another Axiom LLC.
