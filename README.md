# Cosplay Academy
A BepInEx plugin for Koikatu and Koikatsu Party main games that allows "randomization" of outfits that they wear during the story mode.

## How to use 
1. Make sure [BepInEx](https://github.com/BepInEx/BepInEx) is installed and your game is updated.
2. Download the latest release from [here](https://github.com/jalil49/Cosplay-Academy).
3. Extract the plugin into your game directory. The dll file(s) should end up inside the folder `BepInEx\plugins`.
4. Running the game's story mode and starting the day will create all the required folders

## Notes

1. Doesn't currently properly support characters who's hair is made of accessories as it would load the outfit and discard accessories.

2. Some outfits will break, I don't know the exact reason but it probably has to do with overlays/material overrides like in color varients of same costume already being used.

	2a. The more coordinated you leave everyone the less likely to encounter a break.
3. A file structure is required and can be copied from this repositry, but the files will be created when on the "going to school" cutscene happens.

4. I originally wanted to use Tags for the images rather than forcing file paths, but unfortunatly PNG doesn't actually support it.

5. The Sets folder is used for folders of outfits that have similar asthetics such as "Beezys School Uniforms" and want people to follow a theme if choosen and hopefully don't conflict with each other.

	5A. Beware of simple color varients in sets such as NekoMaids (the skirts in particular), for example (hope it gets patched), as they will conflict with each other.
	
6. Anything that is in the parent folder of \Sets will be coordinating with itself rather than with those that share the folder unlike the Sets folder.

## Known issues
1. Reloading a Save will cause this to break for characters already loaded. Going back to Title will fix this.
	
	Example: start day, immediate go home, and load same save.

	Cause: Characters probably stored in memory
	
	Potential solution: Force reload characters in memory

2. KPlug seems to affect only the main heroine and any who invited or joined will load with default.
	
	Cause:Kplug load method

	Potential solution: Unavailable without further modifying Kplug code

3. Weird accessories load with characters, I think i've seen 3 consistent types
	
	Cause: Unknown

	Potential solution: Unknown

4. Character's with coordinate based hair

	Cause: ...

	Potential solution: look-a-like hair with nothing head acessory; potentially making a classroom menu overrride on loading without accessories hence potentially breaking some outfits instead.;

5. Outfits not loading on starting Freeplay or Maker; can be swapped to with custom preset.
	
	Cause: Probably character loading first

	Potential solution: Forced reload

6. Expanded outfit isn't supported atm so accessories above 20 won't probably load nor be replaced


7. Main NPC's in post intial cutscenes don't wear the clothing generated.

	Cause: character already loaded; allowing to run as is will mismatch the outfit between both cutscene and in-game model.

	potential solution: none without creating another array to store existing character outfits; Possible to do using just ints to avoid a significantly larger memory footprint.

## Kplug fix

This is a simple fix I made for Kplug 2.5 public to compensate for those who are bothered by the mandatory file structure of this mod and also happen to have Kplug enabled and don't want to have to have copies of files in the folder.

This fix doesn't apply to this mod itself but rather enable the use of KK_Browser files that is installed with HFPatch.

The fix won't apply the outfits generated by this mod to invited girls or girls who joined themselves, but will allow them to be affected by cosplay mode as Kplug 2.5 doesn't support subfolders. Granted the reason was me... since I enabled the KK_Browser Preset browser support. Don't blame Katarsys for that.

Currently if you were to apply an outfit using KK_Browsers to the main girl she would stop wearing the outfit when changing. 

Use at your own risk

Open Kplug DLL with DNSpy; this step is required to compile using DNSpy

DNSpy: File->open->select all files in K***_Data/Managed/

Select Kplug.CmpBase->ClothesCtrl

Right click edit class

before the namespace add , I'll recommend at line 13, add

	using System.IO;

inside "public class ClothesCtrl : MonoBehaviour", I'll recommend at line 703, add

	private static string Finder(string filename)
			{
				List<string> list = new List<string>();
				string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
				coordinatepath += "coordinate";
				list.Add(coordinatepath);
				string[] folders = Directory.GetDirectories(coordinatepath, "*", SearchOption.AllDirectories);
				list.AddRange(folders);
				foreach (string path in list)
				{
					string[] files = Directory.GetFiles(path);
					for (int i = 0; i < files.Length; i++)
					{
						if (files[i].EndsWith(filename))
						{
							return files[i];
						}
					}
				}
				return UserData.Path + "coordinate/" + filename;
			}

Go to line 339 and replace it with
	
	string text = ClothesCtrl.Finder(this.currentCostume);





