using ExtensibleSaveFormat;
using KK_Plugins;
using KK_Plugins.DynamicBoneEditor;
using KKABMX.Core;
using KoiClothesOverlayX;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoordinateType = ChaFileDefine.CoordinateType;

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {
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
                    var data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
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

                var ExpandedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Required_ACC");
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

        private static void AccessoryStateSync_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData = new PluginData() { version = 5 };
            Dictionary<int, AccStateSync.OutfitTriggerInfo> CharaTriggerInfo = new Dictionary<int, AccStateSync.OutfitTriggerInfo>();
            Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>> CharaVirtualGroupInfo = new Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>();
            for (int i = 0; i < 7; i++)
            {
                CharaTriggerInfo.Add(i, new AccStateSync.OutfitTriggerInfo(i));
                CharaVirtualGroupInfo.Add(i, new Dictionary<string, AccStateSync.VirtualGroupInfo>());
            }
            for (int outfitnum = 0; outfitnum < Constants.outfitpath; outfitnum++)
            {
                PluginData ExtendedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "madevil.kk.ass");
                if (ExtendedData.version > 5)
                {
                    continue;
                }
                if (ExtendedData != null && ExtendedData.data.TryGetValue("OutfitTriggerInfo", out var loadedOutfitTriggerInfo) && loadedOutfitTriggerInfo != null)
                {
                    if (ExtendedData.version < 2)
                    {
                        AccStateSync.OutfitTriggerInfoV1 OldCharaTriggerInfo = MessagePackSerializer.Deserialize<AccStateSync.OutfitTriggerInfoV1>((byte[])loadedOutfitTriggerInfo);
                        CharaTriggerInfo[outfitnum] = AccStateSync.UpgradeOutfitTriggerInfoV1(OldCharaTriggerInfo);
                    }
                    else
                        CharaTriggerInfo[outfitnum] = MessagePackSerializer.Deserialize<AccStateSync.OutfitTriggerInfo>((byte[])loadedOutfitTriggerInfo);

                    if (ExtendedData.version < 5)
                    {
                        if (ExtendedData.data.TryGetValue("OutfitVirtualGroupNames", out var loadedOutfitVirtualGroupNames) && loadedOutfitVirtualGroupNames != null)
                        {
                            Dictionary<string, string> OutfitVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])loadedOutfitVirtualGroupNames);
                            CharaVirtualGroupInfo[outfitnum] = AccStateSync.UpgradeVirtualGroupNamesV2(OutfitVirtualGroupNames);
                        }
                    }
                    else
                    {
                        if (ExtendedData.data.TryGetValue("OutfitVirtualGroupInfo", out var loadedOutfitVirtualGroupInfo) && loadedOutfitVirtualGroupInfo != null)
                            CharaVirtualGroupInfo[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<string, AccStateSync.VirtualGroupInfo>>((byte[])loadedOutfitVirtualGroupInfo);
                    }
                }
            }

            SavedData.data.Add("CharaTriggerInfo", MessagePackSerializer.Serialize(CharaTriggerInfo));
            SavedData.data.Add("CharaVirtualGroupInfo", MessagePackSerializer.Serialize(CharaVirtualGroupInfo));

            SetExtendedData("madevil.kk.ass", SavedData, ChaControl, ThisOutfitData);
        }

        private static void Personal_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData = new PluginData();

            List<string>[] ThemeNames = new List<string>[Constants.outfitpath];
            List<bool>[] RelativeThemeBool = new List<bool>[Constants.outfitpath];
            List<Color[]>[] colors = new List<Color[]>[Constants.outfitpath];
            List<int>[] AccKeep = new List<int>[Constants.outfitpath];
            List<int>[] HairAcc = new List<int>[Constants.outfitpath];
            Dictionary<int, int>[] ACC_Theme_Dictionary = new Dictionary<int, int>[Constants.outfitpath];
            Dictionary<int, bool>[] ColorRelativity = new Dictionary<int, bool>[Constants.outfitpath];
            Dictionary<int, List<int[]>>[] Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>[Constants.outfitpath];
            bool[][] CoordinateSaveBools = new bool[Constants.outfitpath][];
            Color[] PersonalColorSkew = new Color[Constants.outfitpath];
            bool[] PersonalClothingBools = new bool[9];
            for (int i = 0; i < Constants.outfitpath; i++)
            {
                ThemeNames[i] = new List<string>();
                RelativeThemeBool[i] = new List<bool>();
                colors[i] = new List<Color[]>();
                AccKeep[i] = new List<int>();
                HairAcc[i] = new List<int>();
                ACC_Theme_Dictionary[i] = new Dictionary<int, int>();
                ColorRelativity[i] = new Dictionary<int, bool>();
                Relative_ACC_Dictionary[i] = new Dictionary<int, List<int[]>>();
            }
            for (int outfitnum = 0; outfitnum < Constants.outfitpath; outfitnum++)
            {
                var MyData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Required_ACC");
                if (MyData != null)
                {
                    if (MyData.data.TryGetValue("Theme_Names", out var S_ThemeName) && S_ThemeName != null)
                    {
                        ThemeNames[outfitnum] = MessagePackSerializer.Deserialize<List<string>>((byte[])S_ThemeName);
                    }
                    if (MyData.data.TryGetValue("Theme_dic", out var S_ThemeDic) && S_ThemeDic != null)
                    {
                        ACC_Theme_Dictionary[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])S_ThemeDic);
                    }
                    if (MyData.data.TryGetValue("Color_Theme_dic", out var S_ColorThemeDic) && S_ColorThemeDic != null)
                    {
                        colors[outfitnum] = MessagePackSerializer.Deserialize<List<Color[]>>((byte[])S_ColorThemeDic);
                    }
                    if (MyData.data.TryGetValue("Color_Relativity", out var S_Relativity) && S_Relativity != null)
                    {
                        ColorRelativity[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])S_Relativity);
                    }
                    if (MyData.data.TryGetValue("Relative_Theme_Bools", out var S_Theme_Bools) && S_Theme_Bools != null)
                    {
                        RelativeThemeBool[outfitnum] = MessagePackSerializer.Deserialize<List<bool>>((byte[])S_Theme_Bools);
                    }
                    if (MyData.data.TryGetValue("CoordinateSaveBools", out var S_CoordinateSaveBools) && S_CoordinateSaveBools != null)
                    {
                        CoordinateSaveBools[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])S_CoordinateSaveBools);
                    }
                    if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out var S_Relative_ACC_Dictionary) && S_Relative_ACC_Dictionary != null)
                    {
                        Relative_ACC_Dictionary[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>>((byte[])S_Relative_ACC_Dictionary);
                    }
                }
                HairAcc[outfitnum].AddRange(ThisOutfitData.HairKeepReturn[outfitnum]);
                AccKeep[outfitnum].AddRange(ThisOutfitData.ACCKeepReturn[outfitnum]);
                if (ThemeNames[outfitnum].Count == 0)
                {
                    ThemeNames[outfitnum].Add("None");
                }
            }
            SavedData.data.Add("Theme_Names", MessagePackSerializer.Serialize(ThemeNames));
            SavedData.data.Add("Theme_dic", MessagePackSerializer.Serialize(ACC_Theme_Dictionary));
            SavedData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc));
            SavedData.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep));
            SavedData.data.Add("Color_Theme_dic", MessagePackSerializer.Serialize(colors));
            SavedData.data.Add("Color_Relativity", MessagePackSerializer.Serialize(ColorRelativity));
            SavedData.data.Add("Relative_Theme_Bools", MessagePackSerializer.Serialize(RelativeThemeBool));
            SavedData.data.Add("Relative_ACC_Dictionary", MessagePackSerializer.Serialize(Relative_ACC_Dictionary));
            var personaldata = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Required_ACC");
            if (personaldata != null)
            {
                if (personaldata.data.TryGetValue("Personal_Clothing_Save", out var S_Clothing_Save) && S_Clothing_Save != null)
                {
                    PersonalClothingBools = MessagePackSerializer.Deserialize<bool[]>((byte[])S_Clothing_Save);
                }
                if (personaldata.data.TryGetValue("Color_Skews", out var S_PersonalColorSkew) && S_PersonalColorSkew != null)
                {
                    PersonalColorSkew = MessagePackSerializer.Deserialize<Color[]>((byte[])S_PersonalColorSkew);
                }
            }
            SavedData.data.Add("Personal_Clothing_Save", MessagePackSerializer.Serialize(PersonalClothingBools));
            SavedData.data.Add("Color_Skews", MessagePackSerializer.Serialize(PersonalColorSkew));

            SetExtendedData("Required_ACC", SavedData, ChaControl, ThisOutfitData);
        }

        public static void Reload_RePacks(ChaControl ChaControl)
        {
            ControllerReload_Loop(typeof(KoiClothesOverlayController), ChaControl);
            ControllerReload_Loop(typeof(KK_Plugins.MaterialEditor.MaterialEditorCharaController), ChaControl);
            ControllerReload_Loop(typeof(ClothingUnlockerController), ChaControl);
            ControllerReload_Loop(typeof(Pushup.PushupController), ChaControl);
            ControllerReload_Loop(typeof(BoneController), ChaControl);
            ControllerReload_Loop(typeof(KK_Plugins.DynamicBoneEditor.CharaController), ChaControl);
            ControllerReload_Loop(Type.GetType("Required_Accessory_Info.Required_ACC_Controller, Required Accessory Info", false), ChaControl);
            ControllerReload_Loop(Type.GetType("KK_AccStateSync.AccStateSyncController, KK_AccStateSync", false), ChaControl);
            ControllerReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl);
        }
    }
}
