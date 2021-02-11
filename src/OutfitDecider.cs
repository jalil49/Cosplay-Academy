using Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cosplay_Academy
{
    public static class OutfitDecider
    {
        private static OutfitData[] outfitData;
        private static bool IsInitialized;

        public static List<string> ProcessedNames; //list of processed characters
        private static readonly Game _gameMgr;
        private static List<SaveData.Heroine> heroines;
        private static SaveData.Heroine person;
        public static bool Reset; //

        private static int HExperience;

        static OutfitDecider()
        {
            _gameMgr = Game.Instance;
            ProcessedNames = new List<string>();//initiate string
            IsInitialized = false;
            Reset = true;
            outfitData = new OutfitData[Constants.InputStrings.Length];
            for (int i = 0; i < outfitData.Length; i++)
            {
                outfitData[i] = new OutfitData();
            }
        }



        public static void Decision(string name)
        {

            if (Reset)//Initialize upon request first happens on load event
            {
                ProcessedNames.Clear(); //reset list
                Reset = false;
                if (IsInitialized)
                {
                    for (int i = 0; i < outfitData.Length; i++)
                    {
                        outfitData[i].Clear();
                    }
                }
                IsInitialized = false;

                ExpandedOutfit.Logger.LogInfo("Reset has occured");
            }

            heroines = _gameMgr.HeroineList;
            for (int i = 0; i < heroines.Count; i++)
            {
                if (name == heroines[i].Name)
                {
                    person = heroines[i];
                    break;
                }
                else if (i == heroines.Count - 1)
                { return; }
            }

            if (!IsInitialized)
            {
                Get_Outfits();
                GrabUniform();
                GrabSwimsuits();
                IsInitialized = true;
                for (int i = 0; i < outfitData.Length; i++)
                {
                    outfitData[i].Anger = person.isAnger;
                    outfitData[i].Coordinate();
                }
                HExperience = (int)person.HExperience;
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
            ClubOutfit(person.clubActivities);
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
                if (UnityEngine.Random.Range(0, 2) == 1)//%50 chance
                {
                    Constants.outfitpath[1] = Constants.outfitpath[5];//assign casual outfit to afterschool
                }
            }

            ExpandedOutfit.Logger.LogDebug(name + " is processed.");
            ProcessedNames.Add(name);//character is processed
        }
        private static void Get_Outfits()
        {
            List<string> temp2;
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            int set = -1;//-1 so it can be on top of for each
            foreach (string Input1 in Constants.InputStrings)
            {
                set++;
                int exp = -1;
                foreach (string Input2 in Constants.InputStrings2)
                {
                    exp++;
                    if (outfitData[set].IsSet(exp))//Skip set items
                    {
                        continue;
                    }
                    temp2 = DirectoryFinder.Grab_All_Files(coordinatepath + "coordinate" + Input1 + Input2);
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];

                    if (!result.Contains(@"\Sets\") || !ExpandedOutfit.EnableSets.Value)
                    {
                        //outfitData[i].Path_set(j, coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j]);
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(coordinatepath + "coordinate" + Input1 + Input2);
                        outfitData[set].Insert(exp, temp2.ToArray(), false);//Assign "not" set and store data
                    }
                    else
                    {
                        string[] split = result.Split('\\');
                        //outfitData[i].Path_set(j, coordinatepath + InputStrings[i] + InputStrings2[j] + @"\" + split[split.Length - 2] + @"\" + split[split.Length - 1]);
                        temp2 = DirectoryFinder.Get_Set_Paths(@"\Sets\" + split[split.Length - 1]);
                        if (ExpandedOutfit.FullSet.Value)
                        {
                            string[] array = temp2.ToArray();//this area of the code is unstable for unknown reason as temp2 will be corrupted by setsfunction have to store in array
                            Setsfunction(array);
                        }
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result);
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
                    HStates temp = (HStates)Enum.Parse(typeof(HStates), folder, true);
                    if (Enum.IsDefined(typeof(HStates), temp))
                    {
                        exp = (int)temp;
                        break;
                    }
                }
                for (int j = 0; j < outfitData.Length; j++)
                {
                    if (item.Contains(Constants.InputStrings[j]))
                    {
                        if (outfitData[j].IsSet(exp))
                        {
                            break;
                        }
                        //outfitData[j].Path_set(exp, result[i]);
                        List<string> temp = DirectoryFinder.Get_Outfits_From_Path(item);
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
            Generalized_Assignment(ExpandedOutfit.AfterUniform.Value, 1, 1);
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
                    Constants.outfitpath[4] = Constants.outfitpath[0];
                    break;
            }
            if (person.isStaff)
            {
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    Generalized_Assignment(ExpandedOutfit.MatchKoiClub.Value, 4, 11);
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
        private static void GrabUniform()
        {
            if (ExpandedOutfit.GrabUniform.Value)
            {
                for (int i = 0; i < Constants.InputStrings2.Length; i++) //0 is FirstTime to 3 which is lewd
                {
                    if (outfitData[0].IsSet(i))
                    {
                        outfitData[1].Insert(i, outfitData[0].Exportarray(i), true);
                    }
                }
            }
        }
        private static void GrabSwimsuits()
        {
            if (ExpandedOutfit.GrabSwimsuits.Value)
            {
                for (int i = 0; i < Constants.InputStrings2.Length; i++) //0 is FirstTime to 3 which is lewd
                {
                    if (outfitData[5].IsSet(i))
                    {
                        outfitData[5].Insert(i, outfitData[3].Exportarray(i), true);
                    }
                }
            }
        }
        private static void Generalized_Assignment(bool uniform_type, int Path_Num, int Data_Num)
        {
            if (uniform_type)
            {
                Constants.outfitpath[Path_Num] = outfitData[Data_Num].RandomSet(HExperience, uniform_type);
            }
            else
            {
                Constants.outfitpath[Path_Num] = outfitData[Data_Num].Random(UnityEngine.Random.Range(0, HExperience) + 1);
            }
        }
    }
}


