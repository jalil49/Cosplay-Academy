using BepInEx;
using System.Collections.Generic;
using Sideloader.AutoResolver;
#if KK
using System.ComponentModel;
#endif
namespace Cosplay_Academy
{
    static class Constants
    {
        internal static void PluginCheck()
        {
            foreach (var item in PluginList)
            {
                PluginResults[item] = TryfindPluginInstance(item);
            }
        }

        internal static void ExpandedOutfit()
        {
            for (int i = 0; i < InputStrings.Length; i++)
            {
                if (OutfitnumPairs.ContainsKey(i))
                {
                    continue;
                }
#if KK
                if (i == 4)
                {
                    OutfitnumPairs.Add(i, 6);
                    continue;
                }
#endif
                OutfitnumPairs.Add(i, 1);
            }

            foreach (var item in IgnoredTops_Original_A)
            {
                var intlist = new List<int>();
                foreach (var resolve in item.Value)
                {
                    var result = UniversalAutoResolver.TryGetResolutionInfo(resolve.Slot, resolve.CategoryNo, resolve.GUID);
                    if (result != null)
                    {
                        intlist.Add(result.LocalSlot);
                    }
                }
                IgnoredTopIDs_A[item.Key] = intlist;
            }

            foreach (var item in IgnoredTops_Mods_Main)
            {
                var main = UniversalAutoResolver.TryGetResolutionInfo(item.Slot, item.CategoryNo, item.GUID);
                if (main == null)
                {
                    continue;
                }
                IgnoredTopIDs_Main.Add(main.LocalSlot);
            }

            foreach (var item in IgnoredBots_Mods)
            {
                var main = UniversalAutoResolver.TryGetResolutionInfo(item.Slot, item.CategoryNo, item.GUID);
                if (main == null)
                {
                    continue;
                }
                IgnoredBotsIDs_Main.Add(main.LocalSlot);
            }
        }

        //Increasing this will not break the code but the code isn't written in a way in which it can scale to increase readbility
        //I'd imagine it's possible to scale clubs easily
        public static readonly string[] InputStrings =
            {
#if KK
                    @"\School Uniform", //0
                    @"\AfterSchool", //1
                    @"\Gym" ,//2
                    @"\Swimsuit" , //3
                    @"\Club\Swim" , //4
                    @"\Club\Manga", //5
                    @"\Club\Cheer", //6
                    @"\Club\Tea", //7
                    @"\Club\Track", //8
                    @"\Club\Koi", //9
                    @"\Casual" , //10
                    @"\Nightwear", //11
                    @"\Underwear"//12                 
#elif KKS
                    @"\Casual", //0
                    @"\Swimsuit", //1
                    @"\Nightwear", //2
                    @"\Bathroom", //3
                    @"\Underwear"//4
#endif
};
        public static readonly string[] AllCoordinatePaths =
            {
                    @"\School Uniform", //0
                    @"\AfterSchool", //1
                    @"\Gym" ,//2
                    @"\Swimsuit" , //3
                    @"\Club" , //4
                    @"\Casual" , //5
                    @"\Nightwear", //6
                    @"\Underwear",//7                 
                    @"\Bathroom", //8
};
        public static readonly string[] ClubPaths =
        {
                    @"\Home" , //0
                    @"\Swim" , //1
                    @"\Manga", //2
                    @"\Cheer", //3
                    @"\Tea", //4
                    @"\Track", //5
                    @"\Koi", //6
        };

        public static readonly string[] InputStrings2 = {
            @"\FirstTime", //0
            @"\Amateur", //1
            @"\Pro", //2
            @"\Lewd" //3
        };//Experience States; easy to make scale with size

        public static readonly string[] Generic_Inclusion =
        {
            "a_n_headtop",//0
            "a_n_headflont",//1
            "a_n_head", //2
            "a_n_headside", //3
            "a_n_waist_b", //4
            "a_n_hair_pony", //5
            "a_n_hair_twin_L", //6
            "a_n_hair_twin_R", //7
            "a_n_earrings_R", //8
            "a_n_earrings_L", //9
            "a_n_megane", //10
            "a_n_nose", //11
            "a_n_mouth", //12
            "a_n_hair_pin", //13
            "a_n_hair_pin_R" //14
        };

        public static List<ChaDefault> ChaDefaults = new List<ChaDefault>();

        public static string[] KCOX_Cat = {
            "ct_clothesTop",//0
            "ct_clothesBot",//1
            "ct_bra",//2
            "ct_shorts",//3
            "ct_gloves",//4
            "ct_panst",//5
            "ct_socks",//6
            "ct_shoes_inner",//7
            "ct_shoes_outer"//8
        };

        public static List<string>[] Inclusion = new List<string>[11]
{
            new List<string> {"None"}, //0
            new List<string> { "a_n_hair_pony", "a_n_hair_twin_L", "a_n_hair_twin_R", "a_n_hair_pin", "a_n_hair_pin_R" }, //1
            new List<string> { "a_n_headtop", "a_n_headflont", "a_n_head", "a_n_headside" }, //2

            new List<string> { "a_n_earrings_L", "a_n_earrings_R", "a_n_megane", "a_n_nose", "a_n_mouth" }, //3
            new List<string> { "a_n_neck", "a_n_bust_f", "a_n_bust" }, //4
            new List<string> { "a_n_nip_L", "a_n_nip_R", "a_n_back", "a_n_back_L", "a_n_back_R" }, //5

            new List<string> { "a_n_waist", "a_n_waist_f", "a_n_waist_b", "a_n_waist_L", "a_n_waist_R" }, //6
            new List<string> { "a_n_leg_L", "a_n_knee_L", "a_n_ankle_L", "a_n_heel_L", "a_n_leg_R", "a_n_knee_R", "a_n_ankle_R", "a_n_heel_R" },//7
            new List<string> { "a_n_shoulder_L", "a_n_elbo_L", "a_n_arm_L", "a_n_wrist_L", "a_n_shoulder_R", "a_n_elbo_R", "a_n_arm_R", "a_n_wrist_R" },//8

            new List<string> { "a_n_hand_L", "a_n_ind_L", "a_n_mid_L", "a_n_ring_L", "a_n_hand_R", "a_n_ind_R", "a_n_mid_R", "a_n_ring_R" },//9
            new List<string> { "a_n_dan", "a_n_kokan", "a_n_ana" }//10
};

        public static string[] Kill_Data = new string[] { };

        public static Dictionary<string, bool> PluginResults = new Dictionary<string, bool>();

        private static readonly string[] PluginList = new string[] { "Additional_Card_Info", "Accessory_Themes", "Accessory_Parents", "Accessory_States", "madevil.kk.ass" };

        internal static bool TryfindPluginInstance(string pluginName)
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out PluginInfo target);
            if (null != target)
            {
                return true;
            }
            return false;
        }

        public static SortedDictionary<int, int> OutfitnumPairs = new SortedDictionary<int, int>();

        public static readonly List<int> IgnoredTopIDs_Main = new List<int>() { 0, 31, 53, 59, 60, 222 };
        public static readonly List<int> IgnoredBotsIDs_Main = new List<int>() { 0, 38, 40, };

        public static readonly Dictionary<int, List<int>> IgnoredTopIDs_A = new Dictionary<int, List<int>>();

        private static Dictionary<int, List<ResolveInfo>> IgnoredTops_Original_A = new Dictionary<int, List<ResolveInfo>>()
        {
            [1] = new List<ResolveInfo>() { ResolveInfo("yu000.ShirtlessUniform", 200, 100) },
            [2] = new List<ResolveInfo>() { ResolveInfo("nakay.Top jacket", 210, 6), ResolveInfo("yu000.ShirtlessUniform", 210, 100) },
        };

        private static List<ResolveInfo> IgnoredTops_Mods_Main = new List<ResolveInfo>()
        {
           ResolveInfo("WaTaFuk.WMO",105,3001),
           ResolveInfo("mat.Tops",105,1214),
           ResolveInfo("mat.Corset",105,1200),
           ResolveInfo("gyobobo.AngelaSet",105,1643),
           ResolveInfo("[Ω-G16] HS2-KK Set",105,15110),
           ResolveInfo("yamadamod.texchange",105,512),
           ResolveInfo("gyobobo.GantzSuitSet3",105,1602),
           ResolveInfo("gyobobo.GantzSuitSet3",105,1603),
           ResolveInfo("gyobobo.GantzSuitSet3",105,1604),
           ResolveInfo("GaryuX.Wu Zetian",105,70010),
           ResolveInfo("earthship.CutoutQipao",105,69),
           ResolveInfo("com.nammiyoo.shibari",105,2929222),
           ResolveInfo("com.nammiyoo.shibari",105,2929223),
           ResolveInfo("xne_mdsweater",105,9225),
           ResolveInfo("xne_mdsweater",105,9226),
           ResolveInfo("xne_mdsweater",105,9227),
           ResolveInfo("xne_mdsweater",105,9228),
           ResolveInfo("xne_mdsweater",105,9229),
           ResolveInfo("xne_dksweater",105,9207),
           ResolveInfo("xne_dksweater",105,9208),
           ResolveInfo("xne_dksweater",105,9209),
           ResolveInfo("xne_dksweater",105,9210),
           ResolveInfo("xne_skdrs01",105,9212),
           ResolveInfo("xne_skdrs01",105,9213),
           ResolveInfo("xne_skdrs01",105,9214),
           ResolveInfo("xne_sukecami",105,9211),
           ResolveInfo("xne_minichina",105,9203),
           ResolveInfo("xne_minichina",105,9204),
           ResolveInfo("xne_minichina",105,9205),
           ResolveInfo("xne_minichina",105,9206),
           ResolveInfo("xne_egyptianset",105,9218),
           ResolveInfo("xne_dabotsyatu",105,9221),
           ResolveInfo("xne_dabotsyatu",105,9222),
           ResolveInfo("xne_altbunny",105,9230),
           ResolveInfo("xne_altbunny",105,9231),
           ResolveInfo("xne_altbunny",105,9232),
           ResolveInfo("shinyBunny",105,997),
        };

        private static List<ResolveInfo> IgnoredBots_Mods = new List<ResolveInfo>()
        {
            ResolveInfo("xne_fobtms",106,9211),
            ResolveInfo("xne_fobtms",106,9212),
            ResolveInfo("xne_fobtms",106,9213),
            ResolveInfo("xne_skcdg",106,9214),
            ResolveInfo("xne_skcdg",106,9215),
            ResolveInfo("xne_skcdg",106,9216),
            ResolveInfo("xne_dancepjmset",106,9211),
            ResolveInfo("xne_dancepjmset",106,9212),
            ResolveInfo("xne_dancepjmset",106,9213),
            ResolveInfo("xne_dancepjmset",106,9214),
            ResolveInfo("xne_egyptianset",106,9201),
            ResolveInfo("xne_egyptianset",106,9202),
            ResolveInfo("xne_egyptianset",106,9203),
            ResolveInfo("US01",106,3680),
            ResolveInfo("US01",106,3681),
            ResolveInfo("US01",106,3682),
            ResolveInfo("Mint_E403",106,802),
            ResolveInfo("Mint_E403",106,803),
            ResolveInfo("hayashi.OpenFrontSkirt",106,884),
            ResolveInfo("hayashi.OpenFrontSkirt",106,885),
            ResolveInfo("com.Quokka.kimonodressskirtopen",106,101),
        };

        private static ResolveInfo ResolveInfo(int category, int slot, string property) => ResolveInfo("", category, slot, property);
        private static ResolveInfo ResolveInfo(string guid, int category, int slot) => ResolveInfo(guid, category, slot, "");

        private static ResolveInfo ResolveInfo(string guid, int category, int slot, string property)
        {
            return new ResolveInfo() { GUID = guid, Slot = slot, CategoryNo = (ChaListDefine.CategoryNo)category, Property = property };
        }
    }

    public enum Hexp
    {
        Randomize,
        RandConstant,
        Maximize,
    }

    public enum HStates
    {
        FirstTime, //0
        Amateur, //1
        Pro, //2
        Lewd //3
    }

#if !KKS
    public enum OutfitUpdate
    {
        [Description("Update outfits everyday")]
        Daily,
        [Description("Update outfits on Mondays")]
        Weekly,
        [Description("Update every period")]
        EveryPeriod
    }

    public enum Club
    {
        HomeClub, //0
        SwimClub, //1
        MangaClub, //2
        CheerClub, //3
        TeaClub, //4
        TrackClub //5
    }
#endif
}
