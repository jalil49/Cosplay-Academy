#if !KKS
using ActionGame;
using KKAPI.MainGame;
namespace Cosplay_Academy
{
    public class GameEvent : GameCustomFunctionController
    {
        protected override void OnPeriodChange(Cycle.Type period)
        {
            if (period == Cycle.Type.Morning && Settings.UpdateFrequency.Value == OutfitUpdate.Daily || Settings.UpdateFrequency.Value == OutfitUpdate.EveryPeriod && (period == Cycle.Type.StaffTime || period == Cycle.Type.AfterSchool || Cycle.Type.MyHouse == period))
            {
                OutfitDecider.ResetDecider();
            }
            foreach (var item in CharaEvent.ChaDefaults)
            {
                item.Changestate = true;
            }
        }

        protected override void OnDayChange(Cycle.Week day)
        {
            if ((Cycle.Week.Monday == day && Settings.UpdateFrequency.Value == OutfitUpdate.Weekly) || Cycle.Week.Holiday == day && Settings.SundayDate.Value)
            {
                OutfitDecider.ResetDecider();
            }
        }

        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            CharaEvent.ChaDefaults.Clear();
            OutfitDecider.ResetDecider();
        }

        protected override void OnNewGame()
        {
            CharaEvent.ChaDefaults.Clear();
            OutfitDecider.ResetDecider();
        }

        protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
        {
            if (Settings.EnableSetting.Value)
            {
                foreach (var Heroine in hFlag.lstHeroine)
                {
                    Heroine.chaCtrl.ChangeCoordinateTypeAndReload();
                }
            }
            CharaEvent.inH = true;

            base.OnStartH(proc, hFlag, vr);
        }

        protected override void OnEndH(BaseLoader proc, HFlag hFlag, bool vr)
        {
            if (hFlag.isFreeH)
            {
                //CharaEvent.FreeHHeroines.Clear();
                CharaEvent.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
            CharaEvent.inH = false;

            base.OnEndH(proc, hFlag, vr);
        }
    }
}
#endif