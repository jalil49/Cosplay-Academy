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

        internal Dictionary<int, List<ChaFileAccessory.PartsInfo>> CoordinatePartsQueue = new Dictionary<int, List<ChaFileAccessory.PartsInfo>>();
        internal Dictionary<int, string> alloutfitpaths = new Dictionary<int, string>();
        internal readonly Dictionary<int, string> outfitpaths = new Dictionary<int, string>();

        internal int Outfit_Size => ChaControl.chaFile.coordinate.Length;

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

        internal ClothingLoader ClothingLoader;
        internal Dictionary<int, ChaFileCoordinate> Original_Coordinates = new Dictionary<int, ChaFileCoordinate>();
        internal Dictionary<string, PluginData> ExtendedCharacterData = new Dictionary<string, PluginData>();
        internal Dictionary<int, List<bool>> HairKeepQueue = new Dictionary<int, List<bool>>();
        internal Dictionary<int, List<bool>> ACCKeepQueue = new Dictionary<int, List<bool>>();

        #region hair accessories
        public Dictionary<int, List<HairSupport.HairAccessoryInfo>> HairAccQueue = new Dictionary<int, List<HairSupport.HairAccessoryInfo>>();
        #endregion

        #region Material Editor Save
        public Dictionary<int, ME_List> Original_Accessory_Data = new Dictionary<int, ME_List>();
        #endregion

        #region Material Editor Return
        public ME_List Finished = new ME_List();
        #endregion

        internal Dictionary<int, List<int>> HairKeepReturn = new Dictionary<int, List<int>>();
        internal Dictionary<int, List<int>> ACCKeepReturn = new Dictionary<int, List<int>>();

        public ChaDefault(ChaControl chaControl)
        {
            ChaControl = chaControl;
            ClothingLoader = new ClothingLoader(this);
        }

        public void Clear_Firstpass()
        {
            for (int i = 0, n = Outfit_Size; i < n; i++)
            {
                if (!HairKeepQueue.ContainsKey(i))
                {
                    HairKeepQueue[i] = new List<bool>();
                    ACCKeepQueue[i] = new List<bool>();
                    HairKeepReturn[i] = new List<int>();
                    ACCKeepReturn[i] = new List<int>();
                    Original_Accessory_Data[i] = new ME_List();
                    HairAccQueue[i] = new List<HairSupport.HairAccessoryInfo>();
                    CoordinatePartsQueue[i] = new List<ChaFileAccessory.PartsInfo>();
                    continue;
                }

                HairKeepQueue[i].Clear();
                ACCKeepQueue[i].Clear();
                HairKeepReturn[i].Clear();
                ACCKeepReturn[i].Clear();
                Original_Accessory_Data[i].Clear();
                HairAccQueue[i].Clear();
                CoordinatePartsQueue[i].Clear();
            }
            for (int i = Outfit_Size, n = HairKeepQueue.Keys.Count; i < n; i++)
            {
                HairKeepQueue.Remove(i);
                ACCKeepQueue.Remove(i);
                HairKeepReturn.Remove(i);
                ACCKeepReturn.Remove(i);
                Original_Accessory_Data.Remove(i);
                HairAccQueue.Remove(i);
                CoordinatePartsQueue.Remove(i);
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

        private void SpecialCondition(int coordinate, Dictionary<int, string> outfitpath, int datanum)
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
                        outfitpath[coordinate] = KoiOutfitpath;
                    }
                }

                if (club == 0)
                {
                    outfitpath[coordinate] = outfitpath[0];
                    return;
                }

                outfitpath[coordinate] = alloutfitpaths[datanum + club];
            }
#endif
        }
    }
}
