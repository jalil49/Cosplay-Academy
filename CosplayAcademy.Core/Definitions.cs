using BepInEx;
using System;
using System.Collections.Generic;
#if !KKS
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
                //Settings.Logger.LogWarning($"Found {item}: {PluginResults[item]}");
            }
        }

        //Increasing this will not break the code but the code isn't written in a way in which it can scale to increase readbility
        //I'd imagine it's possible to scale clubs easily
        public static readonly string[] InputStrings =
            {
#if !KKS
            
                    @"\School Uniform", //0
                    @"\AfterSchool", //1
                    @"\Gym" ,//2
                    @"\Swimsuit" , //3
                    @"\Club\Swim" , //4
                    @"\Club\Manga", //5
                    @"\Club\Cheer", //6
                    @"\Club\Tea", //7
                    @"\Club\Track", //8
                    @"\Casual" , //9
                    @"\Nightwear", //10
                    @"\Club\Koi", //11
                    @"\Underwear"//12
                 
#elif Sun
                    @"\Casual", //0
                    @"\Swimsuit", //1
                    @"\Nightwear", //2
                    @"\Bathroom", //3
                    @"\Underwear"//4
#endif
};
        public static readonly string[] InputStrings2 = {
            @"\FirstTime", //0
            @"\Amateur", //1
            @"\Pro", //2
            @"\Lewd" //3
        };//Experience States; easy to make scale with size
        public static int Outfit_Size = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length;//Number of outfits starting from uniform to nightwear
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

        internal static bool TryfindPluginInstance(string pluginName, Version minimumVersion = null)
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(pluginName, out PluginInfo target);
            if (null != target)
            {
                if (target.Metadata.Version >= minimumVersion)
                {
                    return true;
                }
            }
            return false;
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
