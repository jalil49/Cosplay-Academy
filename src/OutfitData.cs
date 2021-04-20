﻿using Cosplay_Academy.Hair;
using ExtensibleSaveFormat;
using System.Collections.Generic;
using System.Linq;
using CoordinateType = ChaFileDefine.CoordinateType;
namespace Cosplay_Academy
{
    public class OutfitData
    {
        //can be made to be scaleable at a sacrifice of readability will need to make scaleable settings tho.
        private bool Set_FirstTime;
        private bool Set_Amateur;
        private bool Set_Pro;
        private bool Set_Lewd;
        public string[] FirstTime;
        public string[] Amateur;
        public string[] Pro;
        public string[] Lewd;
        private string Path_FirstTime;
        private string Path_Amateur;
        private string Path_Pro;
        private string Path_Lewd;
        public bool Anger = false;
        //private bool[] ScaleableBool ; //All Scale with Constants.inputstrings2
        //private string[] ScaleableString []; 
        //private string[] ScaleablePaths; 

        public OutfitData()
        {

            Path_FirstTime = Path_Amateur = Path_Pro = Path_Lewd = "Default";
            FirstTime = new string[0];
            Amateur = new string[0];
            Pro = new string[0];
            Lewd = new string[0];
            Set_FirstTime = Set_Amateur = Set_Pro = Set_Lewd = Anger = false;
        }

        public void Clear()
        {
            FirstTime = new string[0];
            Amateur = new string[0];
            Pro = new string[0];
            Lewd = new string[0];
            Set_FirstTime = false;
            Set_Amateur = false;
            Set_Pro = false;
            Set_Lewd = false;
        }
        public List<string> Sum(int level)//returns list that is the sum of all available lists.
        {
            List<string> temp = new List<string>();
            if (!Anger)
            {
                if (level >= 3)
                    temp.AddRange(Lewd);
                if (level >= 2)
                    temp.AddRange(Pro);
                if (level >= 1)
                    temp.AddRange(Amateur);
            }
            temp.AddRange(FirstTime);
            return temp;
        }
        public void Insert(int level, string[] Data, bool IsSet)//Insert data according to lewd state and confirm if it is a setitem.
        {
            //List<string> temp = new List<string>();
            //if (Exportarray(level).Length > 0)//append existing data
            //{
            //    string[] temp2 = Exportarray(level);
            //    temp.AddRange(temp2);
            //}
            if (level == 3)
            {
                //temp.AddRange(Data);
                //Lewd = temp.ToArray();
                Lewd = Data;
                Set_Lewd = IsSet;
            }
            else if (level == 2)
            {
                //temp.AddRange(Data);
                //Pro = temp.ToArray();
                Pro = Data;

                Set_Pro = IsSet;
            }
            else if (level == 1)
            {
                //temp.AddRange(Data);
                //Amateur = temp.ToArray();
                Amateur = Data;

                Set_Amateur = IsSet;
            }
            else
            {
                //temp.AddRange(Data);
                //FirstTime = temp.ToArray();
                FirstTime = Data;

                Set_FirstTime = IsSet;
            }
        }
        private string Random(int level)//get any random outfit according to experience
        {
            if (ExpandedOutfit.SumRandom.Value)
            {
                string result;
                List<string> temp = Sum(level);
                result = temp[UnityEngine.Random.Range(0, temp.Count)];
                return result;
            }
            if (!Anger)
            {
                if (level == 3)
                    return Lewd[UnityEngine.Random.Range(0, Lewd.Length)];
                else if (level == 2)
                    return Pro[UnityEngine.Random.Range(0, Pro.Length)];
                else if (level == 1)
                    return Amateur[UnityEngine.Random.Range(0, Amateur.Length)];
            }
            return FirstTime[UnityEngine.Random.Range(0, FirstTime.Length)];
        }
        //public bool SetExists(int level)//Does a set exist for this outfit and lewd state
        //{
        //    if (!Anger)
        //    {
        //        if (level == 3)
        //            return (Set_FirstTime || Set_Amateur || Set_Pro || Set_Lewd);
        //        if (level == 2)
        //            return (Set_FirstTime || Set_Amateur || Set_Pro);
        //        if (level == 1)
        //            return (Set_FirstTime || Set_Amateur);
        //    }
        //    return (Set_FirstTime);
        //}
        public string RandomSet(int level, bool Match)//if set exists add its items to pool along with any coordinated outfit and other choices
        {
            List<string> temp = new List<string>();
            if (!Anger)
            {
                if (level >= 3 && (Set_Lewd || !Match))
                    temp.AddRange(Lewd);
                else if (level >= 3)
                    temp.Add(Path_Lewd);

                if (level >= 2 && (Set_Pro || !Match))
                    temp.AddRange(Pro);
                else if (level >= 2)
                    temp.Add(Path_Pro);

                if (level >= 1 && (Set_Amateur || !Match))
                    temp.AddRange(Amateur);
                else if (level >= 1)
                    temp.Add(Path_Amateur);
            }
            if (Set_FirstTime || !Match)
                temp.AddRange(FirstTime);
            else
                temp.Add(Path_FirstTime);

            return temp[UnityEngine.Random.Range(0, temp.Count)];
        }
        public string[] Exportarray(int level)
        {
            if (level == 3)
                return Lewd;
            if (level == 2)
                return Pro;
            if (level == 1)
                return Amateur;
            return FirstTime;
        }
        public void Coordinate()//set a random outfit to coordinate for non-set items when coordinated
        {
            Path_Lewd = Random(3);
            Path_Pro = Random(2);
            Path_Amateur = Random(1);
            Path_FirstTime = Random(0);
        }
        public bool IsSet(int level)
        {
            if (level == 3)
                return Set_Lewd;
            if (level == 2)
                return Set_Pro;
            if (level == 1)
                return Set_Amateur;
            return Set_FirstTime;
        }
        //public void Path_set(int level, string path) //Testing code
        //{
        //    if (level == 3)
        //        Path_Lewd = path;
        //    else if (level == 2)
        //        Path_Pro = path;
        //    else if (level == 1)
        //        Path_Amateur = path;
        //    else Path_FirstTime = path;
        //}
        //public string Path_print(int level)//Testing code to see if path setting is correct
        //{
        //    if (level == 3)
        //        return (Path_Lewd);
        //    if (level == 2)
        //        return (Path_Pro);
        //    if (level == 1)
        //        return (Path_Amateur);
        //    return (Path_FirstTime);
        //}
    }
    public class ChaDefault
    {
        internal bool firstpass = true;
        internal bool processed = false;
        //public string ChaName;//not actual name but ChaControl.Name
        internal List<ChaFileAccessory.PartsInfo>[] CoordinatePartsQueue = new List<ChaFileAccessory.PartsInfo>[Constants.outfitpath];
        internal string[] outfitpath = new string[Constants.outfitpath];
        internal string Underwear = "";
        internal int Personality;
        internal string BirthDay;
        internal string FullName;
        internal SaveData.Heroine heroine;
        internal string Koipath;
        internal string PreviousPath;
        internal bool ChangeKoiToClub;
        internal bool ChangeClubToKoi;
        internal bool Changestate = false;
        internal bool SkipFirstPriority = false;
        internal ME_Support ME = new ME_Support();
        internal CharaEvent CharaEvent;
        internal ChaFileCoordinate[] Original_Coordinates = new ChaFileCoordinate[Constants.outfitpath];
        internal Dictionary<string, PluginData> ExtendedCharacterData = new Dictionary<string, PluginData>();

        internal List<bool>[] HairKeepQueue = new List<bool>[Constants.outfitpath];
        internal List<bool>[] ACCKeepQueue = new List<bool>[Constants.outfitpath];
        //internal List<bool>[] HairPluginQueue = new List<bool>[Constants.outfitpath];

        #region hair accessories
        public List<HairSupport.HairAccessoryInfo>[] HairAccQueue = new List<HairSupport.HairAccessoryInfo>[Constants.outfitpath];
        public ChaControl ThisControl;
        #endregion

        #region Material Editor Save
        public List<RendererProperty>[] RendererPropertyQueue = new List<RendererProperty>[Constants.outfitpath];
        public List<MaterialFloatProperty>[] MaterialFloatPropertyQueue = new List<MaterialFloatProperty>[Constants.outfitpath];
        public List<MaterialColorProperty>[] MaterialColorPropertyQueue = new List<MaterialColorProperty>[Constants.outfitpath];
        public List<MaterialTextureProperty>[] MaterialTexturePropertyQueue = new List<MaterialTextureProperty>[Constants.outfitpath];
        public List<MaterialShader>[] MaterialShaderQueue = new List<MaterialShader>[Constants.outfitpath];
        public Dictionary<int, byte[]>[] importDictionaryQueue = new Dictionary<int, byte[]>[Constants.outfitpath];
        #endregion

        #region Material Editor Return
        public bool ME_Work = false;
        public List<RendererProperty> ReturnRenderer = new List<RendererProperty>();
        public List<MaterialFloatProperty> ReturnMaterialFloat = new List<MaterialFloatProperty>();
        public List<MaterialColorProperty> ReturnMaterialColor = new List<MaterialColorProperty>();
        public List<MaterialTextureProperty> ReturnMaterialTexture = new List<MaterialTextureProperty>();
        public List<MaterialShader> ReturnMaterialShade = new List<MaterialShader>();
        internal ChaControl ChaControl;
        internal ChaFile Chafile;

        #endregion

        internal List<int>[] HairKeepReturn = new List<int>[Constants.outfitpath];
        internal List<int>[] ACCKeepReturn = new List<int>[Constants.outfitpath];

        public ChaDefault()
        {
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                RendererPropertyQueue[i] = new List<RendererProperty>();
                MaterialFloatPropertyQueue[i] = new List<MaterialFloatProperty>();
                MaterialColorPropertyQueue[i] = new List<MaterialColorProperty>();
                MaterialTexturePropertyQueue[i] = new List<MaterialTextureProperty>();
                MaterialShaderQueue[i] = new List<MaterialShader>();
                importDictionaryQueue[i] = new Dictionary<int, byte[]>();
                HairAccQueue[i] = new List<HairSupport.HairAccessoryInfo>();
                CoordinatePartsQueue[i] = new List<ChaFileAccessory.PartsInfo>();
                outfitpath[i] = " ";
                HairKeepQueue[i] = new List<bool>();
                ACCKeepQueue[i] = new List<bool>();
                HairKeepReturn[i] = new List<int>();
                ACCKeepReturn[i] = new List<int>();
                //HairPluginQueue[i] = new List<bool>();
                //ExtendedCharacterData[i] = new Dictionary<string, PluginData>();
            }
        }
        public void Clear_ME()
        {
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                RendererPropertyQueue[i].Clear();
                MaterialFloatPropertyQueue[i].Clear();
                MaterialColorPropertyQueue[i].Clear();
                MaterialTexturePropertyQueue[i].Clear();
                MaterialShaderQueue[i].Clear();
                importDictionaryQueue[i].Clear();
                HairAccQueue[i].Clear();
                CoordinatePartsQueue[i].Clear();
                //outfitpath[i] = " ";
            }
            ME.TextureDictionary.Clear();
            //ME_Work = false;
            //ReturnRenderer.Clear();
            //ReturnMaterialFloat.Clear();
            //ReturnMaterialColor.Clear();
            //ReturnMaterialTexture.Clear();
            //ReturnMaterialShade.Clear();
            Soft_Clear_ME();
        }
        public void Soft_Clear_ME()
        {
            ME_Work = false;
            ReturnRenderer.Clear();
            ReturnMaterialFloat.Clear();
            ReturnMaterialColor.Clear();
            ReturnMaterialTexture.Clear();
            ReturnMaterialShade.Clear();
        }
    }
}
