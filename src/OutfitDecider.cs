using System;
using System.Collections.Generic;

namespace Cosplay_Academy
{
    public static partial class OutfitDecider
    {
        private static void Grabber(List<string> temp2, int set, string Input2)
        {
            string coordinatepath = Settings.CoordinatePath.Value;
            if (set == 1 && Settings.GrabUniform.Value)
            {
                temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + Constants.InputStrings[0] + Input2));
            }
            else if (set == 4 && Settings.GrabSwimsuits.Value)
            {
                temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + Constants.InputStrings[3] + Input2));
            }
        }

        private static void SpecialProcess()
        {
            ThisOutfitData.KoiOutfitpath = outfitData[9].RandomSet(HExperience, Settings.MatchGeneric[9].Value);

            if (!Settings.MatchGeneric[1].Value)
            {
                ThisOutfitData.alloutfitpaths[1] = ThisOutfitData.alloutfitpaths[0];
            }

            //If Characters can use casual outfits after school
            if (Settings.AfterSchoolCasual.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= Settings.AfterSchoolcasualchance.Value)
                {
                    ThisOutfitData.alloutfitpaths[1] = ThisOutfitData.alloutfitpaths[10];//assign casual outfit to afterschool
                }
            }
        }
    }
}


