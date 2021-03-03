using HarmonyLib;
//using KK_Plugins;
//using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Chara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosplay_Academy
{
    internal static class Hooks
    {
        private static Harmony _instance;
        public static void HarmonyInit()
        {
            //_instance = Harmony.CreateAndPatchAll(typeof(Hooks));
            //TryPatchClothesOverlayX(_instance);
            //ShowTypeInfo(typeof(MaterialEditorCharaController.MaterialColorProperty));
            //ShowTypeInfo(typeof(MaterialEditorCharaController.MaterialFloatProperty));
            //ShowTypeInfo(typeof(MaterialEditorCharaController.MaterialShader));
            //ShowTypeInfo(typeof(MaterialEditorCharaController.MaterialTextureProperty));
            //ShowTypeInfo(typeof(MaterialEditorCharaController.RendererProperty));
        }
        public static void OtherInit()
        {
        }
        private static void ShowTypeInfo(Type t)
        {
            ExpandedOutfit.Logger.LogWarning($"Name: {t.Name}");
            ExpandedOutfit.Logger.LogWarning($"Full Name: {t.FullName}");
            ExpandedOutfit.Logger.LogWarning($"ToString:  {t}");
            ExpandedOutfit.Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
            ExpandedOutfit.Logger.LogWarning("");
        }
        private static void TryPatchClothesOverlayX(Harmony Instance)
        {
            var C_OverlayX = Type.GetType("KoiClothesOverlayX.KoiClothesOverlayController, KK_OverlayMods", false);
            if (C_OverlayX != null)
            {
                ExpandedOutfit.Logger.LogWarning("Success");
            }
            else
            {
                ExpandedOutfit.Logger.LogWarning(
                                    "Could not find KoiClothesOverlayX.KoiClothesOverlayController, Clothing Textures will not work properly (please report this if you do have latest version of KK_OverlayMods installed)");
            }
        }

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
