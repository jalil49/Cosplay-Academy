using ActionGame;
using HarmonyLib;
using KKAPI.MainGame;
using System.Collections.Generic;
namespace Cosplay_Academy
{
    public class GameEvent : GameCustomFunctionController
    {
        protected override void OnPeriodChange(Cycle.Type period)
        {
            if (period == Cycle.Type.GotoSchool && ExpandedOutfit.UpdateFrequency.Value == OutfitUpdate.Daily)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.Reset = true;
            }
            foreach (var item in Constants.ChaDefaults)
            {
                item.Changestate = true;
            }
        }
        protected override void OnDayChange(Cycle.Week day)
        {
            if ((Cycle.Week.Monday == day && ExpandedOutfit.UpdateFrequency.Value == OutfitUpdate.Weekly) || Cycle.Week.Holiday == day && ExpandedOutfit.SundayDate.Value)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.Reset = true;
            }
        }
        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {

            Constants.ChaDefaults.Clear();
            OutfitDecider.Reset = true;
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }
        protected override void OnNewGame()
        {
            Constants.ChaDefaults.Clear();
            OutfitDecider.Reset = true;
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }
        protected override void OnStartH(HSceneProc hSceneProc, bool freeH)
        {
            if (freeH && ExpandedOutfit.EnableSetting.Value)
            {
                foreach (var item in Constants.ChaDefaults)
                {
                    item.ChaControl.ChangeCoordinateTypeAndReload();
                    item.ChaControl.SetAccessoryStateAll(true);
                }
            }
            else if (!freeH && ExpandedOutfit.EnableSetting.Value) //required when starting H from special scenes: 3P, caught playing with self, les
            {
                ClothingLoader clothingLoader = new ClothingLoader();
                List<ChaControl> lstFemale = (List<ChaControl>)Traverse.Create(hSceneProc).Field("lstFemale").GetValue();
                foreach (var item in lstFemale)
                {
                    //chacontrols from special scenes do not seem to be the same as the ones from reload as changes don't take effect
                    //used to do it in CharaEvent, but it broke with textures after talkng to someone
                    var ChaPara = item.fileParam;

                    var ThisOutfitData = Constants.ChaDefaults.Find(x => ChaPara.personality == x.Personality && x.FullName == ChaPara.fullname && x.BirthDay == ChaPara.strBirthDay);
                    if (ThisOutfitData != null)
                    {
                        //Storedwaifus[ChaPara.fullname + ChaPara.personality + ChaPara.strBirthDay] = ThisOutfitData.ChaControl;
                        //ThisOutfitData.ChaControl = item;
                        //ThisOutfitData.Chafile = item.chaFile;
                        int retain = item.chaFile.status.coordinateType;
                        clothingLoader.FullLoad(ThisOutfitData, item, item.chaFile);
                        item.chaFile.status.coordinateType = retain;
                        item.ChangeCoordinateTypeAndReload();
                    }
                }
            }
        }
        protected override void OnEndH(HSceneProc hSceneProc, bool freeH)
        {
            //ExpandedOutfit.Logger.LogWarning($"freeh is {freeH}");
            if (freeH)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.Reset = true;
            }
            //else if (!freeH)
            //{
            //    foreach (var item in Storedwaifus)
            //    {
            //        var ThisOutfitData = Constants.ChaDefaults.Find(x => (x.FullName + x.Personality + x.BirthDay) == item.Key);
            //        if (ThisOutfitData != null)
            //        {
            //            ThisOutfitData.ChaControl = item.Value;
            //            ThisOutfitData.Chafile = item.Value.chaFile;
            //        }
            //        else
            //        {
            //            ExpandedOutfit.Logger.LogError("Unable to revert chacontrol to normal");
            //        }
            //    }
            //    Storedwaifus.Clear();
            //}
        }
    }
}