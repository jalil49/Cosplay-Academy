using BepInEx;
using KKAPI;
using KKAPI.Studio;
using System.IO;
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
            MatchGeneric[0] = Config.Bind("Match Outfit", "Coordinated Casual Outfits", false, "It's an option");
            MatchGeneric[1] = Config.Bind("Match Outfit", "Coordinated Swimsuit outfits", false, "Everyone wears the same swimsuit");
            MatchGeneric[2] = Config.Bind("Match Outfit", "Coordinated Nightwear", false, "It's an option");
            MatchGeneric[3] = Config.Bind("Match Outfit", "Coordinated Bathroom outfits", false, "Everyone wears same Bathroom outfit");
            MatchGeneric[4] = Config.Bind("Match Outfit", "Coordinated Underwear", false, "It's an option");

            //Alternative path for other games
            AlternativePath = Config.Bind("Other Games", "KK or KKP UserData", new DirectoryInfo(UserData.Path).FullName.ToString(), "UserData Path of KK or KKP");
            UseAlternativePath = Config.Bind("Other Games", "Pull outfits from KK or KKP", false, "Use applicable outfits from Sunshine");
            AlternativePath.SettingChanged += AlternativePath_SettingChanged;
        }
    }
}