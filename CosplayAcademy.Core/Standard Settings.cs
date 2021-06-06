using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections;
using System.IO;
namespace Cosplay_Academy
{
    public partial class Settings : BaseUnityPlugin
    {
        public const string GUID = "Cosplay_Academy";
        public const string Version = "0.8.3";
        public static Settings Instance;
        internal static new ManualLogSource Logger { get; private set; }


        public static ConfigEntry<bool>[] MatchGeneric = new ConfigEntry<bool>[Constants.InputStrings.Length];

        public static ConfigEntry<bool> UseAlternativePath { get; private set; }
        public static ConfigEntry<string> AlternativePath { get; private set; }
        public static ConfigEntry<bool> EnableSetting { get; private set; }
        public static ConfigEntry<bool> EnableSets { get; private set; }
        public static ConfigEntry<bool> IndividualSets { get; private set; }
        public static ConfigEntry<bool> EnableDefaults { get; private set; }
        public static ConfigEntry<bool> StoryModeChange { get; private set; }
        public static ConfigEntry<bool> KeepOldBehavior { get; private set; }

        public static ConfigEntry<bool> HairMatch { get; private set; }

        public static ConfigEntry<bool> Makerview { get; private set; }
        public static ConfigEntry<bool> FullSet { get; private set; }
        public static ConfigEntry<bool> ResetMaker { get; set; }

        public static ConfigEntry<bool> ChangeOutfit { get; set; }

        public static ConfigEntry<int>[] HStateWeights { get; private set; } = new ConfigEntry<int>[Enum.GetValues(typeof(HStates)).Length];
        public static ConfigEntry<Hexp> H_EXP_Choice { get; private set; }


        public static ConfigEntry<bool> AccKeeper { get; private set; }
        public static ConfigEntry<bool> RandomizeUnderwear { get; private set; }
        public static ConfigEntry<bool> RandomizeUnderwearOnly { get; private set; }
        public static ConfigEntry<bool> UnderwearStates { get; private set; }
        public static ConfigEntry<bool> ExtremeAccKeeper { get; private set; }

        public static ConfigEntry<HStates> MakerHstate { get; private set; }

        public static ConfigEntry<string>[] ListOverride { get; private set; } = new ConfigEntry<string>[Constants.InputStrings.Length];
        public static ConfigEntry<bool>[] ListOverrideBool { get; private set; } = new ConfigEntry<bool>[Constants.InputStrings.Length];

        internal void StandardSettings()
        {
            Instance = this;
            Logger = base.Logger;
            DirectoryFinder.CheckMissingFiles();
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                Constants.PluginCheck();
                if (!Constants.PluginResults["Additional_Card_Info"]) //provide access to info even if plugin-doesn't exist
                {
                    CharacterApi.RegisterExtraBehaviour<Dummy>("Additional_Card_Info");
                }
                yield return null;
                DirectoryFinder.Organize();
            }
            Constants.ExpandedOutfit();
            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID, 900);

            //Accessories
            ExtremeAccKeeper = Config.Bind("Accessories", "KEEP ALL ACCESSORIES", false, "Keep all accessories a character starts with\nUsed for Characters whos bodies require accessories such as amputee types\nNot Recommended for use with characters wth unnecessary accessories");
            HairMatch = Config.Bind("Accessories", "Force Hair Color on accessories", false, "Match items with Custom Hair Component to Character's Hair Color.");

            //Main Game
            AccKeeper = Config.Bind("Main Game", "On Coordinate Load Support", true, "Keep head and tail accessories\nUsed for characters who have accessory based hair and avoid them going bald\nWorks best with a Cosplay Academy Ready character marked by Additional Card Info");
            RandomizeUnderwear = Config.Bind("Main Game", "Randomize Underwear", false, "Loads underwear from Underwear folder (Does not apply to Gym/Swim outfits)\nWill probably break some outfits that depends on underwear outside of Gym/Swim if not set up with Expanded Outfit plugin");
            RandomizeUnderwearOnly = Config.Bind("Main Game", "Randomize Underwear Only", false, "Its an option");
            EnableSetting = Config.Bind("Main Game", "Enable Cosplay Academy", true, "Doesn't require Restart\nDoesn't Disable On Coordinate Load Support or Force Hair Color");

            //StoryMode
            StoryModeChange = Config.Bind("Story Mode", "Koikatsu Outfit Change", false, "Experimental: probably has a performance impact when reloading the character when they enter/leave the club\nKoikatsu Club Members will change when entering the club room and have a chance of not changing depending on experience and lewdness");
            KeepOldBehavior = Config.Bind("Story Mode", "Koikatsu Probability behavior", true, "Old Behavior: Koikatsu Club Members have a chance (Probabilty slider) of spawning with a koikatsu outfit rather than reloading");

            //Sets
            EnableSets = Config.Bind("Outfit Sets", "Enable Outfit Sets", true, "Outfits in set folders can be pulled from a group for themed sets");
            IndividualSets = Config.Bind("Outfit Sets", "Do not Find Matching Sets", false, "Don't look for other sets that are shared per coordinate type");
            FullSet = Config.Bind("Outfit Sets", "Assign available sets only", false, "Prioritize sets in order: Uniform > Gym > Swim > Club > Casual > Nightwear\nDisabled priority reversed: example Nightwear set will overwrite all clothes if same folder is found");

            //Additional Outfit
            EnableDefaults = Config.Bind("Additional Outfits", "Enable Default in rolls", false, "Adds default outfit to roll tables");

            //prob
            H_EXP_Choice = Config.Bind("Probability", "Outfit Picker logic", Hexp.Randomize, "Randomize: Each outfit can be from different H States\nRandConstant: Randomizes H State, but will choose the same level if outfit is found (will get next highest if Enable Default is not enabled)\nMaximize: Do I really gotta say?");
            for (int i = 0; i < HStateWeights.Length; i++)
            {
                HStateWeights[i] = Config.Bind("Probability", $"Weight of {(HStates)i}", 50, new ConfigDescription($"Weight of {(HStates)i} category\nNot actually % chance", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = false }));
            }

            //Maker
            Makerview = Config.Bind("Maker", "Enable Maker Mode", false);
            MakerHstate = Config.Bind("Maker", "H state", HStates.FirstTime, "Maximum outfit category to roll");
            ResetMaker = Config.Bind("Maker", "Reset Sets", false, "Will overwrite current day outfit in storymode if you wanted to view that version.");
            ChangeOutfit = Config.Bind("Maker", "Change generated outfit", false, "Pick new coordinates in maker");

            //Other Mods
            UnderwearStates = Config.Bind("Other Mods", "Randomize Underwear: ACC_States", true, "Attempts to write logic for AccStateSync and Accessory states to use.");

            //Overrides
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName + @"coordinate";
            for (int i = 0; i < ListOverride.Length; i++)
            {
                ListOverride[i] = Config.Bind("Outfit Folder Override", Constants.InputStrings[i].Trim('\\').Replace('\\', ' '), coordinatepath + Constants.InputStrings[i], "Choose a particular folder you wish to see used, this will be prioritzed and treated as a set\nThere is no lewd experience suport here");
                ListOverrideBool[i] = Config.Bind("Outfit Folder Override", Constants.InputStrings[i].Trim('\\').Replace('\\', ' ') + " Enable override", false, "Enables the above folder override");
            }

            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;
            MakerAPI.MakerExiting += (s, e) => CharaEvent.MakerAPI_MakerExiting();
        }

        private void AlternativePath_SettingChanged(object sender, EventArgs e)
        {
            if (!AlternativePath.Value.EndsWith(@"\"))
            {
                AlternativePath.Value += '\\';
            }
        }
    }
}