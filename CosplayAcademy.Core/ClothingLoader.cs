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
        private readonly Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();
        private readonly ChaDefault ThisOutfitData;
        private ChaControl ChaControl;
        private ChaFile ChaFile;
        private static readonly int underwearindex = Constants.InputStrings.ToList().IndexOf(@"\Underwear");
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
        private ME_List Underwear_ME_Data;
        #endregion

        #region ACI_Data
        internal bool[] PersonalClothingBools = new bool[9];
        internal Dictionary<int, bool[]> CharacterClothingKeep_Coordinate = new Dictionary<int, bool[]>();
        internal Dictionary<int, bool> MakeUpKeep = new Dictionary<int, bool>();
        public bool Character_Cosplay_Ready = false;
        #endregion

        private readonly Dictionary<int, List<int>> ME_Dont_Touch = new Dictionary<int, List<int>>();

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
            Underwear.LoadFile(ThisOutfitData.alloutfitpaths[underwearindex]);
            Settings.Logger.LogDebug($"loaded underwear " + ThisOutfitData.alloutfitpaths[underwearindex]);

            Underwear_ME_Data = new ME_List(ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.materialeditor"), ThisOutfitData, true);

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

                if (!ME_Dont_Touch.ContainsKey(i)) ME_Dont_Touch[i] = new List<int>();

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

            Original_ME_Data(); //Load existing where applicable

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
            ME_Dont_Touch[outfitnum].Clear();
            ThisOutfitData.Finished.ClearCoord(outfitnum);
            UnderwearAccessoriesLocations[outfitnum].Clear();
            HairAccessories.Remove(outfitnum);
            ThisOutfitData.HairKeepReturn[outfitnum].Clear();
            ThisOutfitData.ACCKeepReturn[outfitnum].Clear();
            ChaControl.fileStatus.coordinateType = outfitnum;
            UnderwearProcessed[outfitnum] = new bool[9];
            var ThisCoordinate = ChaControl.chaFile.coordinate[outfitnum];

            #region Queue accessories to keep

            Queue<ChaFileAccessory.PartsInfo> PartsQueue = new Queue<ChaFileAccessory.PartsInfo>();
            Queue<HairSupport.HairAccessoryInfo> HairQueue = new Queue<HairSupport.HairAccessoryInfo>();

            Queue<bool> HairKeepQueue = new Queue<bool>();
            Queue<bool> ACCKeepqueue = new Queue<bool>();

            Queue<RendererProperty> RenderQueue = new Queue<RendererProperty>();
            Queue<MaterialFloatProperty> FloatQueue = new Queue<MaterialFloatProperty>();
            Queue<MaterialColorProperty> ColorQueue = new Queue<MaterialColorProperty>();
            Queue<MaterialTextureProperty> TextureQueue = new Queue<MaterialTextureProperty>();
            Queue<MaterialShader> ShaderQueue = new Queue<MaterialShader>();

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
                //only requeue items if a new file is loaded as they are unloaded.
                PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
                HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

                HairKeepQueue = new Queue<bool>(ThisOutfitData.HairKeepQueue[outfitnum]);
                ACCKeepqueue = new Queue<bool>(ThisOutfitData.ACCKeepQueue[outfitnum]);

                RenderQueue = new Queue<RendererProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].RendererProperty);
                FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialFloatProperty);
                ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialColorProperty);
                TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialTextureProperty);
                ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialShader);
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
            ME_List.PrimaryData(MaterialEditorData, ThisOutfitData, outfitnum);
            var Import_ME_Data = new ME_List();
            #endregion

            if (Settings.RandomizeUnderwear.Value && Underwear.GetLastErrorCode() == 0)
            {
                var UnderwearProcessed = this.UnderwearProcessed[outfitnum];
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

                            foreach (var item in Underwear_ME_Data.Color_FindAll(ObjectTypeList, i, outfitnum))
                            {
                                ColorQueue.Enqueue(item);
                            }
                            foreach (var item in Underwear_ME_Data.Float_FindAll(ObjectTypeList, i, outfitnum))
                            {
                                FloatQueue.Enqueue(item);
                            }
                            foreach (var item in Underwear_ME_Data.Shader_FindAll(ObjectTypeList, i, outfitnum))
                            {
                                ShaderQueue.Enqueue(item);
                            }
                            foreach (var item in Underwear_ME_Data.Texture_FindAll(ObjectTypeList, i, outfitnum))
                            {
                                TextureQueue.Enqueue(item);
                            }
                            foreach (var item in Underwear_ME_Data.Render_FindAll(ObjectTypeList, i, outfitnum))
                            {
                                RenderQueue.Enqueue(item);
                            }

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
                foreach (var item in HairToColor)
                {
                    HairMatchProcess(outfitnum, item, haircolor,
#if !KKS
            NewRAW
#else   
                        new List<ChaFileAccessory.PartsInfo>()
#endif
                        );
                }
            }
            int insert = 0;
            int ACCpostion = 0;
            bool Empty;
#if !KKS
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

                        ME_Render_Loop(RenderQueue, ACCpostion, Import_ME_Data.RendererProperty);

                        ME_Color_Loop(ColorQueue, ACCpostion, Import_ME_Data.MaterialColorProperty);

                        ME_Texture_Loop(TextureQueue, ACCpostion, Import_ME_Data.MaterialTextureProperty);

                        ME_Float_Loop(FloatQueue, ACCpostion, Import_ME_Data.MaterialFloatProperty);

                        ME_Shader_Loop(ShaderQueue, ACCpostion, Import_ME_Data.MaterialShader);
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor,
#if !KKS
                            NewRAW
#else
                            new List<ChaFileAccessory.PartsInfo>()
#endif
);
                    }
                }
#if !KKS
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

                        ME_Render_Loop(RenderQueue, ACCpostion, Import_ME_Data.RendererProperty);

                        ME_Color_Loop(ColorQueue, ACCpostion, Import_ME_Data.MaterialColorProperty);

                        ME_Texture_Loop(TextureQueue, ACCpostion, Import_ME_Data.MaterialTextureProperty);

                        ME_Float_Loop(FloatQueue, ACCpostion, Import_ME_Data.MaterialFloatProperty);

                        ME_Shader_Loop(ShaderQueue, ACCpostion, Import_ME_Data.MaterialShader);
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, NewRAW);
                    }
                }
#endif
            }
#if !KKS
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
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, NewRAW);
                    }
                    HairAccInfo[ACCpostion] = HairInfo;
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

                ME_Render_Loop(RenderQueue, ACCpostion, Import_ME_Data.RendererProperty);

                ME_Color_Loop(ColorQueue, ACCpostion, Import_ME_Data.MaterialColorProperty);

                ME_Texture_Loop(TextureQueue, ACCpostion, Import_ME_Data.MaterialTextureProperty);

                ME_Float_Loop(FloatQueue, ACCpostion, Import_ME_Data.MaterialFloatProperty);

                ME_Shader_Loop(ShaderQueue, ACCpostion, Import_ME_Data.MaterialShader);

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

            ThisOutfitData.Finished.MaterialColorProperty.AddRange(Import_ME_Data.MaterialColorProperty);

            ThisOutfitData.Finished.MaterialFloatProperty.AddRange(Import_ME_Data.MaterialFloatProperty);

            ThisOutfitData.Finished.MaterialShader.AddRange(Import_ME_Data.MaterialShader);

            ThisOutfitData.Finished.MaterialTextureProperty.AddRange(Import_ME_Data.MaterialTextureProperty);

            ThisOutfitData.Finished.RendererProperty.AddRange(Import_ME_Data.RendererProperty);
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

            var RenderQueue = new Queue<RendererProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].RendererProperty);
            var FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialFloatProperty);
            var ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialColorProperty);
            var TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialTextureProperty);
            var ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.Original_Accessory_Data[outfitnum].MaterialShader);

            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor");

            var Coordinate_ME_Data = new ME_List(MaterialEditorData, ThisOutfitData, outfitnum);
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

                    ME_Render_Loop(RenderQueue, ACCpostion, Coordinate_ME_Data.RendererProperty);

                    ME_Color_Loop(ColorQueue, ACCpostion, Coordinate_ME_Data.MaterialColorProperty);

                    ME_Texture_Loop(TextureQueue, ACCpostion, Coordinate_ME_Data.MaterialTextureProperty);

                    ME_Float_Loop(FloatQueue, ACCpostion, Coordinate_ME_Data.MaterialFloatProperty);

                    ME_Shader_Loop(ShaderQueue, ACCpostion, Coordinate_ME_Data.MaterialShader);
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

                    ME_Render_Loop(RenderQueue, ACCpostion, Coordinate_ME_Data.RendererProperty);

                    ME_Color_Loop(ColorQueue, ACCpostion, Coordinate_ME_Data.MaterialColorProperty);

                    ME_Texture_Loop(TextureQueue, ACCpostion, Coordinate_ME_Data.MaterialTextureProperty);

                    ME_Float_Loop(FloatQueue, ACCpostion, Coordinate_ME_Data.MaterialFloatProperty);

                    ME_Shader_Loop(ShaderQueue, ACCpostion, Coordinate_ME_Data.MaterialShader);

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

                ME_Render_Loop(RenderQueue, ACCpostion, Coordinate_ME_Data.RendererProperty);

                ME_Color_Loop(ColorQueue, ACCpostion, Coordinate_ME_Data.MaterialColorProperty);

                ME_Texture_Loop(TextureQueue, ACCpostion, Coordinate_ME_Data.MaterialTextureProperty);

                ME_Float_Loop(FloatQueue, ACCpostion, Coordinate_ME_Data.MaterialFloatProperty);

                ME_Shader_Loop(ShaderQueue, ACCpostion, Coordinate_ME_Data.MaterialShader);

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
            var TextureDictionary = ThisOutfitData.ME.TextureDictionary.Where(pair => Coordinate_ME_Data.MaterialTextureProperty.Any(x => x.TexID == pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value.Data);
            if (TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(TextureDictionary));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (Coordinate_ME_Data.RendererProperty.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(Coordinate_ME_Data.RendererProperty));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (Coordinate_ME_Data.MaterialFloatProperty.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(Coordinate_ME_Data.MaterialFloatProperty));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

            if (Coordinate_ME_Data.MaterialColorProperty.Count > 0)
                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(Coordinate_ME_Data.MaterialColorProperty));
            else
                SaveData.data.Add("MaterialColorPropertyList", null);

            if (Coordinate_ME_Data.MaterialTextureProperty.Count > 0)
                SaveData.data.Add("MaterialTexturePropertyList", MessagePackSerializer.Serialize(Coordinate_ME_Data.MaterialTextureProperty));
            else
                SaveData.data.Add("MaterialTexturePropertyList", null);

            if (Coordinate_ME_Data.MaterialShader.Count > 0)
                SaveData.data.Add("MaterialShaderList", MessagePackSerializer.Serialize(Coordinate_ME_Data.MaterialShader));
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

        #region ME_Loops
        private static void ME_Float_Loop(Queue<MaterialFloatProperty> FloatQueue, int ACCpostion, List<MaterialFloatProperty> MaterialFloat)
        {
            if (FloatQueue.Count == 0)
            {
                return;
            }

            if (FloatQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                int slot = FloatQueue.Peek().Slot;
                while (FloatQueue.Count != 0)
                {
                    MaterialFloatProperty ME_Info = FloatQueue.Dequeue();
                    ME_Info.Slot = ACCpostion;
                    MaterialFloat.Add(ME_Info);
                    if (FloatQueue.Count == 0 || FloatQueue.Peek().Slot != slot)
                    {
                        break;
                    }
                }
            }
            else
            {
                FloatQueue.Dequeue();
            }
        }

        private static void ME_Color_Loop(Queue<MaterialColorProperty> ColorQueue, int ACCpostion, List<MaterialColorProperty> MaterialColor)
        {
            if (ColorQueue.Count == 0)
            {
                return;
            }
            if (ColorQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                int slot = ColorQueue.Peek().Slot;
                while (ColorQueue.Count != 0)
                {
                    MaterialColorProperty ME_Info = ColorQueue.Dequeue();
                    ME_Info.Slot = ACCpostion;
                    MaterialColor.Add(ME_Info);
                    if (ColorQueue.Count == 0 || ColorQueue.Peek().Slot != slot)
                    {
                        break;
                    }
                }
            }
            else
            {
                ColorQueue.Dequeue();
            }
        }

        private static void ME_Texture_Loop(Queue<MaterialTextureProperty> TextureQueue, int ACCpostion, List<MaterialTextureProperty> MaterialTexture)
        {
            if (TextureQueue.Count == 0)
            {
                return;
            }

            if (TextureQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                int slot = TextureQueue.Peek().Slot;
                while (TextureQueue.Count != 0)
                {
                    MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                    ME_Info.Slot = ACCpostion;
                    MaterialTexture.Add(ME_Info);
                    if (TextureQueue.Count == 0 || TextureQueue.Peek().Slot != slot)
                    {
                        break;
                    }
                }
            }
            else
            {
                TextureQueue.Dequeue();
            }
        }

        private static void ME_Shader_Loop(Queue<MaterialShader> ShaderQueue, int ACCpostion, List<MaterialShader> MaterialShader)
        {
            if (ShaderQueue.Count == 0)
            {
                return;
            }

            if (ShaderQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                int slot = ShaderQueue.Peek().Slot;
                while (ShaderQueue.Count != 0)
                {
                    MaterialShader ME_Info = ShaderQueue.Dequeue();
                    ME_Info.Slot = ACCpostion;
                    MaterialShader.Add(ME_Info);
                    if (ShaderQueue.Count == 0 || ShaderQueue.Peek().Slot != slot)
                    {
                        break;
                    }
                }
            }
            else
            {
                ShaderQueue.Dequeue();
            }
        }

        private static void ME_Render_Loop(Queue<RendererProperty> RendererQueue, int ACCpostion, List<RendererProperty> Renderer)
        {
            if (RendererQueue.Count == 0)
            {
                return;
            }

            if (RendererQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                int slot = RendererQueue.Peek().Slot;
                while (RendererQueue.Count != 0)
                {
                    RendererProperty ME_Info = RendererQueue.Dequeue();
                    ME_Info.Slot = ACCpostion;
                    Renderer.Add(ME_Info);
                    if (RendererQueue.Count == 0 || RendererQueue.Peek().Slot != slot)
                    {
                        break;
                    }
                }
            }
            else
            {
                RendererQueue.Dequeue();
            }
        }
        #endregion

        private void Additional_Clothing_Process(int index, int outfitnum, ME_List ME_Data)
        {
            ME_Dont_Touch[outfitnum].Add(index);
            #region Remove Existing Data
            ThisOutfitData.Finished.MaterialColorProperty.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.Finished.MaterialShader.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.Finished.RendererProperty.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.Finished.MaterialFloatProperty.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.Finished.MaterialTextureProperty.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            #endregion

            #region AddData
            ThisOutfitData.Finished.MaterialColorProperty.AddRange(ME_Data.MaterialColorProperty.Where(x => x.Slot == index));
            ThisOutfitData.Finished.MaterialShader.AddRange(ME_Data.MaterialShader.Where(x => x.Slot == index));
            ThisOutfitData.Finished.RendererProperty.AddRange(ME_Data.RendererProperty.Where(x => x.Slot == index));
            ThisOutfitData.Finished.MaterialFloatProperty.AddRange(ME_Data.MaterialFloatProperty.Where(x => x.Slot == index));
            ThisOutfitData.Finished.MaterialTextureProperty.AddRange(ME_Data.MaterialTextureProperty.Where(x => x.Slot == index));
            #endregion
        }

        private void HairMatchProcess(int outfitnum, int ACCPosition, Color[] haircolor, List<ChaFileAccessory.PartsInfo> NewRAW)
        {
            if (ACCPosition < 20)
            {
                ChaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCPosition].color = haircolor;
            }
            else
            {
                NewRAW[ACCPosition - 20].color = haircolor;
            }
            var haircomponent = ThisOutfitData.Finished.MaterialColorProperty.FindAll(x => x.CoordinateIndex == outfitnum && x.Slot == ACCPosition && x.ObjectType == ObjectType.Accessory);
            for (int i = 0; i < haircomponent.Count; i++)
            {
                if (haircomponent[i].Property == "Color")
                {
                    haircomponent[i].Value = ChaControl.fileHair.parts[1].baseColor;
                }
                else if (haircomponent[i].Property == "Color2")
                {
                    haircomponent[i].Value = ChaControl.fileHair.parts[1].startColor;
                }
                else if (haircomponent[i].Property == "Color3")
                {
                    haircomponent[i].Value = ChaControl.fileHair.parts[1].endColor;
                }
                else if (haircomponent[i].Property == "ShadowColor")
                {
                    haircomponent[i].Value = ChaControl.fileHair.parts[1].outlineColor;
                }
            }
        }

        private void Original_ME_Data()
        {
            var KeepCloth = new List<int>[ThisOutfitData.Outfit_Size];

            for (int outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                KeepCloth[outfitnum] = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (CharacterClothingKeep_Coordinate[outfitnum][i] || PersonalClothingBools[i])
                    {
                        KeepCloth[outfitnum].Add(i);
                    }
                }
            }

            var Original_ME_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");

            var ME_Data = new ME_List(Original_ME_Data, ThisOutfitData, KeepCloth);

            for (int outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                foreach (var index in KeepCloth[outfitnum])
                {
                    ChaControl.chaFile.coordinate[outfitnum].clothes.parts[index] = ThisOutfitData.Original_Coordinates[outfitnum].clothes.parts[index];
                    if (!ME_Data.NoData)
                    {
                        Additional_Clothing_Process(index, outfitnum, ME_Data);
                    }
                }
            }
        }
    }
}