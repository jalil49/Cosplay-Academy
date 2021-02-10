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

        public static string[] outfitpath = { " ", " ", " ", " ", " ", " ", " " };
        private static readonly string[] InputStrings = { @"\School Uniform" , @"\AfterSchool", @"\Gym" , @"\Swimsuit" , @"\Club\Swim" ,
            @"\Club\Manga", @"\Club\Cheer", @"\Club\Tea", @"\Club\Track", @"\Casual" , @"\Nightwear", @"\Club\Koi" };
        private static readonly string[] InputStrings2 = { @"\FirstTime", @"\Amateur", @"\Pro", @"\Lewd" };

        private static OutfitData[] outfitData;
        private static bool IsInitialized;

        private static bool Anger;

        public static List<string> ProcessedNames; //list of processed characters
        private static readonly Game _gameMgr;
        private static List<SaveData.Heroine> heroines;
        private static SaveData.Heroine person;
        public static bool Reset; //
        private static bool OutfitCoordination = false;


        private static int HExperience //Get copied from in-game file but modified into int and anger
        {
            get
            {
                person.lewdness = Mathf.Clamp(person.lewdness, 0, 100);
                if (person.hCount >= 1 && !Anger)
                {
                    List<float> temp=new List<float>();
                    temp.AddRange(person.hAreaExps);
                    //Array.Copy(person.hAreaExps, 1, array, 0, person.hAreaExps.Length);
                    if (person.countKokanH >= 100f && person.countAnalH >= 100f)
                    {
                        if (!temp.Any((float a) => a < 100f))
                        {
                            return (person.lewdness != 100) ? 2 : 3;
                        }
                    }
                    return 1;
                }
                return 0;
            }
        }



        static OutfitDecider()
        {
            _gameMgr = Game.Instance;
            ProcessedNames = new List<string>();//initiate string
            IsInitialized = false;
            Reset = true;
            outfitData = new OutfitData[12];
            for (int i = 0; i < 12; i++)
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
                OutfitCoordination = false;
                if (IsInitialized)
                {
                    for (int i = 0; i < 12; i++)
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
            }
            if (!OutfitCoordination)
            {
                for (int i = 0; i < 12; i++)
                {
                    outfitData[i].Coordinate();
                }
                OutfitCoordination = true;
            }
            Anger = person.isAnger;
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
                    outfitpath[1] = outfitpath[5];//assign casual outfit to afterschool
                }
            }

            ExpandedOutfit.Logger.LogDebug(name + " is processed.");
            ProcessedNames.Add(name);//character is processed

        }
        private static void Get_Outfits()
        {
            List<string> temp2;
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            for (int i = 0; i < 12; i++)
            {

                for (int j = 0; j < 4; j++)
                {

                    if (outfitData[i].IsSet(j))//Skip set items
                    {
                        continue;
                    }
                    temp2 = DirectoryFinder.Grab_All_Files(coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j]);
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];

                    if (!result.Contains(@"\Sets\"))
                    {
                        outfitData[i].Path_set(j, coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j]);
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j]);
                        outfitData[i].Insert(j, temp2.ToArray(), false);//store the data not set
                    }
                    else
                    {
                        string[] split = result.Split('\\');
                        outfitData[i].Path_set(j, coordinatepath + InputStrings[i] + InputStrings2[j] + @"\" + split[split.Length - 2] + @"\" + split[split.Length - 1]);
                        temp2 = DirectoryFinder.Get_Set_Paths(@"\Sets\" + split[split.Length - 1]);
                        if (ExpandedOutfit.FullSet.Value)
                        {
                            string[] array = temp2.ToArray();//this area of the code is unstable for unknown reason as temp2 will be corrupted by setsfunction have to store in array
                            Setsfunction(array);
                        }
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result);
                        outfitData[i].Insert(j, temp2.ToArray(), true);//assign set and store data
                    }
                }
            }
        }
        private static void Setsfunction(string[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                int exp = 0;
                if (result[i].Contains("Amateur"))
                { exp = 1; }
                else if (result[i].Contains("Pro"))
                { exp = 2; }
                else if (result[i].Contains("Lewd"))
                { exp = 3; }
                for (int j = 0; j < 12; j++)
                {
                    if (result[i].Contains(InputStrings[j]))
                    {
                        if (outfitData[j].IsSet(exp))
                        {
                            break;
                        }
                        outfitData[j].Path_set(exp, result[i]);
                        List<string> temp = DirectoryFinder.Get_Outfits_From_Path(result[i]);
                        outfitData[j].Insert(exp, temp.ToArray(), true);
                        break;
                    }
                    else if (j == 11)
                    {
                        ExpandedOutfit.Logger.LogWarning("Fail :" + result[i] + " Hexp: " + exp);
                    }
                }
            }
        }
        private static void UniformOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[0].SetExists(HExperience))
            {
                outfitpath[0] = outfitData[0].RandomSet(HExperience);
            }
            else if (ExpandedOutfit.MatchUniform.Value)
            {
                outfitpath[0] = outfitData[0].RandomPath(UnityEngine.Random.Range(0, HExperience) + 1);
            }
            else
            {
                outfitpath[0] = outfitData[0].Random(UnityEngine.Random.Range(0, HExperience) + 1);
            }
        }
        private static void AfterSchoolOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[1].SetExists(HExperience))
            {
                outfitpath[1] = outfitData[1].RandomSet(HExperience);
            }
            else if (ExpandedOutfit.AfterUniform.Value)
            {
                outfitpath[1] = outfitData[1].RandomPath(HExperience);
            }
            else
            {
                outfitpath[1] = outfitData[1].Random(UnityEngine.Random.Range(0, HExperience) + 1);
            }

        }
        private static void GymOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[2].SetExists(HExperience))
            {
                outfitpath[2] = outfitData[2].RandomSet(HExperience);
            }
            else if (ExpandedOutfit.MatchGym.Value)
            {
                outfitpath[2] = outfitData[2].RandomPath(HExperience);
            }
            else
            {
                outfitpath[2] = outfitData[2].Random(UnityEngine.Random.Range(0, HExperience + 1));
            }
        }
        private static void SwimOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[3].SetExists(HExperience))
            {
                outfitpath[3] = outfitData[3].RandomSet(HExperience);
            }
            else if (ExpandedOutfit.MatchSwim.Value)
            {
                outfitpath[3] = outfitData[3].RandomPath(HExperience);
            }
            else
            {
                outfitpath[3] = outfitData[3].Random(UnityEngine.Random.Range(0, HExperience + 1));
            }
        }
        private static void ClubOutfit(int club)
        {
            switch (club)
            {
                case 1:
                    if (ExpandedOutfit.EnableSets.Value && outfitData[club + 3].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[club + 3].RandomSet(HExperience);
                    }
                    else if (ExpandedOutfit.MatchSwimClub.Value)
                    {
                        outfitpath[4] = outfitData[club + 3].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[club + 3].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }
                    break;

                case 2:
                    if (ExpandedOutfit.EnableSets.Value && outfitData[club + 3].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[club + 3].RandomSet(HExperience);
                    }

                    else if (ExpandedOutfit.MatchMangaClub.Value)
                    {
                        outfitpath[4] = outfitData[club + 3].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[club + 3].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }
                    break;

                case 3:
                    if (ExpandedOutfit.EnableSets.Value && outfitData[club + 3].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[club + 3].RandomSet(HExperience);
                    }

                    else if (ExpandedOutfit.MatchCheerClub.Value)
                    {
                        outfitpath[4] = outfitData[club + 3].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[club + 3].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }
                    break;

                case 4:
                    if (ExpandedOutfit.EnableSets.Value && outfitData[club + 3].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[club + 3].RandomSet(HExperience);
                    }

                    else if (ExpandedOutfit.MatchTeaClub.Value)
                    {
                        outfitpath[4] = outfitData[club + 3].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[club + 3].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }
                    break;

                case 5:
                    if (ExpandedOutfit.EnableSets.Value && outfitData[club + 3].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[club + 3].RandomSet(HExperience);
                    }

                    else if (ExpandedOutfit.MatchTrackClub.Value)
                    {
                        outfitpath[4] = outfitData[club + 3].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[club + 3].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }
                    break;
                default:
                    outfitpath[4] = outfitpath[0];
                    break;
            }
            if (person.isStaff)
            {
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    if (ExpandedOutfit.EnableSets.Value && outfitData[11].SetExists(HExperience))
                    {
                        outfitpath[4] = outfitData[11].RandomSet(HExperience);
                    }

                    else if (ExpandedOutfit.MatchKoiClub.Value)
                    {
                        outfitpath[4] = outfitData[11].RandomPath(HExperience);
                    }
                    else
                    {
                        outfitpath[4] = outfitData[11].Random(UnityEngine.Random.Range(0, HExperience + 1));
                    }

                }
            }

        }
        private static void CasualOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[9].SetExists(HExperience))
            {
                outfitpath[5] = outfitData[9].RandomSet(HExperience);
            }

            else if (!ExpandedOutfit.MatchCasual.Value)
            {
                outfitpath[5] = outfitData[9].RandomPath(HExperience);
            }
            else
            {
                outfitpath[5] = outfitData[9].Random(UnityEngine.Random.Range(0, HExperience + 1));
            }
        }
        private static void NightOutfit()
        {
            if (ExpandedOutfit.EnableSets.Value && outfitData[10].SetExists(HExperience))
            {
                outfitpath[6] = outfitData[10].RandomSet(HExperience);
            }

            else if (!ExpandedOutfit.MatchNightwear.Value)
            {
                outfitpath[6] = outfitData[10].RandomPath(HExperience);
            }
            else
            {
                outfitpath[6] = outfitData[10].Random(UnityEngine.Random.Range(0, HExperience + 1));
            }
        }
        private static void GrabUniform()
        {
            if (ExpandedOutfit.GrabUniform.Value)
            {
                for (int i = 0; i < 4; i++)
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
                for (int i = 0; i < 4; i++)
                {
                    if (outfitData[5].IsSet(i))
                    {
                        outfitData[5].Insert(i, outfitData[3].Exportarray(i), true);
                    }
                }
            }
        }
    }
}


