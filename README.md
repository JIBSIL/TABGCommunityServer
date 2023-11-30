# TABGCommunityServer
Efficient server software for Totally Accurate Battlegrounds built from the ground up using ENet and C#

Welcome to the TABGCommunityServer repository.

Running a TABG Community Server has many advantages (such as being able to access any item in the game, create custom gamemodes, etc). However, consider that the server may require significant resources to run. Make sure you have a decently spec'd computer, or spare PC/server to run it on.

## Creating and Running a Server

Creating a server is as simple. Just follow these steps:
1. Clone the repository using the "Download as ZIP" button or `git clone`.
2. Open the project in [Visual Studio 2022](https://visualstudio.microsoft.com/) and press the green play button at the top, you may need to install .NET 7.0 runtime when opening the project for the first time.
3. If you wish to run it outside of Visual Studio, build it using "Build Solution" in the top Build menu.

Running the server if built:

Locate the "TABGCommunityServer" folder and go to TABGServer\bin\Debug\net7.0
Here you will find the executable "TABGCommunityServer.exe". Run this and the server will start.

Joining with a client is slightly harder as we don't have patching software released for the client yet.

First, get your server listed on a community server list (listed at the bottom of this page) or host your own list.

Instructions on how to do that, as well as how to patch the client to use a community server list, are on our sister repository [TABGServerProxy](https://github.com/JIBSIL/TABGServersProxy)

## Chat Commands:

The following commands can be typed in chat:
* `/kill victimid killerid victimname` (for example: `/kill 0 0 PLAYERNAME`)
* `/give itemid itemamount` (for example: `/give 123 1` NOTE: you have to add the quantity at the end of the command otherwise you will not receive the item)
* `/notification`
* `/kit`
* `/coords`
* `/broadcast MESSAGE`
* `/revive`
* `/heal`
* `/state PLAYERSTATE`
* `/gamestate flying/waiting/started/countdown` (countdown has an integer argument of how many seconds to count down)

## FAQ:
**I'd rather dip my hand into a nest of angry hornets than patch the client. What do I do?**
Ask your friend for a Assembly-CSharp.dll file, and follow the "Disabling AntiCheat and Running" section [here](https://github.com/JIBSIL/TABGServersProxy)

**Are you going to ever distribute server binaries?**
Yes! I will eventually set up Github Actions for automatically built binaries.

**Is this illegal? Do you use Landfall's copyrighted code in the server?**
Not in the US, and no. This is built from the ground up with zero Landfall code. We did not use leaks or any other non-public information to build the server.

**How does this compare to the regular server?**
The regular server is built in Unity, and is quite a bit more inefficient than this one. This software has been built specifically for performance on large player counts.

## Server Lists
None exist that we know of. If you're willing to host a 24/7 list, contact me on Discord (incomprehensibl) or via a Github Issue
