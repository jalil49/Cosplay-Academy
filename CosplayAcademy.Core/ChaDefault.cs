using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using System.Collections.Generic;

namespace Cosplay_Academy
{
    public class ChaDefault
    {
        internal ChaControl ChaControl;
        internal ChaFile Chafile;

        internal bool firstpass = true;
        internal bool processed = false;

        internal int Outfit_Size;

        internal List<ChaFileAccessory.PartsInfo>[] CoordinatePartsQueue;
        internal string[] alloutfitpaths = new string[Constants.InputStrings.Length];
        internal readonly string[] outfitpaths;

        internal int Personality;
        internal string BirthDay;
        internal string FullName;
#if KK
        internal static int LastClub = -1;
        internal SaveData.Heroine heroine;
        internal string KoiOutfitpath;
        internal string ClubOutfitPath;
        internal bool ChangeKoiToClub;
        internal bool ChangeClubToKoi;
#endif
        internal bool Changestate = false;
        internal bool SkipFirstPriority = false;
        internal ME_Support ME = new ME_Support();

        internal ChaFileCoordinate[] Original_Coordinates;
        internal Dictionary<string, PluginData> ExtendedCharacterData = new Dictionary<string, PluginData>();
        internal ClothingLoader ClothingLoader;
        internal List<bool>[] HairKeepQueue;
        internal List<bool>[] ACCKeepQueue;

        #region hair accessories
        public List<HairSupport.HairAccessoryInfo>[] HairAccQueue;
        #endregion

        #region Material Editor Save
        public ME_List[] Original_Accessory_Data;
        #endregion

        #region Material Editor Return
        public ME_List Finished = new ME_List();
        #endregion

        internal List<int>[] HairKeepReturn;
        internal List<int>[] ACCKeepReturn;

        public ChaDefault(ChaControl chaControl, ChaFile cha)
        {
            Outfit_Size = cha.coordinate.Length;

            CoordinatePartsQueue = new List<ChaFileAccessory.PartsInfo>[Outfit_Size];
            outfitpaths = new string[Outfit_Size];
            Original_Coordinates = new ChaFileCoordinate[Outfit_Size];
            HairKeepReturn = new List<int>[Outfit_Size];
            ACCKeepReturn = new List<int>[Outfit_Size];
            HairKeepQueue = new List<bool>[Outfit_Size];
            ACCKeepQueue = new List<bool>[Outfit_Size];
            HairAccQueue = new List<HairSupport.HairAccessoryInfo>[Outfit_Size];
            Original_Accessory_Data = new ME_List[Outfit_Size];
            ClothingLoader = new ClothingLoader(this);

            for (int i = 0; i < Outfit_Size; i++)
            {
                HairAccQueue[i] = new List<HairSupport.HairAccessoryInfo>();
                CoordinatePartsQueue[i] = new List<ChaFileAccessory.PartsInfo>();
                alloutfitpaths[i] = " ";
                HairKeepQueue[i] = new List<bool>();
                ACCKeepQueue[i] = new List<bool>();
                HairKeepReturn[i] = new List<int>();
                ACCKeepReturn[i] = new List<int>();
                Original_Accessory_Data[i] = new ME_List();
            }
        }

        public void Clear_Firstpass()
        {
            for (int i = 0; i < Outfit_Size; i++)
            {
                HairKeepQueue[i].Clear();
                ACCKeepQueue[i].Clear();
                HairKeepReturn[i].Clear();
                ACCKeepReturn[i].Clear();
                Original_Accessory_Data[i].Clear();
                HairAccQueue[i].Clear();
                CoordinatePartsQueue[i].Clear();
            }
            ME.TextureDictionary.Clear();
            Finished.Clear();
        }

        public void FillOutfitpaths()
        {
            var datanum = 0;
            for (int i = 0; i < Outfit_Size; i++)
            {
                var count = Constants.OutfitnumPairs[i];
                if (count == 1)
                {
                    outfitpaths[i] = alloutfitpaths[datanum];
                }
                else
                {
                    SpecialCondition(i, outfitpaths, datanum);
                }
                datanum += count;
            }
        }

        private void SpecialCondition(int coordinate, string[] v, int datanum)
        {
#if KK
            if (coordinate == 4)
            {
                var club = 0;
                bool heroinenull = heroine == null;
                if (!heroinenull)
                    club = heroine.clubActivities;
                else if (heroinenull && LastClub != -1)
                    club = LastClub;
                else
                    club = (int)Settings.ClubChoice.Value;

                LastClub = club;

                if (heroine == null ? Settings.KoiClub.Value : heroine.isStaff && Settings.KeepOldBehavior.Value)
                {
                    if (UnityEngine.Random.Range(1, 101) <= Settings.KoiChance.Value)
                    {
                        v[coordinate] = KoiOutfitpath;
                    }
                }

                if (club == 0)
                {
                    v[coordinate] = v[0];
                    return;
                }

                v[coordinate] = alloutfitpaths[datanum + club];
            }
#endif
        }

    }
}
