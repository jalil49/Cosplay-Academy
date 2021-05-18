using ActionGame;
using KKAPI.MainGame;
namespace Cosplay_Academy
{
    public class GameEvent : GameCustomFunctionController
    {
        protected override void OnPeriodChange(Cycle.Type period)
        {
            if (period == Cycle.Type.Morning && ExpandedOutfit.UpdateFrequency.Value == OutfitUpdate.Daily)
            {
                OutfitDecider.ResetDecider();
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
                //Constants.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
        }

        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            Constants.ChaDefaults.Clear();
            OutfitDecider.ResetDecider();
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }

        protected override void OnNewGame()
        {
            Constants.ChaDefaults.Clear();
            OutfitDecider.ResetDecider();
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }

        protected override void OnStartH(HSceneProc hSceneProc, bool freeH)
        {
            if (ExpandedOutfit.EnableSetting.Value)
            {
                foreach (var Heroine in hSceneProc.dataH.lstFemale)
                {
                    Heroine.chaCtrl.ChangeCoordinateTypeAndReload();
                }
            }
        }

        protected override void OnEndH(HSceneProc hSceneProc, bool freeH)
        {
            if (freeH)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
        }
    }
}