﻿using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using ToolBox;
using UnityEngine;

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {
        private Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> HairAccessories;
        private ChaDefault ThisOutfitData;
        private ChaControl ChaControl;
        private ChaFile ChaFile;
        public readonly ChaFileCoordinate Underwear = new ChaFileCoordinate();
        //private ME_Support Underwear_ME = new ME_Support();
        private readonly bool[][] Underwearbools = new bool[Constants.Outfit_Size][];
        //private int[] UnderwearAccessoryStart = new int[Constants.Outfit_Size];
        private readonly List<int>[] UnderwearAccessoriesLocations = new List<int>[Constants.Outfit_Size];
        private List<ChaFileAccessory.PartsInfo> Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>();
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
        //private static Stopwatch FullTime = new Stopwatch();
        private static bool InsideMaker = false;
        public ClothingLoader()
        {
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                UnderwearAccessoriesLocations[i] = new List<int>();
                CharacterClothingKeep_Coordinate[i] = new bool[9];
            }
        }

        #region ACI_Data
        private bool[] CharacterClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
        private bool[][] CharacterClothingKeep_Coordinate = new bool[Constants.Outfit_Size][];
        private bool[] MakeUpKeep = new bool[] { false, false, false, false, false, false, false, false };
        public bool Character_Cosplay_Ready = false;
        #endregion

        public void FullLoad(ChaDefault InputOutfitData, ChaControl character, ChaFile file)
        {
            InsideMaker = MakerAPI.InsideMaker;
            ChaControl = character;
            ChaFile = file;
            ThisOutfitData = InputOutfitData;
            var HairPlugin = new PluginData();
            ThisOutfitData.Soft_Clear_ME();

            Extract_Personal_Data();

            HairAccessories = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();

            bool retain = (bool)Traverse.Create(MoreAccessories._self).Field("_inH").GetValue();
            Traverse.Create(MoreAccessories._self).Field("_inH").SetValue(false);
            int holdoutfitstate = ChaControl.fileStatus.coordinateType;

            Underwear.LoadFile(ThisOutfitData.Underwear);
            Settings.Logger.LogDebug($"loaded underwear " + ThisOutfitData.Underwear);

            //ExpandedOutfit.Logger.LogWarning($"underwear is {ThisOutfitData.Underwear}");
            int Original_Coord = ChaControl.fileStatus.coordinateType;

            if (_accessoriesByChar.TryGetValue(ChaFile, out var SaveAccessory) == false)
            {
                SaveAccessory = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ThisOutfitData.Chafile, SaveAccessory);
            }

            ChaControl.chaFile.coordinate[Original_Coord].LoadFile(ThisOutfitData.Underwear);
            Underwear_PartsInfos = new List<ChaFileAccessory.PartsInfo>(ChaControl.chaFile.coordinate[Original_Coord].accessory.parts);
            Underwear_PartsInfos.AddRange(new List<ChaFileAccessory.PartsInfo>(SaveAccessory.nowAccessories));
            //FullTime.Start();
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                UnderwearAccessoriesLocations[i].Clear();
                GeneralizedLoad(i);
                Settings.Logger.LogDebug($"loaded {i} " + ThisOutfitData.outfitpath[i]);
            }
            //FullTime.Stop();
            //ExpandedOutfit.Logger.LogWarning($"Total Generalized load time {FullTime.ElapsedMilliseconds}");
            //ChaControl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)Original_Coord);
            ChaControl.fileStatus.coordinateType = holdoutfitstate;
            Traverse.Create(MoreAccessories._self).Field("_inH").SetValue(retain);
            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin, ChaControl, ThisOutfitData);

            ThisOutfitData.ME_Work = true;
            Run_Repacks(character, ThisOutfitData);
        }

        private void GeneralizedLoad(int outfitnum)
        {
            //queue Accessorys to keep
            #region Queue accessories to keep

            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            var HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);
            var UnderwearAccessoryStart = PartsQueue.Count();
            var HairKeepQueue = new Queue<bool>(ThisOutfitData.HairKeepQueue[outfitnum]);
            var ACCKeepqueue = new Queue<bool>(ThisOutfitData.ACCKeepQueue[outfitnum]);

            var RenderQueue = new Queue<RendererProperty>(ThisOutfitData.RendererPropertyQueue[outfitnum]);
            var FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.MaterialFloatPropertyQueue[outfitnum]);
            var ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.MaterialColorPropertyQueue[outfitnum]);
            var TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.MaterialTexturePropertyQueue[outfitnum]);
            var ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.MaterialShaderQueue[outfitnum]);

            bool[] UnderClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
            Underwearbools[outfitnum] = new bool[] { false, false, false };

            #endregion
            //Load new outfit
            ChaControl.fileStatus.coordinateType = outfitnum;
            var ThisCoordinate = ChaControl.chaFile.coordinate[outfitnum];
            ThisCoordinate.LoadFile(ThisOutfitData.outfitpath[outfitnum]);

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

            if (_accessoriesByChar.TryGetValue(ChaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaFile, data);
            }

            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }
            var Inputdata = ExtendedSave.GetExtendedDataById(ThisCoordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            var HairAccInfo = new Dictionary<int, HairSupport.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    HairAccInfo = MessagePackSerializer.Deserialize<Dictionary<int, HairSupport.HairAccessoryInfo>>((byte[])loadedHairAccessories);

            List<RendererProperty> Renderer = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloat = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColor = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexture = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShade = new List<MaterialShader>();


            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(ThisCoordinate, "com.deathweasel.bepinex.materialeditor");

            if (MaterialEditorData?.data != null)
            {
                List<ObjectType> objectTypesToLoad = new List<ObjectType>
                {
                    ObjectType.Accessory,
                    ObjectType.Character,
                    ObjectType.Clothing,
                    ObjectType.Hair
                };
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

                if (MaterialEditorData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                {
                    foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                        importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                }

                if (MaterialEditorData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialShade.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnRenderer.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialFloat.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialColor.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                            ThisOutfitData.ReturnMaterialTexture.Add(newTextureProperty);
                        }
                    }
                }
            }
            #endregion

            if (Settings.RandomizeUnderwear.Value && outfitnum != 3 && Underwear.GetLastErrorCode() == 0)
            {
                #region additonal ME_Data
                List<MaterialShader> ME_MS_properties = new List<MaterialShader>();
                List<RendererProperty> ME_R_properties = new List<RendererProperty>();
                List<MaterialColorProperty> ME_MC_properties = new List<MaterialColorProperty>();
                List<MaterialFloatProperty> ME_MF_properties = new List<MaterialFloatProperty>();
                List<MaterialTextureProperty> ME_MT_properties = new List<MaterialTextureProperty>();
                #endregion

                var Underwear_ME_Data = ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.materialeditor");

                if (Underwear_ME_Data?.data != null)
                {
                    List<ObjectType> objectTypesToLoad = new List<ObjectType>
                    {
                        ObjectType.Accessory,
                        ObjectType.Character,
                        ObjectType.Clothing,
                        ObjectType.Hair
                    };
                    Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

                    int offset = ThisOutfitData.ME.TextureDictionary.Count;
                    if (Underwear_ME_Data.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key + offset] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                                ME_MS_properties.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                                ME_R_properties.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                                ME_MF_properties.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                                ME_MC_properties.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (Underwear_ME_Data.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            {
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionaryList[(int)loadedProperty.TexID + offset];

                                ME_MT_properties.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            }
                        }
                    }
                }

                for (int i = 0; i < Underwear_PartsInfos.Count; i++)
                {
                    if (Underwear_PartsInfos[i].id != 120 && Underwear_PartsInfos[i].id != 0)
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

                        var ColorList = ME_MC_properties.FindAll(x => x.ObjectType == ObjectType.Accessory && x.Slot == i);
                        var FloatList = ME_MF_properties.FindAll(x => x.ObjectType == ObjectType.Accessory && x.Slot == i);
                        var ShaderList = ME_MS_properties.FindAll(x => x.ObjectType == ObjectType.Accessory && x.Slot == i);
                        var TextureList = ME_MT_properties.FindAll(x => x.ObjectType == ObjectType.Accessory && x.Slot == i);
                        var RenderList = ME_R_properties.FindAll(x => x.ObjectType == ObjectType.Accessory && x.Slot == i);

                        if (ColorList.Count == 0)
                        {
                            Color color = new Color(0, 0, 0);
                            ColorList.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, -1, "", "", color, color));
                        }
                        if (FloatList.Count == 0)
                        {
                            FloatList.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, -1, "", "", "", ""));
                        }
                        if (ShaderList.Count == 0)
                        {
                            ShaderList.Add(new MaterialShader(ObjectType.Unknown, outfitnum, -1, "", 0, 0));
                        }
                        if (TextureList.Count == 0)
                        {
                            TextureList.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, -1, "", ""));
                        }
                        if (RenderList.Count == 0)
                        {
                            RenderList.Add(new RendererProperty(ObjectType.Unknown, outfitnum, -1, "", RendererProperties.Enabled, "", ""));
                        }

                        foreach (var item in ColorList)
                        {
                            ColorQueue.Enqueue(item);
                        }
                        foreach (var item in FloatList)
                        {
                            FloatQueue.Enqueue(item);
                        }
                        foreach (var item in ShaderList)
                        {
                            ShaderQueue.Enqueue(item);
                        }
                        foreach (var item in TextureList)
                        {
                            TextureQueue.Enqueue(item);
                        }
                        foreach (var item in RenderList)
                        {
                            RenderQueue.Enqueue(item);
                        }
                        PartsQueue.Enqueue(Underwear_PartsInfos[i]);
                        HairQueue.Enqueue(ACCdata);
                        //}
                        //ExpandedOutfit.Logger.LogWarning($"Queued new part: {i} ID: {Underwear_PartsInfos[i].id}");

                    }
                }


                if (ThisCoordinate.clothes.parts[0].id != 0)
                {
                    if (!UnderClothingKeep[2] && !Underwearbools[outfitnum][1] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[2].id != 0)
                    {
                        ThisCoordinate.clothes.parts[2] = Underwear.clothes.parts[2];
                        Additional_Clothing_Process(2, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                    if (Underwearbools[outfitnum][0])
                    {
                        if (!UnderClothingKeep[3] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[3].id != 0)
                        {
                            ThisCoordinate.clothes.parts[3] = Underwear.clothes.parts[3];
                            Additional_Clothing_Process(3, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                        }
                    }
                }

                if (ThisCoordinate.clothes.parts[1].id != 0 && !Underwearbools[outfitnum][0])
                {
                    if (!UnderClothingKeep[3] && !Underwearbools[outfitnum][2] && ThisCoordinate.clothes.parts[3].id != 0)
                    {
                        ThisCoordinate.clothes.parts[3] = Underwear.clothes.parts[3];
                        Additional_Clothing_Process(3, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                }

                if (outfitnum != 2)
                {
                    if (ThisCoordinate.clothes.parts[5].id != 0)
                    {
                        if (!UnderClothingKeep[5])
                        {
                            ThisCoordinate.clothes.parts[5] = Underwear.clothes.parts[5];
                            Additional_Clothing_Process(5, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                        }
                        if (!UnderClothingKeep[6])
                        {
                            ThisCoordinate.clothes.parts[6] = Underwear.clothes.parts[6];
                            Additional_Clothing_Process(6, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                        }
                    }

                    if (!UnderClothingKeep[6] && ThisCoordinate.clothes.parts[6].id != 0)
                    {
                        ThisCoordinate.clothes.parts[6] = Underwear.clothes.parts[6];
                        Additional_Clothing_Process(6, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                }
            }

            Color[] haircolor = new Color[] { ChaControl.fileHair.parts[1].baseColor, ChaControl.fileHair.parts[1].startColor, ChaControl.fileHair.parts[1].endColor, ChaControl.fileHair.parts[1].outlineColor };
            if (Settings.HairMatch.Value && !MakerAPI.InsideMaker)
            {
                foreach (var item in HairToColor)
                {
                    HairMatchProcess(outfitnum, item, haircolor, NewRAW);
                }
            }
            int insert = 0;
            int ACCpostion = 0;
            bool Empty;
            bool print = false;
            if (MakerAPI.InsideMaker)
            {
                //Normal
                for (int n = ThisCoordinate.accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
                {
                    Empty = ThisCoordinate.accessory.parts[ACCpostion].type == 120;
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

                        ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                        ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                        ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                        ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                        ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, NewRAW);
                    }
                }

                //MoreAccessories
                for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
                {
                    Empty = NewRAW[ACCpostion - 20].type == 120;
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

                        ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                        ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                        ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                        ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                        ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);
                    }
                    if (Settings.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                    {
                        info.ColorMatch = true;
                        HairMatchProcess(outfitnum, ACCpostion, haircolor, NewRAW);
                    }
                }
                print = true;
            }
            else
            {
                ACCpostion = 20 + NewRAW.Count;
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

                ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);

                ACCpostion++;
            }

            HairAccessories.Add(outfitnum, HairAccInfo);
            while (data.infoAccessory.Count < data.nowAccessories.Count)
                data.infoAccessory.Add(null);
            while (data.objAccessory.Count < data.nowAccessories.Count)
                data.objAccessory.Add(null);
            while (data.objAcsMove.Count < data.nowAccessories.Count)
                data.objAcsMove.Add(new GameObject[2]);
            while (data.cusAcsCmp.Count < data.nowAccessories.Count)
                data.cusAcsCmp.Add(null);
            while (data.showAccessories.Count < data.nowAccessories.Count)
                data.showAccessories.Add(true);

            ThisOutfitData.ReturnMaterialColor.AddRange(MaterialColor);

            ThisOutfitData.ReturnMaterialFloat.AddRange(MaterialFloat);

            ThisOutfitData.ReturnMaterialShade.AddRange(MaterialShade);

            ThisOutfitData.ReturnMaterialTexture.AddRange(MaterialTexture);

            ThisOutfitData.ReturnRenderer.AddRange(Renderer);
            #endregion

        }

        public void CoordinateLoad(ChaDefault ThisOutfitData, ChaFileCoordinate coordinate, ChaControl ChaControl, bool Raw = false)
        {
            InsideMaker = MakerAPI.InsideMaker;
            ChaFile ChaFile = ChaControl.chaFile;
            #region Queue accessories to keep

            int outfitnum = ChaControl.fileStatus.coordinateType;

            Queue<ChaFileAccessory.PartsInfo> PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            Queue<HairSupport.HairAccessoryInfo> HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

            Queue<bool> ACCKeepQueue = new Queue<bool>();
            Queue<bool> HairKeepQueue = new Queue<bool>();
            if (Constants.PluginResults["Additional_Card_Info"] && InsideMaker)
            {
                HairKeepQueue = new Queue<bool>(ThisOutfitData.HairKeepQueue[outfitnum]);
                ACCKeepQueue = new Queue<bool>(ThisOutfitData.ACCKeepQueue[outfitnum]);
            }
            List<int> HairKeepResult = new List<int>();
            List<int> ACCKeepResult = new List<int>();

            Queue<RendererProperty> RenderQueue = new Queue<RendererProperty>(ThisOutfitData.RendererPropertyQueue[outfitnum]);
            Queue<MaterialFloatProperty> FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.MaterialFloatPropertyQueue[outfitnum]);
            Queue<MaterialColorProperty> ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.MaterialColorPropertyQueue[outfitnum]);
            Queue<MaterialTextureProperty> TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.MaterialTexturePropertyQueue[outfitnum]);
            Queue<MaterialShader> ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.MaterialShaderQueue[outfitnum]);

            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor");
            List<RendererProperty> Renderer = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloat = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColor = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexture = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShade = new List<MaterialShader>();
            Dictionary<int, int> importDictionary = new Dictionary<int, int>();
            if (MaterialEditorData?.data != null)
            {
                List<ObjectType> objectTypesToLoad = new List<ObjectType>
                {
                    ObjectType.Accessory,
                    ObjectType.Character,
                    ObjectType.Clothing,
                    ObjectType.Hair
                };

                if (MaterialEditorData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                {
                    foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                        importDictionary[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                }

                if (MaterialEditorData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialShade.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            Renderer.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialFloat.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialColor.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionary[(int)loadedProperty.TexID];

                            MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                            MaterialTexture.Add(newTextureProperty);
                        }
                    }
                }
            }
            #endregion

            #endregion

            //Apply pre-existing Accessories in any open slot or final slots.
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaFile, data);
            }
            List<ChaFileAccessory.PartsInfo> MoreACCData;
            ChaFileAccessory.PartsInfo[] OriginalData;
            if (Raw)
            {
                MoreACCData = data.rawAccessoriesInfos[ChaFile.status.coordinateType];
                OriginalData = ChaFile.coordinate[ChaFile.status.coordinateType].accessory.parts;

            }
            else
            {
                MoreACCData = data.nowAccessories;
                OriginalData = ChaControl.nowCoordinate.accessory.parts;
            }

            #region Reassign Exisiting Accessories

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
                        HairQueue.Dequeue();
                    }

                    ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                    ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                    ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                    ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                    ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);
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
                        HairQueue.Dequeue();
                    }

                    ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                    ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                    ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                    ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                    ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);

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

                ME_Render_Loop(RenderQueue, ACCpostion, Renderer);

                ME_Color_Loop(ColorQueue, ACCpostion, MaterialColor);

                ME_Texture_Loop(TextureQueue, ACCpostion, MaterialTexture, ThisOutfitData);

                ME_Float_Loop(FloatQueue, ACCpostion, MaterialFloat);

                ME_Shader_Loop(ShaderQueue, ACCpostion, MaterialShade);

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

            while (data.infoAccessory.Count < data.nowAccessories.Count)
                data.infoAccessory.Add(null);
            while (data.objAccessory.Count < data.nowAccessories.Count)
                data.objAccessory.Add(null);
            while (data.objAcsMove.Count < data.nowAccessories.Count)
                data.objAcsMove.Add(new GameObject[2]);
            while (data.cusAcsCmp.Count < data.nowAccessories.Count)
                data.cusAcsCmp.Add(null);
            while (data.showAccessories.Count < data.nowAccessories.Count)
                data.showAccessories.Add(true);
            #endregion

            #region Pack
            var SaveData = new PluginData();

            if (ThisOutfitData.ME.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ThisOutfitData.ME.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (Renderer.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(Renderer));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (MaterialFloat.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(MaterialFloat));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

            if (MaterialColor.Count > 0)
                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(MaterialColor));
            else
                SaveData.data.Add("MaterialColorPropertyList", null);

            if (MaterialTexture.Count > 0)
                SaveData.data.Add("MaterialTexturePropertyList", MessagePackSerializer.Serialize(MaterialTexture));
            else
                SaveData.data.Add("MaterialTexturePropertyList", null);

            if (MaterialShade.Count > 0)
                SaveData.data.Add("MaterialShaderList", MessagePackSerializer.Serialize(MaterialShade));
            else
                SaveData.data.Add("MaterialShaderList", null);

            ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor", SaveData);

            if (Constants.PluginResults["Additional_Card_Info"] && InsideMaker)
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
                ControllerCoordReload_Loop(Type.GetType("Additional_Card_Info.CharaEvent, Additional_Card_Info", false), ChaControl, coordinate);
            }

            #endregion

            ControllerCoordReload_Loop(typeof(KK_Plugins.MaterialEditor.MaterialEditorCharaController), ChaControl, coordinate);

            if (Settings.HairMatch.Value)
            {
                var Plugdata = new PluginData();

                Plugdata.data.Add("CoordinateHairAccessories", MessagePackSerializer.Serialize(HairACCDictionary));
                ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer", Plugdata);

                ControllerCoordReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl, coordinate);
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

        private static void ME_Texture_Loop(Queue<MaterialTextureProperty> TextureQueue, int ACCpostion, List<MaterialTextureProperty> MaterialTexture, ChaDefault ThisOutfitData)
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
                    if (!ThisOutfitData.ME_Work)
                    {
                        if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                        {
                            ME_Info.TexID = ThisOutfitData.ME.SetAndGetTextureID(imgbyte);
                        }
                    }
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

        public static void SetExtendedData(string IDtoSET, PluginData data, ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            ChaFile ChaFile = ChaControl.chaFile;
            ExtendedSave.SetExtendedDataById(ChaFile, IDtoSET, data);
            ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);

            if (ThisOutfitData.heroine != null && ChaControl.sex == 1)
            {
                ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.charFile, IDtoSET, data);
                //ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.chaCtrl.chaFile, IDtoSET, data);
            }
        }

        private void Additional_Clothing_Process(int index, int outfitnum, List<MaterialColorProperty> ME_MC_properties, List<MaterialShader> ME_MS_properties, List<RendererProperty> ME_R_properties, List<MaterialFloatProperty> ME_MF_properties, List<MaterialTextureProperty> ME_MT_properties)
        {
            #region Remove Existing Data
            ThisOutfitData.ReturnMaterialColor.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialShade.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnRenderer.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialFloat.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialTexture.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            #endregion

            #region AddData
            ThisOutfitData.ReturnMaterialColor.AddRange(ME_MC_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialShade.AddRange(ME_MS_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnRenderer.AddRange(ME_R_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialFloat.AddRange(ME_MF_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialTexture.AddRange(ME_MT_properties.Where(x => x.Slot == index).ToList());
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
            var haircomponent = ThisOutfitData.ReturnMaterialColor.FindAll(x => x.CoordinateIndex == outfitnum && x.Slot == ACCPosition && x.ObjectType == ObjectType.Accessory);
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
            var KeepCloth = new List<int>();
            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                KeepCloth.Clear();
                for (int i = 0; i < 9; i++)
                {
                    if (CharacterClothingKeep_Coordinate[outfitnum][i])
                    {
                        KeepCloth.Add(i);
                    }
                }
                if (KeepCloth.Count == 0)
                {
                    continue;
                }

                #region Orginal Coord ME Import
                var Original_ME_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");
                List<MaterialShader> Original_ME_MS_properties = new List<MaterialShader>();
                List<RendererProperty> Original_ME_R_properties = new List<RendererProperty>();
                List<MaterialColorProperty> Original_ME_MC_properties = new List<MaterialColorProperty>();
                List<MaterialFloatProperty> Original_ME_MF_properties = new List<MaterialFloatProperty>();
                List<MaterialTextureProperty> Original_ME_MT_properties = new List<MaterialTextureProperty>();

                if (Original_ME_Data?.data != null)
                {
                    Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

                    if (Original_ME_Data.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }

                    if (Original_ME_Data.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth.Contains(loadedProperty.Slot))
                                Original_ME_MS_properties.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }

                    if (Original_ME_Data.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth.Contains(loadedProperty.Slot))
                                Original_ME_R_properties.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (Original_ME_Data.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth.Contains(loadedProperty.Slot))
                                Original_ME_MF_properties.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (Original_ME_Data.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth.Contains(loadedProperty.Slot))
                                Original_ME_MC_properties.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (Original_ME_Data.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth.Contains(loadedProperty.Slot))
                            {
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionaryList[(int)loadedProperty.TexID];

                                Original_ME_MT_properties.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }
                foreach (var index in KeepCloth)
                {
                    ChaControl.chaFile.coordinate[outfitnum].clothes.parts[index] = ThisOutfitData.Original_Coordinates[outfitnum].clothes.parts[index];

                    Additional_Clothing_Process(index, outfitnum, Original_ME_MC_properties, Original_ME_MS_properties, Original_ME_R_properties, Original_ME_MF_properties, Original_ME_MT_properties);
                }
                #endregion
            }
        }
    }
}