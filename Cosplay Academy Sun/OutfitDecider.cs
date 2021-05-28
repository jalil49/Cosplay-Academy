﻿using System;
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
            Settings.Logger.LogInfo("Reset has occured");
        }

        public static void Decision(string name, ChaDefault cha)
        {
            ThisOutfitData = cha;
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
            OutfitData.Anger = false;
            HExperience = (int)Settings.MakerHstate.Value;
            RandHExperience = UnityEngine.Random.Range(0, HExperience + 1);
            //UnderwearChoice();
            Generalized_Assignment(Settings.MatchUnderwear.Value, 4, 4);
            Settings.Logger.LogDebug("Underwear completed");

            if (Settings.RandomizeUnderwearOnly.Value)
            {
                return;
            }

            //CasualOutfit();
            Generalized_Assignment(Settings.MatchCasual.Value, 0, 0);

            //SwimOutfit();
            Generalized_Assignment(Settings.MatchSwim.Value, 1, 1);

            //NightOutfit();
            Generalized_Assignment(Settings.MatchNightwear.Value, 2, 2);

            //Bathroom
            Generalized_Assignment(Settings.MatchSwim.Value, 1, 1);

            Settings.Logger.LogDebug(name + " is processed.");
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
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];
                    if (!Settings.EnableSets.Value || !result.Contains(@"\Sets\"))
                    {
                        string choosen = result;
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
    }
}

