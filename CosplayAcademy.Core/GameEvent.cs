using ActionGame;
using KKAPI.MainGame;
using UnityEngine;

namespace Cosplay_Academy
{
    public class GameEvent : GameCustomFunctionController
    {
        protected override void OnPeriodChange(Cycle.Type period)
        {
#if KK
            if (period == Cycle.Type.Morning && Settings.UpdateFrequency.Value == OutfitUpdate.Daily || Settings.UpdateFrequency.Value == OutfitUpdate.EveryPeriod && (period == Cycle.Type.StaffTime || period == Cycle.Type.AfterSchool || Cycle.Type.MyHouse == period))
#elif KKS
            if (period == Cycle.Type.Morning && Settings.UpdateFrequency.Value == OutfitUpdate.Daily || Settings.UpdateFrequency.Value == OutfitUpdate.EveryPeriod)
#endif
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
#if KK
        protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
#elif KKS
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
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

#if KK
        protected override void OnEndH(BaseLoader proc, HFlag hFlag, bool vr)
#elif KKS
        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
#endif
        {
            if (hFlag.isFreeH)
            {
                CharaEvent.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
            CharaEvent.inH = false;

            base.OnEndH(proc, hFlag, vr);
        }
    }
}
