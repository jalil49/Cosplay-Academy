using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using System.Collections.Generic;
using System.IO;
namespace Cosplay_Academy
{
    [BepInPlugin(Guid, "Cosplay Academy", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class ExpandedOutfit : BaseUnityPlugin
    {
        public const string Guid = "Cosplay_Academy";
        public const string Version = Versions.Version;

        internal static new ManualLogSource Logger { get; private set; }

        public static ConfigEntry<bool> EnableSetting { get; private set; }
        public static ConfigEntry<bool> EnableSets { get; private set; }
        public static ConfigEntry<bool> IndividualSets { get; private set; }
        public static ConfigEntry<bool> EnableDefaults { get; private set; }
        public static ConfigEntry<bool> SumRandom { get; private set; }
        public static ConfigEntry<bool> StoryModeChange { get; private set; }
        public static ConfigEntry<bool> KeepOldBehavior { get; private set; }
        public static ConfigEntry<bool> NonMatchWeight { get; private set; }


        public static ConfigEntry<bool> MatchUniform { get; private set; }
        public static ConfigEntry<bool> AfterUniform { get; private set; }
        public static ConfigEntry<bool> MatchGym { get; private set; }
        public static ConfigEntry<bool> MatchSwim { get; private set; }
        public static ConfigEntry<bool> MatchSwimClub { get; private set; }
        public static ConfigEntry<bool> MatchMangaClub { get; private set; }
        public static ConfigEntry<bool> MatchTeaClub { get; private set; }
        public static ConfigEntry<bool> MatchTrackClub { get; private set; }
        public static ConfigEntry<bool> MatchCheerClub { get; private set; }
        public static ConfigEntry<bool> MatchKoiClub { get; private set; }
        public static ConfigEntry<bool> MatchCasual { get; private set; }
        public static ConfigEntry<bool> MatchNightwear { get; private set; }

        public static ConfigEntry<bool> HairMatch { get; private set; }

        public static ConfigEntry<bool> GrabUniform { get; private set; }
        public static ConfigEntry<bool> GrabSwimsuits { get; private set; }
        public static ConfigEntry<bool> SundayDate { get; private set; }
        public static ConfigEntry<bool> Makerview { get; private set; }
        public static ConfigEntry<bool> FullSet { get; private set; }
        public static ConfigEntry<bool> KoiClub { get; private set; }
        public static ConfigEntry<bool> ResetMaker { get; set; }
        public static ConfigEntry<bool> PermReset { get; set; }
        public static ConfigEntry<bool> ChangeOutfit { get; set; }
        public static ConfigEntry<bool> PermChangeOutfit { get; set; }
        public static ConfigEntry<int> KoiChance { get; private set; }
        public static ConfigEntry<int> AfterSchoolcasualchance { get; private set; }


        public static ConfigEntry<bool> AccKeeper { get; private set; }
        public static ConfigEntry<bool> ExtremeAccKeeper { get; private set; }


        public static ConfigEntry<bool> AfterSchoolCasual { get; private set; }
        public static ConfigEntry<bool> ChangeToClubatKoi { get; private set; }

        public static ConfigEntry<OutfitUpdate> UpdateFrequency { get; private set; }
        public static ConfigEntry<HStates> MakerHstate { get; private set; }
        public static ConfigEntry<Club> ClubChoice { get; private set; }
        public static ConfigEntry<string>[] ListOverride { get; private set; } = new ConfigEntry<string>[Constants.InputStrings.Length];
        public static ConfigEntry<bool>[] ListOverrideBool { get; private set; } = new ConfigEntry<bool>[Constants.InputStrings.Length];

        public void Awake()
        {
            Logger = base.Logger;
            Hooks.Init();
            //Hooks.CharaFinallyFinished += HairAccessory.Attempt;
            EnableSetting = Config.Bind("Main Game", "Enable Cosplay Academy", true, "Doesn't require Restart\nDoesn't Disable On Coordinate Load Support or Force Hair Color");

            GameAPI.RegisterExtraBehaviour<GameEvent>("Cosplay Academy");
            CharacterApi.RegisterExtraBehaviour<CharaEvent>("Cosplay Academy: Chara");
            MakerAPI.MakerExiting += MakerAPI_Clear;
            MakerAPI.MakerStartedLoading += MakerAPI_Clear;
            UpdateFrequency = Config.Bind("Main Game", "Update Frequency", OutfitUpdate.Daily);
            EnableDefaults = Config.Bind("Main Game", "Enable Default in rolls", true, "Adds default outfit to roll tables");
            SumRandom = Config.Bind("Main Game", "Use Sum random", false, "Tables are added together and drawn from based on experience. This probably makes lewd outfits rarer. \nDefault based on Random with a cap of heroine experience lewd rolls are guaranteed if heroine lands on lewd roll.");
            AccKeeper = Config.Bind("Main Game", "On Coordinate Load Support", true, "Keep head and tail accessories\nUsed for characters who have accessory based hair and avoid them going bald");
            ExtremeAccKeeper = Config.Bind("Main Game", "KEEP ALL ACCESSORIES", false, "Keep all accessories a character starts with\nUsed for Characters whos bodies require accessories such as amputee types\nNot Recommended for use with characters wth unnecessary accessories");
            HairMatch = Config.Bind("Main Game", "Force Hair Color on accessories", false, "Match items with Custom Hair Component to Character's Hair Color.");
            NonMatchWeight = Config.Bind("Main Game", "Non-Set weight Adjustment", true, "When outfit is not part of a set, give equal weight to a full set.\nIf this is disabled and you have one set folder the chance would be 50% of not being a set item if there are 9 items not in the set the set will have a 10% chance");
            StoryModeChange = Config.Bind("Story Mode", "KoiKatsu Outfit Change", false, "Experimental: probably has a performance impact when reloading the character when they enter/leave the club\nKoikatsu Club Members will change when entering the club room and have a chance of not changing depending on experience and lewdness");
            KeepOldBehavior = Config.Bind("Story Mode", "Koikatsu Probability behaviour", true, "Old Behavior: Koikatsu Club Members have a chance (Probabilty slider) of spawning with a koikatsu outfit rather than reloading");
            ChangeToClubatKoi = Config.Bind("Story Mode", "Change at Koikatsu Start", false, "Change Heroine to club outfit when they start in Koikatsu room");
            //Sets
            EnableSets = Config.Bind("Outfit Sets", "Enable Outfit Sets", true, "Outfits in set folders can be pulled from a group for themed sets");
            IndividualSets = Config.Bind("Outfit Sets", "Individual Outfit Sets", false, "Don't look for other sets that are shared");
            FullSet = Config.Bind("Outfit Sets", "Assign available sets only", false, "Priortize sets in order: Uniform > Gym > Swim > Club > Casual > Nightwear\nDisabled priority reversed: example Nightwear set will overwrite all clothes if same folder is found");


            //match uniforms
            MatchUniform = Config.Bind("Match Outfit", "Coordinated Uniforms", true, "Everyone wears same uniform");
            AfterUniform = Config.Bind("Match Outfit", "Different Uniform for afterschool", false, "Everyone wears different uniform afterschool");
            GrabUniform = Config.Bind("Additional Outfit", "Grab Normal uniforms for afterschool", true, "50% chance of being overwritten by AfterSchool Casual");
            MatchGym = Config.Bind("Match Outfit", "Coordinated Gym Uniforms", true, "Everyone wears same uniform during Gym");
            MatchSwim = Config.Bind("Match Outfit", "Coordinated Swim class outfits", false, "Everyone wears same uniform during Swim Class");
            MatchSwimClub = Config.Bind("Match Outfit", "Coordinated Swim Club outfits", true, "Everyone wears same uniform during Swim Club");
            MatchCheerClub = Config.Bind("Match Outfit", "Coordinated Cheerleader Uniforms", true, "Everyone wears same uniform during Track & Field");
            MatchTrackClub = Config.Bind("Match Outfit", "Coordinated Track & Field Uniforms", true, "Everyone wears same uniform during Track & Field");
            MatchMangaClub = Config.Bind("Match Outfit", "Coordinated Manga Cosplay", false, "Everyone wears same uniform during clubs");
            MatchTeaClub = Config.Bind("Match Outfit", "Coordinated Tea Ceremony Uniforms", false, "Everyone wears same uniform during clubs");
            MatchKoiClub = Config.Bind("Match Outfit", "Coordinated Koikatsu Uniforms", false, "Everyone wears same uniform during clubs");
            MatchCasual = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, "It's an option");
            MatchNightwear = Config.Bind("Match Outfit", "Coordinated Nightwear", false, "It's an option");
            GrabSwimsuits = Config.Bind("Additional Outfit", "Grab Swimsuits for Swimclub", true);
            //Probability
            KoiChance = Config.Bind("Probability", "Koikatsu outfit for club", 50, new ConfigDescription("Chance of wearing a koikatsu club outfit instead of normal club outfit", new AcceptableValueRange<int>(0, 100)));
            AfterSchoolcasualchance = Config.Bind("Probability", "Casual getup afterschool", 50, new ConfigDescription("Chance of wearing casual clothing after school", new AcceptableValueRange<int>(0, 100)));
            //Additional Outfits
            AfterSchoolCasual = Config.Bind("Additional Outfit", "After School Casual", true, "Everyone can be in casual wear after school");
            SundayDate = Config.Bind("Additional Outfit", "Sunday Date Special", true, "Date will wear something different on Sunday");
            //Maker
            Makerview = Config.Bind("Maker", "Enable maker view", false, "View in creator mode\ndoesn't load School Uniform upon entering maker from Main Menu swap uniform type to view");
            KoiClub = Config.Bind("Maker", "Is member of Koikatsu club", false, "Adds possibilty of choosing Koi outfit");
            MakerHstate = Config.Bind("Maker", "H state", HStates.FirstTime, "Maximum outfit category to roll");
            ClubChoice = Config.Bind("Maker", "Club choice", Club.HomeClub, "Affects club outfit in FreeH and cutscene non-heroine NPCs in story mode");
            ResetMaker = Config.Bind("Maker", "Randomize Sets", false, "Will overwrite current day outfit in storymode if you wanted to view that version.");
            PermReset = Config.Bind("Maker", "Keep Randomize Sets on", false, "Reset randomize Sets to disable once called");
            ChangeOutfit = Config.Bind("Maker", "Change generated outfit", false);
            PermChangeOutfit = Config.Bind("Maker", "Keep change generated outit on", false, "Reset change generated outfit to disable once called");
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName + @"coordinate";
            for (int i = 0; i < Constants.InputStrings.Length; i++)
            {
                ListOverride[i] = Config.Bind("Outfit Folder Override", Constants.InputStrings[i].Trim('\\').Replace('\\', ' '), coordinatepath + Constants.InputStrings[i], "Choose a particular folder you wish to see used, this will be prioritzed and treated as a set\nThere is no lewd experience suport here");
                ListOverrideBool[i] = Config.Bind("Outfit Folder Override", Constants.InputStrings[i].Trim('\\').Replace('\\', ' ') + " Enable override", false, "Enables the above folder override");
            }
        }

        private void MakerAPI_Clear(object sender, System.EventArgs e)
        {
            Constants.ChaDefaults.Clear();
            OutfitDecider.ResetDecider();
        }
    }
}