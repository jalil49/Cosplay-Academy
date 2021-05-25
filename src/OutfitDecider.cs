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
        private static int RandHExperience;
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
            Settings.Logger.LogInfo("Reset has occured");
        }

        public static void Decision(string name, ChaDefault cha)
        {
            ThisOutfitData = cha;
            SaveData.Heroine person = ThisOutfitData.heroine;
            if (!IsInitialized)
            {
                OutfitData.Anger = false;
                Get_Outfits();
                IsInitialized = true;
                foreach (var data in outfitData)
                {
                    data.Coordinate();
                }
            }
            if (person != null)
            {
                OutfitData.Anger = person.isAnger;
                HExperience = (int)person.HExperience;
            }
            else
            {
                OutfitData.Anger = false;
                HExperience = (int)Settings.MakerHstate.Value;
            }
            RandHExperience = UnityEngine.Random.Range(0, HExperience + 1);
            UnderwearChoice();
            Settings.Logger.LogDebug("Underwear completed");

            if (Settings.RandomizeUnderwearOnly.Value)
            {
                if (person != null)
                {
                    Settings.Logger.LogDebug(name + " is processed.");
                }
                return;
            }

            //Set up Normal uniform
            UniformOutfit();
            Settings.Logger.LogDebug("Uniform completed");

            //set up after school outfit
            Settings.Logger.LogDebug("AfterSchool");
            AfterSchoolOutfit();
            Settings.Logger.LogDebug("AfterSchool completed");

            //set up gym outfits
            Settings.Logger.LogDebug("Gym");
            GymOutfit();
            Settings.Logger.LogDebug("Gym completed");

            //set up swim outfit
            Settings.Logger.LogDebug("Swim");
            SwimOutfit();
            Settings.Logger.LogDebug("Swim completed");

            //set up Club outfits
            Settings.Logger.LogDebug("Clubs");
            if (person == null && LastHeroineClub != -1)
            {
                ClubOutfit(LastHeroineClub);
            }
            else
            {
                LastHeroineClub = (person == null ? (int)Settings.ClubChoice.Value : person.clubActivities);
                ClubOutfit(LastHeroineClub);
            }
            Settings.Logger.LogDebug("Clubs completed");

            Settings.Logger.LogDebug("Casual");
            CasualOutfit();
            Settings.Logger.LogDebug("Casual completed");

            Settings.Logger.LogDebug("Nightwear");
            NightOutfit();
            Settings.Logger.LogDebug("Nightwear completed");

            //If Characters can use casual outfits after school
            if (Settings.AfterSchoolCasual.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= Settings.AfterSchoolcasualchance.Value)
                {
                    ThisOutfitData.outfitpath[1] = ThisOutfitData.outfitpath[5];//assign casual outfit to afterschool
                }
            }
            if (person != null)
            {
                Settings.Logger.LogDebug(name + " is processed.");
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
                    if (Settings.ListOverrideBool[set].Value)
                    {
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(Settings.ListOverride[set].Value, false); //when sets are enabled don't include them in rolls, but do if disabled
                        outfitData[set].Insert(exp, temp2.ToArray(), true);//assign "is" set and store data
                        continue;
                    }
                    if (outfitData[set].IsSet(exp))//Skip set items
                    {
                        continue;
                    }
                    temp2 = DirectoryFinder.Grab_All_Directories(coordinatepath + "coordinate" + Input1 + Input2);
                    if (Input1 == @"\AfterSchool" && Settings.GrabUniform.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + @"coordinate\School Uniform" + Input2));
                    }
                    else if (Input1 == @"\Club\Swim" && Settings.GrabSwimsuits.Value)
                    {
                        temp2.AddRange(DirectoryFinder.Grab_All_Directories(coordinatepath + @"coordinate\Swimsuit" + Input2));
                    }
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];
                    if (!Settings.EnableSets.Value || !result.Contains(@"\Sets\"))
                    {
                        string choosen = Grabber(Input1, result);
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(coordinatepath + "coordinate" + choosen + Input2, Settings.EnableSets.Value); //when sets are enabled don't include them in rolls, but do if disabled
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
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result, false);
                        if (Settings.EnableDefaults.Value && temp2.Count != 1)
                        {
                            temp2.Add("Defaults");
                        }
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
                        if (Settings.FullSet.Value && outfitData[j].IsSet(exp))
                        {
                            break;
                        }
                        List<string> temp = DirectoryFinder.Get_Outfits_From_Path(item, false);
                        outfitData[j].Insert(exp, temp.ToArray(), true);
                        break;
                    }
                    else if (j == outfitData.Length - 1)
                    {
                        Settings.Logger.LogWarning("Fail :" + item + " Hexp: " + exp);
                    }
                }
            }
        }

        private static void UniformOutfit()
        {
            Generalized_Assignment(Settings.MatchUniform.Value, 0, 0);
        }

        private static void AfterSchoolOutfit()
        {
            if (Settings.AfterUniform.Value /*|| ExpandedOutfit.EnableSets.Value && Constants.outfitpath[1].Contains(@"\Sets\")*/)
            {
                Generalized_Assignment(Settings.AfterUniform.Value, 1, 1);
            }
            else
            {
                ThisOutfitData.outfitpath[1] = ThisOutfitData.outfitpath[0];
            }
        }

        private static void GymOutfit()
        {
            Generalized_Assignment(Settings.MatchGym.Value, 2, 2);
        }

        private static void SwimOutfit()
        {
            Generalized_Assignment(Settings.MatchSwim.Value, 3, 3);
        }

        private static void ClubOutfit(int club)
        {
            switch (club)
            {
                case 1:
                    Generalized_Assignment(Settings.MatchSwimClub.Value, 4, 4);
                    break;

                case 2:
                    Generalized_Assignment(Settings.MatchMangaClub.Value, 4, 5);
                    break;

                case 3:
                    Generalized_Assignment(Settings.MatchCheerClub.Value, 4, 6);
                    break;

                case 4:
                    Generalized_Assignment(Settings.MatchTeaClub.Value, 4, 7);
                    break;

                case 5:
                    Generalized_Assignment(Settings.MatchTrackClub.Value, 4, 8);
                    break;

                default:
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.outfitpath[0];
                    break;
            }
            ThisOutfitData.KoiOutfitpath = outfitData[11].RandomSet(HExperience, Settings.MatchKoiClub.Value);
            if (ThisOutfitData.heroine == null ? Settings.KoiClub.Value : ThisOutfitData.heroine.isStaff && Settings.KeepOldBehavior.Value)
            {
                if (UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
                {
                    ThisOutfitData.outfitpath[4] = ThisOutfitData.KoiOutfitpath;
                }
            }
        }

        private static void CasualOutfit()
        {
            Generalized_Assignment(Settings.MatchCasual.Value, 5, 9);
        }

        private static void NightOutfit()
        {
            Generalized_Assignment(Settings.MatchNightwear.Value, 6, 10);
        }

        private static void UnderwearChoice()
        {
            Generalized_Assignment(Settings.MatchUnderwear.Value, Constants.Outfit_Size, 12);
        }

        private static string Generalized_Assignment(bool uniform_type, int Path_Num, int Data_Num)
        {
            switch (Settings.H_EXP_Choice.Value)
            {
                case Hexp.RandConstant:
                    return ThisOutfitData.outfitpath[Path_Num] = outfitData[Data_Num].Random(RandHExperience, uniform_type);
                case Hexp.Maximize:
                    return ThisOutfitData.outfitpath[Path_Num] = outfitData[Data_Num].Random(HExperience, uniform_type);
                default:
                    return ThisOutfitData.outfitpath[Path_Num] = outfitData[Data_Num].RandomSet(HExperience, uniform_type);
            }
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


