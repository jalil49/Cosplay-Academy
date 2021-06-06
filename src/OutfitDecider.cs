using System.Collections.Generic;
using System.IO;

namespace Cosplay_Academy
{
    public static partial class OutfitDecider
    {
        private static void Get_Outfits()
        {
            List<string> temp2;
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            int set = -1;//-1 so it can be on top of foreach
            foreach (string Input1 in Constants.InputStrings)
            {
                set++;
                int exp = -1;
                foreach (string Input2 in Constants.InputStrings2)
                {
                    exp++;
                    if (Settings.ListOverrideBool[set].Value)
                    {
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(Settings.ListOverride[set].Value, null, false); //when sets are enabled don't include them in rolls, but do if disabled
                        outfitData[set].Insert(exp, temp2.ToArray(), true);//assign "is" set and store data
                        continue;
                    }
                    if (outfitData[set].IsSet(exp))//Skip set items
                    {
                        continue;
                    }
                    temp2 = DirectoryFinder.Grab_All_Directories(coordinatepath + "coordinate" + Input1 + Input2, (Settings.UseAlternativePath.Value) ? Settings.AlternativePath.Value + "coordinate" + Input1 + Input2 : null);
                    if (Input1 == @"\AfterSchool" && Settings.GrabUniform.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + @"coordinate\School Uniform" + Input2, (Settings.UseAlternativePath.Value) ? Settings.AlternativePath.Value + @"coordinate\School Uniform" + Input2 : null));
                    }
                    else if (Input1 == @"\Club\Swim" && Settings.GrabSwimsuits.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + @"coordinate\Swimsuit" + Input2, (Settings.UseAlternativePath.Value) ? Settings.AlternativePath.Value + @"coordinate\Swimsuit" + Input2 : null));
                    }
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];
                    if (!Settings.EnableSets.Value || !result.Contains(@"\Sets\"))
                    {
                        string choosen = Grabber(Input1, result, coordinatepath + "coordinate", Input2);
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(choosen, string.Copy(choosen).Replace(coordinatepath, Settings.AlternativePath.Value), Settings.EnableSets.Value); //when sets are enabled don't include them in rolls, but do if disabled
                        if (Settings.EnableDefaults.Value && temp2.Count != 1)
                        {
                            temp2.Add("Defaults");
                        }
                        outfitData[set].Insert(exp, temp2.ToArray(), false);//Assign "not" set and store data
                    }
                    else
                    {
                        string[] split = result.Split('\\');
                        temp2 = DirectoryFinder.Get_Set_Paths(@"\Sets\" + split[split.Length - 1]);
                        string[] array = temp2.ToArray();//this area of the code is unstable for unknown reason as temp2 will be corrupted by setsfunction have to store in array
                        if (!Settings.IndividualSets.Value)
                        {
                            Setsfunction(array);
                        }
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result, string.Copy(result).Replace(coordinatepath, Settings.AlternativePath.Value), false);
                        if (Settings.EnableDefaults.Value && temp2.Count != 1)
                        {
                            temp2.Add("Defaults");
                        }
                        outfitData[set].Insert(exp, temp2.ToArray(), true);//assign "is" set and store data
                    }
                }
            }
        }

        private static void SpecialProcess()
        {
            ThisOutfitData.KoiOutfitpath = outfitData[9].RandomSet(HExperience, Settings.MatchGeneric[9].Value);
            if (ThisOutfitData.heroine == null ? Settings.KoiClub.Value : ThisOutfitData.heroine.isStaff && Settings.KeepOldBehavior.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
                {
                    ThisOutfitData.alloutfitpaths[4] = ThisOutfitData.KoiOutfitpath;
                }
            }

            //If Characters can use casual outfits after school
            if (Settings.AfterSchoolCasual.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= Settings.AfterSchoolcasualchance.Value)
                {
                    ThisOutfitData.alloutfitpaths[1] = ThisOutfitData.alloutfitpaths[5];//assign casual outfit to afterschool
                }
            }
        }
    }
}


