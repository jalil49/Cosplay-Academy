using ActionGame;
using KKAPI.MainGame;

namespace Cosplay_Academy
{
    public class GameEvent : GameCustomFunctionController
    {
        protected override void OnPeriodChange(Cycle.Type period)
        {
            if (period == Cycle.Type.GotoSchool && ExpandedOutfit.UpdateFrequency.Value == OutfitUpdate.Daily)
            {
                OutfitDecider.Reset = true;
            }
        }
        protected override void OnDayChange(Cycle.Week day)
        {
            if ((Cycle.Week.Monday == day && ExpandedOutfit.UpdateFrequency.Value == OutfitUpdate.Weekly) || Cycle.Week.Holiday == day && ExpandedOutfit.SundayDate.Value)
            {
                OutfitDecider.Reset = true;
            }
        }
        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            OutfitDecider.Reset = true;
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }
        protected override void OnNewGame()
        {
            OutfitDecider.Reset = true;
            ExpandedOutfit.Logger.LogInfo("Reset has applied");
        }
        protected override void OnStartH(HSceneProc hSceneProc, bool freeH)
        {
            ExpandedOutfit.Logger.LogWarning($"freeh is {freeH}");

            if (freeH)
            {
                OutfitDecider.Reset = true;
                ExpandedOutfit.Logger.LogInfo("Reset has applied");
            }
        }
        protected override void OnEndH(HSceneProc hSceneProc, bool freeH)
        {
            ExpandedOutfit.Logger.LogWarning($"freeh is {freeH}");

            if (freeH)
            {
                OutfitDecider.Reset = true;
                ExpandedOutfit.Logger.LogInfo("Reset has applied");
            }
        }
    }
}