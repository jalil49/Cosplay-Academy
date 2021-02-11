namespace Cosplay_Academy
{
    public static class ClothingLoader
    {
        private static ChaControl chaControl;
        public static void FullLoad(ChaControl input)
        {
            chaControl = input;
            UniformLoad();
            AfterSchoolLoad();
            GymLoad();
            SwimLoad();
            ClubLoad();
            CasualLoad();
            NightwearLoad();
        }
        private static void UniformLoad()
        {
            chaControl.chaFile.coordinate[0].LoadFile(Constants.outfitpath[0]);
            ExpandedOutfit.Logger.LogDebug("loaded 0 " + Constants.outfitpath[0]);
        }
        private static void AfterSchoolLoad()
        {
            chaControl.chaFile.coordinate[1].LoadFile(Constants.outfitpath[1]);
            ExpandedOutfit.Logger.LogDebug("loaded 1 " + Constants.outfitpath[1]);
        }
        private static void GymLoad()
        {
            chaControl.chaFile.coordinate[2].LoadFile(Constants.outfitpath[2]);
            ExpandedOutfit.Logger.LogDebug("loaded 2 " + Constants.outfitpath[2]);
        }
        private static void SwimLoad()
        {
            chaControl.chaFile.coordinate[3].LoadFile(Constants.outfitpath[3]);
            ExpandedOutfit.Logger.LogDebug("loaded 3 " + Constants.outfitpath[3]);
        }
        private static void ClubLoad()
        {
            chaControl.chaFile.coordinate[4].LoadFile(Constants.outfitpath[4]);
            ExpandedOutfit.Logger.LogDebug("loaded 4 " + Constants.outfitpath[4]);
        }
        private static void CasualLoad()
        {
            chaControl.chaFile.coordinate[5].LoadFile(Constants.outfitpath[5]);
            ExpandedOutfit.Logger.LogDebug("loaded 5 " + Constants.outfitpath[5]);

        }
        private static void NightwearLoad()
        {
            chaControl.chaFile.coordinate[6].LoadFile(Constants.outfitpath[6]);
            ExpandedOutfit.Logger.LogDebug("loaded 6 " + Constants.outfitpath[6]);
        }
    }
}