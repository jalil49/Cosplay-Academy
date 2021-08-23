using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Maker;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if !KKS
using MoreAccessoriesKOI;
using ToolBox;
#endif
#if TRACE
using System.Diagnostics;
#endif

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {
        private static readonly char sep = System.IO.Path.DirectorySeparatorChar;
        private readonly Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();
        private readonly ChaDefault ThisOutfitData;
        private ChaControl ChaControl;
        private ChaFile ChaFile;
        private static readonly int underwearindex = Constants.InputStrings.ToList().IndexOf($"{sep}Underwear");
        private static bool InsideMaker = false;

        #region MoreAccessories
#if !KKS
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
        private MoreAccessories.CharAdditionalData More_Char_Data
        {
            get
            {
                if (MakerAPI.InsideMaker)
                {
                    return MoreAccessories._self._charaMakerData;
                }
                if (_accessoriesByChar.TryGetValue(ChaFile, out MoreAccessories.CharAdditionalData data) == false)
                {
                    data = new MoreAccessories.CharAdditionalData();
                }
                return data;
            }
        }
        private static readonly Traverse InH_Field = Traverse.Create(MoreAccessories._self).Field("_inH");
#endif
        #endregion

        #region Underwear stuff
        public readonly ChaFileCoordinate Underwear = new ChaFileCoordinate();
        private readonly Dictionary<int, bool[]> Underwearbools = new Dictionary<int, bool[]>(); //0: not bot; 1: notbra; 2: notshorts
        private readonly Dictionary<int, List<int>> UnderwearAccessoriesLocations = new Dictionary<int, List<int>>();
        private List<ChaFileAccessory.PartsInfo> Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>();
        private readonly Dictionary<int, bool[]> UnderClothingKeep = new Dictionary<int, bool[]>();
        private readonly Dictionary<int, bool[]> UnderwearProcessed = new Dictionary<int, bool[]>();
        private ME_Coordinate Underwear_ME_Data;
        #endregion

        #region ACI_Data
        public Additional_Card_Info.Cardinfo CardInfo { get; internal set; }
        internal bool[] PersonalClothingBools => CardInfo.PersonalClothingBools;
        public bool Character_Cosplay_Ready => CardInfo.CosplayReady;

        internal Dictionary<int, bool[]> CharacterClothingKeep_Coordinate = new Dictionary<int, bool[]>();
        internal Dictionary<int, bool> MakeUpKeep = new Dictionary<int, bool>();
        #endregion
#if TRACE
        #region StopWatches
        private static bool TimeProcess = true;
        private static readonly Stopwatch[] TimeWatch = new Stopwatch[4];
        private static List<long>[] Average;
        #endregion
#endif
        private readonly Dictionary<int, bool> ValidOutfits = new Dictionary<int, bool>();

        public ClothingLoader(ChaDefault ThisOutfitData)
        {
            this.ThisOutfitData = ThisOutfitData;
#if TRACE
            if (TimeProcess)// do once
            {
                TimeProcess = false;
                Average = new List<long>[TimeWatch.Length];
                for (int i = 0; i < TimeWatch.Length; i++)
                {
                    TimeWatch[i] = new Stopwatch();
                    Average[i] = new List<long>();
                }
            }
#endif
        }

        public void FullLoad(ChaControl character, ChaFile file)
        {
#if TRACE
            var Start = TimeWatch[0].ElapsedMilliseconds;
            TimeWatch[0].Start();
#endif
            InsideMaker = MakerAPI.InsideMaker;
            ChaControl = character;
            ChaFile = file;

            ThisOutfitData.Finished.Clear();
            ThisOutfitData.FillOutfitpaths();
            int holdoutfitstate = ChaControl.fileStatus.coordinateType;
#if !KKS
            bool retain = (bool)InH_Field.GetValue();
#endif
            Underwear.LoadFile(ThisOutfitData.alloutfitpaths[underwearindex].GetFullPath());
            Settings.Logger.LogDebug($"loaded underwear " + ThisOutfitData.alloutfitpaths[underwearindex]);

            Underwear_ME_Data = new ME_Coordinate(ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.materialeditor"), ThisOutfitData, 0);

            Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>(Underwear.accessory.parts);
            Underwear_PartsInfos.AddRange(Support.MoreAccessories.Coordinate_Accessory_Extract(Underwear));
#if !KKS
            InH_Field.SetValue(false);
#endif

            for (int i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                if (!UnderwearAccessoriesLocations.ContainsKey(i)) UnderwearAccessoriesLocations[i] = new List<int>();

                if (!MakeUpKeep.ContainsKey(i)) MakeUpKeep[i] = false;

                if (!Underwearbools.ContainsKey(i)) Underwearbools[i] = new bool[3];

                if (!UnderwearProcessed.ContainsKey(i)) UnderwearProcessed[i] = new bool[9];

                ValidOutfits[i] = ThisOutfitData.outfitpaths.TryGetValue(i, out var path) && path.EndsWith(".png");
                if (ValidOutfits[i] || Settings.RandomizeUnderwear.Value && Underwear.GetLastErrorCode() == 0)
                {
                    GeneralizedLoad(i, ValidOutfits[i]);
                    if (ValidOutfits[i])
                    {
                        Settings.Logger.LogDebug($"loaded {(ChaFileDefine.CoordinateType)i} " + ThisOutfitData.outfitpaths[i]);
                    }
                    else
                    {
                        Settings.Logger.LogDebug($"loaded {(ChaFileDefine.CoordinateType)i} Default with changed underwear");
                    }
                }
                else
                {
                    Settings.Logger.LogDebug($"No valid outfits found for {(ChaFileDefine.CoordinateType)i}");
                }
            }

            ChaControl.fileStatus.coordinateType = holdoutfitstate;
#if !KKS
            InH_Field.SetValue(retain);
#endif
#if TRACE
            TimeWatch[0].Stop();
            var temp = TimeWatch[0].ElapsedMilliseconds - Start;
            Average[0].Add(temp);
            Settings.Logger.LogWarning($"\tFullLoad: Total elapsed time {TimeWatch[0].ElapsedMilliseconds}ms\n\tRun {Average[0].Count}: {temp}ms\n\tAverage: {Average[0].Average()}ms");
#endif
            Run_Repacks(character);
        }

        public void GeneralizedLoad(int outfitnum, bool load)
        {
#if TRACE
            var Start = TimeWatch[1].ElapsedMilliseconds;
            TimeWatch[1].Start();
#endif
            if (!ThisOutfitData.Finished.Coordinates.TryGetValue(outfitnum, out var ME_coord))
            {
                ThisOutfitData.Finished.Coordinates[outfitnum] = ME_coord = new ME_Coordinate();
            }
            UnderwearAccessoriesLocations[outfitnum].Clear();
            HairAccessories.Remove(outfitnum);
            ThisOutfitData.HairKeepReturn[outfitnum].Clear();
            ThisOutfitData.ACCKeepReturn[outfitnum].Clear();
            ChaControl.fileStatus.coordinateType = outfitnum;
            UnderwearProcessed[outfitnum] = new bool[9];
            var ThisCoordinate = ChaControl.chaFile.coordinate[outfitnum];

            #region Queue accessories to keep

            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>();
            var HairQueue = new Queue<HairSupport.HairAccessoryInfo>();

            var HairKeepQueue = new Queue<bool>();
            var ACCKeepqueue = new Queue<bool>();

            var ME_Queue = new Queue<MaterialEditorProperties>(ThisOutfitData.Original_Accessory_Data[outfitnum]);

            bool[] UnderClothingKeep = new bool[9];

            #endregion
            //Load new outfit

            if (load)
            {
                load = ThisOutfitData.outfitpaths.TryGetValue(outfitnum, out var path);
                if (load)
                {
                    load = ThisCoordinate.LoadFile(path);//in case it fails
                }
            }

            ValidOutfits[outfitnum] = load;
            if (load)
            {
                ME_coord.SoftClear(PersonalClothingBools);
                //only requeue items if a new file is loaded as they are unloaded.
                PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
                HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

                HairKeepQueue = new Queue<bool>(ThisOutfitData.HairKeepQueue[outfitnum]);
                ACCKeepqueue = new Queue<bool>(ThisOutfitData.ACCKeepQueue[outfitnum]);

                ME_Queue = new Queue<MaterialEditorProperties>(ThisOutfitData.Original_Accessory_Data[outfitnum]);
            }

            int UnderwearAccessoryStart = PartsQueue.Count();
            #region MakeUp
            if (MakeUpKeep[outfitnum])
            {
                ThisCoordinate.enableMakeup = ThisOutfitData.Original_Coordinates[outfitnum].enableMakeup;
                ThisCoordinate.makeup = ThisOutfitData.Original_Coordinates[outfitnum].makeup;
            }
            #endregion
            List<int> HairToColor = new List<int>();
            #region Reassign Existing Accessories

            var ExpandedData = ExtendedSave.GetExtendedDataById(ThisCoordinate, "Additional_Card_Info");
            if (ExpandedData != null)
            {
                switch (ExpandedData.version)
                {
                    case 0:
                        if (ExpandedData.data.TryGetValue("CoordinateSaveBools", out var bytedata) && bytedata != null)
                        {
                            UnderClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])bytedata);
                        }
                        if (ExpandedData.data.TryGetValue("HairAcc", out bytedata) && bytedata != null)
                        {
                            HairToColor = MessagePackSerializer.Deserialize<List<int>>((byte[])bytedata);
                        }
                        if (ExpandedData.data.TryGetValue("ClothNot", out bytedata) && bytedata != null)
                        {
                            Underwearbools[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])bytedata);
                        }
                        break;
                    case 1:

                        if (ExpandedData.data.TryGetValue("CoordinateInfo", out bytedata) && bytedata != null)
                        {
                            var coordinfo = MessagePackSerializer.Deserialize<Additional_Card_Info.CoordinateInfo>((byte[])bytedata);
                            UnderClothingKeep = coordinfo.CoordinateSaveBools;
                            HairToColor = coordinfo.HairAcc;
                            Underwearbools[outfitnum] = coordinfo.ClothNotData;
                        }
                        break;
                    default:
                        Settings.Logger.LogWarning("New version detected please update");
                        break;
                }
            }
            if (UnderClothingKeep == null) UnderClothingKeep = new bool[9];
            if (HairToColor == null) HairToColor = new List<int>();
            if (Underwearbools[outfitnum] == null) Underwearbools[outfitnum] = new bool[3];
            for (int i = 0; i < 9; i++)
            {
                if (PersonalClothingBools[i])
                {
                    UnderClothingKeep[i] = true;
                }
            }
            this.UnderClothingKeep[outfitnum] = UnderClothingKeep;
            var underwearbools = Underwearbools[outfitnum];
#if !KKS
            var Local_More_Char = More_Char_Data;
            if (Local_More_Char.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }
#endif
            var Inputdata = ExtendedSave.GetExtendedDataById(ThisCoordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            var HairAccInfo = new Dictionary<int, HairSupport.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    HairAccInfo = MessagePackSerializer.Deserialize<Dictionary<int, HairSupport.HairAccessoryInfo>>((byte[])loadedHairAccessories);
            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(ThisCoordinate, "com.deathweasel.bepinex.materialeditor");
            ThisOutfitData.Finished.LoadCoordinate(MaterialEditorData, ThisOutfitData, outfitnum);
            var Import_ME_Data = new MaterialEditorProperties();
            #endregion

            if (Settings.RandomizeUnderwear.Value && Underwear.GetLastErrorCode() == 0)
            {

                var UnderwearProcessed = this.UnderwearProcessed[outfitnum];
                Underwear_ME_Data.ChangeCoord(outfitnum);
                var Local_Underwear_ACC_Info = new List<ChaFileAccessory.PartsInfo>(Underwear_PartsInfos);
                var ObjectTypeList = new List<ObjectType>() { ObjectType.Accessory };
                if (outfitnum != 3)
                    for (int i = 0; i < Local_Underwear_ACC_Info.Count; i++)
                    {
                        if (Local_Underwear_ACC_Info[i].type > 120)
                        {
                            var ACCdata = new HairSupport.HairAccessoryInfo
                            {
                                HairLength = -999
                            };
                            if (Settings.HairMatch.Value)
                            {
                                ACCdata.ColorMatch = true;
                            }
                            HairKeepQueue.Enqueue(false);
                            ACCKeepqueue.Enqueue(false);

                            MaterialEditorProperties editorProperties;
                            if (!Underwear_ME_Data.AccessoryProperties.TryGetValue(i, out editorProperties))
                            {
                                editorProperties = new MaterialEditorProperties();
                            }
                            ME_Queue.Enqueue(editorProperties);
                            PartsQueue.Enqueue(Local_Underwear_ACC_Info[i]);
                            HairQueue.Enqueue(ACCdata);
                        }
                    }
                bool forceunder = Settings.ForceRandomUnderwear.Value;
                //When Top is not empty and bra is not kept
                var underclothesparts = Underwear.clothes.parts;
                var clothes_mainsubpart = ThisCoordinate.clothes.subPartsId[0];
                var clothespart = ThisCoordinate.clothes.parts;
                var CharacterClothingKeep_Coordinate = this.CharacterClothingKeep_Coordinate[outfitnum];
                if (!Constants.IgnoredTopIDs_Main.Contains(clothespart[0].id) && (!Constants.IgnoredTopIDs_A.TryGetValue(clothespart[0].id, out var list) || !list.Contains(clothes_mainsubpart)) && !CharacterClothingKeep_Coordinate[2])
                {
                    if (!UnderClothingKeep[2] && !underwearbools[1] && !underwearbools[2] && (clothespart[2].id != 0 || forceunder))
                    {
                        UnderwearProcessed[2] = true;
                        clothespart[2] = underclothesparts[2];
                        Additional_Clothing_Process(2, outfitnum, Underwear_ME_Data);
                    }

                    if (underwearbools[0])
                    {
                        if (!UnderClothingKeep[3] && !underwearbools[2] && (clothespart[3].id != 0 || forceunder))
                        {
                            UnderwearProcessed[3] = true;
                            clothespart[3] = underclothesparts[3];
                            Additional_Clothing_Process(3, outfitnum, Underwear_ME_Data);
                        }
                    }
                }
                //When bot is not empty and underwear is not kept
                if (!Constants.IgnoredBotsIDs_Main.Contains(clothespart[1].id) && !underwearbools[0] && !CharacterClothingKeep_Coordinate[3])
                {
                    if (!UnderClothingKeep[3] && !underwearbools[2] && (clothespart[3].id != 0 || forceunder))
                    {
                        UnderwearProcessed[3] = true;
                        clothespart[3] = underclothesparts[3];
                        Additional_Clothing_Process(3, outfitnum, Underwear_ME_Data);
                    }
                }

                if (outfitnum != 3)
                {
                    if (!CharacterClothingKeep_Coordinate[5] && (clothespart[5].id != 0 || forceunder))
                    {
                        if (!UnderClothingKeep[5])
                        {
                            UnderwearProcessed[5] = true;
                            clothespart[5] = underclothesparts[5];
                            Additional_Clothing_Process(5, outfitnum, Underwear_ME_Data);
                        }
                        if (!UnderClothingKeep[6] && !CharacterClothingKeep_Coordinate[6])
                        {
                            UnderwearProcessed[6] = true;
                            clothespart[6] = underclothesparts[6];
                            Additional_Clothing_Process(6, outfitnum, Underwear_ME_Data);
                        }
                    }

                    if (!UnderClothingKeep[6] && !CharacterClothingKeep_Coordinate[6] && (clothespart[6].id != 0 || forceunder))
                    {
                        UnderwearProcessed[6] = true;
                        clothespart[6] = underclothesparts[6];
                        Additional_Clothing_Process(6, outfitnum, Underwear_ME_Data);
                    }
                }
            }
            Color[] haircolor = new Color[] { ChaControl.fileHair.parts[1].baseColor, ChaControl.fileHair.parts[1].startColor, ChaControl.fileHair.parts[1].endColor, ChaControl.fileHair.parts[1].outlineColor };
            if (Settings.HairMatch.Value && !MakerAPI.InsideMaker)
            {
#if KKS
var emptylist=new List<ChaFileAccessory.PartsInfo>();
#endif
                foreach (var item in HairToColor)
                {
                    HairMatchProcess(outfitnum, item, haircolor, ref
#if !KKS
             NewRAW
#else
                        emptylist
#endif
                        );
                }
            }
            int insert = 0;
            int ACCpostion = 0;
            bool Empty;
#if KK
            bool print = true;
#endif

            //Don't Skip if inside Maker

            if (MakerAPI.InsideMaker)
            {
                //Normal
                for (int n = ThisCoordinate.accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
                {
                    Empty = ThisCoordinate.accessory.parts[ACCpostion].type < 121;
                    if (Empty) //120 is empty/default
                    {
                        if (insert++ >= UnderwearAccessoryStart)
                        {
                            UnderwearAccessoriesLocations[outfitnum].Add(ACCpostion);
                        }
                        ThisCoordinate.accessory.parts[ACCpostion] = PartsQueue.Dequeue();
                        if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                        {
                            HairAccInfo[ACCpostion] = HairQueue.Dequeue();
                        }
                        else
                        {
                            HairAccInfo.Remove(ACCpostion);
                            HairQueue.Dequeue();
                        }
                        if (HairKeepQueue.Dequeue())
                        {
                            ThisOutfitData.HairKeepReturn[outfitnum].Add(ACCpostion);
                        }
                        if (ACCKeepqueue.Dequeue())
                        {
                            ThisOutfitData.ACCKeepReturn[outfitnum].Add(ACCpostion);
                        }

                        ME_coord.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
#if KKS
var emptylist=new List<ChaFileAccessory.PartsInfo>();
#endif
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, ref
#if !KKS
                            NewRAW
#else
                            emptylist
#endif
);
                    }
                }
#if KK
                //MoreAccessories
                for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
                {
                    Empty = NewRAW[ACCpostion - 20].type < 121;
                    if (Empty) //120 is empty/default
                    {
                        if (insert++ >= UnderwearAccessoryStart)
                        {
                            UnderwearAccessoriesLocations[outfitnum].Add(ACCpostion);
                        }

                        NewRAW[ACCpostion - 20] = PartsQueue.Dequeue();

                        if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                        {
                            HairAccInfo[ACCpostion] = HairQueue.Dequeue();
                        }
                        else
                        {
                            HairAccInfo.Remove(ACCpostion);
                            HairQueue.Dequeue();
                        }

                        if (HairKeepQueue.Dequeue())
                        {
                            ThisOutfitData.HairKeepReturn[outfitnum].Add(ACCpostion);
                        }
                        if (ACCKeepqueue.Dequeue())
                        {
                            ThisOutfitData.ACCKeepReturn[outfitnum].Add(ACCpostion);
                        }

                        ME_coord.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, ref NewRAW);
                    }
                }
#endif
            }
#if KK
            else
            {
                ACCpostion = 20 + NewRAW.Count;
                print = false;
            }

            //original accessories
            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    Settings.Logger.LogDebug($"Ran out of space in new coordinate adding {PartsQueue.Count}");
                    print = false;
                }
                if (insert++ >= UnderwearAccessoryStart)
                {
                    UnderwearAccessoriesLocations[outfitnum].Add(ACCpostion);
                }
                NewRAW.Add(PartsQueue.Dequeue());
                if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                {
                    var HairInfo = HairQueue.Dequeue();
                    if (Settings.HairMatch.Value)
                    {
                        HairInfo.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, ref NewRAW);
                    }
                    HairAccInfo[ACCpostion] = HairInfo;
                }
                else
                {
                    HairAccInfo.Remove(ACCpostion);
                    HairQueue.Dequeue();
                }

                ME_coord.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());

                if (HairKeepQueue.Dequeue())
                {
                    ThisOutfitData.HairKeepReturn[outfitnum].Add(ACCpostion);
                }
                if (ACCKeepqueue.Dequeue())
                {
                    ThisOutfitData.ACCKeepReturn[outfitnum].Add(ACCpostion);
                }

                ACCpostion++;
            }
#endif

            HairAccessories.Add(outfitnum, HairAccInfo);
#if !KKS
            while (Local_More_Char.infoAccessory.Count < Local_More_Char.nowAccessories.Count)
                Local_More_Char.infoAccessory.Add(null);
            while (Local_More_Char.objAccessory.Count < Local_More_Char.nowAccessories.Count)
                Local_More_Char.objAccessory.Add(null);
            while (Local_More_Char.objAcsMove.Count < Local_More_Char.nowAccessories.Count)
                Local_More_Char.objAcsMove.Add(new GameObject[2]);
            while (Local_More_Char.cusAcsCmp.Count < Local_More_Char.nowAccessories.Count)
                Local_More_Char.cusAcsCmp.Add(null);
            while (Local_More_Char.showAccessories.Count < Local_More_Char.nowAccessories.Count)
                Local_More_Char.showAccessories.Add(true);
#endif

            #endregion

#if TRACE
            TimeWatch[1].Stop();
            var temp = TimeWatch[1].ElapsedMilliseconds - Start;
            Average[1].Add(temp);
            Settings.Logger.LogWarning($"\tGeneralLoad: Total elapsed time {TimeWatch[1].ElapsedMilliseconds}ms\n\tRun {Average[1].Count}: {temp}ms\n\tAverage: {Average[1].Average()}ms");
#endif
        }

        public void CoordinateLoad(ChaFileCoordinate coordinate, ChaControl ChaControl)
        {
            this.ChaControl = ChaControl;
            ChaFile = ThisOutfitData.Chafile;
            InsideMaker = MakerAPI.InsideMaker;
            #region Queue accessories to keep

            int outfitnum = ChaControl.fileStatus.coordinateType;

            Queue<ChaFileAccessory.PartsInfo> PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            Queue<HairSupport.HairAccessoryInfo> HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

            Queue<bool> ACCKeepQueue = new Queue<bool>(ThisOutfitData.ACCKeepQueue[outfitnum]);
            Queue<bool> HairKeepQueue = new Queue<bool>(ThisOutfitData.HairKeepQueue[outfitnum]);
            List<int> HairKeepResult = new List<int>();
            List<int> ACCKeepResult = new List<int>();

            var ME_Queue = new Queue<MaterialEditorProperties>(ThisOutfitData.Original_Accessory_Data[outfitnum]);

            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor");

            var Coordinate_ME_Data = new ME_Coordinate(MaterialEditorData, ThisOutfitData, outfitnum);
            #endregion

            #endregion

            //Apply pre-existing Accessories in any open slot or final slots.
#if !KKS
            var Local_More = More_Char_Data;
            List<ChaFileAccessory.PartsInfo> MoreACCData = Local_More.nowAccessories;
#endif

            ChaFileAccessory.PartsInfo[] OriginalData = ChaControl.nowCoordinate.accessory.parts;

            #region Reassign Existing Accessories

            var Inputdata = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            var HairACCDictionary = new Dictionary<int, HairSupport.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    HairACCDictionary = MessagePackSerializer.Deserialize<Dictionary<int, HairSupport.HairAccessoryInfo>>((byte[])loadedHairAccessories);

            int ACCpostion = 0;
            bool Empty;
            for (int n = OriginalData.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = OriginalData[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    OriginalData[ACCpostion] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                    {
                        HairACCDictionary[ACCpostion] = HairQueue.Dequeue();
                    }
                    else
                    {
                        HairACCDictionary.Remove(ACCpostion);
                        HairQueue.Dequeue();
                    }

                    Coordinate_ME_Data.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());

                    if (HairKeepQueue.Dequeue())
                    {
                        HairKeepResult.Add(ACCpostion);
                    }
                    if (ACCKeepQueue.Dequeue())
                    {
                        ACCKeepResult.Add(ACCpostion);
                    }
                }
                if (Settings.HairMatch.Value && HairACCDictionary.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
            }
#if !KKS

            for (int n = MoreACCData.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
            {
                Empty = MoreACCData[ACCpostion - 20].type == 120;
                if (Empty) //120 is empty/default
                {
                    MoreACCData[ACCpostion - 20] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                    {
                        HairACCDictionary[ACCpostion] = HairQueue.Dequeue();
                    }
                    else
                    {
                        HairACCDictionary.Remove(ACCpostion);
                        HairQueue.Dequeue();
                    }

                    Coordinate_ME_Data.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());

                    if (HairKeepQueue.Dequeue())
                    {
                        HairKeepResult.Add(ACCpostion);
                    }
                    if (ACCKeepQueue.Dequeue())
                    {
                        ACCKeepResult.Add(ACCpostion);
                    }
                }
                if (Settings.HairMatch.Value && HairACCDictionary.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
            }

            bool print = true;

            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    Settings.Logger.LogDebug($"Ran out of space in new coordiante adding {PartsQueue.Count}");
                    print = false;
                }
                MoreACCData.Add(PartsQueue.Dequeue());
                if (HairQueue.Peek() != null && HairQueue.Peek().HairLength > -998)
                {
                    var HairInfo = HairQueue.Dequeue();
                    if (Settings.HairMatch.Value)
                    {
                        HairInfo.ColorMatch = true;
                    }
                    HairACCDictionary[ACCpostion] = HairInfo;
                }
                else
                {
                    HairQueue.Dequeue();
                }

                Coordinate_ME_Data.AddAccessory(outfitnum, ACCpostion, ME_Queue.Dequeue());

                if (InsideMaker)
                {
                    if (HairKeepQueue.Dequeue())
                    {
                        HairKeepResult.Add(ACCpostion);
                    }
                    if (ACCKeepQueue.Dequeue())
                    {
                        ACCKeepResult.Add(ACCpostion);
                    }
                }
                ACCpostion++;
            }

            while (Local_More.infoAccessory.Count < Local_More.nowAccessories.Count)
                Local_More.infoAccessory.Add(null);
            while (Local_More.objAccessory.Count < Local_More.nowAccessories.Count)
                Local_More.objAccessory.Add(null);
            while (Local_More.objAcsMove.Count < Local_More.nowAccessories.Count)
                Local_More.objAcsMove.Add(new GameObject[2]);
            while (Local_More.cusAcsCmp.Count < Local_More.nowAccessories.Count)
                Local_More.cusAcsCmp.Add(null);
            while (Local_More.showAccessories.Count < Local_More.nowAccessories.Count)
                Local_More.showAccessories.Add(true);
#endif
            #endregion

            #region Pack
            var SaveData = new PluginData();

            Coordinate_ME_Data.AllProperties(out var rendererProperties, out var materialFloatProperties, out var materialColorProperties, out var materialShaders, out var materialTextureProperties);

            var TextureDictionary = ThisOutfitData.ME.TextureDictionary.Where(pair => materialTextureProperties.Any(x => x.TexID == pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value.Data);
            if (TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(TextureDictionary));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (rendererProperties.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(rendererProperties));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (materialFloatProperties.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(materialFloatProperties));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

            if (materialColorProperties.Count > 0)
                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(materialColorProperties));
            else
                SaveData.data.Add("MaterialColorPropertyList", null);

            if (materialTextureProperties.Count > 0)
                SaveData.data.Add("MaterialTexturePropertyList", MessagePackSerializer.Serialize(materialTextureProperties));
            else
                SaveData.data.Add("MaterialTexturePropertyList", null);

            if (materialShaders.Count > 0)
                SaveData.data.Add("MaterialShaderList", MessagePackSerializer.Serialize(materialShaders));
            else
                SaveData.data.Add("MaterialShaderList", null);

            ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor", SaveData);

            if (InsideMaker && Constants.PluginResults["Additional_Card_Info"])
            {
                SaveData = new PluginData() { version = 1 };

                var NowCoordinateInfo = new Additional_Card_Info.CoordinateInfo();
                var NowRestrictionInfo = NowCoordinateInfo.RestrictionInfo;

                Inputdata = ExtendedSave.GetExtendedDataById(coordinate, "Additional_Card_Info");
                if (Inputdata != null)
                {
                    switch (Inputdata.version)
                    {
                        case 0:
                            {
                                NowCoordinateInfo = Additional_Card_Info.Migrator.CoordinateMigrateV0(Inputdata);
                                NowRestrictionInfo = NowCoordinateInfo.RestrictionInfo;
                            }
                            break;
                        case 1:
                            if (Inputdata.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                            {
                                NowCoordinateInfo = MessagePackSerializer.Deserialize<Additional_Card_Info.CoordinateInfo>((byte[])ByteData);
                                NowRestrictionInfo = NowCoordinateInfo.RestrictionInfo;
                            }
                            if (Inputdata.data.TryGetValue("RestrictionInfo", out ByteData) && ByteData != null)
                            {
                                NowRestrictionInfo = MessagePackSerializer.Deserialize<Additional_Card_Info.RestrictionInfo>((byte[])ByteData);
                            }
                            break;
                        default:
                            Settings.Logger.LogWarning("New Version Detected Please Update");
                            return;
                    }
                }

                NowCoordinateInfo.AccKeep.AddRange(ACCKeepResult);
                NowCoordinateInfo.HairAcc.AddRange(HairKeepResult);

                SaveData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(NowCoordinateInfo));
                SaveData.data.Add("RestrictionInfo", MessagePackSerializer.Serialize(NowRestrictionInfo));

                ExtendedSave.SetExtendedDataById(coordinate, "Additional_Card_Info", SaveData);
                //ControllerCoordReload_Loop(Type.GetType("Additional_Card_Info.CharaEvent, Additional_Card_Info", false), ChaControl, coordinate);
            }

            #endregion

            //ControllerCoordReload_Loop(typeof(KK_Plugins.MaterialEditor.MaterialEditorCharaController), ChaControl, coordinate);

            if (Settings.HairMatch.Value)
            {
                var Plugdata = new PluginData();

                Plugdata.data.Add("CoordinateHairAccessories", MessagePackSerializer.Serialize(HairACCDictionary));
                ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer", Plugdata);

                //ControllerCoordReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl, coordinate);
            }
        }

        private void Additional_Clothing_Process(int index, int outfitnum, ME_Coordinate ME_Data)
        {
            var finishcoords = ThisOutfitData.Finished.Coordinates;
            if (!finishcoords.TryGetValue(outfitnum, out var finishcoord))
            {
                finishcoords[outfitnum] = new ME_Coordinate();
            }

            if (ME_Data.ClothingProperties.TryGetValue(index, out var editorProperties))
            {
                finishcoord.ClothingProperties[index] = editorProperties;
                return;
            }

            finishcoord.ClothingProperties.Remove(index);
        }

        private void HairMatchProcess(int outfitnum, int ACCPosition, Color[] haircolor, ref List<ChaFileAccessory.PartsInfo> NewRAW)
        {
            if (ACCPosition < 20)
            {
                ChaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCPosition].color = haircolor;
            }
            else
            {
                NewRAW[ACCPosition - 20].color = haircolor;
            }
            if (!ThisOutfitData.Finished.Coordinates.TryGetValue(outfitnum, out var coord))
            {
                ThisOutfitData.Finished.Coordinates[outfitnum] = coord = new ME_Coordinate();
            }
            if (!coord.AccessoryProperties.TryGetValue(ACCPosition, out var editorProperties))
            {
                return;
            }
            var haircomponent = editorProperties.MaterialColorProperty;
            var hairpart = ChaControl.fileHair.parts[1];
            for (int i = 0; i < haircomponent.Count; i++)
            {
                if (haircomponent[i].Property == "Color")
                {
                    haircomponent[i].Value = hairpart.baseColor;
                    continue;
                }
                if (haircomponent[i].Property == "Color2")
                {
                    haircomponent[i].Value = hairpart.startColor;
                    continue;
                }
                if (haircomponent[i].Property == "Color3")
                {
                    haircomponent[i].Value = hairpart.endColor;
                    continue;
                }
                if (haircomponent[i].Property == "ShadowColor")
                {
                    haircomponent[i].Value = hairpart.outlineColor;
                    continue;
                }
            }
        }
    }
}