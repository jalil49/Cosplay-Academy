using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Maker;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;
#if TRACE
using System.Diagnostics;
#endif
using System.Linq;
#if !KKS
using MoreAccessoriesKOI;
using ToolBox;
#endif

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {
        private readonly Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();
        private readonly ChaDefault ThisOutfitData;
        private ChaControl ChaControl;
        private ChaFile ChaFile;

        private static bool InsideMaker = false;

        #region MoreAccessories
#if !KKS
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
        private MoreAccessories.CharAdditionalData More_Char_Data
        {
            get
            {
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
        private readonly bool[][] Underwearbools = new bool[Constants.Outfit_Size][]; //0: not bot; 1: notbra; 2: notshorts
        private readonly List<int>[] UnderwearAccessoriesLocations = new List<int>[Constants.Outfit_Size];
        private List<ChaFileAccessory.PartsInfo> Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>();
        private ME_List Underwear_ME_Data;
        #endregion

        #region ACI_Data
        private bool[] CharacterClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
        private bool[][] CharacterClothingKeep_Coordinate = new bool[Constants.Outfit_Size][];
        private bool[] MakeUpKeep = new bool[] { false, false, false, false, false, false, false, false };
        public bool Character_Cosplay_Ready = false;
        #endregion

        private readonly List<int>[] ME_Dont_Touch = new List<int>[Constants.Outfit_Size];

#if TRACE
        #region StopWatches
        private static bool TimeProcess = true;
        private static readonly Stopwatch[] TimeWatch = new Stopwatch[4];
        private static List<long>[] Average;
        #endregion
#endif
        private readonly bool[] ValidOutfits = new bool[Constants.Outfit_Size];

        public ClothingLoader(ChaDefault ThisOutfitData)
        {
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

            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                UnderwearAccessoriesLocations[i] = new List<int>();
                CharacterClothingKeep_Coordinate[i] = new bool[9];
                ME_Dont_Touch[i] = new List<int>();
            }
            this.ThisOutfitData = ThisOutfitData;
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

            Extract_Personal_Data();
            ThisOutfitData.Finished.Clear();

            int holdoutfitstate = ChaControl.fileStatus.coordinateType;
#if !KKS
            bool retain = (bool)InH_Field.GetValue();
#endif
            Underwear.LoadFile(ThisOutfitData.outfitpath[Constants.Outfit_Size]);
            Settings.Logger.LogDebug($"loaded underwear " + ThisOutfitData.outfitpath[Constants.Outfit_Size]);

            Underwear_ME_Data = new ME_List(ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.materialeditor"), ThisOutfitData, true);

            Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>(Underwear.accessory.parts);
            Underwear_PartsInfos.AddRange(Support.MoreAccessories.Coordinate_Accessory_Extract(Underwear));
#if !KKS
            InH_Field.SetValue(false);
#endif

            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                ValidOutfits[i] = ThisOutfitData.outfitpath[i].EndsWith(".png");
                if (ValidOutfits[i] || Settings.RandomizeUnderwear.Value && Underwear.GetLastErrorCode() == 0)
                {
                    GeneralizedLoad(i, ValidOutfits[i]);
                    if (ValidOutfits[i])
                    {
                        Settings.Logger.LogDebug($"loaded {(ChaFileDefine.CoordinateType)i} " + ThisOutfitData.outfitpath[i]);
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
            ValidOutfits[outfitnum] = load;
            ThisOutfitData.Finished.ClearCoord(outfitnum);
            UnderwearAccessoriesLocations[outfitnum].Clear();
            HairAccessories.Remove(outfitnum);
            ThisOutfitData.HairKeepReturn[outfitnum].Clear();
            ThisOutfitData.ACCKeepReturn[outfitnum].Clear();

            ChaControl.fileStatus.coordinateType = outfitnum;
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

            bool[] UnderClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
            Underwearbools[outfitnum] = new bool[] { false, false, false };

            #endregion

            //Load new outfit

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

                ThisCoordinate.LoadFile(ThisOutfitData.outfitpath[outfitnum]);
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
                if (ExpandedData.data.TryGetValue("CoordinateSaveBools", out var S_CoordinateSaveBools) && S_CoordinateSaveBools != null)
                {
                    UnderClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_CoordinateSaveBools);
                }
                if (ExpandedData.data.TryGetValue("HairAcc", out var S_HairAcc) && S_HairAcc != null)
                {
                    HairToColor = MessagePackSerializer.Deserialize<List<int>>((byte[])S_HairAcc);
                }
                if (ExpandedData.data.TryGetValue("ClothNot", out S_HairAcc) && S_HairAcc != null)
                {
                    Underwearbools[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])S_HairAcc);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (CharacterClothingKeep[i])
                {
                    UnderClothingKeep[i] = true;
                }
            }
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

            if (Settings.RandomizeUnderwear.Value && outfitnum != 3 && Underwear.GetLastErrorCode() == 0)
            {
                var Local_Underwear_ACC_Info = new List<ChaFileAccessory.PartsInfo>(Underwear_PartsInfos);
                var ObjectTypeList = new List<ObjectType>() { ObjectType.Accessory };
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

                if (ThisCoordinate.clothes.parts[0].id != 0 && !CharacterClothingKeep_Coordinate[outfitnum][2])
                {
                    if (!UnderClothingKeep[2] && !Underwearbools[outfitnum][1] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[2].id != 0)
                    {
                        ThisCoordinate.clothes.parts[2] = Underwear.clothes.parts[2];
                        Additional_Clothing_Process(2, outfitnum, Underwear_ME_Data);
                    }
                    if (Underwearbools[outfitnum][0])
                    {
                        if (!UnderClothingKeep[3] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[3].id != 0)
                        {
                            ThisCoordinate.clothes.parts[3] = Underwear.clothes.parts[3];
                            Additional_Clothing_Process(3, outfitnum, Underwear_ME_Data);
                        }
                    }
                }

                if (ThisCoordinate.clothes.parts[1].id != 0 && !Underwearbools[outfitnum][0] && !CharacterClothingKeep_Coordinate[outfitnum][3])
                {
                    if (!UnderClothingKeep[3] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[3].id != 0)
                    {
                        ThisCoordinate.clothes.parts[3] = Underwear.clothes.parts[3];
                        Additional_Clothing_Process(3, outfitnum, Underwear_ME_Data);
                    }
                }

                if (outfitnum != 2)
                {
                    if (ThisCoordinate.clothes.parts[5].id != 0 && !CharacterClothingKeep_Coordinate[outfitnum][5])
                    {
                        if (!UnderClothingKeep[5])
                        {
                            ThisCoordinate.clothes.parts[5] = Underwear.clothes.parts[5];
                            Additional_Clothing_Process(5, outfitnum, Underwear_ME_Data);
                        }
                        if (!UnderClothingKeep[6] && !CharacterClothingKeep_Coordinate[outfitnum][6])
                        {
                            ThisCoordinate.clothes.parts[6] = Underwear.clothes.parts[6];
                            Additional_Clothing_Process(6, outfitnum, Underwear_ME_Data);
                        }
                    }

                    if (!UnderClothingKeep[6] && ThisCoordinate.clothes.parts[6].id != 0 && !CharacterClothingKeep_Coordinate[outfitnum][6])
                    {
                        ThisCoordinate.clothes.parts[6] = Underwear.clothes.parts[6];
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
            bool print = true;

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
                SaveData = new PluginData();
                Inputdata = ExtendedSave.GetExtendedDataById(coordinate, "Additional_Card_Info");
                if (Inputdata != null)
                {
                    if (Inputdata.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                    {
                        HairKeepResult.AddRange(MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData));
                    }
                    if (Inputdata.data.ContainsKey("CoordinateSaveBools"))
                    {
                        SaveData.data.Add("CoordinateSaveBools", Inputdata.data["CoordinateSaveBools"]);
                    }
                    if (Inputdata.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                    {
                        ACCKeepResult.AddRange(MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData));
                    }
                    if (Inputdata.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("PersonalityType_Restriction", Inputdata.data["PersonalityType_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("TraitType_Restriction", Inputdata.data["TraitType_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("HstateType_Restriction", Inputdata.data["HstateType_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("ClubType_Restriction", Inputdata.data["ClubType_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("Height_Restriction", Inputdata.data["Height_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("Breastsize_Restriction", Inputdata.data["Breastsize_Restriction"]);
                    }
                    if (Inputdata.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("CoordinateType", Inputdata.data["CoordinateType"]);
                    }
                    if (Inputdata.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("CoordinateSubType", Inputdata.data["CoordinateSubType"]);
                    }
                    if (Inputdata.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("Creator", Inputdata.data["Creator"]);
                    }
                    if (Inputdata.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("Set_Name", Inputdata.data["Set_Name"]);
                    }
                    if (Inputdata.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("SubSetNames", Inputdata.data["SubSetNames"]);
                    }
                    if (Inputdata.data.TryGetValue("ClothNot", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("ClothNot", Inputdata.data["ClothNot"]);
                    }
                    if (Inputdata.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
                    {
                        SaveData.data.Add("GenderType", Inputdata.data["GenderType"]);
                    }
                }
                SaveData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairKeepResult));
                SaveData.data.Add("AccKeep", MessagePackSerializer.Serialize(ACCKeepResult));

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

        private void Extract_Personal_Data()
        {
            var PersonalData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
            if (PersonalData != null)
            {
                if (PersonalData.data.TryGetValue("Personal_Clothing_Save", out var ByteData) && ByteData != null)
                {
                    CharacterClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (PersonalData.data.TryGetValue("MakeUpKeep", out ByteData) && ByteData != null)
                {
                    MakeUpKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                }
                if (PersonalData.data.TryGetValue("Personal_Coordinate_Clothing_Save", out ByteData) && ByteData != null)
                {
                    CharacterClothingKeep_Coordinate = MessagePackSerializer.Deserialize<bool[][]>((byte[])ByteData);
                }
            }
        }

        private void Original_ME_Data()
        {
            var KeepCloth = new List<int>[Constants.Outfit_Size];

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                KeepCloth[outfitnum] = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (CharacterClothingKeep_Coordinate[outfitnum][i] || CharacterClothingKeep[i])
                    {
                        KeepCloth[outfitnum].Add(i);
                    }
                }
            }

            var Original_ME_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");

            var ME_Data = new ME_List(Original_ME_Data, ThisOutfitData, KeepCloth);

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
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