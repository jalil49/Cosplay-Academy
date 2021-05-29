using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using System;
using System.Collections;
using System.IO;
namespace Cosplay_Academy
{
    [BepInProcess("Koikatsu Sunshine")]
    [BepInPlugin(GUID, "Cosplay Academy: Sunshine Trial", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", BepInDependency.DependencyFlags.HardDependency)]
    public class Settings : BaseUnityPlugin
    {
        public const string GUID = "Cosplay_Academy";
        public const string Version = "0.1.0";
        public static Settings Instance;
        internal static new ManualLogSource Logger { get; private set; }

        public static ConfigEntry<bool> UseAlternativePath { get; private set; }
        public static ConfigEntry<string> AlternativePath { get; private set; }
        public static ConfigEntry<bool> EnableSetting { get; private set; }
        public static ConfigEntry<bool> EnableSets { get; private set; }
        public static ConfigEntry<bool> IndividualSets { get; private set; }
        public static ConfigEntry<bool> EnableDefaults { get; private set; }
        public static ConfigEntry<bool> KeepOldBehavior { get; private set; }


        public static ConfigEntry<bool> MatchSwim { get; private set; }
        public static ConfigEntry<bool> MatchCasual { get; private set; }
        public static ConfigEntry<bool> MatchNightwear { get; private set; }
        public static ConfigEntry<bool> MatchBathroom { get; private set; }
        public static ConfigEntry<bool> MatchUnderwear { get; private set; }

        public static ConfigEntry<bool> HairMatch { get; private set; }

        public static ConfigEntry<bool> SundayDate { get; private set; }
        public static ConfigEntry<bool> Makerview { get; private set; }
        public static ConfigEntry<bool> FullSet { get; private set; }
        public static ConfigEntry<bool> ResetMaker { get; set; }

        public static ConfigEntry<bool> ChangeOutfit { get; set; }

        public static ConfigEntry<int> KoiChance { get; private set; }
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

        public void Awake()
        {
            if (StudioAPI.InsideStudio)
            {
                return;
            }
            Instance = this;
            Logger = base.Logger;
            StartCoroutine(Wait());
            DirectoryFinder.CheckMissingFiles();
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

            CharacterApi.RegisterExtraBehaviour<CharaEvent>(GUID);

            //Accessories
            ExtremeAccKeeper = Config.Bind("Accessories", "KEEP ALL ACCESSORIES", false, "Keep all accessories a character starts with\nUsed for Characters whos bodies require accessories such as amputee types\nNot Recommended for use with characters wth unnecessary accessories");
            HairMatch = Config.Bind("Accessories", "Force Hair Color on accessories", false, "Match items with Custom Hair Component to Character's Hair Color.");

            //Main Game
            AccKeeper = Config.Bind("Main Game", "On Coordinate Load Support", true, "Keep head and tail accessories\nUsed for characters who have accessory based hair and avoid them going bald\nWorks best with a Cosplay Academy Ready character marked by Additional Card Info");
            RandomizeUnderwear = Config.Bind("Main Game", "Randomize Underwear", false, "Loads underwear from Underwear folder (Does not apply to Gym/Swim outfits)\nWill probably break some outfits that depends on underwear outside of Gym/Swim if not set up with Expanded Outfit plugin");
            RandomizeUnderwearOnly = Config.Bind("Main Game", "Randomize Underwear Only", false, "Its an option");
            EnableSetting = Config.Bind("Main Game", "Enable Cosplay Academy", true, "Doesn't require Restart\nDoesn't Disable On Coordinate Load Support or Force Hair Color");

            //NonMatchWeight = Config.Bind("Main Game", "Non-Set weight Adjustment", true, "When outfit is not part of a set, give equal weight to a full set.\nIf this is disabled and you have one set folder the chance would be 50% of not being a set item if there are 9 items not in the set the set will have a 10% chance");

            //StoryMode
            KeepOldBehavior = Config.Bind("Story Mode", "Koikatsu Probability behavior", true, "Old Behavior: Koikatsu Club Members have a chance (Probabilty slider) of spawning with a koikatsu outfit rather than reloading");
            SundayDate = Config.Bind("Story Mode", "Sunday Date Special", true, "Date will wear something different on Sunday");

            //Sets
            EnableSets = Config.Bind("Outfit Sets", "Enable Outfit Sets", true, "Outfits in set folders can be pulled from a group for themed sets");
            IndividualSets = Config.Bind("Outfit Sets", "Do not Find Matching Sets", false, "Don't look for other sets that are shared per coordinate type");
            FullSet = Config.Bind("Outfit Sets", "Assign available sets only", false, "Prioritize sets in order: Uniform > Gym > Swim > Club > Casual > Nightwear\nDisabled priority reversed: example Nightwear set will overwrite all clothes if same folder is found");

            //match uniforms
            MatchSwim = Config.Bind("Match Outfit", "Coordinated Swimsuit outfits", false, "Everyone wears the same swimsuit");
            MatchBathroom = Config.Bind("Match Outfit", "Coordinated Bathroom outfits", false, "Everyone wears same Bathroom outfit");
            MatchCasual = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, "It's an option");
            MatchNightwear = Config.Bind("Match Outfit", "Coordinated Nightwear", false, "It's an option");
            MatchUnderwear = Config.Bind("Match Outfit", "Coordinated Underwear", false, "It's an option");

            //Additional Outfit
            EnableDefaults = Config.Bind("Additional Outfits", "Enable Default in rolls", false, "Adds default outfit to roll tables");

            //Probability
            KoiChance = Config.Bind("Probability", "Koikatsu outfit for club", 50, new ConfigDescription("Chance of wearing a koikatsu club outfit instead of normal club outfit", new AcceptableValueRange<int>(0, 100)));
            H_EXP_Choice = Config.Bind("Probability", "Outfit Picker logic", Hexp.Randomize, "Randomize: Each outfit can be from different H States\nRandConstant: Randomizes H State, but will choose the same level if outfit is found (will get next highest if Enable Default is not enabled)\nMaximize: Do I really gotta say?");
            for (int i = 0; i < HStateWeights.Length; i++)
            {
                HStateWeights[i] = Config.Bind("Probability", $"Weight of {(HStates)i}", 50, new ConfigDescription($"Weight of {(HStates)i} category\nNot actually % chance", new AcceptableValueRange<int>(0, 100)));
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

            //Alternative path for other games
            AlternativePath = Config.Bind("Other Games", "KK or KKP UserData", new DirectoryInfo(UserData.Path).FullName.ToString(), "UserData Path of KK or KKP");
            UseAlternativePath = Config.Bind("Other Games", "Pull outfits from KK or KKP", false, "Use applicable outfits from Sunshine");

            MakerAPI.RegisterCustomSubCategories += CharaEvent.RegisterCustomSubCategories;
            MakerAPI.MakerExiting += (s, e) => CharaEvent.MakerAPI_MakerExiting();
        }
    }
}