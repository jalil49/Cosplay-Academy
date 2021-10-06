using BepInEx;
using BepInEx.Configuration;
using KKAPI.MainGame;
using KKAPI.Studio;
namespace Cosplay_Academy
{
    [BepInProcess("Koikatu")]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party")]
    [BepInProcess("Koikatsu Party VR")]
    public partial class Settings : BaseUnityPlugin
    {
        public static ConfigEntry<bool> TeacherDress { get; private set; }

        public static ConfigEntry<bool> GrabUniform { get; private set; }
        public static ConfigEntry<bool> KoiClub { get; private set; }

        public static ConfigEntry<int> KoiChance { get; private set; }
        public static ConfigEntry<int> AfterSchoolcasualchance { get; private set; }

        public static ConfigEntry<bool> AfterSchoolCasual { get; private set; }
        public static ConfigEntry<bool> ChangeToClubatKoi { get; private set; }
        public static ConfigEntry<Club> ClubChoice { get; private set; }

        public void Awake()
        {
            if (StudioAPI.InsideStudio)
            {
                return;
            }
            Hooks.Init();

            StandardSettings();

            var AdvancedConfig = new ConfigurationManagerAttributes { IsAdvanced = true };

            //StoryMode
            StoryModeChange = Config.Bind("Story Mode", "Koikatsu Outfit Change", false, "Experimental: probably has a performance impact when reloading the character when they enter/leave the club\nKoikatsu Club Members will change when entering the club room and have a chance of not changing depending on experience and lewdness");
            KeepOldBehavior = Config.Bind("Story Mode", "Koikatsu Probability behavior", true, "Old Behavior: Koikatsu Club Members have a chance (Probabilty slider) of spawning with a koikatsu outfit rather than reloading");
            ChangeToClubatKoi = Config.Bind("Story Mode", "Change at Koikatsu Start", false, "Change Heroine to club outfit when they start in Koikatsu room");
            TeacherDress = Config.Bind("Story Mode", "Teachers dress up", true, new ConfigDescription("Teachers probably would like to dress up if everyone does it.", null, AdvancedConfig));

            //match uniforms
            MatchGeneric[0] = Config.Bind("Match Outfit", "Coordinated Uniforms", true, new ConfigDescription("Everyone wears same uniform", null, new ConfigurationManagerAttributes { Order = 12 }));
            MatchGeneric[2] = Config.Bind("Match Outfit", "Coordinated Gym Uniforms", true, new ConfigDescription("Everyone wears same uniform during Gym", null, new ConfigurationManagerAttributes { Order = 10 }));
            MatchGeneric[3] = Config.Bind("Match Outfit", "Coordinated Swim class outfits", false, new ConfigDescription("Everyone wears same uniform during Swim Class", null, new ConfigurationManagerAttributes { Order = 9 }));
            MatchGeneric[4] = Config.Bind("Match Outfit", "Coordinated Swim Club outfits", true, new ConfigDescription("Everyone wears same uniform during Swim Club", null, new ConfigurationManagerAttributes { Order = 8 }));
            MatchGeneric[5] = Config.Bind("Match Outfit", "Coordinated Cheerleader Uniforms", true, new ConfigDescription("Everyone wears same uniform during Cheerleading", null, new ConfigurationManagerAttributes { Order = 7 }));
            MatchGeneric[6] = Config.Bind("Match Outfit", "Coordinated Track & Field Uniforms", true, new ConfigDescription("Everyone wears same uniform during Track & Field", null, new ConfigurationManagerAttributes { Order = 6 }));
            MatchGeneric[7] = Config.Bind("Match Outfit", "Coordinated Manga Cosplay", false, new ConfigDescription("Everyone wears same uniform during Manga club", null, new ConfigurationManagerAttributes { Order = 5 }));
            MatchGeneric[8] = Config.Bind("Match Outfit", "Coordinated Tea Ceremony Uniforms", false, new ConfigDescription("Everyone wears same uniform during Tea Ceremony club", null, new ConfigurationManagerAttributes { Order = 4 }));
            MatchGeneric[9] = Config.Bind("Match Outfit", "Coordinated Koikatsu Uniforms", false, new ConfigDescription("Everyone wears same uniform during Koikatsu club", null, new ConfigurationManagerAttributes { Order = 3 }));
            MatchGeneric[10] = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            MatchGeneric[11] = Config.Bind("Match Outfit", "Coordinated Nightwear", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
            MatchGeneric[12] = Config.Bind("Match Outfit", "Coordinated Underwear", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 0 }));

            //Additional Outfit
            GrabSwimsuits = Config.Bind("Additional Outfits", "Grab Swimsuits for Swim club", true, new ConfigDescription("", null, AdvancedConfig));
            GrabUniform = Config.Bind("Additional Outfits", "Grab Normal uniforms for afterschool", true, new ConfigDescription("", null, AdvancedConfig));
            AfterSchoolCasual = Config.Bind("Additional Outfits", "After School Casual", true, new ConfigDescription("Everyone can be in casual wear after school", null));
            MatchGeneric[1] = Config.Bind("Additional Outfits", "Different Uniform for afterschool", false, new ConfigDescription("Everyone wears different uniform afterschool", null, AdvancedConfig));

            //Probability
            KoiChance = Config.Bind("Probability", "Koikatsu outfit for club", 50, new ConfigDescription("Chance of wearing a koikatsu club outfit instead of normal club outfit", new AcceptableValueRange<int>(0, 100)));
            AfterSchoolcasualchance = Config.Bind("Probability", "Casual getup afterschool", 50, new ConfigDescription("Chance of wearing casual clothing after school", new AcceptableValueRange<int>(0, 100)));

            //Maker
            KoiClub = Config.Bind("Maker", "Is member of Koikatsu club", false, new ConfigDescription("Adds possibilty of choosing Koi outfit"));
            ClubChoice = Config.Bind("Maker", "Club choice", Club.HomeClub, new ConfigDescription("Affects club outfit in FreeH and cutscene non-heroine NPCs in story mode"));
        }
    }
}