# Utilla

A PC library for Gorilla Tag that handles various room-related things (and more?)

## Installation

### Automatic installation
If you don't want to manually install, you can install this mod with the [Monke Mod Manager](https://github.com/DeadlyKitten/MonkeModManager/releases/latest)
### Manual Installation

If your game isn't modded with BepinEx, DO THAT FIRST! Simply go to the [latest BepinEx release](https://github.com/BepInEx/BepInEx/releases) and extract BepinEx_x64_VERSION.zip directly into your game's folder, then run the game once to install BepinEx properly.

Next, go to the [latest release of this mod](https://github.com/legoandmars/Utilla/releases/latest) and extract it directly into your game's folder. Make sure it's extracted directly into your game's folder and not into a subfolder!

## For Developers
Handling whether the player is in a public or private room is a very important part of making sure your mod won't be used to cheat. Utilla makes this easy by providing an event for you to handle: `Utilla.Events.RoomJoined` 

Example:
```cs
using System;
using BepInEx;
using Utilla;

namespace ExamplePlugin
{
    [BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.3.0")] // Make sure to add Utilla as a dependency!
    public class ExamplePlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Utilla.Events.RoomJoined += RoomJoined;
        }

        private void RoomJoined(object sender, Events.RoomJoinedArgs e)
        {
            if(e != null && e.isPrivate != null && e.isPrivate)
            {
                // The room is private. Enable mod stuff.
            }
            else
            {
                // The room is public. Disable mod stuff.
            }
        }
    }
}
```

If your mod's logic is too complex to easily swap between doing things in private/public lobbies, you can always add the `[ForcePrivateLobby]` attribute. This will make the player unable to join public lobbies with your mod enabled - instead, it will join a private lobby with a random name. Although this isn't preferred, it's better than no protection and is easier to implement.

Example:
```cs
using System;
using BepInEx;
using Utilla;

namespace ExamplePlugin
{
    [BepInPlugin("org.legoandmars.gorillatag.exampleplugin", "Example Plugin", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.3.0")] // Make sure to add Utilla as a dependency!
    [ForcePrivateLobby] // This will force the player out of public lobbies, and into private ones.
    public class ExamplePlugin : BaseUnityPlugin
    {
        void Awake()
        {
            // Do what you want here - you don't have to worry about it being used in public games.
        }
    }
}
```

### Manually joining private lobbies
If you'd like to join custom private lobbies with your mod, Utilla implements methods for that too.
```cs
Utilla.Utils.RoomUtils.JoinPrivateLobby() // Joins a private lobby with a random 6 character code
Utilla.Utils.RoomUtils.JoinPrivateLobby("TestLobby") // Joins a private lobby with the code TestLobby
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