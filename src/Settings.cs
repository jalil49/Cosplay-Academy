using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
namespace Cosplay_Academy
{
    [BepInPlugin(Guid, "Cosplay Academy", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class ExpandedOutfit : BaseUnityPlugin
    {
        public const string Guid = Versions.Guid;
        public const string Version = Versions.Version;

        internal static new ManualLogSource Logger { get; private set; }

        public static ConfigEntry<bool> EnableSetting { get; private set; }
        public static ConfigEntry<bool> EnableSets { get; private set; }
        public static ConfigEntry<bool> EnableDefaults { get; private set; }
        public static ConfigEntry<bool> SumRandom { get; private set; }


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


        public static ConfigEntry<bool> AfterSchoolCasual { get; private set; }

        public static ConfigEntry<OutfitUpdate> UpdateFrequency { get; private set; }
        public static ConfigEntry<HStates> MakerHstate { get; private set; }
        public static ConfigEntry<Club> ClubChoice { get; private set; }

        public void Awake()
        {
            Logger = base.Logger;
            EnableSetting = Config.Bind("Main game", "Enable Cosplay Academy", true, "unknown");
            if (EnableSetting != null && EnableSetting.Value)
            {
                GameAPI.RegisterExtraBehaviour<GameEvent>("Cosplay Academy");
                CharacterApi.RegisterExtraBehaviour<CharaEvent>("Cosplay Academy: Chara");
            }

            UpdateFrequency = Config.Bind("Main game", "Update Frequency", OutfitUpdate.Daily);
            EnableDefaults = Config.Bind("Main game", "Enable Default in rolls", true, "Adds default outfit to roll tables");
            SumRandom = Config.Bind("Main game", "Use Sum random", false, "Tables are added together and drawn from based on experience. This probably makes lewd outfits rarer. \n Default based on Random with a cap of heroine experience lewd rolls are guaranteed if heroine lands on lewd roll.");
            //Sets
            EnableSets = Config.Bind("Outfit Sets", "Enable Outfit Sets", true, "Choose from same set when available");
            FullSet = Config.Bind("Outfit Sets", "Assign available sets only", false, "Priortize sets in order: Uniform > Gym > Swim > Club > Casual > Nightwear\nDisabled priorty reversed: example Nightwear set will overwrite all clothes if same folder is found");

            //match uniforms
            MatchUniform = Config.Bind("Match Outfit", "Coordinated Uniforms", true, "Everyone wears same uniform");
            AfterUniform = Config.Bind("Match Outfit", "Different Uniform for afterschool", false, "Everyone wears different uniform afterschool");
            GrabUniform = Config.Bind("Additional Outfit", "Grab Normal uniforms for afterschool", true, "50% chance of being verwritten by AfterSchool Casual");
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
            ClubChoice = Config.Bind("Maker", "Club choice", Club.HomeClub);
            ResetMaker = Config.Bind("Maker", "Randomize Sets", false, "Will overwrite current day outfit in storymode if you wanted to view that version.");
            PermReset = Config.Bind("Maker", "Keep Randomize Sets on", false, "Reset randomize Sets to disable once called");
            ChangeOutfit = Config.Bind("Maker", "Change generated outfit", false);
            PermChangeOutfit = Config.Bind("Maker", "Keep change generated outit on", false, "Reset change generated outfit to disable once called");
        }
    }
}