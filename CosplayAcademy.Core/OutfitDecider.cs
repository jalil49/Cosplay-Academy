using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    public static partial class OutfitDecider
    {
        private static readonly OutfitData[] outfitData;

        private static ChaDefault ThisOutfitData;
        private static int HExperience;
        private static int RandHExperience;

        static OutfitDecider()
        {
            outfitData = new OutfitData[Constants.InputStrings.Length];
            for (int i = 0, n = outfitData.Length; i < n; i++)
            {
                outfitData[i] = new OutfitData();
            }
        }

        public static void ResetDecider()
        {
            foreach (OutfitData data in outfitData)
            {
                data.Clear();
            }
            Constants.ChaDefaults.ForEach(x => x.processed = false);
#if KK
            ChaDefault.LastClub = -1;
#endif
            OutfitData.Anger = false;
            Get_Outfits();
            foreach (var data in outfitData)
            {
                data.Coordinate();
            }
        }

        private static void Get_Outfits()
        {
            List<string> temp2;
            string coordinatepath = Settings.CoordinatePath.Value;
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
                    temp2 = DirectoryFinder.Grab_All_Directories(coordinatepath + Input1 + Input2);
#if KK
                    Grabber(temp2, set, Input2);
#endif
                    string result = temp2[UnityEngine.Random.Range(0, temp2.Count)];
                    if (!Settings.EnableSets.Value || !result.Contains(@"\Sets\"))
                    {
                        temp2 = DirectoryFinder.Get_Outfits_From_Path(result, Settings.EnableSets.Value); //when sets are enabled don't include them in rolls, but do if disabled
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


        public static void Decision(string name, ChaDefault cha)
        {
            ThisOutfitData = cha;
#if !KKS
            SaveData.Heroine person = ThisOutfitData.heroine;

            if (person != null)
            {
                OutfitData.Anger = person.isAnger;
                HExperience = (int)person.HExperience;
            }
            else
            {
#endif
                OutfitData.Anger = false;
                HExperience = (int)Settings.MakerHstate.Value;
#if !KKS
            }
#endif
            RandHExperience = UnityEngine.Random.Range(0, HExperience + 1);
            for (int i = 0; i < Constants.InputStrings.Length; i++)
            {
                Generalized_Assignment(Settings.MatchGeneric[i].Value, i, i);
            }

            SpecialProcess();
#if !KKS
            if (person != null)
            {
                Settings.Logger.LogDebug(name + " is processed.");
            }
#endif
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
                        var coordinatepath = Settings.CoordinatePath.Value;
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
                    return ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].Random(RandHExperience, uniform_type);
                case Hexp.Maximize:
                    return ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].Random(HExperience, uniform_type);
                default:
                    return ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].RandomSet(HExperience, uniform_type);
            }
        }
    }
}


