using ActionGame;
using KKAPI.MainGame;
using Manager;
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
            if (freeH)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.Reset = true;
                ExpandedOutfit.Logger.LogInfo("Reset has applied");
            }
        }
        protected override void OnEndH(HSceneProc hSceneProc, bool freeH)
        {
            //ExpandedOutfit.Logger.LogWarning($"freeh is {freeH}");
            if (freeH)
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.Reset = true;
                ExpandedOutfit.Logger.LogInfo("Reset has applied");
            }
        }
    }
}