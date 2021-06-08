using System;
using System.Collections.Generic;

namespace Cosplay_Academy
{
    public class OutfitData
    {
        private readonly bool[] Part_of_Set = new bool[Enum.GetValues(typeof(HStates)).Length];
        public readonly string[][] Outfits_Per_State = new string[Enum.GetValues(typeof(HStates)).Length][];
        private readonly string[] Match_Outfit_Paths = new string[Enum.GetValues(typeof(HStates)).Length];
        public static bool Anger = false;

        public OutfitData()
        {
            for (int i = 0; i < Match_Outfit_Paths.Length; i++)
            {
                Match_Outfit_Paths[i] = "Default";
                Part_of_Set[i] = false;
                Outfits_Per_State[i] = new string[] { "Default" };
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Match_Outfit_Paths.Length; i++)
            {
                Outfits_Per_State[i] = new string[0];
                Part_of_Set[i] = false;
            }
        }

        public List<string> Sum(int level)//returns list that is the sum of all available lists.
        {
            List<string> temp = new List<string>();
            if (!Anger)
            {
                if (level >= 3)
                    temp.AddRange(Outfits_Per_State[3]);
                if (level >= 2)
                    temp.AddRange(Outfits_Per_State[2]);
                if (level >= 1)
                    temp.AddRange(Outfits_Per_State[1]);
            }
            temp.AddRange(Outfits_Per_State[0]);
            return temp;
        }

        public void Insert(int level, string[] Data, bool IsSet)//Insert data according to Outfits_Per_State[3] state and confirm if it is a setitem.
        {
            Outfits_Per_State[level] = Data;
            Part_of_Set[level] = IsSet;
        }

        public string Random(int level, bool Match)//get any random outfit according to experience
        {
            if (Match)
            {
                return Match_Outfit_Paths[level];
            }
            if (!Anger)
            {
                int Tries = 0;
                int EXP = level;
                string Result;
                do
                {
                    Result = Outfits_Per_State[level][UnityEngine.Random.Range(0, Outfits_Per_State[level].Length)];
                    if (Settings.EnableDefaults.Value || Result.EndsWith(".png"))
                    {
                        break;
                    }
                    if (Tries++ >= 3)
                    {
                        EXP--;
                        Tries = 0;
                        while (EXP > -1 && Outfits_Per_State[EXP].Length < 2)
                        {
                            EXP--;
                        }
                    }
                } while (EXP > -1);
                return Result;
            }
            return Outfits_Per_State[0][UnityEngine.Random.Range(0, Outfits_Per_State[0].Length)];
        }

        public string RandomSet(int level, bool Match)//if set exists add its items to pool along with any coordinated outfit and other choices
        {
            int Weight = 0;
            if (Anger)
            {
                level = 0;
            }

            level++;

            for (int i = 0; i < level; i++)
            {
                Weight += Settings.HStateWeights[i].Value;
            }

            if (Weight > 0)
            {
                var RandResult = UnityEngine.Random.Range(0, Weight);
                for (int i = 0; i < level; i++)
                {
                    if (RandResult < Settings.HStateWeights[i].Value)
                    {
                        int EXP = i;
                        int Tries = 0;
                        string Result;
                        do
                        {
                            if (Part_of_Set[i] || !Match)
                            {
                                Result = Outfits_Per_State[EXP][UnityEngine.Random.Range(0, Outfits_Per_State[EXP].Length)];
                            }
                            else
                                Result = Match_Outfit_Paths[EXP];
                            if (Settings.EnableDefaults.Value || Result.EndsWith(".png"))
                            {
                                break;
                            }
                            if ((Tries++ >= 3 || Match))
                            {
                                EXP--;
                                Tries = 0;
                                while (EXP > -1 && Outfits_Per_State[EXP].Length < 1)
                                {
                                    EXP--;
                                }
                            }
                        } while (EXP > -1);
                        return Result;
                    }
                    RandResult -= Settings.HStateWeights[i].Value;
                }
            }

            List<string> temp = new List<string>();

            for (int i = 0; i < level; i++)
            {
                if (Part_of_Set[i] || !Match)
                    temp.AddRange(Outfits_Per_State[i]);
                else
                    temp.Add(Match_Outfit_Paths[i]);
            }
            string LastResult;
            int tries = 0;
            do
            {
                LastResult = temp[UnityEngine.Random.Range(0, temp.Count)];
            } while (++tries < 3 && !LastResult.EndsWith(".png") && !Settings.EnableDefaults.Value);
            return LastResult;
        }

        public string[] Exportarray(int level)
        {
            return Outfits_Per_State[level];
        }

        public void Coordinate()//set a random outfit to coordinate for non-set items when coordinated
        {
            for (int i = 0; i < Part_of_Set.Length; i++)
            {
                Match_Outfit_Paths[i] = Random(i, false);
            }
        }

        public bool IsSet(int level)
        {
            if (level == 3)
                return Part_of_Set[3];
            if (level == 2)
                return Part_of_Set[2];
            if (level == 1)
                return Part_of_Set[1];
            return Part_of_Set[0];
        }
    }
}
