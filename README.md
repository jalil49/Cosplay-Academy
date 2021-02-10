# Cosplay Academy
A BepInEx plugin for Koikatu and Koikatsu Party main games that allows "randomization" of outfits that they wear during the story mode.

## How to use 
1. Make sure [BepInEx](https://github.com/BepInEx/BepInEx) is installed and your game is updated.
2. Download the latest release from [here](https://github.com/jalil49/Cosplay-Academy).
3. Extract the plugin into your game directory. The dll file(s) should end up inside the folder `BepInEx\plugins`.
4. Running the game's story mode and starting the day will create all the required folders

##Notes
1.Doesn't currently properly support characters who's hair is made of accessories as it would load the outfit and discard accessories.
2.Some outfits will break, I don't know the exact reason but it probably has to do with overlays/material overrides like in color varients of same costume already being used.
	2a. The more coordinated you leave everyone the less likely to encounter a break.
3.A file structure is required and can be copied from this repositry, but the files will be created when on the "going to school" cutscene happens.
4.I originally wanted to use Tags for the images rather than forcing file paths, but unfortunatly PNG doesn't actually support it.
5.The Sets folder is used for folders of outfits that have similar asthetics such as "Beezys School Uniforms" and want people to follow a theme if choosen and hopefully don't conflict with each other.
	5A. Beware of simple color varients in sets such as NekoMaids as they will conflict with each other.
6.anything that is in the parent folder of \Sets will be coordinating with itself rather than with those that share the folder unlike the Sets folder.

##Known issues
1. Reloading a Save will cause this to break for characters already loaded. Going back to Title will fix this.
	Example: start day, immediate go home, and load same save.
2. KPlug seems to affect only the main heroine and any who invited or joined will load with default. Didn't check in detail but upon exiting the ones who joined retained the outfit, didn't confirm.


