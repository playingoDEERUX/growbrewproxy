# growbrewproxy
growbrew got leaked once (1.4) so now releasing source code cuz some skids got it someone must have told the link...
NOTE: has growbrew server/client part for chatting etc., you can remove that though.

- The first Growtopia Proxy coded in c# - not affiliated with ama6nen/enetproxy
- Reminder: A proxy actually has NOTHING to do with an internal. A proxy is pure networking. The naming of the tabs is pure marketing startegy. Otherwise, a solid and good proxy with many many features.

NOTE: ama's proxy is more development oriented while Growbrew was aimed at release since its beginning, means it contains a lot more
features and more features that would actually be undetected to avoid ban chance, as this proxy initially was also aimed to not ban as much as ama's proxy, even though there is server sided checking, there are few things we rather didnt add like unlimited zoom punching to avoid people getting banned this easily, however, this was initially not aimed for development or private servers as stated, therefore it should also be more stable as well, which I would not guarantee. Performance wise I have heard better feed back for Growbrew than from ama's enetproxy. 
people who bought it are saying that growbrew is slightly faster than ama's enetproxy, which is weird cause I serialize world packet
and c++ should be faster in general, but heres proof of 1 guy saying that growbrew has a better overall performance:
https://imgur.com/a/lP7lxSI


# Features:
---------------------------------------------------------------
- GUI
- world parsing, has many many visuals included
- Showing players in world list
- Subserver switching is supported
- Name Changer
- Safe vault bypass
- Ignore Autoban packet by client
- Multiselection of players, seeing invisible mods in Player Manager
- Modified Dialog sender
- Tile-o-meter
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
- Growbrew Network (chatting etc, you must register first, in order to register, contact my discord which is stated in the Proxy Main Page)
- many more, might even add something soon to this open src project.
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
the .exe file and binaries are located at GrowbrewProxy/bin/Debug. Changelogs are in the .EXE file itself.

Click Start HTTP server and Start Proxy and you can start.
---------------------------------------------------------------

# TODO:
---------------------------------------------------------------
- Refactor code and maybe better UI
---------------------------------------------------------------

# Pictures/GIFs:
https://gyazo.com/8093ffccfa65574be9105bb081a0c7c5

# Tested with Visual Studio 2017

Credits to moien007 for ENet.Managed and kernys for Kernys.Bson


current version: 1.5.1
# MADE BY DEERUX AND iProgramInCpp - YouTube (me): https://www.youtube.com/channel/UCi88IU-vDvVr6Tay9CT20Gw?view_as=subscriber

Subscribe to support me :)
Credits to Vyte for AAP Bypass
