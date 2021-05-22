using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
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
                    if (Tries++ == 3 || Match)
                    {
                        EXP--;
                        Tries = 0;
                        while (Outfits_Per_State[EXP].Length < 2)
                        {
                            EXP--;
                        }
                    }
                } while (!Settings.EnableDefaults.Value && EXP > -1 && !Result.EndsWith(".png"));
                return Result;
            }
            return Outfits_Per_State[0][UnityEngine.Random.Range(0, Outfits_Per_State[0].Length)];
        }

        //public bool SetExists(int level)//Does a set exist for this outfit and Outfits_Per_State[3] state
        //{
        //    if (!Anger)
        //    {
        //        if (level == 3)
        //            return (Part_of_Set[0] || Part_of_Set[1] || Part_of_Set[2] || Part_of_Set[3]);
        //        if (level == 2)
        //            return (Part_of_Set[0] || Part_of_Set[1] || Part_of_Set[2]);
        //        if (level == 1)
        //            return (Part_of_Set[0] || Part_of_Set[1]);
        //    }
        //    return (Part_of_Set[0]);
        //}

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
                var RandResult = UnityEngine.Random.Range(0, Weight + 1);
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
                            { Result = Match_Outfit_Paths[EXP]; }
                            if (Tries++ == 3 || Match)
                            {
                                EXP--;
                                Tries = 0;
                                while (Outfits_Per_State[EXP].Length < 2)
                                {
                                    EXP--;
                                }
                            }
                        } while (!Settings.EnableDefaults.Value && EXP > -1 && !Result.EndsWith(".png"));
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

            return temp[UnityEngine.Random.Range(0, temp.Count)];
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

        //public void Path_set(int level, string path) //Testing code
        //{
        //    if (level == 3)
        //        Path_Outfits_Per_State[3] = path;
        //    else if (level == 2)
        //        Path_Outfits_Per_State[2] = path;
        //    else if (level == 1)
        //        Path_Outfits_Per_State[1] = path;
        //    else Path_Outfits_Per_State[0] = path;
        //}
        //public string Path_print(int level)//Testing code to see if path setting is correct
        //{
        //    if (level == 3)
        //        return (Path_Outfits_Per_State[3]);
        //    if (level == 2)
        //        return (Path_Outfits_Per_State[2]);
        //    if (level == 1)
        //        return (Path_Outfits_Per_State[1]);
        //    return (Path_Outfits_Per_State[0]);
        //}
    }

    public class ChaDefault
    {
        const int Number_Of_Extra_Outfits = 1; //underwear

        internal bool firstpass = true;
        internal bool processed = false;
        //public string ChaName;//not actual name but ChaControl.Name
        internal List<ChaFileAccessory.PartsInfo>[] CoordinatePartsQueue = new List<ChaFileAccessory.PartsInfo>[Constants.Outfit_Size];
        internal string[] outfitpath = new string[Constants.Outfit_Size + Number_Of_Extra_Outfits];
        internal string Underwear = "";
        internal int Personality;
        internal string BirthDay;
        internal string FullName;
        internal SaveData.Heroine heroine;
        internal string KoiOutfitpath;
        internal string ClubOutfitPath;
        internal bool ChangeKoiToClub;
        internal bool ChangeClubToKoi;
        internal bool Changestate = false;
        internal bool SkipFirstPriority = false;
        internal ME_Support ME = new ME_Support();
        //internal CharaEvent CharaEvent;
        internal ChaFileCoordinate[] Original_Coordinates = new ChaFileCoordinate[Constants.Outfit_Size];
        internal Dictionary<string, PluginData> ExtendedCharacterData = new Dictionary<string, PluginData>();
        internal ClothingLoader ClothingLoader = new ClothingLoader();
        internal List<bool>[] HairKeepQueue = new List<bool>[Constants.Outfit_Size];
        internal List<bool>[] ACCKeepQueue = new List<bool>[Constants.Outfit_Size];
        //internal List<bool>[] HairPluginQueue = new List<bool>[Constants.outfitpath];

        #region hair accessories
        public List<HairSupport.HairAccessoryInfo>[] HairAccQueue = new List<HairSupport.HairAccessoryInfo>[Constants.Outfit_Size];
        #endregion

        #region Material Editor Save
        public ME_List[] Original_Accessory_Data = new ME_List[Constants.Outfit_Size];
        public Dictionary<int, byte[]>[] importDictionaryQueue = new Dictionary<int, byte[]>[Constants.Outfit_Size];
        #endregion

        #region Material Editor Return
        public bool ME_Work = false;
        public ME_List Finished = new ME_List();
        internal ChaControl ChaControl;
        internal ChaFile Chafile;

        #endregion

        internal List<int>[] HairKeepReturn = new List<int>[Constants.Outfit_Size];
        internal List<int>[] ACCKeepReturn = new List<int>[Constants.Outfit_Size];

        public ChaDefault()
        {
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                importDictionaryQueue[i] = new Dictionary<int, byte[]>();
                HairAccQueue[i] = new List<HairSupport.HairAccessoryInfo>();
                CoordinatePartsQueue[i] = new List<ChaFileAccessory.PartsInfo>();
                outfitpath[i] = " ";
                HairKeepQueue[i] = new List<bool>();
                ACCKeepQueue[i] = new List<bool>();
                HairKeepReturn[i] = new List<int>();
                ACCKeepReturn[i] = new List<int>();
                Original_Accessory_Data[i] = new ME_List();
                //HairPluginQueue[i] = new List<bool>();
                //ExtendedCharacterData[i] = new Dictionary<string, PluginData>();
            }
        }
        public void Clear_Firstpass()
        {
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                HairKeepQueue[i].Clear();
                ACCKeepQueue[i].Clear();
                HairKeepReturn[i].Clear();
                ACCKeepReturn[i].Clear();
                Original_Accessory_Data[i].Clear();
                importDictionaryQueue[i].Clear();
                HairAccQueue[i].Clear();
                CoordinatePartsQueue[i].Clear();
            }
            ME.TextureDictionary.Clear();
        }
        public void Soft_Clear_ME()
        {
            ME_Work = false;
            Finished.Clear();
        }
    }
}
