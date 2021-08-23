#if !KKS
using ActionGame;
using ActionGame.Chara;
using Extensions;
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
            Harmony.CreateAndPatchAll(typeof(Hooks));
            Harmony.CreateAndPatchAll(typeof(SetNextOutfitAtMove));
        }

        //private static void ShowTypeInfo(Type t)
        //{
        //    Settings.Logger.LogWarning($"Name: {t.Name}");
        //    Settings.Logger.LogWarning($"Full Name: {t.FullName}");
        //    Settings.Logger.LogWarning($"ToString:  {t}");
        //    Settings.Logger.LogWarning($"Assembly Qualified Name: {t.AssemblyQualifiedName}");
        //    Settings.Logger.LogWarning("");
        //}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WaitPoint), "SetWait")]
        internal static void ChangeOutfitAtWaitPoint(WaitPoint __instance)
        {
            try
            {
                Base Chara = (Base)Traverse.Create(__instance).Property("chara").GetValue();
                if (Chara == null || Chara.chaCtrl == null || Chara.heroine == null || !Settings.StoryModeChange.Value || Chara.heroine.isTeacher)
                {
                    return;
                }
                var ThisOutfitData = CharaEvent.ChaDefaults.Find(x => x.Parameter.Compare(Chara.chaCtrl.fileParam));
                if (ThisOutfitData == null || !ThisOutfitData.processed)
                {
                    return;
                }
                var heroine = Chara.heroine;
                //ChaFileParameter ChaPara = Chara.chaCtrl.fileParam;
                //var ThisOutfitData = CharaEvent.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
                //if (ThisOutfitData == null || !ThisOutfitData.processed)
                //{
                //    return;
                //}
                ThisOutfitData.heroine = heroine;
                if (__instance.MapNo == 46)
                {
                    if (ThisOutfitData.ChangeKoiToClub)
                    {
                        ThisOutfitData.outfitpaths[4] = ThisOutfitData.ClubOutfitPath;
                        ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpaths[4].EndsWith(".png"));
                        ThisOutfitData.ChangeKoiToClub = false;
                        ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                        ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                        Chara.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                    }
                }
                else if (ThisOutfitData.ChangeClubToKoi && __instance.MapNo == 22)
                {
                    ThisOutfitData.ClubOutfitPath = ThisOutfitData.outfitpaths[4];
                    ThisOutfitData.outfitpaths[4] = ThisOutfitData.KoiOutfitpath;
                    int num = heroine.isDresses.Check(false);
                    if (num == -1)
                    {
                        num = 0;
                    }
                    heroine.coordinates[num] = 4;
                    ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpaths[4].EndsWith(".png"));
                    ThisOutfitData.ChangeClubToKoi = false;
                    ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                    ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                    Chara.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                    //Chara.chaCtrl.SetAccessoryStateAll(true);
                }
                else if (ThisOutfitData.ChangeKoiToClub && __instance.MapNo != 22)
                {
                    int remainThreshold = (heroine.lewdness / (4 - (int)heroine.HExperience));
                    if (UnityEngine.Random.Range(0, 101) >= remainThreshold)
                    {
                        ThisOutfitData.outfitpaths[4] = ThisOutfitData.ClubOutfitPath;
                        int num = heroine.isDresses.Check(false);
                        if (num == -1)
                        {
                            num = 0;
                        }
                        heroine.coordinates[num] = 4;
                        ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpaths[4].EndsWith(".png"));
                        ThisOutfitData.ClothingLoader.Run_Repacks(ThisOutfitData.ChaControl);
                        ThisOutfitData.ClothingLoader.Reload_RePacks(ThisOutfitData.ChaControl, true);
                        ThisOutfitData.ChaControl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                        //ThisOutfitData.ChaControl.SetAccessoryStateAll(true);
                    }
                    ThisOutfitData.ChangeKoiToClub = false;
                }
                //ExpandedOutfit.Logger.LogWarning($"SetWait2 success: {Chara.chaCtrl.fileParam.fullname} is waiting at {Chara.mapNo}");

            }
            catch (Exception ex)
            {
                Settings.Logger.LogError("ChangeOutfitAtWaitPoint fail - " + ex);
            }
        }

        [HarmonyPatch]
        static class SetNextOutfitAtMove
        {
            public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("ActionGame.ActionControl+DesireInfo, Assembly-CSharp"), "SetWaitPoint");//Assembly Name because it hates me now that I didn't want to use it
            internal static void Postfix(WaitPoint wp, NPC _npc)
            {
                try
                {
                    if (wp == null || _npc == null || !Settings.StoryModeChange.Value)
                    {
                        return;
                    }
                    ActionScene actScene = Singleton<Game>.Instance.actScene;
                    if (actScene != null && actScene.actCtrl != null)
                    {
                        //ChaFileParameter ChaPara = _npc.chaCtrl.fileParam;
                        //var ThisOutfitData = CharaEvent.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
                        //if (ThisOutfitData == null)
                        //{
                        //    return;
                        //}
                        var ThisOutfitData = CharaEvent.ChaDefaults.Find(x => x.Parameter.Compare(_npc.chaCtrl.fileParam));
                        if (ThisOutfitData == null) return;

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
                catch (Exception ex)
                {

                    Settings.Logger.LogError("SetWaitPoint fail - " + ex);
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
        internal static void NPCRestart(NPC __instance)
        {
            try
            {
                var ThisOutfitData = CharaEvent.ChaDefaults.Find(x => x.Parameter.Compare(__instance.chaCtrl.fileParam));
                if (ThisOutfitData == null || ThisOutfitData.processed || __instance.heroine.isTeacher || !Settings.StoryModeChange.Value)
                {
                    if (Settings.StoryModeChange.Value && Settings.ChangeToClubatKoi.Value && __instance.mapNo == 22)
                    {
                        __instance.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                        __instance.heroine.coordinates[0] = 4;
                    }
                    return;
                }
                ThisOutfitData.ChangeKoiToClub = false;
                ThisOutfitData.ChangeClubToKoi = false;
                if (__instance.mapNo == 22 && UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
                {
                    ThisOutfitData.ClubOutfitPath = ThisOutfitData.outfitpaths[4];
                    ThisOutfitData.outfitpaths[4] = ThisOutfitData.KoiOutfitpath;
                    ThisOutfitData.ClothingLoader.GeneralizedLoad(4, ThisOutfitData.outfitpaths[4].EndsWith(".png"));
                    __instance.heroine.coordinates[0] = 4;
                    ThisOutfitData.SkipFirstPriority = ThisOutfitData.ChangeKoiToClub = true;
                    ThisOutfitData.ClothingLoader.Reload_RePacks(__instance.chaCtrl, true);
                    __instance.chaCtrl.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Club);
                    //__instance.chaCtrl.SetAccessoryStateAll(true);
                    //ExpandedOutfit.Logger.LogError(__instance.chaCtrl.fileParam.fullname + " Action NO: " + __instance.AI.actionNo + " " + ThisOutfitData.heroine.clubActivities + " " + ThisOutfitData.heroine.coordinates.Length);
                }

            }
            catch (Exception ex)
            {

                Settings.Logger.LogError("ReStart fail - " + ex);
            }
            //change NPC's who start at club room to a koi outfit
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.SetState))]
        internal static void LoadSethook(int _status)
        {
            Settings.MakerHstate.Value = (HStates)_status;
        }
    }
}
#endif