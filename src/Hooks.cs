//using ActionGame;
//using BepInEx.Harmony;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Reflection.Emit;

//namespace Cosplay_Academy
//{
//    //internal static class Hooks
//    //{
//    //    //static Harmony _instance;
//    //    public static void Init()
//    //    {
//    //        //_instance = new Harmony("Cosplay_Academy");
//    //        //Harmony.CreateAndPatchAll(typeof(Hooks));
//    //        //ShowTypeInfo(typeof(Cycle));
//    //        //Harmony.CreateAndPatchAll(typeof(HairAccessoryPatch));
//    //        // _instance.Patch(typeof(HairAccessoryPatch));
//    //        // _instance.Patch(HairAccessoryPatch.TargetMethod, null, null, HairAccessoryPatch.Transpiler);
//    //    }
//    //    //private static void ShowTypeInfo(Type t)
//    //    //{
//    //    //    ExpandedOutfit.Logger.LogWarning($"Name: {t.Name}");
//    //    //    ExpandedOutfit.Logger.LogWarning($"Full Name: {t.FullName}");
//    //    //    ExpandedOutfit.Logger.LogWarning($"ToString:  {t}");
//    //    //    ExpandedOutfit.Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
//    //    //    ExpandedOutfit.Logger.LogWarning("");


//    //    //}
//    //    //private static bool CheckEndFinally(CodeInstruction instruction) => instruction.opcode == OpCodes.Endfinally;
//    //    //[HarmonyPatch]
//    //    //static class HairAccessoryPatch
//    //    //{
//    //    //    public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("ActionGame.Cycle+<MapMove>c__Iterator4, Assembly-CSharp"), "MoveNext");//Assembly Name because it hates me now that I didn't want to use it
//    //    //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    //    //    {
//    //    //        List<CodeInstruction> newInstructionSet = new List<CodeInstruction>(instructions);

//    //    //        int searchInfoIndex = newInstructionSet.FindIndex(instruction => CheckEndFinally(instruction));
//    //    //        newInstructionSet.Insert(searchInfoIndex, new CodeInstruction(OpCodes.Call, typeof(Hooks).GetMethod(nameof(CharaFinallyFinishedEvent), AccessTools.all)));
//    //    //        //for (int i = searchInfoIndex; i < newInstructionSet.Count; i++)
//    //    //        //{
//    //    //        //    ExpandedOutfit.Logger.LogWarning(newInstructionSet[i].opcode);
//    //    //        //}
//    //    //        return newInstructionSet;
//    //    //    }

//    //    //}
//    //    //public delegate void FinishedLoadingHandler();
//    //    //public static event FinishedLoadingHandler CharaFinallyFinished;

//    //    //internal static void CharaFinallyFinishedEvent()
//    //    //{
//    //    //    if (CharaFinallyFinished == null || CharaFinallyFinished.GetInvocationList() == null || CharaFinallyFinished.GetInvocationList().Length == 0)
//    //    //    {
//    //    //        return;
//    //    //    }
//    //    //    foreach (var entry in CharaFinallyFinished.GetInvocationList())
//    //    //    {
//    //    //        var handler = (FinishedLoadingHandler)entry;
//    //    //        try
//    //    //        {
//    //    //            handler.Invoke();
//    //    //        }
//    //    //        catch (Exception ex)
//    //    //        {
//    //    //            ExpandedOutfit.Logger.LogError($"Subscriber crash in {nameof(ExpandedOutfit)}.{nameof(CharaFinallyFinished)} - {ex}");
//    //    //        }
//    //    //    }
//    //    //}

//    //    //[HarmonyPrefix]
//    //    //[HarmonyPatch(typeof(ChaFile), "CopyCoordinate")]
//    //    //private static void CopyCoordHook(ref ChaFileCoordinate[] _coordinate) //ExtendedData doesn't transfer
//    //    //{
//    //    //    //ExpandedOutfit.Logger.LogWarning("Copycoord has activaed");
//    //    //    //for (int i = 0; i < 7; i++)
//    //    //    //{
//    //    //    //    ExpandedOutfit.Logger.LogWarning(_coordinate[i].coordinateFileName);
//    //    //    //}

//    //    //    ChaFileCoordinate[] temp = new ChaFileCoordinate[7];
//    //    //    for (int i = 0; i < 7; i++)
//    //    //    {
//    //    //        temp[i] = new ChaFileCoordinate();
//    //    //        temp[i].LoadFile(@"F:\[ScrewThisNoise] Koikatsu BetterRepack R9.2\UserData\coordinate\BR-Chan KKP Outing.png");
//    //    //    }
//    //    //    _coordinate = temp;
//    //    //}
//    //}
//}
