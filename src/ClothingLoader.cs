using Cosplay_Academy.Hair;
using ExtensibleSaveFormat;
using HarmonyLib;
using KK_Plugins;
using KK_Plugins.DynamicBoneEditor;
using KKABMX.Core;
using KKAPI;
using KoiClothesOverlayX;
using MessagePack;
using MoreAccessoriesKOI;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UnityEngine;
using CoordinateType = ChaFileDefine.CoordinateType;
namespace Cosplay_Academy
{
    public class ClothingLoader
    {
        private Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> HairAccessories;
        private ChaDefault ThisOutfitData;
        private ChaControl ChaControl;
        private ChaFile ChaFile;
        private ChaFileCoordinate Underwear = new ChaFileCoordinate();

        public void FullLoad(ChaDefault InputOutfitData, ChaControl character, ChaFile file)
        {
            ChaControl = character;
            ChaFile = file;
            ThisOutfitData = InputOutfitData;
            var HairPlugin = new PluginData();
            ThisOutfitData.Soft_Clear_ME();


            HairAccessories = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();

            bool retain = (bool)Traverse.Create(MoreAccessories._self).Field("_inH").GetValue();
            Traverse.Create(MoreAccessories._self).Field("_inH").SetValue(false);
            int holdoutfitstate = ChaControl.fileStatus.coordinateType;

            Underwear.LoadFile(ThisOutfitData.Underwear);

            for (int i = 0; i < Constants.outfitpath; i++)
            {
                GeneralizedLoad(i);
                ExpandedOutfit.Logger.LogDebug($"loaded {i} " + ThisOutfitData.outfitpath[i]);
            }

            ChaControl.fileStatus.coordinateType = holdoutfitstate;
            Traverse.Create(MoreAccessories._self).Field("_inH").SetValue(retain);
            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin, ChaControl, ThisOutfitData);


            //var Hair_Acc_Controller = Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false);

            //if (Hair_Acc_Controller != null)
            //{
            //    UnityEngine.Component HairACC_Controller = ChaControl.gameObject.GetComponent(Hair_Acc_Controller);
            //    object[] OnReloadArray = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
            //    Traverse.Create(HairACC_Controller).Method("OnReload", OnReloadArray).GetValue();
            //}
            ControllerReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl);

            ThisOutfitData.ME_Work = true;
            ME_RePack(character, ThisOutfitData);
            KCOX_RePack(character, ThisOutfitData);
            KKABM_Repack(character, ThisOutfitData);
            DynamicBone_Repack(character, ThisOutfitData);
            PushUp_RePack(character, ThisOutfitData);
            ClothingUnlocker_RePack(character, ThisOutfitData);
        }

        private void GeneralizedLoad(int outfitnum)
        {
            //queue Accessorys to keep
            #region Queue accessories to keep
            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            var HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

            var RenderQueue = new Queue<RendererProperty>(ThisOutfitData.RendererPropertyQueue[outfitnum]);
            var FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.MaterialFloatPropertyQueue[outfitnum]);
            var ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.MaterialColorPropertyQueue[outfitnum]);
            var TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.MaterialTexturePropertyQueue[outfitnum]);
            var ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.MaterialShaderQueue[outfitnum]);

#if Debug
            ExpandedOutfit.Logger.LogWarning($"Parts: {PartsQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Hair: {HairQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Render: {RenderQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Float: {FloatQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"tColor: {ColorQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Texture: {TextureQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Shader: {ShaderQueue.Count}");
#endif
            #endregion
            //Load new outfit
            ChaControl.fileStatus.coordinateType = outfitnum;
            ChaControl.chaFile.coordinate[outfitnum].LoadFile(ThisOutfitData.outfitpath[outfitnum]);

            bool[] UnderClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
            bool[] CharacterClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };

            var ExpandedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile.coordinate[outfitnum], "Required_ACC");
            var PersonalData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Required_ACC");
            if (ExpandedData != null)
            {
                if (ExpandedData.data.TryGetValue("CoordinateSaveBools", out var S_CoordinateSaveBools) && S_CoordinateSaveBools != null)
                {
                    UnderClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_CoordinateSaveBools);
                }
            }
            if (PersonalData != null)
            {
                if (PersonalData.data.TryGetValue("Personal_Clothing_Save", out var S_Clothing_Save) && S_Clothing_Save != null)
                {
                    CharacterClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_Clothing_Save);
                    for (int i = 0; i < 9; i++)
                    {
                        if (CharacterClothingKeep[i])
                        {
                            UnderClothingKeep[i] = true;
                        }
                    }
                }
            }

            #region Reassign Existing Accessories

            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

            if (_accessoriesByChar.TryGetValue(ChaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaFile, data);
            }

            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }
            var Inputdata = ExtendedSave.GetExtendedDataById(ChaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
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
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(ChaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");

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
            if (ExpandedOutfit.RandomizeUnderwear.Value && outfitnum != 3 && Underwear != null)
            {
                var Underwear_ME_Data = ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.materialeditor");
                List<MaterialShader> ME_MS_properties = new List<MaterialShader>();
                List<RendererProperty> ME_R_properties = new List<RendererProperty>();
                List<MaterialColorProperty> ME_MC_properties = new List<MaterialColorProperty>();
                List<MaterialFloatProperty> ME_MF_properties = new List<MaterialFloatProperty>();
                List<MaterialTextureProperty> ME_MT_properties = new List<MaterialTextureProperty>();
                //Dictionary<int, byte[]> importDict = new Dictionary<int, byte[]>();
                if (Underwear_ME_Data?.data != null)
                {
                    //    List<ObjectType> objectTypesToLoad = new List<ObjectType>
                    //{
                    //    ObjectType.Accessory,
                    //    ObjectType.Character,
                    //    ObjectType.Clothing,
                    //    ObjectType.Hair
                    //};
                    Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();


                    if (Underwear_ME_Data.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing)
                                ME_MS_properties.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing)
                                ME_R_properties.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing)
                                ME_MF_properties.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing)
                                ME_MC_properties.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }


                    if (Underwear_ME_Data.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Clothing)
                            {
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionaryList[(int)loadedProperty.TexID];

                                ME_MT_properties.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            }
                        }
                    }
                }

                if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[0].id != 0)
                {
                    if (!UnderClothingKeep[2] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[2].id != 0)
                    {
                        ChaControl.chaFile.coordinate[outfitnum].clothes.parts[2] = Underwear.clothes.parts[2];
                        Underwear_ME(2, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                    if (!UnderClothingKeep[3] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3].id != 0)
                    {
                        ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3] = Underwear.clothes.parts[3];
                        Underwear_ME(3, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                }
                //if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[1].id != 0)
                //{
                //    ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3] = Underwear.clothes.parts[3];
                //}
                if (outfitnum != 2)
                {
                    if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[5].id != 0)
                    {
                        if (!UnderClothingKeep[5])
                        {
                            ChaControl.chaFile.coordinate[outfitnum].clothes.parts[5] = Underwear.clothes.parts[5];
                            Underwear_ME(5, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                        }
                        if (!UnderClothingKeep[6])
                        {
                            ChaControl.chaFile.coordinate[outfitnum].clothes.parts[6] = Underwear.clothes.parts[6];
                            Underwear_ME(6, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                        }
                    }

                    if (!UnderClothingKeep[6] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[6].id != 0)
                    {
                        ChaControl.chaFile.coordinate[outfitnum].clothes.parts[6] = Underwear.clothes.parts[6];
                        Underwear_ME(6, outfitnum, ME_MC_properties, ME_MS_properties, ME_R_properties, ME_MF_properties, ME_MT_properties);
                    }
                }
            }
            var Original_ME_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");
            List<MaterialShader> Original_ME_MS_properties = new List<MaterialShader>();
            List<RendererProperty> Original_ME_R_properties = new List<RendererProperty>();
            List<MaterialColorProperty> Original_ME_MC_properties = new List<MaterialColorProperty>();
            List<MaterialFloatProperty> Original_ME_MF_properties = new List<MaterialFloatProperty>();
            List<MaterialTextureProperty> Original_ME_MT_properties = new List<MaterialTextureProperty>();
            //Dictionary<int, byte[]> importDict = new Dictionary<int, byte[]>();
            if (Original_ME_Data?.data != null)
            {
                //    List<ObjectType> objectTypesToLoad = new List<ObjectType>
                //{
                //    ObjectType.Accessory,
                //    ObjectType.Character,
                //    ObjectType.Clothing,
                //    ObjectType.Hair
                //};
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
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            Original_ME_MS_properties.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }


                if (Original_ME_Data.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            Original_ME_R_properties.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }


                if (Original_ME_Data.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            Original_ME_MF_properties.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }


                if (Original_ME_Data.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            Original_ME_MC_properties.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }


                if (Original_ME_Data.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            Original_ME_MT_properties.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }
                }
            }
            for (int i = 0; i < CharacterClothingKeep.Length; i++)
            {
                if (!CharacterClothingKeep[i])
                {
                    continue;
                }
                ChaControl.chaFile.coordinate[outfitnum].clothes.parts[i] = ThisOutfitData.Original_Coordinates[outfitnum].clothes.parts[i];
                Underwear_ME(i, outfitnum, Original_ME_MC_properties, Original_ME_MS_properties, Original_ME_R_properties, Original_ME_MF_properties, Original_ME_MT_properties);
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning("Start loading accessories");
#endif
            int ACCpostion = 0;
            bool Empty;
            for (int n = ChaControl.chaFile.coordinate[outfitnum].accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = ChaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    ChaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        HairAccInfo[ACCpostion] = HairQueue.Dequeue();
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
                }
                if (ExpandedOutfit.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
#if Debug
                //ExpandedOutfit.Logger.LogWarning("Force Color Pass");
#endif
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"Start extra accessories at {ACCpostion} {NewRAW.Count}");
#endif
            for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
            {
                Empty = NewRAW[ACCpostion - 20].type == 120;
                if (Empty) //120 is empty/default
                {
                    NewRAW[ACCpostion - 20] = PartsQueue.Dequeue();

                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        HairAccInfo[ACCpostion] = HairQueue.Dequeue();
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
                }
                if (ExpandedOutfit.HairMatch.Value && HairAccInfo.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"Start making extra accessories at {ACCpostion}");
#endif

            bool print = true;

            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    ExpandedOutfit.Logger.LogDebug($"Ran out of space in new coordinate adding {PartsQueue.Count}");
                    print = false;
                }
                NewRAW.Add(PartsQueue.Dequeue());
                if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                {
                    var HairInfo = HairQueue.Dequeue();
                    HairAccInfo[ACCpostion] = HairInfo;
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

                ACCpostion++;
            }
            //data.rawAccessoriesInfos[outfitnum] = NewRAW;

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
#if Debug
            //ExpandedOutfit.Logger.LogWarning("add range");
#endif

            ThisOutfitData.ReturnMaterialColor.AddRange(MaterialColor);

            ThisOutfitData.ReturnMaterialFloat.AddRange(MaterialFloat);

            ThisOutfitData.ReturnMaterialShade.AddRange(MaterialShade);

            ThisOutfitData.ReturnMaterialTexture.AddRange(MaterialTexture);

            ThisOutfitData.ReturnRenderer.AddRange(Renderer);
#if Debug
            ExpandedOutfit.Logger.LogWarning("finish");
#endif

            #endregion

        }

        public static void CoordinateLoad(ChaDefault ThisOutfitData, ChaFileCoordinate coordinate, ChaControl ChaControl, bool Raw = false)
        {
            ChaFile ChaFile = ChaControl.chaFile;
            #region Queue accessories to keep
            int outfitnum = ChaControl.fileStatus.coordinateType;

            Queue<ChaFileAccessory.PartsInfo> PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            Queue<HairSupport.HairAccessoryInfo> HairQueue = new Queue<HairSupport.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

            Queue<RendererProperty> RenderQueue = new Queue<RendererProperty>(ThisOutfitData.RendererPropertyQueue[outfitnum]);
            Queue<MaterialFloatProperty> FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.MaterialFloatPropertyQueue[outfitnum]);
            Queue<MaterialColorProperty> ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.MaterialColorPropertyQueue[outfitnum]);
            Queue<MaterialTextureProperty> TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.MaterialTexturePropertyQueue[outfitnum]);
            Queue<MaterialShader> ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.MaterialShaderQueue[outfitnum]);
#if Debug

            ExpandedOutfit.Logger.LogWarning($"Parts: {PartsQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Hair: {HairQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Render: {RenderQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Float: {FloatQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"tColor: {ColorQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Texture: {TextureQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Shader: {ShaderQueue.Count}");
#endif
            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor");
            //for (int i = 0; i < MaterialEditorData.data.Count; i++)
            //{
            //    ExpandedOutfit.Logger.LogWarning($"Key: {MaterialEditorData.data.ElementAt(i).Key} Value: {MaterialEditorData.data.ElementAt(i).Value}");
            //}
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
            var Temp = new Dictionary<int, HairSupport.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, HairSupport.HairAccessoryInfo>>((byte[])loadedHairAccessories);

            int ACCpostion = 0;
            bool Empty;
            for (int n = OriginalData.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = OriginalData[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    OriginalData[ACCpostion] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        Temp[ACCpostion] = HairQueue.Dequeue();
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
                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
#if Debug
                //ExpandedOutfit.Logger.LogWarning("Force Color Pass");
#endif

            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"Start extra accessories at {ACCpostion} {MoreACCData.Count}");
#endif
            for (int n = MoreACCData.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
            {
                Empty = MoreACCData[ACCpostion - 20].type == 120;
                if (Empty) //120 is empty/default
                {
                    MoreACCData[ACCpostion - 20] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        Temp[ACCpostion] = HairQueue.Dequeue();
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

                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"Start making extra accessories at {ACCpostion}");
#endif

            bool print = true;

            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    ExpandedOutfit.Logger.LogDebug($"Ran out of space in new coordiante adding {PartsQueue.Count}");
                    print = false;
                }
                MoreACCData.Add(PartsQueue.Dequeue());
                if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                {
                    var HairInfo = HairQueue.Dequeue();
                    if (ExpandedOutfit.HairMatch.Value)
                    {
                        HairInfo.ColorMatch = true;
                    }
                    Temp[ACCpostion] = HairInfo;
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
#if Debug
            //ExpandedOutfit.Logger.LogWarning("add range");
#endif

#if Debug
            ExpandedOutfit.Logger.LogWarning("finished coordinate load main process");
#endif

            #endregion

            //Traverse.Create(MoreAccessories._self).Method("UpdateUI").GetValue();

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


            #endregion
            ControllerReload_Loop(typeof(KK_Plugins.MaterialEditor.MaterialEditorCharaController), ChaControl);

            if (ExpandedOutfit.HairMatch.Value)
            {
                var Plugdata = new PluginData();

                Plugdata.data.Add("CoordinateHairAccessories", MessagePackSerializer.Serialize(Temp));
                ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer", Plugdata);

                ControllerReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl);
            }
        }

        private static void ME_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            ChaFile ChaFile = ChaControl.chaFile;
            List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShaderList = new List<MaterialShader>();
            Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

            #region UnPackCoordinates
            if (!ThisOutfitData.ME_Work)
            {
                for (int outfitnum = 0; outfitnum < ChaFile.coordinate.Length; outfitnum++)
                {
                    var data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
                    if (data?.data != null)
                    {
                        if (data.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                            foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                                importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);

                        if (data.data.TryGetValue("MaterialShaderList", out var materialShaders) && materialShaders != null)
                        {
                            MaterialShaderList = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])materialShaders);
                        }

                        if (data.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                        {
                            RendererPropertyList = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        }

                        if (data.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                        {
                            MaterialFloatPropertyList = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        }

                        if (data.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                        {
                            MaterialColorPropertyList = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        }

                        if (data.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                        {
                            var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                            for (var i = 0; i < properties.Count; i++)
                            {
                                var loadedProperty = properties[i];
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionaryList[(int)loadedProperty.TexID];

                                MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                MaterialTexturePropertyList.Add(newTextureProperty);
                            }
                        }
                    }
                }
            }
            else
            {
                RendererPropertyList = ThisOutfitData.ReturnRenderer;
                MaterialFloatPropertyList = ThisOutfitData.ReturnMaterialFloat;
                MaterialColorPropertyList = ThisOutfitData.ReturnMaterialColor;
                MaterialTexturePropertyList = ThisOutfitData.ReturnMaterialTexture;
                MaterialShaderList = ThisOutfitData.ReturnMaterialShade;
            }
            #endregion
            #region Pack
            var SaveData = new PluginData();

            if (ThisOutfitData.ME.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ThisOutfitData.ME.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (RendererPropertyList.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(RendererPropertyList));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (MaterialFloatPropertyList.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(MaterialFloatPropertyList));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

            if (MaterialColorPropertyList.Count > 0)
                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(MaterialColorPropertyList));
            else
                SaveData.data.Add("MaterialColorPropertyList", null);

            if (MaterialTexturePropertyList.Count > 0)
                SaveData.data.Add("MaterialTexturePropertyList", MessagePackSerializer.Serialize(MaterialTexturePropertyList));
            else
                SaveData.data.Add("MaterialTexturePropertyList", null);

            if (MaterialShaderList.Count > 0)
                SaveData.data.Add("MaterialShaderList", MessagePackSerializer.Serialize(MaterialShaderList));
            else
                SaveData.data.Add("MaterialShaderList", null);

            #endregion

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData, ChaControl, ThisOutfitData);
        }

        private static void KCOX_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            var data = new PluginData { version = 1 };
            Dictionary<string, ClothesTexData> storage;
            Dictionary<CoordinateType, Dictionary<string, ClothesTexData>> Final = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
            ChaFileCoordinate Underwear = new ChaFileCoordinate();
            Underwear.LoadFile(ThisOutfitData.Underwear);
            var UnderwearSavedData = ExtendedSave.GetExtendedDataById(Underwear, "KCOX");
            var underweardict = new Dictionary<string, ClothesTexData>();
            Dictionary<CoordinateType, Dictionary<string, ClothesTexData>> CharacterData = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
            if (UnderwearSavedData != null && UnderwearSavedData.data.TryGetValue("Overlays", out var underbytes) && underbytes is byte[] underbyteArr)
            {
                underweardict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(underbyteArr);
            }
            var ExtendedCharacterData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "KCOX");
            if (ExtendedCharacterData != null && ExtendedCharacterData.data.TryGetValue("Overlays", out var coordinatedata) && coordinatedata != null)
            {
                CharacterData = MessagePackSerializer.Deserialize<Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>>((byte[])coordinatedata);
            }
            foreach (var item in CharacterData)
            {
                ExpandedOutfit.Logger.LogWarning(item.Key);
                foreach (var item2 in item.Value)
                {
                    ExpandedOutfit.Logger.LogWarning("\t" + item2.Key);
                }
            }
            for (int outfitnum = 0; outfitnum < Constants.outfitpath; outfitnum++)
            {
                if (!CharacterData.TryGetValue((CoordinateType)outfitnum, out var CurrentCharacterData))
                {
                    CurrentCharacterData = new Dictionary<string, ClothesTexData>();
                }
                bool[] UnderClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
                bool[] CharacterClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };

                var ExpandedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile.coordinate[outfitnum], "Required_ACC");
                var PersonalData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Required_ACC");
                if (ExpandedData != null)
                {
                    if (ExpandedData.data.TryGetValue("CoordinateSaveBools", out var S_CoordinateSaveBools) && S_CoordinateSaveBools != null)
                    {
                        UnderClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_CoordinateSaveBools);
                    }
                }
                if (PersonalData != null)
                {
                    if (PersonalData.data.TryGetValue("Personal_Clothing_Save", out var S_Clothing_Save) && S_Clothing_Save != null)
                    {
                        CharacterClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_Clothing_Save);
                        for (int i = 0; i < 9; i++)
                        {
                            if (CharacterClothingKeep[i])
                            {
                                UnderClothingKeep[i] = true;
                            }
                        }
                    }
                }
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "KCOX");
                storage = new Dictionary<string, ClothesTexData>();
                if (SavedData != null && SavedData.data.TryGetValue("Overlays", out var bytes) && bytes is byte[] byteArr)
                {
                    var dict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(byteArr);
                    if (dict != null)
                    {
                        foreach (var texData in dict)
                        {
                            //int index = Array.IndexOf(Constants.KCOX_Cat, texData.Key);
                            //if (CharacterClothingKeep[index])
                            //{
                            //    continue;
                            //}
                            storage.Add(texData.Key, texData.Value);
                        }
                    }
                }
                if (ExpandedOutfit.RandomizeUnderwear.Value && outfitnum != 3 && Underwear != null)
                {
                    if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[0].id != 0)
                    {
                        if (!UnderClothingKeep[2] && underweardict.TryGetValue("ct_bra", out var bra) && bra != null && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[2].id != 0)
                        {
                            storage["ct_bra"] = bra;
                        }
                        else if (!UnderClothingKeep[2] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[2].id != 0)
                        {
                            storage.Remove("ct_bra");
                        }
                        if (!UnderClothingKeep[3] && underweardict.TryGetValue("ct_shorts", out var Panties) && Panties != null && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3].id != 0)
                        {
                            storage["ct_shorts"] = Panties;
                        }
                        else if (!UnderClothingKeep[3] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3].id != 0)
                        {
                            storage.Remove("ct_shorts");
                        }
                    }
                    //if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[1].id != 0)
                    //{
                    //    ChaControl.chaFile.coordinate[outfitnum].clothes.parts[3] = Underwear.clothes.parts[3];
                    //}
                    if (outfitnum != 2)
                    {
                        if (ChaControl.chaFile.coordinate[outfitnum].clothes.parts[5].id != 0)
                        {
                            if (!UnderClothingKeep[5] && underweardict.TryGetValue("ct_panst", out var Pantyhose) && Pantyhose != null)
                            {
                                storage["ct_panst"] = Pantyhose;
                            }
                            else if (!UnderClothingKeep[5])
                            {
                                storage.Remove("ct_panst");
                            }

                            if (!UnderClothingKeep[6] && underweardict.TryGetValue("ct_socks", out var Socks) && Socks != null)
                            {
                                storage["ct_socks"] = Socks;
                            }
                            else if (!UnderClothingKeep[6])
                            {
                                storage.Remove("ct_socks");
                            }
                        }

                        if (!UnderClothingKeep[6] && ChaControl.chaFile.coordinate[outfitnum].clothes.parts[6].id != 0)
                        {
                            if (underweardict.TryGetValue("ct_socks", out var Socks) && Socks != null)
                            {
                                storage["ct_socks"] = Socks;
                            }
                            else
                            {
                                storage.Remove("ct_socks");
                            }
                        }
                    }
                }
                for (int i = 0; i < CharacterClothingKeep.Length; i++)
                {
                    if (!CharacterClothingKeep[i])
                    {
                        continue;
                    }
                    ExpandedOutfit.Logger.LogWarning($"attempting {Constants.KCOX_Cat[i]}");
                    if (CurrentCharacterData.TryGetValue(Constants.KCOX_Cat[i], out var outfitpart))
                    {
                        ExpandedOutfit.Logger.LogWarning($"\tKept old");
                        storage[Constants.KCOX_Cat[i]] = outfitpart;
                    }
                    else
                    {
                        ExpandedOutfit.Logger.LogWarning($"\tremoved");
                        storage.Remove(Constants.KCOX_Cat[i]);
                    }
                }
                Final.Add((CoordinateType)outfitnum, storage);
            }

            data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
            SetExtendedData("KCOX", data, ChaControl, ThisOutfitData);
        }

        private static void ClothingUnlocker_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            Dictionary<int, bool> Final = new Dictionary<int, bool>();
            bool result;
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                result = false;
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.clothingunlocker");
                if (SavedData != null && SavedData.data.TryGetValue("ClothingUnlockedCoordinate", out var loadedClothingUnlocked))
                {
                    result = (bool)loadedClothingUnlocked;
                }
                Final.Add(i, result);
            }
            var data = new PluginData();
            data.data.Add("ClothingUnlocked", MessagePackSerializer.Serialize(Final));
            SetExtendedData("com.deathweasel.bepinex.clothingunlocker", data, ChaControl, ThisOutfitData);
        }

        private static void PushUp_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            Pushup.ClothData newBraData;
            Pushup.ClothData newTopData;

            PluginData SavedData;
            Dictionary<int, Pushup.ClothData> FinalBra = new Dictionary<int, Pushup.ClothData>();
            Dictionary<int, Pushup.ClothData> FinalTop = new Dictionary<int, Pushup.ClothData>();
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                newBraData = new Pushup.ClothData();
                newTopData = new Pushup.ClothData();
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.pushup");
                if (SavedData != null && SavedData.data.TryGetValue("PushupCoordinate_BraData", out var bytes) && bytes is byte[] byteArr)
                {
                    newBraData = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr);
                }
                if (SavedData != null && SavedData.data.TryGetValue("PushupCoordinate_TopData", out var bytes2) && bytes2 is byte[] byteArr2)
                {
                    newTopData = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr2);
                }
                FinalBra.Add(i, newBraData);
                FinalTop.Add(i, newTopData);
            }
            var data = new PluginData();
            data.data.Add("Pushup_BraData", MessagePackSerializer.Serialize(FinalBra));
            data.data.Add("Pushup_TopData", MessagePackSerializer.Serialize(FinalTop));
            //data.data.Add("Pushup_BodyData", null);
            SetExtendedData("com.deathweasel.bepinex.pushup", data, ChaControl, ThisOutfitData);

            //data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
            //SetExtendedData("KCOX", data, ChaControl, ThisOutfitData);
            //var KoiOverlay = typeof(KoiClothesOverlayController);
            //if (KoiOverlay != null)
            //{
            //    //ExpandedOutfit.Logger.LogWarning("Coordinate Load: Hair Acc");
            //    var temp = ChaControl.GetComponent(KoiOverlay);
            //    object[] KoiInput = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
            //    Traverse.Create(temp).Method("OnReload", KoiInput).GetValue();
            //}
        }

        private static void KKABM_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            List<BoneModifier> Modifiers = new List<BoneModifier>();
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "KKABMPlugin.ABMData");//use thisoutfit instead of chafle from the controller not sure if extended data is attached to it since textures don't render
                if (SavedData != null && SavedData.data.TryGetValue("boneData", out var bytes) && bytes is byte[] byteArr)
                {
                    Dictionary<string, BoneModifierData> import;
                    try
                    {
                        if (SavedData.version != 2)
                            throw new NotSupportedException($"{ChaControl.chaFile.coordinate[i].coordinateFileName} Save version {SavedData.version} is not supported");

                        import = LZ4MessagePackSerializer.Deserialize<Dictionary<string, BoneModifierData>>(byteArr);
                        if (import != null)
                        {
                            foreach (var modifier in import)
                            {
                                var target = new BoneModifier(modifier.Key);
                                Modifiers.Add(target);
                                target.MakeCoordinateSpecific();
                                target.CoordinateModifiers[i] = modifier.Value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExpandedOutfit.Logger.LogError("[Cosplay Academy] =>[KKABMX] Failed to load extended data - " + ex);
                    }
                }
            }
            if (Modifiers.Count == 0)
            {
                SetExtendedData("KKABMPlugin.ABMData", null, ChaControl, ThisOutfitData);
                return;
            }

            var data = new PluginData { version = 2 };
            data.data.Add("boneData", LZ4MessagePackSerializer.Serialize(Modifiers));
            SetExtendedData("KKABMPlugin.ABMData", data, ChaControl, ThisOutfitData);
            //var KoiOverlay = typeof(KoiClothesOverlayController);
            //if (KoiOverlay != null)
            //{
            //    //ExpandedOutfit.Logger.LogWarning("Coordinate Load: Hair Acc");
            //    var temp = ChaControl.GetComponent(KoiOverlay);
            //    object[] KoiInput = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
            //    Traverse.Create(temp).Method("OnReload", KoiInput).GetValue();
            //}
        }

        private static void DynamicBone_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            List<DynamicBoneData> Modifiers = new List<DynamicBoneData>();
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.dynamicboneeditor");//use thisoutfit instead of chafle from the controller not sure if extended data is attached to it since textures don't render
                if (SavedData != null && SavedData.data.TryGetValue("AccessoryDynamicBoneData", out var bytes) && bytes is byte[] byteArr)
                {
                    List<DynamicBoneData> import;

                    import = MessagePackSerializer.Deserialize<List<DynamicBoneData>>(byteArr);
                    if (import != null)
                    {
                        foreach (var dbData in import)
                        {
                            dbData.CoordinateIndex = i;
                            Modifiers.Add(dbData);
                        }
                    }
                }
            }
            if (Modifiers.Count == 0)
            {
                SetExtendedData("com.deathweasel.bepinex.dynamicboneeditor", null, ChaControl, ThisOutfitData);
                return;
            }

            var data = new PluginData();
            data.data.Add("AccessoryDynamicBoneData", MessagePackSerializer.Serialize(Modifiers));
            SetExtendedData("com.deathweasel.bepinex.dynamicboneeditor", data, ChaControl, ThisOutfitData);
        }

        public static void Reload_RePacks(ChaControl ChaControl)
        {
            ControllerReload_Loop(typeof(KoiClothesOverlayController), ChaControl);
            ControllerReload_Loop(typeof(KK_Plugins.MaterialEditor.MaterialEditorCharaController), ChaControl);
            ControllerReload_Loop(typeof(ClothingUnlockerController), ChaControl);
            ControllerReload_Loop(typeof(Pushup.PushupController), ChaControl);
            ControllerReload_Loop(typeof(BoneController), ChaControl);
            ControllerReload_Loop(typeof(KK_Plugins.DynamicBoneEditor.CharaController), ChaControl);
        }

        private static void ControllerReload_Loop(Type Controller, ChaControl ChaControl)
        {
            if (Controller != null)
            {
                var temp = ChaControl.GetComponent(Controller);
                object[] Input_Parameter = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
                Traverse.Create(temp).Method("OnReload", Input_Parameter).GetValue();
            }
        }

        private static void ME_Float_Loop(Queue<MaterialFloatProperty> FloatQueue, int ACCpostion, List<MaterialFloatProperty> MaterialFloat)
        {
            if (FloatQueue.Count != 0 && FloatQueue.Peek().ObjectType != ObjectType.Unknown)
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
            else if (FloatQueue.Count != 0)
            {
                FloatQueue.Dequeue();
            }
        }

        private static void ME_Color_Loop(Queue<MaterialColorProperty> ColorQueue, int ACCpostion, List<MaterialColorProperty> MaterialColor)
        {
            if (ColorQueue.Count != 0 && ColorQueue.Peek().ObjectType != ObjectType.Unknown)
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
            else if (ColorQueue.Count != 0)
            {
                ColorQueue.Dequeue();
            }
        }

        private static void ME_Texture_Loop(Queue<MaterialTextureProperty> TextureQueue, int ACCpostion, List<MaterialTextureProperty> MaterialTexture, ChaDefault ThisOutfitData)
        {
            if (TextureQueue.Count != 0 && TextureQueue.Peek().ObjectType != ObjectType.Unknown)
            {
                MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                if (!ThisOutfitData.ME_Work && ME_Info.TexID != null)
                {
                    if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                    {
                        ME_Info.TexID = ThisOutfitData.ME.SetAndGetTextureID(imgbyte);
                    }
                }
                ME_Info.Slot = ACCpostion;
                MaterialTexture.Add(ME_Info);
            }
            else if (TextureQueue.Count != 0)
            {
                TextureQueue.Dequeue();
            }
        }

        private static void ME_Shader_Loop(Queue<MaterialShader> ShaderQueue, int ACCpostion, List<MaterialShader> MaterialShader)
        {
            if (ShaderQueue.Count != 0 && ShaderQueue.Peek().ObjectType != ObjectType.Unknown)
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
            else if (ShaderQueue.Count != 0)
            {
                ShaderQueue.Dequeue();
            }
        }

        private static void ME_Render_Loop(Queue<RendererProperty> RendererQueue, int ACCpostion, List<RendererProperty> Renderer)
        {
            if (RendererQueue.Count != 0 && RendererQueue.Peek() != null && RendererQueue.Peek().ObjectType != ObjectType.Unknown)
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
            else if (RendererQueue.Count != 0)
            {
                RendererQueue.Dequeue();
            }
        }

        public static void SetExtendedData(string IDtoSET, PluginData data, ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            ChaFile ChaFile = ChaControl.chaFile;
            ExtendedSave.SetExtendedDataById(ChaFile, IDtoSET, data);
            ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);

            //object[] Reload = new object[1] { KoikatuAPI.GetCurrentGameMode() };

            if (ThisOutfitData.heroine != null && ChaControl.sex == 1)
            {
                ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.charFile, IDtoSET, data);
                //ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.chaCtrl.chaFile, IDtoSET, data);
            }
        }

        private void Underwear_ME(int index, int outfitnum, List<MaterialColorProperty> ME_MC_properties, List<MaterialShader> ME_MS_properties, List<RendererProperty> ME_R_properties, List<MaterialFloatProperty> ME_MF_properties, List<MaterialTextureProperty> ME_MT_properties)
        {
            ThisOutfitData.ReturnMaterialColor.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialShade.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnRenderer.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialFloat.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);
            ThisOutfitData.ReturnMaterialTexture.RemoveAll(x => x.ObjectType == ObjectType.Clothing && x.Slot == index && outfitnum == x.CoordinateIndex);


            ThisOutfitData.ReturnMaterialColor.AddRange(ME_MC_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialShade.AddRange(ME_MS_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnRenderer.AddRange(ME_R_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialFloat.AddRange(ME_MF_properties.Where(x => x.Slot == index).ToList());
            ThisOutfitData.ReturnMaterialTexture.AddRange(ME_MT_properties.Where(x => x.Slot == index).ToList());
        }
    }
}