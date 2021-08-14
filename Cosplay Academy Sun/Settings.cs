using BepInEx;
using BepInEx.Configuration;
using KKAPI;
using KKAPI.Studio;

namespace Cosplay_Academy
{
    [BepInProcess("KoikatsuSunshineTrial")]
    [BepInPlugin(GUID, "Cosplay Academy", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class Settings : BaseUnityPlugin
    {
        public void Awake()
        {
            if (StudioAPI.InsideStudio)
            {
                return;
            }

            StandardSettings();

            //match uniforms
            MatchGeneric[0] = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { Order = 4, IsAdvanced = true }));
            MatchGeneric[1] = Config.Bind("Match Outfit", "Coordinated Swimsuit outfits", false, new ConfigDescription("Everyone wears the same swimsuit", null, new ConfigurationManagerAttributes { Order = 3 }));
            MatchGeneric[2] = Config.Bind("Match Outfit", "Coordinated Nightwear", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { Order = 2, IsAdvanced = true }));
            MatchGeneric[3] = Config.Bind("Match Outfit", "Coordinated Bathroom outfits", false, new ConfigDescription("Everyone wears same Bathroom outfit", null, new ConfigurationManagerAttributes { Order = 1 }));
            MatchGeneric[4] = Config.Bind("Match Outfit", "Coordinated Underwear", false, new ConfigDescription("It's an option", null, new ConfigurationManagerAttributes { Order = 0, IsAdvanced = true }));
        }
    }
}