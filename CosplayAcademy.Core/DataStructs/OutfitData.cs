﻿using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosplay_Academy
{
    public class OutfitData
    {
        const string defaultstring = "Default";
        private readonly static CardData Defaultcard = new CardData(defaultstring);

        private readonly bool[] Part_of_Set = new bool[Enum.GetValues(typeof(HStates)).Length];
        public readonly List<CardData>[] Outfits_Per_State = new List<CardData>[Enum.GetValues(typeof(HStates)).Length];
        private readonly CardData[] Match_Outfit_Paths = new CardData[Enum.GetValues(typeof(HStates)).Length];
        public static bool Anger = false;

        public OutfitData()
        {
            for (var i = 0; i < Match_Outfit_Paths.Length; i++)
            {
                Match_Outfit_Paths[i] = Defaultcard;
                Part_of_Set[i] = false;
                Outfits_Per_State[i] = new List<CardData>();
            }
        }

        public void Clear()
        {
            for (var i = 0; i < Match_Outfit_Paths.Length; i++)
            {
                Outfits_Per_State[i].Clear();
                Part_of_Set[i] = false;
            }
        }

        public List<CardData> Sum(int level)//returns list that is the sum of all available lists.
        {
            var temp = new List<CardData>();
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

        public void Insert(int level, List<CardData> Data, bool IsSet)//Insert data according to Outfits_Per_State[3] state and confirm if it is a setitem.
        {
            Data.Add(Defaultcard);
            Outfits_Per_State[level] = Data;
            Part_of_Set[level] = IsSet;
        }

        public CardData Random(int level, bool Match, bool unrestricted, int personality = 0, ChaFileParameter.Attribute trait = null, int breast = 0, int height = 0)//get any random outfit according to experience
        {
            if (Match)
            {
                return Match_Outfit_Paths[level];
            }
            IEnumerable<CardData> applicable;
            if (!Anger)
            {
                var Tries = 0;
                var EXP = level;
                CardData Result;
                do
                {
                    applicable = Outfits_Per_State[EXP].Where(x => Filter(x, unrestricted, personality, trait, breast, height));
                    var rand = UnityEngine.Random.Range(0, applicable.Count());
                    Result = applicable.ElementAt(rand);
                    var isdefault = Result.Filepath == defaultstring;
                    if (Settings.EnableDefaults.Value && isdefault || !isdefault)
                    {
                        break;
                    }
                    if (Tries++ >= 10)
                    {
                        EXP--;
                        Tries = 0;
                        while (EXP > -1 && Outfits_Per_State[EXP].Count == 1)
                        {
                            EXP--;
                        }
                    }
                } while (EXP > -1);
                return Result;
            }
            applicable = Outfits_Per_State[0].Where(x => Filter(x, unrestricted, personality, trait, breast, height));
            return applicable.ElementAt(UnityEngine.Random.Range(0, applicable.Count()));
        }

        public CardData RandomSet(int level, bool Match, bool unrestricted, int personality = 0, ChaFileParameter.Attribute trait = null, int breast = 0, int height = 0)//if set exists add its items to pool along with any coordinated outfit and other choices
        {
            var Weight = 0;
            if (Anger)
            {
                level = 0;
            }

            level++;
            for (var i = 0; i < level; i++)
            {
                Weight += Settings.HStateWeights[i].Value;
            }
            IEnumerable<CardData> applicable;
            if (Weight > 0)
            {
                var RandResult = UnityEngine.Random.Range(0, Weight);
                for (var i = 0; i < level; i++)
                {
                    if (RandResult < Settings.HStateWeights[i].Value)
                    {
                        var EXP = i;
                        var Tries = 0;
                        var Result = Defaultcard;
                        do
                        {
                            if (Part_of_Set[i] || !Match)
                            {
                                applicable = Outfits_Per_State[EXP].Where(x => Filter(x, unrestricted, personality, trait, breast, height));
                                var rand = UnityEngine.Random.Range(0, applicable.Count());
                                Result = applicable.ElementAt(rand);
                            }
                            else
                                Result = Match_Outfit_Paths[EXP];
                            var isdefault = Result.Filepath == defaultstring;
                            if (Settings.EnableDefaults.Value && isdefault || !isdefault)
                            {
                                break;
                            }
                            if ((Tries++ >= 3 || Match))
                            {
                                EXP--;
                                Tries = 0;
                                while (EXP > -1 && Outfits_Per_State[EXP].Count < 2)
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

            var temp = new List<CardData>();

            for (var i = 0; i < level; i++)
            {
                if (Part_of_Set[i] || !Match)
                    temp.AddRange(Outfits_Per_State[i].Where(x => Filter(x, unrestricted, personality, trait, breast, height)));
                else
                    temp.Add(Match_Outfit_Paths[i]);
            }
            CardData LastResult;
            var tries = 0;
            do
            {
                LastResult = temp[UnityEngine.Random.Range(0, temp.Count)];
            } while (++tries < 3 && LastResult == Defaultcard && !Settings.EnableDefaults.Value);
            return LastResult;
        }

        public List<CardData> Exportarray(int level)
        {
            return Outfits_Per_State[level];
        }

        public void Coordinate()//set a random outfit to coordinate for non-set items when coordinated
        {
            for (var i = 0; i < Part_of_Set.Length; i++)
            {
                Match_Outfit_Paths[i] = Random(i, false, true);
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

        private bool Filter(CardData check, bool unrestricted, int personality, ChaFileParameter.Attribute trait, int breast, int height)
        {
            if (!check.DefinedData)
            {
                return true;
            }

            if (unrestricted)
            {
                return check.RestrictedPersonality.Count == 0 && check.Restricted.AllFalse() && check.Breastsize_Restriction.All(x => !x) && check.Height_Restriction.All(x => !x);
            }

            if (check.RestrictedPersonality.TryGetValue(personality, out var intresult) && intresult < 0)
            {
                return false;
            }

            if (check.Restricted.AnyOverlap(trait))
            {
                return false;
            }

            if (check.Breastsize_Restriction[breast])
            {
                return false;
            }

            if (check.Height_Restriction[height])
            {
                return false;
            }

            return true;
        }
    }
}
