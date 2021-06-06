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
                        var coordinatepath = new DirectoryInfo(UserData.Path).FullName;
                        List<string> temp = DirectoryFinder.Get_Outfits_From_Path(string.Copy(item).Replace(Settings.AlternativePath.Value, coordinatepath), string.Copy(item).Replace(coordinatepath, Settings.AlternativePath.Value), false);
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

        private static string Grabber(string Input1, string result, string Coordinatepath, string input2)
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
                        return Coordinatepath + @"\School Uniform" + input2;
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
                        return Coordinatepath + @"\Swimsuit" + input2;
                    }
                }
            }
            return result;
        }
    }
}


