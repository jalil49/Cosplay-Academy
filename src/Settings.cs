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
        public const string Guid = Constants.Guid;
        public const string Version = Constants.Version;

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


        public static ConfigEntry<bool> AfterSchoolCasual { get; private set; }

        public static ConfigEntry<OutfitUpdate> UpdateFrequency { get; private set; }

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
            EnableSets = Config.Bind("Main game", "Enable Outfit Sets", true, "Choose from same set when available");
            EnableDefaults = Config.Bind("Main game", "Enable Default in rolls", true, "Adds default outfit to roll tables");
            SumRandom = Config.Bind("Main game", "Use Sum random", false, "Tables are added together and drawn from based on experience. This probably makes lewd outfits rarer. \n Default based on Random with a cap of heroine experience lewd rolls are guaranteed if heroine lands on lewd roll.");
            FullSet = Config.Bind("Main game", "Assign available sets", false, "Example:if uniforms and swimsuit share sets apply same set to swimsuit (one way write).");

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

            //
            AfterSchoolCasual = Config.Bind("Additional Outfit", "After School Casual", true, "Everyone can be in casual wear after school");
            SundayDate = Config.Bind("Additional Outfit", "Sunday Date Special", true, "Date will wear something different on Sunday");
            //Coordination helper
            //Makerview = Config.Bind("Compatibility Check", "Enable maker view", false, "Enable to view in maker");
        }
    }
}