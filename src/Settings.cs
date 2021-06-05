﻿using BepInEx;
using BepInEx.Configuration;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using System;
using System.Collections;
using System.IO;
namespace Cosplay_Academy
{
    [BepInPlugin(GUID, "Cosplay Academy", Version)]
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", BepInDependency.DependencyFlags.HardDependency)]
    public partial class Settings : BaseUnityPlugin
    {
        public static ConfigEntry<bool> TeacherDress { get; private set; }

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
        public static ConfigEntry<bool> MatchUnderwear { get; private set; }

        public static ConfigEntry<bool> GrabUniform { get; private set; }
        public static ConfigEntry<bool> GrabSwimsuits { get; private set; }
        public static ConfigEntry<bool> SundayDate { get; private set; }
        public static ConfigEntry<bool> KoiClub { get; private set; }

        public static ConfigEntry<int> KoiChance { get; private set; }
        public static ConfigEntry<int> AfterSchoolcasualchance { get; private set; }

        public static ConfigEntry<bool> AfterSchoolCasual { get; private set; }
        public static ConfigEntry<bool> ChangeToClubatKoi { get; private set; }

        public static ConfigEntry<OutfitUpdate> UpdateFrequency { get; private set; }
        public static ConfigEntry<Club> ClubChoice { get; private set; }
        public void Awake()
        {
            if (StudioAPI.InsideStudio)
            {
                return;
            }
            Instance = this;
            Logger = base.Logger;
            Hooks.Init();
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
            GameAPI.RegisterExtraBehaviour<GameEvent>(GUID);

            //Main Game
            EnableSetting = Config.Bind("Main Game", "Enable Cosplay Academy", true, "Doesn't require Restart\nDoesn't Disable On Coordinate Load Support or Force Hair Color");

            //StoryMode
            StoryModeChange = Config.Bind("Story Mode", "Koikatsu Outfit Change", false, "Experimental: probably has a performance impact when reloading the character when they enter/leave the club\nKoikatsu Club Members will change when entering the club room and have a chance of not changing depending on experience and lewdness");
            KeepOldBehavior = Config.Bind("Story Mode", "Koikatsu Probability behavior", true, "Old Behavior: Koikatsu Club Members have a chance (Probabilty slider) of spawning with a koikatsu outfit rather than reloading");
            ChangeToClubatKoi = Config.Bind("Story Mode", "Change at Koikatsu Start", false, "Change Heroine to club outfit when they start in Koikatsu room");
            TeacherDress = Config.Bind("Story Mode", "Teachers dress up", true, "Teachers probably would like to dress up if everyone does it.");
            UpdateFrequency = Config.Bind("Story Mode", "Update Frequency", OutfitUpdate.Daily);
            SundayDate = Config.Bind("Story Mode", "Sunday Date Special", true, "Date will wear something different on Sunday");

            //Sets
            EnableSets = Config.Bind("Outfit Sets", "Enable Outfit Sets", true, "Outfits in set folders can be pulled from a group for themed sets");
            IndividualSets = Config.Bind("Outfit Sets", "Do not Find Matching Sets", false, "Don't look for other sets that are shared per coordinate type");
            FullSet = Config.Bind("Outfit Sets", "Assign available sets only", false, "Prioritize sets in order: Uniform > Gym > Swim > Club > Casual > Nightwear\nDisabled priority reversed: example Nightwear set will overwrite all clothes if same folder is found");

            //match uniforms
            MatchUniform = Config.Bind("Match Outfit", "Coordinated Uniforms", true, "Everyone wears same uniform");
            AfterUniform = Config.Bind("Match Outfit", "Different Uniform for afterschool", false, "Everyone wears different uniform afterschool");
            MatchGym = Config.Bind("Match Outfit", "Coordinated Gym Uniforms", true, "Everyone wears same uniform during Gym");
            MatchSwim = Config.Bind("Match Outfit", "Coordinated Swim class outfits", false, "Everyone wears same uniform during Swim Class");
            MatchSwimClub = Config.Bind("Match Outfit", "Coordinated Swim Club outfits", true, "Everyone wears same uniform during Swim Club");
            MatchCheerClub = Config.Bind("Match Outfit", "Coordinated Cheerleader Uniforms", true, "Everyone wears same uniform during Cheerleading");
            MatchTrackClub = Config.Bind("Match Outfit", "Coordinated Track & Field Uniforms", true, "Everyone wears same uniform during Track & Field");
            MatchMangaClub = Config.Bind("Match Outfit", "Coordinated Manga Cosplay", false, "Everyone wears same uniform during clubs");
            MatchTeaClub = Config.Bind("Match Outfit", "Coordinated Tea Ceremony Uniforms", false, "Everyone wears same uniform during clubs");
            MatchKoiClub = Config.Bind("Match Outfit", "Coordinated Koikatsu Uniforms", false, "Everyone wears same uniform during clubs");
            MatchCasual = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, "It's an option");
            MatchNightwear = Config.Bind("Match Outfit", "Coordinated Nightwear", false, "It's an option");
            MatchUnderwear = Config.Bind("Match Outfit", "Coordinated Underwear", false, "It's an option");

            //Additional Outfit
            GrabSwimsuits = Config.Bind("Additional Outfits", "Grab Swimsuits for Swim club", true);
            GrabUniform = Config.Bind("Additional Outfits", "Grab Normal uniforms for afterschool", true, "Uses Casual getup afterschool");
            EnableDefaults = Config.Bind("Additional Outfits", "Enable Default in rolls", false, "Adds default outfit to roll tables");
            AfterSchoolCasual = Config.Bind("Additional Outfits", "After School Casual", true, "Everyone can be in casual wear after school");

            //Probability
            KoiChance = Config.Bind("Probability", "Koikatsu outfit for club", 50, new ConfigDescription("Chance of wearing a koikatsu club outfit instead of normal club outfit", new AcceptableValueRange<int>(0, 100)));
            AfterSchoolcasualchance = Config.Bind("Probability", "Casual getup afterschool", 50, new ConfigDescription("Chance of wearing casual clothing after school", new AcceptableValueRange<int>(0, 100)));

            //Maker
            Makerview = Config.Bind("Maker", "Enable Maker Mode", false);
            KoiClub = Config.Bind("Maker", "Is member of Koikatsu club", false, "Adds possibilty of choosing Koi outfit");
            MakerHstate = Config.Bind("Maker", "H state", HStates.FirstTime, "Maximum outfit category to roll");
            ClubChoice = Config.Bind("Maker", "Club choice", Club.HomeClub, "Affects club outfit in FreeH and cutscene non-heroine NPCs in story mode");
            ResetMaker = Config.Bind("Maker", "Reset Sets", false, "Will overwrite current day outfit in storymode if you wanted to view that version.");
            ChangeOutfit = Config.Bind("Maker", "Change generated outfit", false, "Pick new coordinates in maker");

            //Other Mods
            UnderwearStates = Config.Bind("Other Mods", "Randomize Underwear: ACC_States", true, "Attempts to write logic for AccStateSync and Accessory states to use.");

            //Alternative path for other games
            AlternativePath = Config.Bind("Other Games", "Sunshine UserData", new DirectoryInfo(UserData.Path).FullName.ToString(), "UserData Path of Sunshine");
            AlternativePath.SettingChanged += AlternativePath_SettingChanged;
            UseAlternativePath = Config.Bind("Other Games", "Pull outfits from Sunshine", false, "Use applicable outfits from Sunshine");
        }
    }
}