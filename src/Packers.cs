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
#if TRACE
using System.Diagnostics;
#endif
using System.Linq;
using UnityEngine;
using CoordinateType = ChaFileDefine.CoordinateType;

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {
        public void Run_Repacks(ChaControl character)
        {
#if TRACE
            var Start = TimeWatch[2].ElapsedMilliseconds;
            TimeWatch[2].Start();
#endif
            ME_RePack(character);
            KCOX_RePack(character);
            KKABM_Repack(character);
            DynamicBone_Repack(character);
            PushUp_RePack(character);
            ClothingUnlocker_RePack(character);
            HairACC_Repack(character);

            if (Constants.PluginResults["Additional_Card_Info"])
            {
                Additional_Card_Info_Repack(character);
            }
            if (Constants.PluginResults["Accessory_States"])
            {
                Accessory_States_Repack(character);
            }
            if (Constants.PluginResults["madevil.kk.ass"])
            {
                AccessoryStateSync_Repack(character);
            }
            if (Constants.PluginResults["Accessory_Parents"] && InsideMaker)
            {
                Accessory_Parents_Repack(character);
            }
            if (Constants.PluginResults["Accessory_Themes"] && InsideMaker)
            {
                Accessory_Themes_Repack(character);
            }
#if TRACE
            TimeWatch[2].Stop();
            var temp = TimeWatch[2].ElapsedMilliseconds - Start;
            Average[2].Add(temp);
            Settings.Logger.LogWarning($"\t\tRun_Repacks: Total elapsed time {TimeWatch[2].ElapsedMilliseconds}ms\n\t\tRun {Average[2].Count}: {temp}ms\n\t\tAverage: {Average[2].Average()}ms");
#endif
        }

        public void Reload_RePacks(ChaControl ChaControl, bool ForceALL)
        {
#if TRACE
            var Start = TimeWatch[3].ElapsedMilliseconds;
            TimeWatch[3].Start();
#endif
            ControllerReload_Loop(Type.GetType("KoiClothesOverlayX.KoiClothesOverlayController, KK_OverlayMods", false), ChaControl);

            if (Constants.PluginResults["Accessory_States"])
                ControllerReload_Loop(Type.GetType("Accessory_States.CharaEvent, Accessory_States", false), ChaControl);

            if (Constants.PluginResults["Additional_Card_Info"])
                ControllerReload_Loop(Type.GetType("Additional_Card_Info.CharaEvent, Additional_Card_Info", false), ChaControl);

            if (InsideMaker && Constants.PluginResults["Accessory_Themes"])
                ControllerReload_Loop(Type.GetType("Accessory_Themes.CharaEvent, Accessory_Themes", false), ChaControl);

            if (InsideMaker && Constants.PluginResults["Accessory_Parents"])
                ControllerReload_Loop(Type.GetType("Accessory_Parents.CharaEvent, Accessory_Parents", false), ChaControl);

            if (!ForceALL)
            {
#if TRACE
                TimeWatch[3].Stop();
                var temp = TimeWatch[3].ElapsedMilliseconds - Start;
                Average[3].Add(temp);
                Settings.Logger.LogWarning($"\t\tReload_Repacks: Total elapsed time {TimeWatch[3].ElapsedMilliseconds}ms\n\t\tRun {Average[3].Count}: {temp}ms\n\t\tAverage: {Average[3].Average()}ms");
#endif
                return;
            }

            ControllerReload_Loop(Type.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.ClothingUnlockerController, KK_ClothingUnlocker", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.Pushup+PushupController, KK_Pushup", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KKABMX.Core.BoneController, KKABMX", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.DynamicBoneEditor.CharaController, KK_DynamicBoneEditor", false), ChaControl);

            ControllerReload_Loop(Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false), ChaControl);

            if (Constants.PluginResults["madevil.kk.ass"])
                ControllerReload_Loop(Type.GetType("AccStateSync.AccStateSync+AccStateSyncController, KK_AccStateSync", false), ChaControl);
#if TRACE
            TimeWatch[3].Stop();
            var temp2 = TimeWatch[3].ElapsedMilliseconds - Start;
            Average[3].Add(temp2);
            Settings.Logger.LogWarning($"\t\tReload_Repacks: Total elapsed time {TimeWatch[3].ElapsedMilliseconds}ms\n\t\tRun {Average[3].Count}: {temp2}ms\n\t\tAverage: {Average[3].Average()}ms");
#endif
        }

        private void HairACC_Repack(ChaControl ChaControl)
        {
            if (ValidOutfits.Any(x => !x))
            {
                var ChafileData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.hairaccessorycustomizer");
                if (ChafileData?.data != null && ChafileData.data.TryGetValue("HairAccessories", out var ByteData) && ByteData != null)
                {
                    var original = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, Cosplay_Academy.Hair.HairSupport.HairAccessoryInfo>>>((byte[])ByteData);
                    for (int i = 0; i < Constants.Outfit_Size; i++)
                    {
                        if (!ValidOutfits[i] || !original.ContainsKey(i))
                        {
                            continue;
                        }
                        HairAccessories[i] = original[i];
                    }
                }
            }
            var HairPlugin = new PluginData();

            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin, ChaControl);
        }

        private void ME_RePack(ChaControl ChaControl)
        {
            var ME_Save = ThisOutfitData.Finished;
            var ChafileData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");
            List<ObjectType> objectTypes = new List<ObjectType>() { ObjectType.Accessory, ObjectType.Character, ObjectType.Clothing, ObjectType.Hair };
            var ME_Chafile = new ME_List(ChafileData, ThisOutfitData, objectTypes);
            if (!ME_Chafile.NoData)
            {
                var FailedOutfits = new List<int>();
                for (int i = 0; i < Constants.Outfit_Size; i++)
                {
                    if (!ValidOutfits[i])
                    {
                        FailedOutfits.Add(i);
                    }
                }

                List<ObjectType> FailedTypes = new List<ObjectType>() { ObjectType.Accessory, ObjectType.Clothing };
                List<ObjectType> Always = new List<ObjectType>() { ObjectType.Hair, ObjectType.Character };

                ME_Save.MaterialShader.AddRange(ME_Chafile.MaterialShader.FindAll(x => objectTypes.Contains(x.ObjectType) && FailedOutfits.Contains(x.CoordinateIndex) && !(x.ObjectType == ObjectType.Clothing && ME_Dont_Touch[x.CoordinateIndex].Contains(x.Slot)) || Always.Contains(x.ObjectType)));
                ME_Save.RendererProperty.AddRange(ME_Chafile.RendererProperty.FindAll(x => objectTypes.Contains(x.ObjectType) && FailedOutfits.Contains(x.CoordinateIndex) && !(x.ObjectType == ObjectType.Clothing && ME_Dont_Touch[x.CoordinateIndex].Contains(x.Slot)) || Always.Contains(x.ObjectType)));
                ME_Save.MaterialColorProperty.AddRange(ME_Chafile.MaterialColorProperty.FindAll(x => objectTypes.Contains(x.ObjectType) && FailedOutfits.Contains(x.CoordinateIndex) && !(x.ObjectType == ObjectType.Clothing && ME_Dont_Touch[x.CoordinateIndex].Contains(x.Slot)) || Always.Contains(x.ObjectType)));
                ME_Save.MaterialFloatProperty.AddRange(ME_Chafile.MaterialFloatProperty.FindAll(x => objectTypes.Contains(x.ObjectType) && FailedOutfits.Contains(x.CoordinateIndex) && !(x.ObjectType == ObjectType.Clothing && ME_Dont_Touch[x.CoordinateIndex].Contains(x.Slot)) || Always.Contains(x.ObjectType)));
                ME_Save.MaterialTextureProperty.AddRange(ME_Chafile.MaterialTextureProperty.FindAll(x => objectTypes.Contains(x.ObjectType) && FailedOutfits.Contains(x.CoordinateIndex) && !(x.ObjectType == ObjectType.Clothing && ME_Dont_Touch[x.CoordinateIndex].Contains(x.Slot)) || Always.Contains(x.ObjectType)));
            }
            var SaveData = new PluginData();

            List<int> IDsToPurge = new List<int>();
            foreach (int texID in ThisOutfitData.ME.TextureDictionary.Keys)
                if (ME_Save.MaterialTextureProperty.All(x => x.TexID != texID))
                    IDsToPurge.Add(texID);

            for (var i = 0; i < IDsToPurge.Count; i++)
            {
                int texID = IDsToPurge[i];
                ThisOutfitData.ME.TextureDictionary.Remove(texID);
            }

            if (ThisOutfitData.ME.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ThisOutfitData.ME.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
            else
                SaveData.data.Add("TextureDictionary", null);

            if (ME_Save.RendererProperty.Count > 0)
                SaveData.data.Add("RendererPropertyList", MessagePackSerializer.Serialize(ME_Save.RendererProperty));
            else
                SaveData.data.Add("RendererPropertyList", null);

            if (ME_Save.MaterialFloatProperty.Count > 0)
                SaveData.data.Add("MaterialFloatPropertyList", MessagePackSerializer.Serialize(ME_Save.MaterialFloatProperty));
            else
                SaveData.data.Add("MaterialFloatPropertyList", null);

            if (ME_Save.MaterialColorProperty.Count > 0)
                SaveData.data.Add("MaterialColorPropertyList", MessagePackSerializer.Serialize(ME_Save.MaterialColorProperty));
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

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData, ChaControl);
        }

        private void KCOX_RePack(ChaControl ChaControl)
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

                if (!ValidOutfits[outfitnum])
                {
                    storage.Clear();
                    foreach (var item in CurrentCharacterData)
                    {
                        storage[item.Key] = item.Value;
                    }
                }

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
            SetExtendedData("KCOX", data, ChaControl);
        }

        private void ClothingUnlocker_RePack(ChaControl ChaControl)
        {
            Dictionary<int, bool> FailureBools = new Dictionary<int, bool>();
            var Original = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.clothingunlocker");
            if (Original != null)
                if (Original.data.TryGetValue("ClothingUnlocked", out var loadedClothingUnlocked) && loadedClothingUnlocked != null)
                    FailureBools = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])loadedClothingUnlocked);
            PluginData SavedData;
            Dictionary<int, bool> Final = new Dictionary<int, bool>();
            bool result;
            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                result = false;
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.clothingunlocker");
                if (SavedData != null && SavedData.data.TryGetValue("ClothingUnlockedCoordinate", out var loadedClothingUnlocked))
                {
                    if (!ValidOutfits[i])
                    {
                        FailureBools.TryGetValue(i, out var Failed);
                        result = (bool)loadedClothingUnlocked || Failed;
                    }
                    else
                    {
                        result = (bool)loadedClothingUnlocked;
                    }
                }
                Final.Add(i, result);
            }
            var data = new PluginData();
            data.data.Add("ClothingUnlocked", MessagePackSerializer.Serialize(Final));
            SetExtendedData("com.deathweasel.bepinex.clothingunlocker", data, ChaControl);
        }

        private void PushUp_RePack(ChaControl ChaControl)
        {
            Dictionary<int, Pushup.ClothData> OriginalBra = new Dictionary<int, Pushup.ClothData>();
            Dictionary<int, Pushup.ClothData> OriginalTop = new Dictionary<int, Pushup.ClothData>();
            Pushup.BodyData Body = new Pushup.BodyData();
            var Original = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.pushup");
            if (Original != null)
            {
                if (Original.data.TryGetValue("Pushup_BraData", out var bytes) && bytes != null)
                {
                    OriginalBra = MessagePackSerializer.Deserialize<Dictionary<int, Pushup.ClothData>>((byte[])bytes);
                }
                if (Original.data.TryGetValue("Pushup_TopData", out bytes) && bytes != null)
                {
                    OriginalTop = MessagePackSerializer.Deserialize<Dictionary<int, Pushup.ClothData>>((byte[])bytes);
                }
                if (Original.data.TryGetValue("Pushup_BodyData", out bytes) && bytes != null)
                {
                    Body = MessagePackSerializer.Deserialize<Pushup.BodyData>((byte[])bytes);
                }
            }
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
                if (SavedData != null)
                {
                    if (SavedData.data.TryGetValue("PushupCoordinate_BraData", out var bytes) && bytes is byte[] byteArr)
                    {
                        newBraData = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr);
                    }
                    if (SavedData.data.TryGetValue("PushupCoordinate_TopData", out var bytes2) && bytes2 is byte[] byteArr2)
                    {
                        newTopData = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr2);
                    }
                }
                if (!ValidOutfits[i])
                {
                    if (OriginalBra.ContainsKey(i))
                    {
                        newBraData = OriginalBra[i];
                    }
                    if (OriginalBra.ContainsKey(i))
                    {
                        newTopData = OriginalTop[i];
                    }
                }
                FinalBra.Add(i, newBraData);
                FinalTop.Add(i, newTopData);
            }
            var data = new PluginData();
            data.data.Add("Pushup_BraData", MessagePackSerializer.Serialize(FinalBra));
            data.data.Add("Pushup_TopData", MessagePackSerializer.Serialize(FinalTop));
            data.data.Add("Pushup_BodyData", MessagePackSerializer.Serialize(Body));
            SetExtendedData("com.deathweasel.bepinex.pushup", data, ChaControl);

            //data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
            //SetExtendedData("KCOX", data, ChaControl);
            //var KoiOverlay = typeof(KoiClothesOverlayController);
            //if (KoiOverlay != null)
            //{
            //    //ExpandedOutfit.Logger.LogWarning("Coordinate Load: Hair Acc");
            //    var temp = ChaControl.GetComponent(KoiOverlay);
            //    object[] KoiInput = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
            //    Traverse.Create(temp).Method("OnReload", KoiInput).GetValue();
            //}
        }

        private void KKABM_Repack(ChaControl ChaControl)
        {
            PluginData SavedData;
            List<BoneModifier> Modifiers = new List<BoneModifier>();
            SavedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "KKABMPlugin.ABMData");
            if (SavedData != null && SavedData.data.TryGetValue("boneData", out var bytes) && bytes != null)
            {
                try
                {
                    switch (SavedData.version)
                    {
                        case 2:
                            Modifiers = LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])bytes);
                            break;

                        case 1:
                            Settings.Logger.LogDebug($"[Cosplay Academy][KKABMX] Loading legacy embedded ABM data from card: {ChaFile.parameter?.fullname}");
                            Modifiers = KKAMBX_Migrate.MigrateOldExtData(SavedData);
                            break;

                        default:
                            throw new NotSupportedException($"Save version {SavedData.version} is not supported");
                    }
                }
                catch (Exception ex)
                {
                    Settings.Logger.LogError("[Cosplay Academy][KKABMX] Failed to load extended data - " + ex);
                }
                if (Modifiers == null)
                {
                    Modifiers = new List<BoneModifier>();
                }
                //unknown
                //for (int i = 0; i < Constants.Outfit_Size; i++)
                //{
                //    if (ValidOutfits[i])                    
                //        continue;
                //}
                //Modifiers.AddRange(Modifiers.Where(x => !x.IsCoordinateSpecific()));
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
                SetExtendedData("KKABMPlugin.ABMData", null, ChaControl);
                return;
            }

            var data = new PluginData { version = 2 };
            data.data.Add("boneData", LZ4MessagePackSerializer.Serialize(Modifiers));
            SetExtendedData("KKABMPlugin.ABMData", data, ChaControl);
        }

        private void DynamicBone_Repack(ChaControl ChaControl)
        {
            List<DynamicBoneData> Modifiers = new List<DynamicBoneData>();
            if (ValidOutfits.Any(x => !x))
            {
                List<int> Invalid = new List<int>();
                for (int i = 0; i < Constants.Outfit_Size; i++)
                    if (!ValidOutfits[i])
                    {
                        Invalid.Add(i);
                    }

                var original = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.dynamicboneeditor");
                if (original?.data != null)
                {
                    if (original.data.TryGetValue("AccessoryDynamicBoneData", out var ByteData) && ByteData != null)
                        Modifiers = MessagePackSerializer.Deserialize<List<DynamicBoneData>>((byte[])ByteData).FindAll(x => Invalid.Contains(x.CoordinateIndex));
                }
            }
            PluginData SavedData;
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
                SetExtendedData("com.deathweasel.bepinex.dynamicboneeditor", null, ChaControl);
                return;
            }

            var data = new PluginData();
            data.data.Add("AccessoryDynamicBoneData", MessagePackSerializer.Serialize(Modifiers));
            SetExtendedData("com.deathweasel.bepinex.dynamicboneeditor", data, ChaControl);
        }

        private void AccessoryStateSync_Repack(ChaControl ChaControl)
        {
            Dictionary<int, AccStateSync.OutfitTriggerInfo> OriginalCharaTriggerInfo = new Dictionary<int, AccStateSync.OutfitTriggerInfo>();
            Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>> OriginalCharaVirtualGroupInfo = new Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>();
            if (ValidOutfits.Any(x => !x))
            {
                var ExtendedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "madevil.kk.ass");
                if (ExtendedData != null && ExtendedData.data.TryGetValue("OutfitTriggerInfo", out var loadedOutfitTriggerInfo) && loadedOutfitTriggerInfo != null)
                {
                    if (ExtendedData.version < 2)
                    {
                        var OldCharaTriggerInfo = MessagePackSerializer.Deserialize<List<AccStateSync.OutfitTriggerInfoV1>>((byte[])loadedOutfitTriggerInfo);
                        for (int i = 0; i < 7; i++)
                        {
                            OriginalCharaTriggerInfo[i] = AccStateSync.UpgradeOutfitTriggerInfoV1(OldCharaTriggerInfo[i]);
                        }
                    }
                    else
                        OriginalCharaTriggerInfo = MessagePackSerializer.Deserialize<Dictionary<int, AccStateSync.OutfitTriggerInfo>>((byte[])loadedOutfitTriggerInfo);

                    if (ExtendedData.version < 5)
                    {
                        if (ExtendedData.data.TryGetValue("CharaVirtualGroupNames", out var loadedCharaVirtualGroupNames) && loadedCharaVirtualGroupNames != null)
                        {
                            if (ExtendedData.version < 2)
                            {
                                var OldCharaVirtualGroupNames = MessagePackSerializer.Deserialize<List<Dictionary<string, string>>>((byte[])loadedCharaVirtualGroupNames);
                                if (OldCharaVirtualGroupNames.Count() == 7)
                                {
                                    for (int i = 0; i < 7; i++)
                                    {
                                        Dictionary<string, string> VirtualGroupNames = AccStateSync.UpgradeVirtualGroupNamesV1(OldCharaVirtualGroupNames[i]);
                                        OriginalCharaVirtualGroupInfo[i] = AccStateSync.UpgradeVirtualGroupNamesV2(VirtualGroupNames);
                                    }
                                }
                            }
                            else
                            {
                                Dictionary<int, Dictionary<string, string>> CharaVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>((byte[])loadedCharaVirtualGroupNames);
                                for (int i = 0; i < 7; i++)
                                    OriginalCharaVirtualGroupInfo[i] = AccStateSync.UpgradeVirtualGroupNamesV2(CharaVirtualGroupNames[i]);
                            }
                        }
                    }
                    else
                    {
                        if (ExtendedData.data.TryGetValue("CharaVirtualGroupInfo", out var loadedCharaVirtualGroupInfo) && loadedCharaVirtualGroupInfo != null)
                            OriginalCharaVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, AccStateSync.VirtualGroupInfo>>>((byte[])loadedCharaVirtualGroupInfo);
                    }
                }
            }
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
                if (!ValidOutfits[outfitnum])
                {
                    if (OriginalCharaTriggerInfo.ContainsKey(outfitnum))
                    {
                        CharaTriggerInfo[outfitnum] = OriginalCharaTriggerInfo[outfitnum];
                    }
                    if (OriginalCharaVirtualGroupInfo.ContainsKey(outfitnum))
                    {
                        CharaVirtualGroupInfo[outfitnum] = OriginalCharaVirtualGroupInfo[outfitnum];
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

            SetExtendedData("madevil.kk.ass", SavedData, ChaControl);
        }

        private void Accessory_Themes_Repack(ChaControl ChaControl)
        {
            PluginData SavedData = new PluginData();

            List<string>[] ThemeNames = new List<string>[Constants.Outfit_Size];
            List<bool>[] RelativeThemeBool = new List<bool>[Constants.Outfit_Size];
            List<Color[]>[] colors = new List<Color[]>[Constants.Outfit_Size];
            Dictionary<int, int>[] ACC_Theme_Dictionary = new Dictionary<int, int>[Constants.Outfit_Size];
            Dictionary<int, List<int[]>>[] Relative_ACC_Dictionary = new Dictionary<int, List<int[]>>[Constants.Outfit_Size];
            bool[][] CoordinateSaveBools = new bool[Constants.Outfit_Size][];

            for (int i = 0; i < Constants.Outfit_Size; i++)
            {
                ThemeNames[i] = new List<string>() { "None" };
                RelativeThemeBool[i] = new List<bool>() { false };
                colors[i] = new List<Color[]> { new Color[] { new Color(), new Color(), new Color(), new Color() } };
                ACC_Theme_Dictionary[i] = new Dictionary<int, int>();
                Relative_ACC_Dictionary[i] = new Dictionary<int, List<int[]>>();
            }
            if (ValidOutfits.Any(x => !x))
            {
                var MyData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_Themes");
                if (MyData != null)
                {
                    if (MyData.data.TryGetValue("Theme_Names", out var ByteData) && ByteData != null)
                    {
                        ThemeNames = MessagePackSerializer.Deserialize<List<string>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Theme_dic", out ByteData) && ByteData != null)
                    {
                        ACC_Theme_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Color_Theme_dic", out ByteData) && ByteData != null)
                    {
                        colors = MessagePackSerializer.Deserialize<List<Color[]>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Relative_Theme_Bools", out ByteData) && ByteData != null)
                    {
                        RelativeThemeBool = MessagePackSerializer.Deserialize<List<bool>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Relative_ACC_Dictionary", out ByteData) && ByteData != null)
                    {
                        Relative_ACC_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, List<int[]>>[]>((byte[])ByteData);
                    }
                }
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
            }
            SavedData.data.Add("Theme_Names", MessagePackSerializer.Serialize(ThemeNames));
            SavedData.data.Add("Theme_dic", MessagePackSerializer.Serialize(ACC_Theme_Dictionary));
            SavedData.data.Add("Color_Theme_dic", MessagePackSerializer.Serialize(colors));
            SavedData.data.Add("Relative_Theme_Bools", MessagePackSerializer.Serialize(RelativeThemeBool));
            SavedData.data.Add("Relative_ACC_Dictionary", MessagePackSerializer.Serialize(Relative_ACC_Dictionary));

            SetExtendedData("Accessory_Themes", SavedData, ChaControl);
        }

        private void Additional_Card_Info_Repack(ChaControl ChaControl)
        {
            List<int>[] AccKeep = new List<int>[Constants.Outfit_Size];
            List<int>[] HairAcc = new List<int>[Constants.Outfit_Size];
            bool[][] CoordinateSaveBools = new bool[Constants.Outfit_Size][];
            Dictionary<int, int>[] PersonalityType_Restriction = new Dictionary<int, int>[Constants.Outfit_Size];
            Dictionary<int, int>[] TraitType_Restriction = new Dictionary<int, int>[Constants.Outfit_Size];
            int[] HstateType_Restriction = new int[Constants.Outfit_Size];
            int[] ClubType_Restriction = new int[Constants.Outfit_Size];
            bool[][] Height_Restriction = new bool[Constants.Outfit_Size][];
            bool[][] Breastsize_Restriction = new bool[Constants.Outfit_Size][];
            int[] CoordinateType = new int[Constants.Outfit_Size];
            int[] CoordinateSubType = new int[Constants.Outfit_Size];
            string[] CreatorNames = new string[Constants.Outfit_Size];
            string[] SetNames = new string[Constants.Outfit_Size];
            string[] SubSetNames = new string[Constants.Outfit_Size];
            int[] GenderType = new int[Constants.Outfit_Size];

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
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
            }

            if (ValidOutfits.Any(x => !x))
            {
                var MyData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
                if (MyData != null)
                {
                    if (MyData.data.TryGetValue("HairAcc", out var ByteData) && ByteData != null)
                    {
                        HairAcc = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("AccKeep", out ByteData) && ByteData != null)
                    {
                        AccKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])ByteData);
                    }
                }
            }

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {

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

            SetExtendedData("Additional_Card_Info", SavedData, ChaControl);
        }

        private void Accessory_Parents_Repack(ChaControl ChaControl)
        {
            Dictionary<int, List<int>>[] Bindings = new Dictionary<int, List<int>>[Constants.Outfit_Size];
            Dictionary<string, int>[] Custom_Names = new Dictionary<string, int>[Constants.Outfit_Size];
            Dictionary<int, Vector3[,]>[] Relative_Data = new Dictionary<int, Vector3[,]>[Constants.Outfit_Size];

            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                Bindings[outfitnum] = new Dictionary<int, List<int>>();
                Custom_Names[outfitnum] = new Dictionary<string, int>();
                Relative_Data[outfitnum] = new Dictionary<int, Vector3[,]>();
            }
            if (ValidOutfits.Any(x => !x))
            {
                var MyData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_Parents");
                if (MyData != null)
                {
                    if (MyData.data.TryGetValue("Parenting_Data", out var ByteData) && ByteData != null)
                    {
                        Bindings = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Parenting_Names", out ByteData) && ByteData != null)
                    {
                        Custom_Names = MessagePackSerializer.Deserialize<Dictionary<string, int>[]>((byte[])ByteData);
                    }
                    if (MyData.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
                    {
                        Relative_Data = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>[]>((byte[])ByteData);
                    }
                }
            }
            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
                var Data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_Parents");
                if (Data != null)
                {
                    if (Data.data.TryGetValue("Parenting_Data", out var ByteData) && ByteData != null)
                    {
                        Bindings[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, List<int>>>((byte[])ByteData);
                    }
                    if (Data.data.TryGetValue("Parenting_Names", out ByteData) && ByteData != null)
                    {
                        Custom_Names[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<string, int>>((byte[])ByteData);
                    }
                    if (Data.data.TryGetValue("Relative_Data", out ByteData) && ByteData != null)
                    {
                        Relative_Data[outfitnum] = MessagePackSerializer.Deserialize<Dictionary<int, Vector3[,]>>((byte[])ByteData);
                    }
                }
            }
            PluginData SavedData = new PluginData();
            SavedData.data.Add("Parenting_Data", MessagePackSerializer.Serialize(Bindings));
            SavedData.data.Add("Parenting_Names", MessagePackSerializer.Serialize(Custom_Names));
            SavedData.data.Add("Relative_Data", MessagePackSerializer.Serialize(Relative_Data));

            SetExtendedData("Accessory_Parents", SavedData, ChaControl);
        }

        private void Accessory_States_Repack(ChaControl ChaControl)
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
            }
            if (ValidOutfits.Any(x => !x))
            {
                var Extended_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_States");
                if (Extended_Data != null)
                {
                    if (Extended_Data.data.TryGetValue("ACC_Binding_Dictionary", out var ByteData) && ByteData != null)
                    {
                        ACC_Binding_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, int>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_State_array", out ByteData) && ByteData != null)
                    {
                        ACC_State_array = MessagePackSerializer.Deserialize<Dictionary<int, int[]>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_Name_Dictionary", out ByteData) && ByteData != null)
                    {
                        ACC_Name_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, string>[]>((byte[])ByteData);
                    }
                    if (Extended_Data.data.TryGetValue("ACC_Parented_Dictionary", out ByteData) && ByteData != null)
                    {
                        ACC_Parented_Dictionary = MessagePackSerializer.Deserialize<Dictionary<int, bool>[]>((byte[])ByteData);
                    }
                }
            }
            for (int outfitnum = 0; outfitnum < Constants.Outfit_Size; outfitnum++)
            {
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

            SetExtendedData("Accessory_States", SavedData, ChaControl);
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

        public void SetExtendedData(string IDtoSET, PluginData data, ChaControl ChaControl)
        {
            ExtendedSave.SetExtendedDataById(ChaControl.chaFile, IDtoSET, data);
            ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);

            if (ThisOutfitData.heroine != null)
            {
                ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.charFile, IDtoSET, data);
            }
        }
    }
}
