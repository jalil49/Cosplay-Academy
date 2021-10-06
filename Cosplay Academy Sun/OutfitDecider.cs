using System.Collections.Generic;

namespace Cosplay_Academy
{
    public static partial class OutfitDecider
    {
        private static void Grabber(ref List<FolderData> temp2, int set, int exp)
        {
            if (set == 1 && Settings.GrabSwimsuits.Value)
            {
                temp2.AddRange(DataStruct.DefaultFolder[8].FolderData[exp].GetAllFolders());
                return;
            }
        }

        private static void SpecialProcess()
        {

        }
    }
}


