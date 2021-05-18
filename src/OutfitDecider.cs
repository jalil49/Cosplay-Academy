using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    public static class OutfitDecider
    {
        private static readonly OutfitData[] outfitData;
        private static bool IsInitialized;

        private static ChaDefault ThisOutfitData;
        private static int HExperience;
        private static int LastHeroineClub = -1;
        static OutfitDecider()
        {
            IsInitialized = false;
            outfitData = new OutfitData[Constants.InputStrings.Length];
            for (int i = 0, n = outfitData.Length; i < n; i++)
            {
                outfitData[i] = new OutfitData();
            }
        }

        public static void ResetDecider()
        {
            if (IsInitialized)
            {
                foreach (OutfitData data in outfitData)
                {
                    data.Clear();
                }
            }
            Constants.ChaDefaults.ForEach(x => x.processed = false);
            IsInitialized = false;
            LastHeroineClub = -1;
            ExpandedOutfit.Logger.LogInfo("Reset has occured");
        }

        public static void Decision(string name, ChaDefault cha)
        {
            ThisOutfitData = cha;
            SaveData.Heroine person = ThisOutfitData.heroine;
            if (!IsInitialized)
            {
                Get_Outfits();
                IsInitialized = true;
                foreach (OutfitData data in outfitData)
                {
                    data.Coordinate();
                }
            }
            if (person != null)
            {
                for (int i = 0; i < outfitData.Length; i++)
                {
                    outfitData[i].Anger = person.isAnger;
                }
                HExperience = (int)person.HExperience;
            }
            else
            {
                HExperience = (int)ExpandedOutfit.MakerHstate.Value;
            }
            UnderwearChoice();
            if (ExpandedOutfit.RandomizeUnderwearOnly.Value)
            {
                if (person != null)
                {
                    ExpandedOutfit.Logger.LogDebug(name + " is processed.");
                }
                return;
            }

            //Set up Normal uniform
            ExpandedOutfit.Logger.LogDebug("Uniform");
            UniformOutfit();
            ExpandedOutfit.Logger.LogDebug("Uniform completed");

            //set up after school outfit
            ExpandedOutfit.Logger.LogDebug("AfterSchool");
            AfterSchoolOutfit();
            ExpandedOutfit.Logger.LogDebug("AfterSchool completed");

            //set up gym outfits
            ExpandedOutfit.Logger.LogDebug("Gym");
            GymOutfit();
            ExpandedOutfit.Logger.LogDebug("Gym completed");

            //set up swim outfit
            ExpandedOutfit.Logger.LogDebug("Swim");
            SwimOutfit();
            ExpandedOutfit.Logger.LogDebug("Swim completed");

            //set up Club outfits
            ExpandedOutfit.Logger.LogDebug("Clubs");
            if (person == null && LastHeroineClub != -1)
            {
                ClubOutfit(LastHeroineClub);
            }
            else
            {
                LastHeroineClub = (person == null ? (int)ExpandedOutfit.ClubChoice.Value : person.clubActivities);
                ClubOutfit(LastHeroineClub);
            }
            ExpandedOutfit.Logger.LogDebug("Clubs completed");

            ExpandedOutfit.Logger.LogDebug("Casual");
            CasualOutfit();
            ExpandedOutfit.Logger.LogDebug("Casual completed");

            ExpandedOutfit.Logger.LogDebug("Nightwear");
            NightOutfit();
            ExpandedOutfit.Logger.LogDebug("Nightwear completed");

            //If Characters can use casual outfits after school
            if (ExpandedOutfit.AfterSchoolCasual.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= ExpandedOutfit.AfterSchoolcasualchance.Value)
                {
                    ThisOutfitData.outfitpath[1] = ThisOutfitData.outfitpath[5];//assign casual outfit to afterschool
                }
            }
            if (person != null)
            {
                ExpandedOutfit.Logger.LogDebug(name + " is processed.");
            }
        }
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
                    if (ExpandedOutfit.ListOverrideBool[set].Value)
                    {
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(ExpandedOutfit.ListOverride[set].Value, false); //when sets are enabled don't include them in rolls, but do if disabled
                        outfitData[set].Insert(exp, temp2.ToArray(), true);//assign "is" set and store data
                        continue;
                    }
                    if (outfitData[set].IsSet(exp))//Skip set items
                    {
                        continue;
                    }
                    temp2 = DirectoryFinder.Grab_All_Files(coordinatepath + "coordinate" + Input1 + Input2);
                    if (Input1 == @"\AfterSchool" && ExpandedOutfit.GrabUniform.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Files(coordinatepath + @"coordinate\School Uniform" + Input2));
                    }
                    else if (Input1 == @"\Club\Swim" && ExpandedOutfit.GrabSwimsuits.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Files(coordinatepath + @"coordinate\Swimsuit" + Input2));
                    }
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];
                    if (!ExpandedOutfit.EnableSets.Value || !result.Contains(@"\Sets\"))
                    {
                        string choosen = Grabber(Input1, result);
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(coordinatepath + "coordinate" + choosen + Input2, ExpandedOutfit.EnableSets.Value); //when sets are enabled don't include them in rolls, but do if disabled
                        if (ExpandedOutfit.EnableDefaults.Value)
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
                        if (!ExpandedOutfit.IndividualSets.Value)
                        {
                            Setsfunction(array);
                        }
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result, false);
                        outfitData[set].Insert(exp, temp2.ToArray(), true);//assign "is" set and store data
                    }
                }
            }
        }
        private static void Setsfunction(string[] result)
        {
            foreach (string item in result)
            {
                string[] split = item.Split('\\');
                int exp = 0;
                foreach (var folder in split.Reverse())//reverse cause it's probably faster to start at rear, but rear can be longer than forward; for loop might be faster tho
                {
                    try
                    {
                        HStates temp = (HStates)Enum.Parse(typeof(HStates), folder, true);
                        if (Enum.IsDefined(typeof(HStates), temp))
                        {
                            exp = (int)temp;
                            break;
                        }
                    }
                    catch
                    { }
                }
                for (int j = 0, n = outfitData.Length; j < n; j++)
                {
                    if (item.Contains(Constants.InputStrings[j]))
                    {
                        if (ExpandedOutfit.FullSet.Value && outfitData[j].IsSet(exp))
                        {
                            break;
                        }
                        //outfitData[j].Path_set(exp, result[i]);
                        List<string> temp = DirectoryFinder.Get_Outfits_From_Path(item, false);
                        outfitData[j].Insert(exp, temp.ToArray(), true);
                        break;
                    }
                    else if (j == outfitData.Length - 1)
                    {
                        ExpandedOutfit.Logger.LogWarning("Fail :" + item + " Hexp: " + exp);
                    }
                }
            }
        }
        private static void UniformOutfit()
        {
            Generalized_Assignment(ExpandedOutfit.MatchUniform.Value, 0, 0);
        }
        private static void AfterSchoolOutfit()
        {
            if (ExpandedOutfit.AfterUniform.Value /*|| ExpandedOutfit.EnableSets.Value && Constants.outfitpath[1].Contains(@"\Sets\")*/)
            {
                Generalized_Assignment(ExpandedOutfit.AfterUniform.Value, 1, 1);
            }
            else
            {
                ThisOutfitData.outfitpath[1] = ThisOutfitData.outfitpath[0];
            }
        }
        private static void GymOutfit()
        {
            Generalized_Assignment(ExpandedOutfit.MatchGym.Value, 2, 2);
        }
        private static void SwimOutfit()
        {
            Generalized_Assignment(ExpandedOutfit.MatchSwim.Value, 3, 3);
        }
        private static void ClubOutfit(int club)
        {
            switch (club)
            {
                case 1:
                    Generalized_Assignment(ExpandedOutfit.MatchSwimClub.Value, 4, 4);
                    break;

                case 2:
                    Generalized_Assignment(ExpandedOutfit.MatchMangaClub.Value, 4, 5);
                    break;

                case 3:
                    Generalized_Assignment(ExpandedOutfit.MatchCheerClub.Value, 4, 6);
                    break;

                case 4:
                    Generalized_Assignment(ExpandedOutfit.MatchTeaClub.Value, 4, 7);
                    break;

                case 5:
                    Generalized_Assignment(ExpandedOutfit.MatchTrackClub.Value, 4, 8);
                    break;
                default:
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.outfitpath[0];
                    break;
            }
            ThisOutfitData.Koipath = outfitData[11].RandomSet(HExperience, ExpandedOutfit.MatchKoiClub.Value);
            if (ThisOutfitData.heroine == null ? ExpandedOutfit.KoiClub.Value : ThisOutfitData.heroine.isStaff && ExpandedOutfit.KeepOldBehavior.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= ExpandedOutfit.KoiChance.Value)
                {
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.Koipath;
                }
            }
        }
        private static void CasualOutfit()
        {
            Generalized_Assignment(ExpandedOutfit.MatchCasual.Value, 5, 9);
        }
        private static void NightOutfit()
        {
            Generalized_Assignment(ExpandedOutfit.MatchNightwear.Value, 6, 10);
        }
        private static void UnderwearChoice()
        {
            ThisOutfitData.Underwear = outfitData[12].RandomSet(HExperience, ExpandedOutfit.MatchUnderwear.Value);
        }
        private static string Generalized_Assignment(bool uniform_type, int Path_Num, int Data_Num)
        {
            return ThisOutfitData.outfitpath[Path_Num] = outfitData[Data_Num].RandomSet(HExperience, uniform_type);
        }
        private static string Grabber(string Input1, string result)
        {
            if (Input1 == @"\AfterSchool")
            {
                string[] split = result.Split('\\');
                for (int i = split.Length - 1; i >= 0; i--)
                {
                    if (split[i] == "AfterSchool")
                    {
                        break;
                    }
                    else if (split[i] == "School Uniform")
                    {
                        return @"\School Uniform";
                    }
                }
            }
            else if (Input1 == @"\Club\Swim")
            {
                string[] split = result.Split('\\');

                for (int i = split.Length - 1; i >= 0; i--)
                {
                    if (split[i] == @"Swim")
                    {
                        break;
                    }
                    else if (split[i] == "Swimsuit")
                    {
                        return @"\Swimsuit";
                    }
                }
            }
            return Input1;
        }
    }
}


