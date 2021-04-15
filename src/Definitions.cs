using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace Cosplay_Academy
{
    public enum OutfitUpdate
    {
        [Description("Update outfits everyday")]
        Daily,
        [Description("Update outfits on Mondays")]
        Weekly/*,*/
        //[Description("Update everytime character reloads")]
        //EveryReload
    }
    static class Constants
    {
        //Increasing this will not break the code but the code isn't written in a way in which it can scale to increase readbility
        //I'd imagine it's possible to scale clubs easily
        public static readonly string[] InputStrings = {
            @"\School Uniform" , //0
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
        };//Folders
        public static readonly string[] InputStrings2 = {
            @"\FirstTime", //0
            @"\Amateur", //1
            @"\Pro", //2
            @"\Lewd" //3
        };//Experience States; easy to make scale with size
        public static int outfitpath = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length;//Number of outfits starting from uniform to nightwear
        public static readonly string[] Inclusion =
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
    }

    public enum HStates
    {
        FirstTime, //0
        Amateur, //1
        Pro, //2
        Lewd //3
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
    //public enum Personailty //Will probably be a main folder that just gets appended onto available options if this ever becomes a feature rather than going into folder hell mode
    //{
    //    Airhead,
    //    Angel,
    //    Athlete,
    //    BigSister,
    //    Bookworm,
    //    Classicheroine,
    //    DarkLord,
    //    Emotionless,
    //    Enigma,
    //    Extrovert,
    //    Fangirl,
    //    Flirt,
    //    Geek,
    //    GirlNextdoor,
    //    Heiress,
    //    HonorStudent,
    //    Introvert,
    //    JapaneseIdeal,
    //    Loner,
    //    MisfortuneMagnet,
    //    Motherfigure,
    //    OldSchool,
    //    Perfectionist,
    //    PsychoStalker,
    //    PureHeart,
    //    Rebel,
    //    Returnee,
    //    ScaredyCat,
    //    Seductress,
    //    Ski,
    //    Slacker,
    //    Slangy,
    //    Snob,
    //    Sourpuss,
    //    SpaceCase,
    //    Tomboy,
    //    ToughGirl,
    //    Underclassman,
    //    WildChild,
    //}
    //public enum Traits
    //{
    //    PeesOften,
    //    Hungry,
    //    Insensitive,
    //    Simple,
    //    Slutty,
    //    Gloomy,
    //    LikesReading,
    //    LikesMusic,
    //    Lively,
    //    Passive,
    //    Friendly,
    //    LikesCleanliness,
    //    Lazy,
    //    SuddenlyAppears,
    //    LikesBeingAone,
    //    LikesExcercising,
    //    Diligent,
    //    LikesGirls
    //}
}
