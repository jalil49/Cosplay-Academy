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
            @"\Club\Koi" //11
        };//Folders
        public static readonly string[] InputStrings2 = {
            @"\FirstTime", //0
            @"\Amateur", //1
            @"\Pro", //2
            @"\Lewd" //3
        };//Experience States, increasing 
        public static string[] outfitpath = { " ", " ", " ", " ", " ", " ", " " };//Number of outfits starting from uniform to nightwear
    }
    public enum HStates
    {
        FirstTime, //0
        Amateur, //1
        Pro, //2
        Lewd //3
    }
    //{
    //    FirstTime,
    //    Amateur,
    //    Pro,
    //    Lewd
    //}
    //    public enum Personailty
    //    {
    //        Airhead,
    //        Angel,
    //        Athlete,
    //        BigSister,
    //        Bookworm,
    //        Classicheroine,
    //        DarkLord,
    //        Emotionless,
    //        Enigma,
    //        Extrovert,
    //        Fangirl,
    //        Flirt,
    //        Geek,
    //        GirlNextdoor,
    //        Heiress,
    //        HonorStudent,
    //        Introvert,
    //        JapaneseIdeal,
    //        Loner,
    //        MisfortuneMagnet,
    //        Motherfigure,
    //        OldSchool,
    //        Perfectionist,
    //        PsychoStalker,
    //        PureHeart,
    //        Rebel,
    //        Returnee,
    //        ScaredyCat,
    //        Seductress,
    //        Ski,
    //        Slacker,
    //        Slangy,
    //        Snob,
    //        Sourpuss,
    //        SpaceCase,
    //        Tomboy,
    //        ToughGirl,
    //        Underclassman,
    //        WildChild,
    //    }
    //    public enum Traits
    //    {
    //        PeesOften,
    //        Hungry,
    //        Insensitive,
    //        Simple,
    //        Slutty,
    //        Gloomy,
    //        LikesReading,
    //        LikesMusic,
    //        Lively,
    //        Passive,
    //        Friendly,
    //        LikesCleanliness,
    //        Lazy,
    //        SuddenlyAppears,
    //        LikesBeingAone,
    //        LikesExcercising,
    //        Diligent,
    //        LikesGirls
    //    }
}
