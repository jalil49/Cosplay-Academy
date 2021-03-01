using KKAPI;
using KKAPI.Chara;

namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        protected override void OnReload(GameMode currentGameMode) //from KKAPI.Chara when characters enter reload state
        {
            if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode /*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            { return; }//if disabled don't run
            //base.OnReload(currentGameMode);
            if (GameMode.Maker == currentGameMode && ExpandedOutfit.ResetMaker.Value)
            {
                OutfitDecider.Reset = true;
                if (!ExpandedOutfit.PermReset.Value)
                {
                    ExpandedOutfit.ResetMaker.Value = false;
                }
            }
            //use Chacontrol.name instead of ChaControl.fileParam.fullname to probably avoid same name conflicts
            if (ChaControl.sex == 1 && (GameMode.Maker == currentGameMode || OutfitDecider.Reset || !OutfitDecider.ProcessedNames.Contains(ChaControl.name)))//run the following if female and unprocessed
            {
                if (currentGameMode == GameMode.MainGame || ExpandedOutfit.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    OutfitDecider.Decision(ChaControl.fileParam.fullname);//Generate outfits
                    OutfitDecider.ProcessedNames.Add(ChaControl.name);//character is processed
                    if (!ExpandedOutfit.PermChangeOutfit.Value)
                    {
                        ExpandedOutfit.ChangeOutfit.Value = false;
                    }
                }
                int HoldOutfit = ChaControl.fileStatus.coordinateType;
                ClothingLoader.FullLoad(ChaControl);//Load outfits
                ChaControl.fileStatus.coordinateType = HoldOutfit;
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