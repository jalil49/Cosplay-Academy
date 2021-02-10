namespace Cosplay_Academy
{
    public static class ClothingLoader
    {
        public static ChaControl chaControl;
        public static void FullLoad()
        {
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
            chaControl.chaFile.coordinate[0].LoadFile(OutfitDecider.outfitpath[0]);
            ExpandedOutfit.Logger.LogDebug("loaded 0 " + OutfitDecider.outfitpath[0]);
        }
        private static void AfterSchoolLoad()
        {
            chaControl.chaFile.coordinate[1].LoadFile(OutfitDecider.outfitpath[1]);
            ExpandedOutfit.Logger.LogDebug("loaded 1 " + OutfitDecider.outfitpath[1]);
        }
        private static void GymLoad()
        {
            chaControl.chaFile.coordinate[2].LoadFile(OutfitDecider.outfitpath[2]);
            ExpandedOutfit.Logger.LogDebug("loaded 2 " + OutfitDecider.outfitpath[2]);
        }
        private static void SwimLoad()
        {
            chaControl.chaFile.coordinate[3].LoadFile(OutfitDecider.outfitpath[3]);
            ExpandedOutfit.Logger.LogDebug("loaded 3 " + OutfitDecider.outfitpath[3]);
        }
        private static void ClubLoad()
        {
            chaControl.chaFile.coordinate[4].LoadFile(OutfitDecider.outfitpath[4]);
            ExpandedOutfit.Logger.LogDebug("loaded 4 " + OutfitDecider.outfitpath[4]);
        }
        private static void CasualLoad()
        {
            chaControl.chaFile.coordinate[5].LoadFile(OutfitDecider.outfitpath[5]);
            ExpandedOutfit.Logger.LogDebug("loaded 5 " + OutfitDecider.outfitpath[5]);

        }
        private static void NightwearLoad()
        {
            chaControl.chaFile.coordinate[6].LoadFile(OutfitDecider.outfitpath[6]);
            ExpandedOutfit.Logger.LogDebug("loaded 6 " + OutfitDecider.outfitpath[6]);
        }
    }
}