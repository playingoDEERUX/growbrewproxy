# growbrewproxy
Growbrew - An advanced Growtopia ENet Proxy made in C#, giving you the advantage of networking for superior and good exploits/cheats!

# Compatibility:
- Windows 7 - Windows 10 (windows versions older than than that may work with modifications and such, this is not supported by default though.)
.NET requirements: .NET 5.0+ (ATTENTION, THIS IS NECESSARY NOW!)
If you have issues with ENet.Managed, try reinstalling the NuGet packet ENet.Managed please!
NOTE: V2.2.1 Supports x64 only as of right now, if you use 32 bit or ARM for some reason contact me on discord DEERUX#1551

# Features:
---------------------------------------------------------------
- GUI
- world serialization, has many many visuals included in form of serialization in order to keep track of world packet values
- Showing players in world list
- Subserver switching is supported
- Name Changer
- Safe vault bypass
- Ignore Autoban packet by client
- Multiselection of players, seeing invisible mods/developers in Player Manager
- Modified Dialog sender
- Tile-o-meter
- able to send custom states etc. already (Super Speed and Mod Noclip so far)
- Can ignore set back packet when using speed hack (must be selected in "Internal Extra") tab
- Print all variant list functions sent by server.
- AAP bypass no infos, only password and growid. (SPOOF YOUR MAC TO 02:00:00:00:00:00 (it has ":00" missing so add that before clicking change mac button) (EDIT: used ios platform id, since it got patched it uses android platform id now)
- RGB Skin (somewhat, was too lazy to fix having too dark / light skin)
- Magplant hack (pickup range 11 blocks exploit)
- overall better and more cheats/features (it used to be a paid hack anyway, therefore has "premium quality")
- Ability to send any action packet to server and to client. (message type 2 and 3)
- Some nuker hack (inspired from minecraft)
- Speed hack on all versions
- Version spoofer
- Integrated HTTP Server
- Player Manager
- Several GTPS Crash methods
- Unlimited Zoom
- Packet Logger (supports extended data too) (will be logged in proxy tab)
- RGB Skin
- Growbrew Network (chatting etc, you must register first, in order to register, contact my discord which is stated in the Proxy Main Page) (removed, discontinuing this the proxy itself is enough.)
- Ignore autoban packets by client (if the check was server sided, you can still get banned)
- Ignore tracking packets and crash log requests
- Spoofed RID all the time, and login packet is made so most identifiers/information wont be included.
- Instant MAC Spoofer (use mac 02:00:00:00:00:00 for AAP bypass)
- many more, might even add something soon to this open src project.
- *NEW* Very simple packet load balancer

ADDED SINCE V2:
- *NEW* Account Checker
- *NEW* Red damage to block exploit (NOT VISUAL, EVERYONE SEES IT)
- *NEW* Drop entire inventory with spreading
- *NEW* Config exporting/saving
- A few fixes.
- Many more

ADDED SINCE V2.1:
This update focused mostly on performance/stability. Would highly recommend this one over V2.0.
- *NEW* Multibotting, the better way of multiboxing in only a single Growtopia window using the proxy. (Only in Growbrew Proxy Extreme Edition or Auto-CCS soon)
- Upgraded ENet wrapper to ENet.Managed v4 (huge improvement)
- Fixed autofarming may cause autoban after some time
- Fixed random disconnects on very populated worlds
- Extended customizability (added more to toggle in Config menu)
- Fixed "drop entire inventory" autoban.
- Fixed some other minor bugs, such as UI bugs etc.
- Optimized few other things.

ADDED SINCE V2.2.1:
This update fixes what Ubisoft has done to prevent proxies lately.
- Add HashString to C# (func name is "HashBytes" - credits iProgramInCpp)
- Upgraded ENet wrapper to ENet.Managed v5 + ENet protocol fix for server and client side (type2|1)
- Prepare dll injector code for future usage of our own internal
- 3 *NEW* exploits: BRB status change credits ama6nen and ghost slime spam (on_step_on_tile_mod, credits iProgramInCpp) and the doorid "exploit"
- Fix other stuff
- Extreme version will be sold again soon, especially with our new multibotting feature.
- Enjoy!


---------------------------------------------------------------


# Usage:
---------------------------------------------------------------
To use, add this into your hosts:

########################

127.0.0.1 growtopia1.com

127.0.0.1 growtopia2.com

########################

OPEN THE .sln file in order to begin without further issues
If you just want to use the growbrew proxy program it self, without doing any modifications or any coding,
the .exe file and binaries are currently located at https://github.com/playingoDEERUX/growbrewproxy/releases/

Click Start HTTP server and Start Proxy and you can start.
---------------------------------------------------------------

# TODO:
---------------------------------------------------------------
- Refactor code and maybe better UI
- Add vector2, rect and vector3 support for receiving func call packets
---------------------------------------------------------------

# Pictures/GIFs:
https://gyazo.com/8093ffccfa65574be9105bb081a0c7c5

# Tested with Visual Studio 2017 and 2019

Other versions may work too, they are not tested. Credits to moien007 for ENet.Managed and kernys for Kernys.Bson


current version: V2.2.1
# MADE BY DEERUX (quit) AND iProgramInCpp - YouTube (me): https://www.youtube.com/channel/UC0htMnKS9EGPlaeIkcVkxhw

(IMPORTANT: THIS DEERUX IS A FAKER/IMPERSONATOR): https://www.youtube.com/channel/UCjUmKOedwc7gDa8Fl9E5HMA If you have subbed to him, unsubscribe now and this is my real channel: https://www.youtube.com/channel/UC0htMnKS9EGPlaeIkcVkxhw

(UPDATE: second channel is terminated too, dont trust anyone called DEERUX or playingo on youtube for now, youtube copyright system is great at this!11)

Subscribe to support me :)

Enjoy the **ultimate** haXing in Growtopia :) Star this if you liked my project, thanks!

NOTE: can be combined with any trainer of your choice, this is a proxy and not a trainer so this will only offer exclusive proxy / networking features

NOTE: Make sure you allow unsafe code when compiling
