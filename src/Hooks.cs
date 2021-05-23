using ActionGame;
using ActionGame.Chara;
using HarmonyLib;
using Illusion.Extensions;
using Manager;
using System;
using System.Reflection;

namespace Cosplay_Academy
{
    internal static class Hooks
    {
        public static void Init()
        {
            //Harmony _instance = new Harmony("Cosplay_Academy");
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Harmony.CreateAndPatchAll(typeof(SetNextOutfitAtMove));
            //ShowTypeInfo(typeof(HairAccessoryCustomizer.HairAccessoryController));
        }

        private static void ShowTypeInfo(Type t)
        {
            Settings.Logger.LogWarning($"Name: {t.Name}");
            Settings.Logger.LogWarning($"Full Name: {t.FullName}");
            Settings.Logger.LogWarning($"ToString:  {t}");
            Settings.Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
            Settings.Logger.LogWarning("");
        }

        //private static bool CheckEndFinally(CodeInstruction instruction) => instruction.opcode == OpCodes.Endfinally;
        //[HarmonyPatch]
        //static class HairAccessoryPatch
        //{
        //    public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("ActionGame.Cycle+<MapMove>c__Iterator4, Assembly-CSharp"), "MoveNext");//Assembly Name because it hates me now that I didn't want to use it
        //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //    {
        //        List<CodeInstruction> newInstructionSet = new List<CodeInstruction>(instructions);

        //        int searchInfoIndex = newInstructionSet.FindIndex(instruction => CheckEndFinally(instruction));
        //        newInstructionSet.Insert(searchInfoIndex, new CodeInstruction(OpCodes.Call, typeof(Hooks).GetMethod(nameof(CharaFinallyFinishedEvent), AccessTools.all)));
        //        //for (int i = searchInfoIndex; i < newInstructionSet.Count; i++)
        //        //{
        //        //    ExpandedOutfit.Logger.LogWarning(newInstructionSet[i].opcode);
        //        //}
        //        return newInstructionSet;
        //    }
        //}
        //public delegate void FinishedLoadingHandler();
        //public static event FinishedLoadingHandler CharaFinallyFinished;

        //internal static void CharaFinallyFinishedEvent()
        //{
        //    if (CharaFinallyFinished == null || CharaFinallyFinished.GetInvocationList() == null || CharaFinallyFinished.GetInvocationList().Length == 0)
        //    {
        //        return;
        //    }
        //    foreach (var entry in CharaFinallyFinished.GetInvocationList())
        //    {
        //var handler = (FinishedLoadingHandler)entry;
        //        try
        //        {
        //            handler.Invoke();
        //        }
        //        catch (Exception ex)
        //        {
        //            ExpandedOutfit.Logger.LogError($"Subscriber crash in {nameof(ExpandedOutfit)}.{nameof(CharaFinallyFinished)} - {ex}");
        //        }
        //    }
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WaitPoint), "SetWait")]
        private static void ChangeOutfitAtWaitPoint(WaitPoint __instance)
        {
            Base Chara = (Base)Traverse.Create(__instance).Property("chara").GetValue();
            if (Chara == null || Chara.chaCtrl || Chara.heroine == null || !Settings.StoryModeChange.Value || Chara.heroine.isTeacher)
            {
                return;
            }
            ChaFileParameter ChaPara = Chara.chaCtrl.fileParam;
            var ThisOutfitData = Constants.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
            if (ThisOutfitData == null || !ThisOutfitData.processed)
            {
                return;
            }
            if (__instance.MapNo == 46)
            {
                if (ThisOutfitData.ChangeKoiToClub)
                {
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.ClubOutfitPath;
                    ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpath[4].EndsWith(".png"));
                    ThisOutfitData.ChangeKoiToClub = false;
                    ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                    ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                    Chara.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                }
            }
            else if (ThisOutfitData.ChangeClubToKoi && __instance.MapNo == 22)
            {
                ThisOutfitData.ClubOutfitPath = ThisOutfitData.outfitpath[4];
                ThisOutfitData.outfitpath[4] = ThisOutfitData.KoiOutfitpath;
                int num = ThisOutfitData.heroine.isDresses.Check(false);
                if (num == -1)
                {
                    num = 0;
                }
                ThisOutfitData.heroine.coordinates[num] = 4;
                ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpath[4].EndsWith(".png"));
                ThisOutfitData.ChangeClubToKoi = false;
                ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                Chara.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                //Chara.chaCtrl.SetAccessoryStateAll(true);
            }
            else if (ThisOutfitData.ChangeKoiToClub && __instance.MapNo != 22)
            {
                int remainThreshold = (ThisOutfitData.heroine.lewdness / (4 - (int)ThisOutfitData.heroine.HExperience));
                if (UnityEngine.Random.Range(0, 101) >= remainThreshold)
                {
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.ClubOutfitPath;
                    int num = ThisOutfitData.heroine.isDresses.Check(false);
                    if (num == -1)
                    {
                        num = 0;
                    }
                    ThisOutfitData.heroine.coordinates[num] = 4;
                    ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpath[4].EndsWith(".png"));
                    ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                    ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                    ThisOutfitData.ChaControl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                    //ThisOutfitData.ChaControl.SetAccessoryStateAll(true);
                }
                ThisOutfitData.ChangeKoiToClub = false;
            }
            //ExpandedOutfit.Logger.LogWarning($"SetWait2 success: {Chara.chaCtrl.fileParam.fullname} is waiting at {Chara.mapNo}");
        }

        [HarmonyPatch]
        static class SetNextOutfitAtMove
        {
            public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("ActionGame.ActionControl+DesireInfo, Assembly-CSharp"), "SetWaitPoint");//Assembly Name because it hates me now that I didn't want to use it
            static void Postfix(WaitPoint wp, NPC _npc)
            {
                if (wp == null || _npc == null || !Settings.StoryModeChange.Value)
                {
                    return;
                }
                ActionScene actScene = Singleton<Game>.Instance.actScene;
                if (actScene != null)
                {
                    ActionControl actCtrl = actScene.actCtrl;
                    if (actCtrl != null)
                    {
                        ChaFileParameter ChaPara = _npc.chaCtrl.fileParam;
                        var ThisOutfitData = Constants.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
                        if (ThisOutfitData == null)
                        {
                            return;
                        }
                        if (wp.MapNo == 22 && _npc.mapNo != 22) //characters who walk to clubroom should be expected to change to koioutfit maybe.
                        {
                            if (ThisOutfitData.Changestate)
                            {
                                ThisOutfitData.Changestate = false;
                                return;
                            }
                            //var tempcoord = ThisOutfitData.heroine.coordinates.ToList();
                            //var tempdress = ThisOutfitData.heroine.isDresses.ToList();
                            //tempcoord.Add(4);
                            //tempdress.Add(false);
                            //ThisOutfitData.heroine.coordinates = tempcoord.ToArray();
                            //ThisOutfitData.heroine.isDresses = tempdress.ToArray();
                            //actCtrl.SetDesire(0, ThisOutfitData.heroine, 100);
                            //ExpandedOutfit.Logger.LogWarning($"{_npc.chaCtrl.fileParam.fullname} is heading to club room...probably");
                            if (UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
                            {
                                ThisOutfitData.ChangeClubToKoi = true;
                            }
                        }
                        else if (_npc.mapNo == 22 && wp.MapNo == 46)
                        {
                            ThisOutfitData.ChangeKoiToClub = true;
                        }
                        else if (_npc.mapNo == 22 && wp.MapNo != 22)
                        {
                            ThisOutfitData.ChangeKoiToClub = true;
                            //var tempcoord = ThisOutfitData.heroine.coordinates.ToList();
                            //var tempdress = ThisOutfitData.heroine.isDresses.ToList();
                            //tempcoord.Add(4);
                            //tempdress.Add(false);
                            //ThisOutfitData.heroine.coordinates = tempcoord.ToArray();
                            //ThisOutfitData.heroine.isDresses = tempdress.ToArray();
                            //actCtrl.SetDesire(0, ThisOutfitData.heroine, 100);
                            //ThisOutfitData.ChangeKoiToClub = true;
                        }
                    }
                }
            }
        }

        //[HarmonyPatch]
        //static class FirstActionPatch
        //{
        //    public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("ActionGame.ActionControl+DesireInfo, Assembly-CSharp"), "FirstAction",new Type[] { typeof(SaveData.Heroine),AccessTools.TypeByName("ActionGame.ActionControl+DesireInfo, Assembly-CSharp")});//Assembly Name because it hates me now that I didn't want to use it
        //    static void Prefix(int _mapNo, NPC _npc, ActionControl __instance, SaveData.Heroine _heroine)
        //    {
        //        if (_mapNo == 22)
        //        {

        //        }
        //    }
        //}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPC), "ReStart")]
        private static void NPCRestart(NPC __instance)
        {
            //change NPC's who start at club room to a koi outfit
            var ChaPara = __instance.chaCtrl.fileParam;
            var ThisOutfitData = Constants.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
            if (ThisOutfitData == null || !ThisOutfitData.processed || __instance.heroine.isTeacher)
            {
                if (!Settings.StoryModeChange.Value)
                {
                    if (Settings.ChangeToClubatKoi.Value && __instance.mapNo == 22)
                    {
                        __instance.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                        __instance.heroine.coordinates[0] = 4;
                    }
                    return;
                }
            }
            ThisOutfitData.ChangeKoiToClub = false;
            ThisOutfitData.ChangeClubToKoi = false;
            if (__instance.mapNo == 22 && UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
            {
                ThisOutfitData.ClubOutfitPath = ThisOutfitData.outfitpath[4];
                ThisOutfitData.outfitpath[4] = ThisOutfitData.KoiOutfitpath;
                ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpath[4].EndsWith(".png"));
                ThisOutfitData.heroine.coordinates[0] = 4;
                ThisOutfitData.SkipFirstPriority = ThisOutfitData.ChangeKoiToClub = true;
                ThisOutfitData.ClothingLoader.Reload_RePacks(__instance.chaCtrl, true);
                __instance.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                //__instance.chaCtrl.SetAccessoryStateAll(true);
                //ExpandedOutfit.Logger.LogError(__instance.chaCtrl.fileParam.fullname + " Action NO: " + __instance.AI.actionNo + " " + ThisOutfitData.heroine.clubActivities + " " + ThisOutfitData.heroine.coordinates.Length);
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(KK_Plugins.HairAccessoryCustomizer.HairAccessoryController), "OnCoordinateBeingLoaded")]
        //private static bool StopCustom()
        //{
        //    ExpandedOutfit.Logger.LogWarning("Cosplay Academy Stop custom");

        //    if (ExpandedOutfit.HairMatch.Value && ExpandedOutfit.AccKeeper.Value && !HairACC_firstPass)
        //    {
        //        ExpandedOutfit.Logger.LogWarning("Cosplay Academy stopped HairACC from loading");
        //        HairACC_firstPass = true;
        //        return true;
        //    }
        //    HairACC_firstPass = false;
        //    return false;
        //}
    }
}
