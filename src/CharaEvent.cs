using KKAPI;
using KKAPI.Chara;

namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode) //from KKAPI.Chara when characters enter reload state
        {
            //if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode || !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode)
            //{ return; }//if disabled don't run
            //base.OnReload(currentGameMode);
            if (ChaControl.sex == 1 && (!OutfitDecider.ProcessedNames.Contains(ChaControl.fileParam.fullname) || OutfitDecider.Reset))//run the following if female and unprocessed
            {                
                OutfitDecider.Decision(ChaControl.fileParam.fullname);//Generate outfits
                ClothingLoader.FullLoad(ChaControl);//Load outfits
                ChaInfo temp = (ChaInfo)ChaControl;
                ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
            }
        }
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //unused mandatory function 
        }
    }
}