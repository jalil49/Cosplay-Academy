using Cosplay_Academy.ME;
using Cosplay_Academy.Support;
using ExtensibleSaveFormat;
using HarmonyLib;
using KK_Plugins;
using KK_Plugins.DynamicBoneEditor;
using KKABMX.Core;
using KKAPI;
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
        public void Run_Repacks(ChaControl character, ChaDefault ThisOutfitData)
        {
            ME_RePack(character, ThisOutfitData);
            KCOX_RePack(character, ThisOutfitData);
            KKABM_Repack(character, ThisOutfitData);
            DynamicBone_Repack(character, ThisOutfitData);
            PushUp_RePack(character, ThisOutfitData);
            ClothingUnlocker_RePack(character, ThisOutfitData);
            HairACC_Repack(character, ThisOutfitData);

            if (Constants.PluginResults["Additional_Card_Info"])
            {
                Additional_Card_Info_Repack(character, ThisOutfitData);
            }
            if (Constants.PluginResults["Accessory_States"])
            {
                Accessory_States_Repack(character, ThisOutfitData);
            }
            if (Constants.PluginResults["madevil.kk.ass"])
            {
                AccessoryStateSync_Repack(character, ThisOutfitData);
            }
            if (Constants.PluginResults["Accessory_Parents"] && InsideMaker)
            {
                Accessory_Parents_Repack(character, ThisOutfitData);
            }
            if (Constants.PluginResults["Accessory_Themes"] && InsideMaker)
            {
                Accessory_Themes_Repack(character, ThisOutfitData);
            }
        }

        public void Reload_RePacks(ChaControl ChaControl)
        {
            ControllerReload_Loop(Type.GetType("KoiClothesOverlayX.KoiClothesOverlayController, KK_OverlayMods", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.ClothingUnlockerController, KK_ClothingUnlocker", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.Pushup+PushupController, KK_Pushup", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KKABMX.Core.BoneController, KKABMX", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.DynamicBoneEditor.CharaController, KK_DynamicBoneEditor", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl);

            if (Constants.PluginResults["madevil.kk.ass"])
                ControllerReload_Loop(Type.GetType("AccStateSync.AccStateSync+AccStateSyncController, KK_AccStateSync", false), ChaControl);

            if (Constants.PluginResults["Accessory_States"])
                ControllerReload_Loop(Type.GetType("Accessory_States.CharaEvent, Accessory_States", false), ChaControl);

            if (Constants.PluginResults["Additional_Card_Info"])
                ControllerReload_Loop(Type.GetType("Additional_Card_Info.CharaEvent, Additional_Card_Info", false), ChaControl);

            if (Constants.PluginResults["Accessory_Themes"] && InsideMaker)
                ControllerReload_Loop(Type.GetType("Accessory_Themes.CharaEvent, Accessory_Themes", false), ChaControl);

            if (Constants.PluginResults["Accessory_Parents"] && InsideMaker)
                ControllerReload_Loop(Type.GetType("Accessory_Parents.CharaEvent, Accessory_Parents", false), ChaControl);
        }

        private void HairACC_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            var HairPlugin = new PluginData();
            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin, ChaControl, ThisOutfitData);
        }

        private void ME_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            var ME_Save = ThisOutfitData.Finished;
            var SaveData = new PluginData();

            List<int> IDsToPurge = new List<int>();
            foreach (int texID in ThisOutfitData.ME.TextureDictionary.Keys)
                if (ME_Save.MaterialTextureProperty.All(x => x.TexID != texID))
                    IDsToPurge.Add(texID);

            for (var i = 0; i < IDsToPurge.Count; i++)
            {
                int texID = IDsToPurge[i];
                if (ThisOutfitData.ME.TextureDictionary.TryGetValue(texID, out var val)) val.Dispose();
                ThisOutfitData.ME.TextureDictionary.Remove(texID);
            }

            if (ThisOutfitData.ME.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ThisOutfitData.ME.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (ThisOutfitData.Finished.RendererProperty.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(ThisOutfitData.Finished.RendererProperty));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (ThisOutfitData.Finished.MaterialFloatProperty.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(ThisOutfitData.Finished.MaterialFloatProperty));
            if (ME_Save.MaterialFloatProperty.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(ME_Save.MaterialFloatProperty));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(ThisOutfitData.Finished.MaterialColorProperty));
            else
                SaveData.data.Add("MaterialColorPropertyList", null);

            if (ME_Save.MaterialTextureProperty.Count > 0)
                SaveData.data.Add("MaterialTexturePropertyList", MessagePackSerializer.Serialize(ME_Save.MaterialTextureProperty));
            else
                SaveData.data.Add("MaterialTexturePropertyList", null);

            if (ME_Save.MaterialShader.Count > 0)
                SaveData.data.Add("MaterialShaderList", MessagePackSerializer.Serialize(ME_Save.MaterialShader));
            else
                SaveData.data.Add("MaterialShaderList", null);

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData, ChaControl, ThisOutfitData);
        }

        private void KCOX_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            var data = new PluginData { version = 1 };
            Dictionary<string, ClothesTexData> storage;
            Dictionary<CoordinateType, Dictionary<string, ClothesTexData>> Final = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
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

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                if (!CharacterData.TryGetValue((CoordinateType)outfitnum, out var CurrentCharacterData))
                {
                    CurrentCharacterData = new Dictionary<string, ClothesTexData>();
                }
                bool[] UnderClothingKeep = new bool[] { false, false, false, false, false, false, false, false, false };
                var ExpandedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Additional_Card_Info");
                if (ExpandedData != null)
                {
                    if (ExpandedData.data.TryGetValue("CoordinateSaveBools", out var S_CoordinateSaveBools) && S_CoordinateSaveBools != null)
                    {
                        UnderClothingKeep = MessagePackSerializer.Deserialize<bool[]>((byte[])S_CoordinateSaveBools);
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    if (CharacterClothingKeep[i])
                    {
                        UnderClothingKeep[i] = true;
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
                            storage.Add(texData.Key, texData.Value);
                        }
                    }
                }
                    storage.Clear();
                    foreach (var item in CurrentCharacterData)
                    {
                        storage[item.Key] = item.Value;
                if (Settings.RandomizeUnderwear.Value && outfitnum != 3 && Underwear != null && Underwear.GetLastErrorCode() == 0)
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
                    if (CurrentCharacterData.TryGetValue(Constants.KCOX_Cat[i], out var outfitpart))
                    {
                        storage[Constants.KCOX_Cat[i]] = outfitpart;
                    }
                    else
                    {
                        storage.Remove(Constants.KCOX_Cat[i]);
                    }
                }

                Final.Add((CoordinateType)outfitnum, storage);
            }

            data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
            SetExtendedData("KCOX", data, ChaControl, ThisOutfitData);
        }

        private void ClothingUnlocker_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            Dictionary<int, bool> Final = new Dictionary<int, bool>();
            bool result;
            for (int i = 0; i < Constants.Outfit_Size; i++)
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

        private void PushUp_RePack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            Pushup.ClothData newBraData;
            Pushup.ClothData newTopData;

            PluginData SavedData;
            Dictionary<int, Pushup.ClothData> FinalBra = new Dictionary<int, Pushup.ClothData>();
            Dictionary<int, Pushup.ClothData> FinalTop = new Dictionary<int, Pushup.ClothData>();
            for (int i = 0; i < Constants.Outfit_Size; i++)
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

        private void KKABM_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            List<BoneModifier> Modifiers = new List<BoneModifier>();
            SavedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "KKABMPlugin.ABMData");
            if (SavedData != null && SavedData.data.TryGetValue("boneData", out var bytes) && bytes != null)
            {
                List<BoneModifier> newModifiers = new List<BoneModifier>();
                try
                {
                    switch (SavedData.version)
                    {
                        case 2:
                            newModifiers = LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])bytes);
                            break;

                        case 1:
                            Settings.Logger.LogDebug($"[Cosplay Academy][KKABMX] Loading legacy embedded ABM data from card: {ChaFile.parameter?.fullname}");
                            newModifiers = KKAMBX_Migrate.MigrateOldExtData(SavedData);
                            break;

                        default:
                            throw new NotSupportedException($"Save version {SavedData.version} is not supported");
                    }
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError("[Cosplay Academy][KKABMX] Failed to load extended data - " + ex);
                }
                if (newModifiers == null)
                {
                    newModifiers = new List<BoneModifier>();
                }
                Modifiers = newModifiers.Where(x => !x.IsCoordinateSpecific()).ToList();
            }
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "KKABMPlugin.ABMData");//use thisoutfit instead of chafle from the controller not sure if extended data is attached to it since textures don't render
                if (SavedData != null && SavedData.data.TryGetValue("boneData", out bytes) && bytes != null)
                {
                    Dictionary<string, BoneModifierData> import;
                    try
                    {
                        if (SavedData.version != 2)
                            throw new NotSupportedException($"{ChaControl.chaFile.coordinate[i].coordinateFileName} Save version {SavedData.version} is not supported");

                        import = LZ4MessagePackSerializer.Deserialize<Dictionary<string, BoneModifierData>>((byte[])bytes);
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
                        Settings.Logger.LogError("[Cosplay Academy] =>[KKABMX] Failed to load extended data - " + ex);
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
        }

        private void DynamicBone_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData;
            List<DynamicBoneData> Modifiers = new List<DynamicBoneData>();
            for (int i = 0; i < Constants.Outfit_Size; i++)
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

        private void AccessoryStateSync_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData = new PluginData() { version = 5 };
            Dictionary<int, AccStateSync.OutfitTriggerInfo> CharaTriggerInfo = new Dictionary<int, AccStateSync.OutfitTriggerInfo>();
            Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>> CharaVirtualGroupInfo = new Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>();
            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                CharaTriggerInfo.Add(outfitnum, new AccStateSync.OutfitTriggerInfo(outfitnum));
                CharaVirtualGroupInfo.Add(outfitnum, new Dictionary<string, AccStateSync.VirtualGroupInfo>());

                PluginData ExtendedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "madevil.kk.ass");
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
                if (Settings.RandomizeUnderwear.Value && Settings.UnderwearStates.Value && UnderwearAccessoriesLocations[outfitnum].Count > 0)
                {
                    int postion = 0;
                    ChaFileClothes.PartsInfo[] clothes = ChaControl.chaFile.coordinate[outfitnum].clothes.parts;
                    foreach (var accessory in Underwear_PartsInfos)
                    {
                        if (accessory.id == 120 || accessory.id == 0)
                        {
                            continue;
                        }
                        //0 None
                        //1 top
                        //2 bot
                        //3 bra
                        //4 panties
                        //5 glove
                        //6 pantyhose
                        //7 socks
                        //8 shoes
                        var states = new List<bool> { true, false, false, false };
                        var location = UnderwearAccessoriesLocations[outfitnum][postion++];
                        var inclusionarray = new bool[Constants.Inclusion.Length];
                        int binder = -1;
                        bool found = false;
                        for (int i = 0; i < inclusionarray.Length; i++)
                        {
                            if (!found)
                            {
                                found = inclusionarray[i] = Constants.Inclusion[i].Contains(accessory.parentKey);
                            }
                            else
                            {
                                inclusionarray[i] = false;
                            }
                        }
                        //gloves
                        if (clothes[4].id != 0 && (inclusionarray[9] || inclusionarray[8]))
                        {
                            binder = 4;
                        }
                        //socks
                        else if (clothes[6].id != 0 && inclusionarray[7])
                        {
                            binder = 6;
                        }
                        //panties
                        else if (clothes[3].id != 0 && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                        {
                            binder = 3;
                            states = new List<bool> { false, true, true, true };
                        }
                        //Pantyhose
                        else if (clothes[5].id != 0 && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                        {
                            binder = 5;
                        }
                        //Bra
                        else if (clothes[2].id != 0 && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                        {
                            binder = 2;
                            states = new List<bool> { false, true, true, true };
                        }
                        //top
                        else if (clothes[0].id != 0 && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                        {
                            binder = 0;
                            states = new List<bool> { false, true, true, true };
                        }
                        //bottom
                        else if (clothes[1].id != 0 && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                        {
                            binder = 1;
                            states = new List<bool> { false, true, true, true };
                        }
                        else
                        {
                            binder = -1;
                        }
                        CharaTriggerInfo[outfitnum].Parts[location] = new AccStateSync.AccTriggerInfo(location) { Kind = binder, State = states };
                    }
                }
            }

            SavedData.data.Add("CharaTriggerInfo", MessagePackSerializer.Serialize(CharaTriggerInfo));
            SavedData.data.Add("CharaVirtualGroupInfo", MessagePackSerializer.Serialize(CharaVirtualGroupInfo));

            SetExtendedData("madevil.kk.ass", SavedData, ChaControl, ThisOutfitData);
        }

        private void Accessory_Themes_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData = new PluginData();

            List<string>[] ThemeNames = new List<string>[Constants.Outfit_Size];
            List<bool>[] RelativeThemeBool = new List<bool>[Constants.Outfit_Size];
            List<Color[]>[] colors = new List<Color[]>[Constants.Outfit_Size];
            Dictionary<int, int>[] ACC_Theme_Dictionary = new Dictionary<int, int>[Constants.Outfit_Size];
            Dictionary<int, bool>[] ColorRelativity = new Dictionary<int, bool>[Constants.Outfit_Size];
            Dictionary<int, List<int[]>>[] Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>[Constants.Outfit_Size];
            bool[][] CoordinateSaveBools = new bool[Constants.Outfit_Size][];
            Color[] PersonalColorSkew = new Color[Constants.Outfit_Size];
            bool[] PersonalClothingBools = new bool[9];

            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                ThemeNames[i] = new List<string>();
                RelativeThemeBool[i] = new List<bool>();
                colors[i] = new List<Color[]>();
                ACC_Theme_Dictionary[i] = new Dictionary<int, int>();
                ColorRelativity[i] = new Dictionary<int, bool>();
                Relative_ACC_Dictionary[i] = new Dictionary<int, List<int[]>>();
            }
            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                var MyData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_Themes");
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
                if (ThemeNames[outfitnum].Count == 0)
                {
                    ThemeNames[outfitnum].Add("None");
                }
            }
            SavedData.data.Add("Theme_Names", MessagePackSerializer.Serialize(ThemeNames));
            SavedData.data.Add("Theme_dic", MessagePackSerializer.Serialize(ACC_Theme_Dictionary));
            SavedData.data.Add("Color_Theme_dic", MessagePackSerializer.Serialize(colors));
            SavedData.data.Add("Color_Relativity", MessagePackSerializer.Serialize(ColorRelativity));
            SavedData.data.Add("Relative_Theme_Bools", MessagePackSerializer.Serialize(RelativeThemeBool));
            SavedData.data.Add("Relative_ACC_Dictionary", MessagePackSerializer.Serialize(Relative_ACC_Dictionary));
            var personaldata = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_Themes");
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

            SetExtendedData("Accessory_Themes", SavedData, ChaControl, ThisOutfitData);
        }

        private void Additional_Card_Info_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            int CoordinateLength = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length;
            List<int>[] AccKeep = new List<int>[CoordinateLength];
            List<int>[] HairAcc = new List<int>[CoordinateLength];
            bool[][] CoordinateSaveBools = new bool[CoordinateLength][];
            Dictionary<int, int>[] PersonalityType_Restriction = new Dictionary<int, int>[CoordinateLength];
            Dictionary<int, int>[] TraitType_Restriction = new Dictionary<int, int>[CoordinateLength];
            int[] HstateType_Restriction = new int[CoordinateLength];
            int[] ClubType_Restriction = new int[CoordinateLength];
            bool[][] Height_Restriction = new bool[CoordinateLength][];
            bool[][] Breastsize_Restriction = new bool[CoordinateLength][];
            int[] CoordinateType = new int[CoordinateLength];
            int[] CoordinateSubType = new int[CoordinateLength];
            string[] CreatorNames = new string[CoordinateLength];
            string[] SetNames = new string[CoordinateLength];
            string[] SubSetNames = new string[CoordinateLength];
            int[] GenderType = new int[CoordinateLength];

            for (int outfitnum = 0; outfitnum < CoordinateLength; outfitnum++)
            {
                AccKeep[outfitnum] = new List<int>();
                HairAcc[outfitnum] = new List<int>();
                CoordinateSaveBools[outfitnum] = new bool[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length];
                PersonalityType_Restriction[outfitnum] = new Dictionary<int, int>();
                TraitType_Restriction[outfitnum] = new Dictionary<int, int>();
                HstateType_Restriction[outfitnum] = 0;
                ClubType_Restriction[outfitnum] = 0;
                Height_Restriction[outfitnum] = new bool[3];
                Breastsize_Restriction[outfitnum] = new bool[3];
                CoordinateType[outfitnum] = 0;
                CoordinateSubType[outfitnum] = 0;
                CreatorNames[outfitnum] = "";
                SetNames[outfitnum] = "";
                SubSetNames[outfitnum] = "";
                GenderType[outfitnum] = 0;
                var MyData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Additional_Card_Info");
                if (MyData != null)
                {
                    if (MyData.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                    {
                        HairAcc[outfitnum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("CoordinateSaveBools", out ByteData) && ByteData != null)
                    {
                        CoordinateSaveBools[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                    {
                        AccKeep[outfitnum] = MessagePackSerializer.Deserialize<List<int>>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("PersonalityType_Restriction", out ByteData) && ByteData != null)
                    {
                        PersonalityType_Restriction[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("TraitType_Restriction", out ByteData) && ByteData != null)
                    {
                        TraitType_Restriction[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("HstateType_Restriction", out ByteData) && ByteData != null)
                    {
                        HstateType_Restriction[outfitnum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("ClubType_Restriction", out ByteData) && ByteData != null)
                    {
                        ClubType_Restriction[outfitnum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Height_Restriction", out ByteData) && ByteData != null)
                    {
                        Height_Restriction[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Breastsize_Restriction", out ByteData) && ByteData != null)
                    {
                        Breastsize_Restriction[outfitnum] = MessagePackSerializer.Deserialize<bool[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("CoordinateType", out ByteData) && ByteData != null)
                    {
                        CoordinateType[outfitnum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("CoordinateSubType", out ByteData) && ByteData != null)
                    {
                        CoordinateSubType[outfitnum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Creator", out ByteData) && ByteData != null)
                    {
                        CreatorNames[outfitnum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Set_Name", out ByteData) && ByteData != null)
                    {
                        SetNames[outfitnum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("SubSetNames", out ByteData) && ByteData != null)
                    {
                        SubSetNames[outfitnum] = MessagePackSerializer.Deserialize<string>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("GenderType", out ByteData) && ByteData != null)
                    {
                        GenderType[outfitnum] = MessagePackSerializer.Deserialize<int>((byte[])ByteData);
                    }
                }
                AccKeep[outfitnum].AddRange(ThisOutfitData.ACCKeepReturn[outfitnum]);
                HairAcc[outfitnum].AddRange(ThisOutfitData.HairKeepReturn[outfitnum]);
            }
            PluginData SavedData = new PluginData();

            SavedData.data.Add("CoordinateSaveBools", MessagePackSerializer.Serialize(CoordinateSaveBools));
            SavedData.data.Add("HairAcc", MessagePackSerializer.Serialize(HairAcc));
            SavedData.data.Add("AccKeep", MessagePackSerializer.Serialize(AccKeep));
            SavedData.data.Add("PersonalityType_Restriction", MessagePackSerializer.Serialize(PersonalityType_Restriction));
            SavedData.data.Add("TraitType_Restriction", MessagePackSerializer.Serialize(TraitType_Restriction));
            SavedData.data.Add("HstateType_Restriction", MessagePackSerializer.Serialize(HstateType_Restriction));
            SavedData.data.Add("ClubType_Restriction", MessagePackSerializer.Serialize(ClubType_Restriction));
            SavedData.data.Add("Height_Restriction", MessagePackSerializer.Serialize(Height_Restriction));
            SavedData.data.Add("Breastsize_Restriction", MessagePackSerializer.Serialize(Breastsize_Restriction));
            SavedData.data.Add("CoordinateType", MessagePackSerializer.Serialize(CoordinateType));
            SavedData.data.Add("CoordinateSubType", MessagePackSerializer.Serialize(CoordinateSubType));
            SavedData.data.Add("Creator", MessagePackSerializer.Serialize(CreatorNames));
            SavedData.data.Add("Set_Name", MessagePackSerializer.Serialize(SetNames));
            SavedData.data.Add("SubSetNames", MessagePackSerializer.Serialize(SubSetNames));
            SavedData.data.Add("Personal_Clothing_Save", MessagePackSerializer.Serialize(CharacterClothingKeep));
            SavedData.data.Add("Personal_Coordinate_Clothing_Save", MessagePackSerializer.Serialize(CharacterClothingKeep_Coordinate));
            SavedData.data.Add("MakeUpKeep", MessagePackSerializer.Serialize(MakeUpKeep));
            SavedData.data.Add("Cosplay_Academy_Ready", MessagePackSerializer.Serialize(Character_Cosplay_Ready));
            SavedData.data.Add("GenderType", MessagePackSerializer.Serialize(GenderType));

            SetExtendedData("Additional_Card_Info", SavedData, ChaControl, ThisOutfitData);
        }

        private void Accessory_Parents_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            int CoordinateLength = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length;

            Dictionary<int, List<int>>[] Bindings = new Dictionary<int, List<int>>[CoordinateLength];
            Dictionary<string, int>[] Custom_Names = new Dictionary<string, int>[CoordinateLength];
            Dictionary<int, Vector3[,]>[] Relative_Data = new Dictionary<int, Vector3[,]>[CoordinateLength];

            for (int outfitnum = 0; outfitnum < CoordinateLength; outfitnum++)
            {
                Bindings[outfitnum] = new Dictionary<int, List<int>>();
                Custom_Names[outfitnum] = new Dictionary<string, int>();
                Relative_Data[outfitnum] = new Dictionary<int, Vector3[,]>();
                var Data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_States");
                if (Data != null)
                {
                    if (Data.data.TryGetValue("Parenting_Data", out var ByteData) && ByteData != null)
                    {
                        Bindings = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])ByteData);
                    }
                    if (Data.data.TryGetValue("Parenting_Names", out ByteData) && ByteData != null)
                    {
                        Custom_Names = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])ByteData);
                    }
                    if (Data.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
                    {
                        Relative_Data = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])ByteData);
                    }
                }
            }
            PluginData SavedData = new PluginData();
            SavedData.data.Add("Parenting_Data", MessagePackSerializer.Serialize(Bindings));
            SavedData.data.Add("Parenting_Names", MessagePackSerializer.Serialize(Custom_Names));
            SavedData.data.Add("Relative_Data", MessagePackSerializer.Serialize(Relative_Data));

            SetExtendedData("Accessory_Parents", SavedData, ChaControl, ThisOutfitData);
        }

        private void Accessory_States_Repack(ChaControl ChaControl, ChaDefault ThisOutfitData)
        {
            PluginData SavedData = new PluginData();
            Dictionary<int, int>[] ACC_Binding_Dictionary = new Dictionary<int, int>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
            Dictionary<int, int[]>[] ACC_State_array = new Dictionary<int, int[]>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
            Dictionary<int, string>[] ACC_Name_Dictionary = new Dictionary<int, string>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];
            Dictionary<int, bool>[] ACC_Parented_Dictionary = new Dictionary<int, bool>[Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).Length];

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                ACC_Binding_Dictionary[outfitnum] = new Dictionary<int, int>();
                ACC_State_array[outfitnum] = new Dictionary<int, int[]>();
                ACC_Name_Dictionary[outfitnum] = new Dictionary<int, string>();
                ACC_Parented_Dictionary[outfitnum] = new Dictionary<int, bool>();

                var State_data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_States");
                if (State_data != null)
                {
                    if (State_data.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
                    {
                        ACC_Binding_Dictionary[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                    }
                    if (State_data.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
                    {
                        ACC_State_array[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])ByteData);
                    }
                    if (State_data.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
                    {
                        ACC_Name_Dictionary[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])ByteData);
                    }
                    if (State_data.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
                    {
                        ACC_Parented_Dictionary[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                    }
                }
                if (Settings.RandomizeUnderwear.Value && Settings.UnderwearStates.Value && UnderwearAccessoriesLocations[outfitnum].Count > 0)
                {
                    //ExpandedOutfit.Logger.LogWarning("Creating underwear data");
                    ChaFileClothes.PartsInfo[] clothes = ChaControl.chaFile.coordinate[outfitnum].clothes.parts;
                    Dictionary<int, int> Underwear_Binding_Dictionary = new Dictionary<int, int>();
                    Dictionary<int, int[]> Underwear_State_array = new Dictionary<int, int[]>();
                    Dictionary<int, string> Underwear_Name_Dictionary = new Dictionary<int, string>();
                    Dictionary<int, bool> Underwear_Parented_Dictionary = new Dictionary<int, bool>();
                    int offset = ACC_Name_Dictionary[outfitnum].Count;
                    var Underwear_state_data = ExtendedSave.GetExtendedDataById(Underwear, "Accessory_States");
                    if (Underwear_state_data != null)
                    {
                        if (Underwear_state_data.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
                        {
                            Underwear_Binding_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>>((byte[])ByteData);
                        }
                        if (Underwear_state_data.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
                        {
                            Underwear_State_array = MessagePackSerializer.Deserialize<Dictionary<int, int[]>>((byte[])ByteData);
                        }
                        if (Underwear_state_data.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
                        {
                            Underwear_Name_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])ByteData);
                        }
                        if (Underwear_state_data.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
                        {
                            Underwear_Parented_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])ByteData);
                        }
                    }
                    else
                    {
                        //foreach (var item in UnderwearAccessoriesLocations[outfitnum])
                        //{
                        //    ExpandedOutfit.Logger.LogWarning($"{(ChaFileDefine.CoordinateType)outfitnum} Location: {item}");
                        //}
                        int postion = 0;
                        foreach (var accessory in Underwear_PartsInfos)
                        {
                            if (accessory.id == 120 || accessory.id == 0)
                            {
                                continue;
                            }
                            //0 None
                            //1 top
                            //2 bot
                            //3 bra
                            //4 panties
                            //5 glove
                            //6 pantyhose
                            //7 socks
                            //8 shoes
                            int binder = 0;
                            int[] states = new int[] { 0, 3 };
                            var location = UnderwearAccessoriesLocations[outfitnum][postion++];
                            var inclusionarray = new bool[Constants.Inclusion.Length];
                            bool found = false;
                            for (int i = 0; i < inclusionarray.Length; i++)
                            {
                                if (!found)
                                {
                                    found = inclusionarray[i] = Constants.Inclusion[i].Contains(accessory.parentKey);
                                }
                                else
                                {
                                    inclusionarray[i] = false;
                                }
                            }
                            //gloves
                            if (clothes[4].id != 0 && (inclusionarray[9] || inclusionarray[8]))
                            {
                                binder = 5;
                            }
                            //socks
                            else if (clothes[6].id != 0 && inclusionarray[7])
                            {
                                binder = 7;
                            }
                            //panties
                            else if ((clothes[3].id != 0 || (clothes[2].id != 0 && Underwearbools[outfitnum][1])) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 4;
                                states = new int[] { 1, 3 };
                            }
                            //Pantyhose
                            else if (clothes[5].id != 0 && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 6;
                            }
                            //Bra
                            else if (clothes[2].id != 0 && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                            {
                                binder = 3;
                                states = new int[] { 1, 3 };
                            }
                            //top
                            else if (clothes[0].id != 0 && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                            {
                                binder = 1;
                                states = new int[] { 1, 3 };
                            }
                            //bottom
                            else if ((clothes[1].id != 0 || clothes[0].id != 0 && Underwearbools[outfitnum][2]) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 2;
                                states = new int[] { 1, 3 };
                            }
                            else
                            {
                                //ExpandedOutfit.Logger.LogWarning("always show");
                                binder = 0;
                            }
                            Underwear_Binding_Dictionary[location] = binder;
                            Underwear_State_array[location] = states;
                        }
                    }
                    foreach (var item in Underwear_Binding_Dictionary)
                    {
                        if (item.Key < 9)
                        {
                            ACC_Binding_Dictionary[outfitnum][item.Key] = item.Value;
                        }
                        else
                        {
                            ACC_Binding_Dictionary[outfitnum][item.Key + offset] = item.Value;
                        }
                    }
                    foreach (var item in Underwear_State_array)
                    {
                        ACC_State_array[outfitnum][item.Key] = item.Value;
                    }
                    foreach (var item in Underwear_Name_Dictionary)
                    {
                        ACC_Name_Dictionary[outfitnum][item.Key + offset] = item.Value;
                    }
                    foreach (var item in Underwear_Parented_Dictionary)
                    {
                        ACC_Parented_Dictionary[outfitnum][item.Key] = item.Value;
                    }
                }
            }

            SavedData.data.Add("ACC_Binding_Dictionary", MessagePackSerializer.Serialize(ACC_Binding_Dictionary));
            SavedData.data.Add("ACC_State_array", MessagePackSerializer.Serialize(ACC_State_array));
            SavedData.data.Add("ACC_Name_Dictionary", MessagePackSerializer.Serialize(ACC_Name_Dictionary));
            SavedData.data.Add("ACC_Parented_Dictionary", MessagePackSerializer.Serialize(ACC_Parented_Dictionary));

            SetExtendedData("Accessory_States", SavedData, ChaControl, ThisOutfitData);
        }

        private void ControllerReload_Loop(Type Controller, ChaControl ChaControl)
        {
            if (Controller != null)
            {
                var temp = ChaControl.GetComponent(Controller);
                object[] Input_Parameter = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
                Traverse.Create(temp).Method("OnReload", Input_Parameter).GetValue();
            }
        }

        private void ControllerCoordReload_Loop(Type Controller, ChaControl ChaControl, ChaFileCoordinate coordinate)
        {
            if (Controller != null)
            {
                var temp = ChaControl.GetComponent(Controller);
                object[] Input_Parameter = new object[2] { coordinate, false };
                Traverse.Create(temp).Method("OnCoordinateBeingLoaded", Input_Parameter).GetValue();
            }
        }
    }
}
