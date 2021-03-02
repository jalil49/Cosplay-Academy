using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosplay_Academy
{
    public static class Hooks
    {
        //public static void Init()
        //{
        //    Harmony.CreateAndPatchAll(typeof(Hooks));
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ChaFile), "CopyCoordinate")]
        //private static void CopyCoordHook(ref ChaFileCoordinate[] _coordinate) //ExtendedData doesn't transfer
        //{
        //    //ExpandedOutfit.Logger.LogWarning("Copycoord has activaed");
        //    //for (int i = 0; i < 7; i++)
        //    //{
        //    //    ExpandedOutfit.Logger.LogWarning(_coordinate[i].coordinateFileName);
        //    //}

        //    ChaFileCoordinate[] temp = new ChaFileCoordinate[7];
        //    for (int i = 0; i < 7; i++)
        //    {
        //        temp[i] = new ChaFileCoordinate();
        //        temp[i].LoadFile(@"F:\[ScrewThisNoise] Koikatsu BetterRepack R9.2\UserData\coordinate\BR-Chan KKP Outing.png");
        //    }
        //    _coordinate = temp;
        //}
    }
}
