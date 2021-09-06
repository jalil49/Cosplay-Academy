using Cosplay_Academy.Support;
using ExtensibleSaveFormat;
using Extensions;
using HarmonyLib;
using KKAPI;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using CoordinateType = ChaFileDefine.CoordinateType;

#if TRACE
using System.Diagnostics;
#endif

namespace Cosplay_Academy
{
    public partial class ClothingLoader
    {

#if TRACE
        public static List<long>[] RepackAverage = new List<long>[12]
        {
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>()
        };

        public static Stopwatch[] RepacksStop = new Stopwatch[12]
        {
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch(),
            new Stopwatch()
        };
#endif        
        public void Run_Repacks(ChaControl character)
        {
#if TRACE
            var Start = TimeWatch[2].ElapsedMilliseconds;
            TimeWatch[2].Start();
#endif
            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                if (!UnderwearAccessoriesLocations.ContainsKey(i)) UnderwearAccessoriesLocations[i] = new List<int>();

                if (!ValidOutfits.ContainsKey(i)) ValidOutfits[i] = false;

                if (!Underwearbools.ContainsKey(i)) Underwearbools[i] = new bool[3];

                if (!UnderwearProcessed.ContainsKey(i)) UnderwearProcessed[i] = new bool[9];

                if (!UnderClothingKeep.ContainsKey(i)) UnderClothingKeep[i] = new bool[9];
            }


#if TRACE
            int j = 0;
            var start = RepacksStop[j].ElapsedMilliseconds;
            RepacksStop[j].Start();
#endif
            ME_RePack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();
#endif
            KCOX_RePack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif
            KKABM_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif
            DynamicBone_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            PushUp_RePack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            ClothingUnlocker_RePack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            HairACC_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif
            Additional_Card_Info_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            Accessory_States_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            Accessory_Parents_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            Accessory_Themes_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
            start = RepacksStop[++j].ElapsedMilliseconds;
            RepacksStop[j].Start();

#endif

            AccessoryStateSync_Repack(character);
#if TRACE
            RepacksStop[j].Stop();
            RepackAverage[j].Add(RepacksStop[j].ElapsedMilliseconds - start);
#endif


#if TRACE
            j = 0;
            string print = "\n";
            print += $"ME {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"KCOX {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"ABM {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"Bone {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"Push {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"unlock {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"hairacc {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"ACI {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"states {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"parents {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"themse {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j++].ElapsedMilliseconds}\n";
            print += $"ASS {j} last: {RepackAverage[j].Last()} average: {RepackAverage[j].Average()} Total:{RepacksStop[j].ElapsedMilliseconds}\n";
            Settings.Logger.LogWarning(print);
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

            ControllerReload_Loop("KoiClothesOverlayX.KoiClothesOverlayController, KK_OverlayMods", ChaControl);

            if (Constants.PluginResults["Accessory_States"])
                ControllerReload_Loop("Accessory_States.CharaEvent, Accessory_States", ChaControl);

            if (Constants.PluginResults["Additional_Card_Info"])
                ControllerReload_Loop("Additional_Card_Info.CharaEvent, Additional_Card_Info", ChaControl);

            if (InsideMaker && Constants.PluginResults["Accessory_Themes"])
                ControllerReload_Loop("Accessory_Themes.CharaEvent, Accessory_Themes", ChaControl);

            if (InsideMaker && Constants.PluginResults["Accessory_Parents"])
                ControllerReload_Loop("Accessory_Parents.CharaEvent, Accessory_Parents", ChaControl);

            ControllerReload_Loop("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", ChaControl);

            ControllerReload_Loop("KK_Plugins.ClothingUnlockerController, KK_ClothingUnlocker", ChaControl);

            ControllerReload_Loop("KK_Plugins.Pushup+PushupController, KK_Pushup", ChaControl);

            ControllerReload_Loop("KKABMX.Core.BoneController, KKABMX", ChaControl);

            ControllerReload_Loop("KK_Plugins.DynamicBoneEditor.CharaController, KK_DynamicBoneEditor", ChaControl);

            ControllerReload_Loop("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", ChaControl);

            if (Constants.PluginResults["madevil.kk.ass"])
                ControllerReload_Loop("AccStateSync.AccStateSync+AccStateSyncController, KK_AccStateSync", ChaControl);
#if TRACE
            TimeWatch[3].Stop();
            var temp2 = TimeWatch[3].ElapsedMilliseconds - Start;
            Average[3].Add(temp2);
            Settings.Logger.LogWarning($"\t\tReload_Repacks: Total elapsed time {TimeWatch[3].ElapsedMilliseconds}ms\n\t\tRun {Average[3].Count}: {temp2}ms\n\t\tAverage: {Average[3].Average()}ms");
#endif
        }

        private void HairACC_Repack(ChaControl ChaControl)
        {
            var ChafileData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.hairaccessorycustomizer");
            if (ChafileData?.data != null && ChafileData.data.TryGetValue("HairAccessories", out var ByteData) && ByteData != null)
            {
                var original = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, Hair.HairSupport.HairAccessoryInfo>>>((byte[])ByteData);
                for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
                {
                    if (!ValidOutfits[i] || !original.ContainsKey(i))
                    {
                        continue;
                    }
                    HairAccessories[i] = original[i];
                }
            }

            var HairPlugin = new PluginData();

            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin, ChaControl);
        }

        private void ME_RePack(ChaControl ChaControl)
        {
            var ME_Save = ThisOutfitData.Finished;
            var SaveData = new PluginData();
            ME_Save.AllProperties(out var rendererProperties, out var materialFloatProperties, out var materialColorProperties, out var materialShaders, out var materialTextureProperties);

            var IDsToPurge = new List<int>();
            foreach (var texID in ThisOutfitData.ME.TextureDictionary.Keys)
                if (materialTextureProperties.All(x => x.TexID != texID))
                    IDsToPurge.Add(texID);

            for (var i = 0; i < IDsToPurge.Count; i++)
            {
                var texID = IDsToPurge[i];
                ThisOutfitData.ME.TextureDictionary.Remove(texID);
            }

            if (ThisOutfitData.ME.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ThisOutfitData.ME.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
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

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData, ChaControl);
        }

        private void KCOX_RePack(ChaControl ChaControl)
        {
            PluginData SavedData;
            var Clothdict = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
            var ExtendedCharacterData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "KCOX");
            if (ExtendedCharacterData != null && ExtendedCharacterData.data.TryGetValue("Overlays", out var coordinatedata) && coordinatedata != null)
            {
                Clothdict = MessagePackSerializer.Deserialize<Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>>((byte[])coordinatedata);
            }
            var originalclothdict = Clothdict.ToNewDictionary();

            var UnderwearSavedData = ExtendedSave.GetExtendedDataById(Underwear, "KCOX");
            var underweardict = new Dictionary<string, ClothesTexData>();

            if (UnderwearSavedData != null && UnderwearSavedData.data.TryGetValue("Overlays", out var underbytes) && underbytes is byte[] underbyteArr)
            {
                underweardict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(underbyteArr);
            }

            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                var underwearproccessed = UnderwearProcessed[outfitnum];
                if (!Clothdict.ContainsKey((CoordinateType)outfitnum))
                {
                    Clothdict[(CoordinateType)outfitnum] = new Dictionary<string, ClothesTexData>();
                }

                if (ValidOutfits[outfitnum])
                {
                    Clothdict[(CoordinateType)outfitnum].Clear();
                    SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "KCOX");
                    if (SavedData != null && SavedData.data.TryGetValue("Overlays", out var bytes) && bytes is byte[] byteArr)
                    {
                        var dict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(byteArr);
                        if (dict != null)
                        {
                            Clothdict[(CoordinateType)outfitnum] = dict;
                        }
                    }
                }

                if (Settings.RandomizeUnderwear.Value && outfitnum != 3 && Underwear.GetLastErrorCode() == 0)
                {
                    for (var i = 2; i < 7; i++)
                    {
                        if (!underwearproccessed[i])
                        {
                            continue;
                        }

                        if (underweardict.TryGetValue(Constants.KCOX_Cat[i], out var clothdata) && clothdata != null)
                        {
                            Clothdict[(CoordinateType)outfitnum][Constants.KCOX_Cat[i]] = clothdata;
                            continue;
                        }

                        Clothdict[(CoordinateType)outfitnum].Remove(Constants.KCOX_Cat[i]);
                    }
                }

                for (var i = 0; i < PersonalClothingBools.Length; i++)
                {
                    if (!PersonalClothingBools[i])
                    {
                        continue;
                    }

                    if (originalclothdict[(CoordinateType)outfitnum].TryGetValue(Constants.KCOX_Cat[i], out var outfitpart) && outfitpart != null)
                    {
                        Clothdict[(CoordinateType)outfitnum][Constants.KCOX_Cat[i]] = outfitpart;
                        continue;
                    }

                    Clothdict[(CoordinateType)outfitnum].Remove(Constants.KCOX_Cat[i]);
                }

            }
            var data = new PluginData { version = 1 };
            data.data.Add("Overlays", MessagePackSerializer.Serialize(Clothdict));
            SetExtendedData("KCOX", data, ChaControl);
        }

        private void ClothingUnlocker_RePack(ChaControl ChaControl)
        {
            var FailureBools = new Dictionary<int, bool>();
            var Original = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.clothingunlocker");
            if (Original != null)
                if (Original.data.TryGetValue("ClothingUnlocked", out var loadedClothingUnlocked) && loadedClothingUnlocked != null)
                    FailureBools = MessagePackSerializer.Deserialize<Dictionary<int, bool>>((byte[])loadedClothingUnlocked);
            PluginData SavedData;
            var Final = new Dictionary<int, bool>();
            bool result;
            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
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
            var Bra = new Dictionary<int, Pushup.ClothData>();
            var Top = new Dictionary<int, Pushup.ClothData>();
            Pushup.BodyData Body = null;
            var Extended = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.pushup");
            if (Extended != null)
            {
                if (Extended.data.TryGetValue("Pushup_BraData", out var bytes) && bytes != null)
                {
                    Bra = MessagePackSerializer.Deserialize<Dictionary<int, Pushup.ClothData>>((byte[])bytes);
                }
                if (Extended.data.TryGetValue("Pushup_TopData", out bytes) && bytes != null)
                {
                    Top = MessagePackSerializer.Deserialize<Dictionary<int, Pushup.ClothData>>((byte[])bytes);
                }
                if (Extended.data.TryGetValue("Pushup_BodyData", out bytes) && bytes != null)
                {
                    Body = MessagePackSerializer.Deserialize<Pushup.BodyData>((byte[])bytes);
                }
            }

            var originalbra = Bra.ToNewDictionary();
            var originaltop = Top.ToNewDictionary();

            Pushup.ClothData UnderBra = null;
            Extended = ExtendedSave.GetExtendedDataById(Underwear, "com.deathweasel.bepinex.pushup");
            if (Extended != null)
            {
                if (Extended.data.TryGetValue("PushupCoordinate_BraData", out var bytes))
                {
                    UnderBra = MessagePackSerializer.Deserialize<Pushup.ClothData>((byte[])bytes);
                }
            }

            PluginData SavedData;
            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                if (!ValidOutfits[outfitnum])
                {
                    continue;
                }
                var UnderwearProcessed = this.UnderwearProcessed[outfitnum];
                var UnderClothingKeep = this.UnderClothingKeep[outfitnum];
                Bra.Remove(outfitnum);
                Top.Remove(outfitnum);

                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.pushup");
                if (SavedData != null)
                {
                    if (SavedData.data.TryGetValue("PushupCoordinate_BraData", out var bytes) && bytes is byte[] byteArr)
                    {
                        Bra[outfitnum] = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr);
                    }
                    if (SavedData.data.TryGetValue("PushupCoordinate_TopData", out var bytes2) && bytes2 is byte[] byteArr2)
                    {
                        Top[outfitnum] = MessagePackSerializer.Deserialize<Pushup.ClothData>(byteArr2);
                    }
                }

                if (UnderClothingKeep[0])
                {
                    Top[outfitnum] = originaltop[outfitnum];
                }
                if (UnderClothingKeep[2])
                {
                    Bra[outfitnum] = originalbra[outfitnum];
                }

                if (UnderwearProcessed[2])
                {
                    Bra.Remove(outfitnum);
                    if (UnderBra != null) Bra[outfitnum] = UnderBra;
                }
            }
            var data = new PluginData();
            data.data.Add("Pushup_BraData", MessagePackSerializer.Serialize(Bra));
            data.data.Add("Pushup_TopData", MessagePackSerializer.Serialize(Top));
            if (Body != null)
            {
                data.data.Add("Pushup_BodyData", MessagePackSerializer.Serialize(Body));
            }
            SetExtendedData("com.deathweasel.bepinex.pushup", data, ChaControl);
        }

        private void KKABM_Repack(ChaControl ChaControl)
        {
            PluginData SavedData;
            var Modifiers = new List<ABMX.BoneModifier>();
            SavedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "KKABMPlugin.ABMData");
            if (SavedData != null && SavedData.data.TryGetValue("boneData", out var bytes) && bytes != null)
            {
                try
                {
                    switch (SavedData.version)
                    {
                        case 2:
                            Modifiers = LZ4MessagePackSerializer.Deserialize<List<ABMX.BoneModifier>>((byte[])bytes);
                            break;

                        case 1:
                            Settings.Logger.LogDebug($"[Cosplay Academy][KKABMX] Loading legacy embedded ABM data from card: {ChaFile.parameter?.fullname}");
                            Modifiers = ABMX.MigrateOldExtData(SavedData);
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
                    Modifiers = new List<ABMX.BoneModifier>();
                }
                //unknown
                //for (int i = 0; i < ThisOutfitData.Outfit_Size; i++)
                //{
                //    if (ValidOutfits[i])                    
                //        continue;
                //}
                //Modifiers.AddRange(Modifiers.Where(x => !x.IsCoordinateSpecific()));
            }
            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "KKABMPlugin.ABMData");
                if (SavedData != null && SavedData.data.TryGetValue("boneData", out bytes) && bytes != null)
                {
                    Dictionary<string, ABMX.BoneModifierData> import;
                    try
                    {
                        if (SavedData.version != 2)
                            throw new NotSupportedException($"{ChaControl.chaFile.coordinate[i].coordinateFileName} Save version {SavedData.version} is not supported");

                        import = LZ4MessagePackSerializer.Deserialize<Dictionary<string, ABMX.BoneModifierData>>((byte[])bytes);
                        if (import != null)
                        {
                            foreach (var modifier in import)
                            {
                                var target = new ABMX.BoneModifier(modifier.Key);
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
            var Modifiers = new List<DynamicBonePlugin
                .DynamicBoneData>();

            var original = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.dynamicboneeditor");
            if (original?.data != null)
            {
                if (original.data.TryGetValue("AccessoryDynamicBoneData", out var ByteData) && ByteData != null)
                    Modifiers = MessagePackSerializer.Deserialize<List<DynamicBonePlugin.DynamicBoneData>>((byte[])ByteData);
            }

            PluginData SavedData;
            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                if (ValidOutfits[i])
                {
                    Modifiers.RemoveAll(x => x.CoordinateIndex == i);
                }
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.dynamicboneeditor");
                if (SavedData != null && SavedData.data.TryGetValue("AccessoryDynamicBoneData", out var bytes) && bytes is byte[] byteArr)
                {
                    List<DynamicBonePlugin.DynamicBoneData> import;

                    import = MessagePackSerializer.Deserialize<List<DynamicBonePlugin.DynamicBoneData>>(byteArr);
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
            var TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
            var TriggerGroupList = new List<AccStateSync.TriggerGroup>();

            var ExtendedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "madevil.kk.ass");
            if (ExtendedData != null)
            {
                if (ExtendedData.version > 6)
                    Settings.Logger.LogWarning($"New version of AccessoryStateSync found, accessory states needs update for compatibility");
                else if (ExtendedData.version < 6)
                {
                    AccStateSync.Migration.ConvertCharaPluginData(ExtendedData, ref TriggerPropertyList, ref TriggerGroupList);
                }
                else
                {
                    if (ExtendedData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                            TriggerPropertyList.AddRange(_tempTriggerProperty);

                        if (ExtendedData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }

                if (TriggerPropertyList == null) TriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                if (TriggerGroupList == null) TriggerGroupList = new List<AccStateSync.TriggerGroup>();
            }

            var UnderwearTrigger = new List<AccStateSync.TriggerProperty>();
            var UnderwearGroups = new List<AccStateSync.TriggerGroup>();

            ExtendedData = ExtendedSave.GetExtendedDataById(Underwear, "madevil.kk.ass");
            if (ExtendedData != null)
            {
                if (ExtendedData.version == 6)
                {
                    if (ExtendedData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                    {
                        var _tempTriggerProperty = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                        if (_tempTriggerProperty?.Count > 0)
                        {
                            _tempTriggerProperty.ForEach(x => x.Coordinate = -1);
                            TriggerPropertyList.AddRange(_tempTriggerProperty);
                        }

                        if (ExtendedData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                        {
                            var _tempTriggerGroup = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                            if (_tempTriggerGroup?.Count > 0)
                            {
                                foreach (var _group in _tempTriggerGroup)
                                {
                                    _group.Coordinate = -1;

                                    if (_group.GUID.IsNullOrEmpty())
                                        _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                }
                                TriggerGroupList.AddRange(_tempTriggerGroup);
                            }
                        }
                    }
                }
                else if (ExtendedData.version < 6)
                {
                    AccStateSync.Migration.ConvertOutfitPluginData(-1, ExtendedData, ref UnderwearTrigger, ref UnderwearGroups);
                }
                else
                {
                    Settings.Logger.LogWarning("Update Cosplay Academy or if no update is available let dev know that ASS support is outdated");
                }

                if (UnderwearTrigger == null) UnderwearTrigger = new List<AccStateSync.TriggerProperty>();
                if (UnderwearGroups == null) UnderwearGroups = new List<AccStateSync.TriggerGroup>();
            }

            for (int outfitnum = 0, nn = ThisOutfitData.Outfit_Size; outfitnum < nn; outfitnum++)
            {
                var tempTriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                var tempTriggerGroupList = new List<AccStateSync.TriggerGroup>();

                if (ValidOutfits[outfitnum])
                {
                    TriggerPropertyList.RemoveAll(x => x.Coordinate == outfitnum);
                    TriggerGroupList.RemoveAll(x => x.Coordinate == outfitnum);

                    ExtendedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "madevil.kk.ass");

                    if (ExtendedData != null)
                    {
                        if (ExtendedData.version == 6)
                        {
                            if (ExtendedData.data.TryGetValue("TriggerPropertyList", out var _loadedTriggerProperty) && _loadedTriggerProperty != null)
                            {
                                tempTriggerPropertyList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerProperty>>((byte[])_loadedTriggerProperty);
                                if (tempTriggerPropertyList?.Count > 0)
                                {
                                    tempTriggerPropertyList.ForEach(x => x.Coordinate = outfitnum);
                                    TriggerPropertyList.AddRange(tempTriggerPropertyList);
                                }

                                if (ExtendedData.data.TryGetValue("TriggerGroupList", out var _loadedTriggerGroup) && _loadedTriggerGroup != null)
                                {
                                    tempTriggerGroupList = MessagePackSerializer.Deserialize<List<AccStateSync.TriggerGroup>>((byte[])_loadedTriggerGroup);
                                    if (tempTriggerGroupList?.Count > 0)
                                    {
                                        foreach (var _group in tempTriggerGroupList)
                                        {
                                            _group.Coordinate = outfitnum;

                                            if (_group.GUID.IsNullOrEmpty())
                                                _group.GUID = Guid.NewGuid().ToString("D").ToUpper();
                                        }
                                        TriggerGroupList.AddRange(tempTriggerGroupList);
                                    }
                                }
                            }
                        }
                        else if (ExtendedData.version < 6)
                        {
                            AccStateSync.Migration.ConvertOutfitPluginData(outfitnum, ExtendedData, ref tempTriggerPropertyList, ref tempTriggerGroupList);
                        }
                        else
                        {
                            Settings.Logger.LogWarning("Update Cosplay Academy or if no update is available let dev know that ASS support is outdated");
                        }
                        if (tempTriggerPropertyList == null) tempTriggerPropertyList = new List<AccStateSync.TriggerProperty>();
                        if (tempTriggerGroupList == null) tempTriggerGroupList = new List<AccStateSync.TriggerGroup>();
                        TriggerGroupList.AddRange(tempTriggerGroupList);
                        TriggerPropertyList.AddRange(tempTriggerPropertyList);
                    }
                }

                if (Settings.RandomizeUnderwear.Value && Settings.UnderwearStates.Value && UnderwearAccessoriesLocations[outfitnum].Count > 0)
                {
                    var postion = 0;
                    var partnum = -1;
                    var Max = 9;
                    var Custom_offset = 0;
                    var clothes = ChaControl.chaFile.coordinate[outfitnum].clothes.parts;
                    var SubUnderwearGroups = new List<AccStateSync.TriggerGroup>(UnderwearGroups);
                    SubUnderwearGroups.ForEach(x => x.Coordinate = outfitnum);
                    var SubUnderwearTrigger = new List<AccStateSync.TriggerProperty>(UnderwearTrigger);
                    var convertdict = new Dictionary<int, int>();
                    foreach (var item in TriggerGroupList.Where(x => x.Coordinate == outfitnum))
                    {
                        Max = Math.Max(Max, item.Kind);
                    }
                    Max++;
                    Custom_offset = (Max > 9) ? Max - 9 : 0;

                    for (int i = 0, n = SubUnderwearGroups.Count; i < n; i++)
                    {
                        var oldkind = SubUnderwearGroups[i].Kind;
                        var newkind = oldkind + Custom_offset;
                        convertdict[oldkind] = newkind;
                        SubUnderwearGroups[i].Kind = newkind;
                    }
                    SubUnderwearTrigger.ForEach(x =>
                    {
                        x.Coordinate = outfitnum;
                        if (x.RefKind > 8)
                        {
                            x.RefKind = convertdict[x.RefKind];
                        }
                    });
                    var Underwearbools = this.Underwearbools[outfitnum];
                    foreach (var accessory in Underwear_PartsInfos)
                    {
                        partnum++;
                        if (accessory.type < 121)
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
                        var binder = -1;
                        var found = false;

                        if (UnderwearTrigger.Any(x => x.Slot == partnum))
                        {
                            var originaltrigger = UnderwearTrigger.First(x => x.Slot == partnum);
                            var kind = originaltrigger.RefKind;
                            if (kind < 9)
                            {
                                if (kind == 2 && clothes[1].id != 0 || kind == 2 && clothes[0].id != 0 && Underwearbools[0] ||
                                    kind == 3 && clothes[2].id != 0 || kind == 3 && clothes[0].id != 0 && Underwearbools[1] ||
                                    kind == 4 && clothes[3].id != 0 || kind == 4 && clothes[1].id != 0 && Underwearbools[2])
                                {
                                    originaltrigger.Slot = location;
                                    continue;
                                }
                                else if (kind != 2 && kind != 3 && kind != 4 && kind < 8)
                                {
                                    originaltrigger.Slot = location;
                                    continue;
                                }
                                else if (kind == 8)
                                {
                                    originaltrigger.Slot = location;
                                    continue;
                                }
                            }
                            continue;
                        }
                        for (var i = 0; i < inclusionarray.Length; i++)
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
                        else if ((clothes[3].id != 0 || clothes[2].id != 0 && Underwearbools[2]) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
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
                        else if ((clothes[2].id != 0 || clothes[0].id != 0 && Underwearbools[1]) && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
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
                        else if ((clothes[1].id != 0 || clothes[0].id != 0 && Underwearbools[0]) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                        {
                            binder = 1;
                            states = new List<bool> { false, true, true, true };
                        }
                        else
                        {
                            binder = -1;
                        }
                        if (binder == -1)
                        {
                            continue;
                        }

                        for (var i = 0; i < states.Count; i++)
                        {
                            SubUnderwearTrigger.Add(new AccStateSync.TriggerProperty(outfitnum, location, binder, i, states[i], 0));
                        }
                    }
                    TriggerPropertyList.AddRange(SubUnderwearTrigger);
                    TriggerGroupList.AddRange(SubUnderwearGroups);
                }
            }
            var SavedData = new PluginData() { version = 6 };

            SavedData.data.Add("TriggerPropertyList", MessagePackSerializer.Serialize(TriggerPropertyList));
            SavedData.data.Add("TriggerGroupList", MessagePackSerializer.Serialize(TriggerGroupList));

            SetExtendedData("madevil.kk.ass", (TriggerPropertyList.Count == 0) ? null : SavedData, ChaControl);
        }

        private void Accessory_Themes_Repack(ChaControl ChaControl)
        {
            var SavedData = new PluginData() { version = 1 };

            var data = new Accessory_Themes.DataStruct();

            var coordinate = data.Coordinate;

            for (int outfitnum = 0, n = ThisOutfitData.Outfit_Size; outfitnum < n; outfitnum++)
            {
                if (!coordinate.ContainsKey(outfitnum))
                {
                    data.Createoutfit(outfitnum);
                }
            }

            var plugindata = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_Themes");
            if (plugindata != null)
            {
                if (plugindata.version == 1)
                {
                    if (plugindata.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                    {
                        data.Coordinate = MessagePackSerializer.Deserialize<Dictionary<int, Accessory_Themes.CoordinateData>>((byte[])ByteData);
                    }
                }
                else if (plugindata.version == 0)
                {
                    Accessory_Themes.Migrator.MigrateV0(plugindata, ref data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
            }

            coordinate = data.Coordinate;

            for (int outfitnum = 0, n = ThisOutfitData.Outfit_Size; outfitnum < n; outfitnum++)
            {
                if (!coordinate.ContainsKey(outfitnum))
                {
                    data.Createoutfit(outfitnum);
                }
                if (!ValidOutfits[outfitnum])
                {
                    continue;
                }
                data.Clearoutfit(outfitnum);
                plugindata = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_Themes");
                if (plugindata != null)
                {
                    switch (plugindata.version)
                    {
                        case 0:
                            data.Coordinate[outfitnum] = Accessory_Themes.Migrator.CoordinateMigrateV0(plugindata);
                            break;
                        case 1:
                            if (plugindata.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                            {
                                data.Coordinate[outfitnum] = MessagePackSerializer.Deserialize<Accessory_Themes.CoordinateData>((byte[])ByteData);
                            }
                            break;
                        default:
                            Settings.Logger.LogWarning("New version detected please update");
                            break;
                    }
                }
            }

            foreach (var item in coordinate)
            {
                item.Value.CleanUp();
            }

            var nulldata = coordinate.All(x => x.Value.Themes.Count == 0);

            SavedData.data.Add("CoordinateData", MessagePackSerializer.Serialize(data.Coordinate));

            SetExtendedData("Accessory_Themes", (nulldata) ? null : SavedData, ChaControl);
        }

        private void Additional_Card_Info_Repack(ChaControl ChaControl)
        {
            var data = new Additional_Card_Info.DataStruct();

            var CardInfo = data.CardInfo;
            var CoordinateInfo = data.CoordinateInfo;

            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                if (!CoordinateInfo.ContainsKey(i))
                    data.Createoutfit(i);
            }

            var Cha_ACI_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
            if (Cha_ACI_Data != null)
            {
                if (Cha_ACI_Data.version == 1)
                {
                    if (Cha_ACI_Data.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                    {
                        CardInfo = MessagePackSerializer.Deserialize<Additional_Card_Info.Cardinfo>((byte[])ByteData);
                    }
                    if (Cha_ACI_Data.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                    {
                        data.CoordinateInfo = MessagePackSerializer.Deserialize<Dictionary<int, Additional_Card_Info.CoordinateInfo>>((byte[])ByteData);
                    }
                }
                else if (Cha_ACI_Data.version == 0)
                {
                    Additional_Card_Info.Migrator.MigrateV0(Cha_ACI_Data, ref data);
                }
                else
                {
                    Settings.Logger.LogWarning("New plugin version found on card please update");
                }
            }

            CoordinateInfo = data.CoordinateInfo;

            for (int outfitnum = 0, n = ThisOutfitData.Outfit_Size; outfitnum < n; outfitnum++)
            {
                if (!CoordinateInfo.ContainsKey(outfitnum))
                    data.Createoutfit(outfitnum);

                if (!ValidOutfits[outfitnum])
                {
                    continue;
                }

                data.Clearoutfit(outfitnum);

                var ACI_Data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Additional_Card_Info");
                if (ACI_Data != null)
                {
                    if (ACI_Data.version == 1)
                    {
                        if (ACI_Data.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                        {
                            CoordinateInfo[outfitnum] = MessagePackSerializer.Deserialize<Additional_Card_Info.CoordinateInfo>((byte[])ByteData);
                        }
                    }
                    else if (ACI_Data.version == 0)
                    {
                        CoordinateInfo[outfitnum] = Additional_Card_Info.Migrator.CoordinateMigrateV0(ACI_Data);
                    }
                    else
                    {
                        Settings.Logger.LogWarning("New plugin version found on card please update");
                    }
                }

                var coordinforef = CoordinateInfo[outfitnum];

                coordinforef.AccKeep.AddRange(ThisOutfitData.ACCKeepReturn[outfitnum]);
                coordinforef.HairAcc.AddRange(ThisOutfitData.HairKeepReturn[outfitnum]);
            }

            var SavedData = new PluginData() { version = 1 };

            SavedData.data.Add("CardInfo", MessagePackSerializer.Serialize(CardInfo));
            SavedData.data.Add("CoordinateInfo", MessagePackSerializer.Serialize(CoordinateInfo));

            SetExtendedData("Additional_Card_Info", SavedData, ChaControl);
        }

        private void Accessory_Parents_Repack(ChaControl ChaControl)
        {
            var Parent_Data = new Dictionary<int, Accessory_Parents.CoordinateData>();

            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                if (!Parent_Data.ContainsKey(outfitnum))
                    Parent_Data[outfitnum] = new Accessory_Parents.CoordinateData();
            }

            var MyData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_Parents");
            if (MyData != null)
            {
                if (MyData.version == 1)
                {
                    if (MyData.data.TryGetValue("Coordinate_Data", out var ByteData) && ByteData != null)
                    {
                        Parent_Data = MessagePackSerializer.Deserialize<Dictionary<int, Accessory_Parents.CoordinateData>>((byte[])ByteData);
                    }
                }
                else if (MyData.version == 0)
                {
                    Accessory_Parents.Migrator.MigrateV0(MyData, ref Parent_Data);
                }
                else
                {
                    Settings.Logger.LogWarning("New version of plugin detected please update");
                }
            }

            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                if (!Parent_Data.ContainsKey(outfitnum))
                    Parent_Data[outfitnum] = new Accessory_Parents.CoordinateData();

                if (!ValidOutfits[outfitnum])
                {
                    continue;
                }

                Parent_Data[outfitnum].Clear();
                var plugindata = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_Parents");
                if (plugindata != null)
                {
                    if (plugindata.version == 1)
                    {
                        if (plugindata.data.TryGetValue("Coordinate_Data", out var DataBytes) && DataBytes != null)
                        {
                            Parent_Data[outfitnum] = MessagePackSerializer.Deserialize<Accessory_Parents.CoordinateData>((byte[])DataBytes);
                        }
                    }
                    else if (plugindata.version == 0)
                    {
                        Parent_Data[outfitnum] = Accessory_Parents.Migrator.CoordinateMigrateV0(plugindata);
                    }
                    else
                    {
                        Settings.Logger.LogWarning("New version of plugin detected please update");
                    }
                }
            }

            var SavedData = new PluginData() { version = 1 };

            foreach (var item in Parent_Data)
            {
                item.Value.CleanUp();
            }
            var nulldata = Parent_Data.All(x => x.Value.Parent_Groups.Count == 0);

            SavedData.data.Add("Coordinate_Data", MessagePackSerializer.Serialize(Parent_Data));

            SetExtendedData("Accessory_Parents", (nulldata) ? null : SavedData, ChaControl);
        }

        private void Accessory_States_Repack(ChaControl ChaControl)
        {
            var SavedData = new PluginData() { version = 1 };

            var data = new Accessory_States.Data();
            var Coordinate = data.Coordinate;

            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                if (!Coordinate.ContainsKey(outfitnum))
                {
                    Coordinate[outfitnum] = new Accessory_States.CoordinateData();
                }
            }

            var Extended_Data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Accessory_States");
            if (Extended_Data != null)
            {
                switch (Extended_Data.version)
                {
                    case 0:
                        Accessory_States.Migrator.MigrateV0(Extended_Data, ref data);
                        break;
                    case 1:
                        if (Extended_Data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                        {
                            data.Coordinate = MessagePackSerializer.Deserialize<Dictionary<int, Accessory_States.CoordinateData>>((byte[])ByteData);
                        }
                        break;
                    default:
                        break;
                }
            }
            Coordinate = data.Coordinate;
            var underwear = new Accessory_States.CoordinateData();

            var State_data = ExtendedSave.GetExtendedDataById(Underwear, "Accessory_States");
            if (State_data != null)
            {
                switch (State_data.version)
                {
                    case 0:
                        underwear = Accessory_States.Migrator.CoordinateMigrateV0(State_data);
                        break;
                    case 1:
                        if (State_data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                        {
                            underwear = MessagePackSerializer.Deserialize<Accessory_States.CoordinateData>((byte[])ByteData);
                        }
                        break;
                    default:
                        break;
                }
            }

            for (var outfitnum = 0; outfitnum < ThisOutfitData.Outfit_Size; outfitnum++)
            {
                if (!Coordinate.ContainsKey(outfitnum))
                {
                    Coordinate[outfitnum] = new Accessory_States.CoordinateData();
                }

                if (ValidOutfits[outfitnum])
                {
                    data.Clearoutfit(outfitnum);

                    State_data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "Accessory_States");

                    if (State_data != null)
                    {
                        switch (State_data.version)
                        {
                            case 0:
                                data.Coordinate[outfitnum] = Accessory_States.Migrator.CoordinateMigrateV0(State_data);
                                break;
                            case 1:
                                if (State_data.data.TryGetValue("CoordinateData", out var ByteData) && ByteData != null)
                                {
                                    data.Coordinate[outfitnum] = MessagePackSerializer.Deserialize<Accessory_States.CoordinateData>((byte[])ByteData);
                                }
                                break;
                            default:
                                Settings.Logger.LogWarning("New version detected please update");
                                break;
                        }
                    }
                }

                if (Settings.RandomizeUnderwear.Value && Settings.UnderwearStates.Value && UnderwearAccessoriesLocations[outfitnum].Count > 0)
                {
                    var clothes = ChaControl.chaFile.coordinate[outfitnum].clothes.parts;
                    var postion = 0;
                    var local = -1;
                    var new_key = 10;
                    var InclusionLength = Constants.Inclusion.Length;
                    var coordinateinfo = data.Coordinate[outfitnum];
                    var slotinfodict = coordinateinfo.Slotinfo;
                    var namedict2 = coordinateinfo.Names;

                    var conversiondict = new Dictionary<int, int>();

                    foreach (var item in underwear.Names)
                    {
                        while (namedict2.ContainsKey(new_key))
                        {
                            new_key++;
                        }
                        conversiondict[item.Key] = new_key;
                        namedict2[new_key] = item.Value;
                    }
                    var Underwearbools = this.Underwearbools[outfitnum];
                    foreach (var accessory in Underwear_PartsInfos)
                    {
                        local++;
                        if (accessory.type < 121)
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
                        var binder = -1;
                        var states = new int[] { 0, 3 };
                        var ClothFound = false;
                        var location = UnderwearAccessoriesLocations[outfitnum][postion++];
                        var inclusionarray = new bool[InclusionLength];
                        var found = false;

                        if (underwear.Slotinfo.TryGetValue(local, out var localslotinfo))
                        {
                            var kind = localslotinfo.Binding;
                            if (kind < 0)
                            {
                                if (localslotinfo.Parented)
                                {
                                    slotinfodict[location] = localslotinfo;
                                    continue;
                                }
                            }
                            else if (kind < 10)
                            {
                                if (
                                    kind == 2 && clothes[1].id != 0 || kind == 2 && clothes[0].id != 0 && Underwearbools[0] ||
                                    kind == 3 && clothes[2].id != 0 || kind == 3 && clothes[0].id != 0 && Underwearbools[1] ||
                                    kind == 4 && clothes[3].id != 0 || kind == 4 && clothes[1].id != 0 && Underwearbools[2]
                                   )
                                {
                                    ClothFound = true;
                                }
                                else if (kind != 2 && kind != 3 && kind != 4 && kind < 9)
                                {
                                    ClothFound = true;
                                }
                                else if (kind == 9)
                                {
                                    if (slotinfodict.ContainsKey(location))
                                    {
                                        slotinfodict[location] = localslotinfo;
                                        slotinfodict[location].Binding = 9;
                                    }
                                    else
                                    {
                                        slotinfodict[location] = new Accessory_States.Slotdata() { Binding = 9, States = new List<int[]>() { new int[] { 0, 1 } } };
                                    }

                                    if (clothes[7].id != 0 && clothes[8].id != 0)
                                    {
                                        ClothFound = true;
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                slotinfodict[location] = localslotinfo;
                                slotinfodict[location].Binding = conversiondict[kind];
                                ClothFound = true;
                            }
                        }

                        if (ClothFound)
                        {
                            continue;
                        }

                        for (var i = 0; i < inclusionarray.Length; i++)
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

                        if (found)
                        {
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
                            else if ((clothes[3].id != 0 || (clothes[2].id != 0 && Underwearbools[2])) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 3;
                                states = new int[] { 1, 3 };
                            }
                            //Pantyhose
                            else if (clothes[5].id != 0 && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 5;
                            }
                            //Bra
                            else if ((clothes[2].id != 0 || clothes[0].id != 0 && Underwearbools[1]) && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                            {
                                binder = 2;
                                states = new int[] { 1, 3 };
                            }
                            //top
                            else if (clothes[0].id != 0 && (inclusionarray[4] || inclusionarray[5] || inclusionarray[8]))
                            {
                                binder = 0;
                                states = new int[] { 1, 3 };
                            }
                            //bottom
                            else if ((clothes[1].id != 0 || clothes[0].id != 0 && Underwearbools[0]) && (inclusionarray[6] || inclusionarray[7] || inclusionarray[10]))
                            {
                                binder = 1;
                                states = new int[] { 1, 3 };
                            }
                        }

                        if (binder < 0 || !found)
                        {
                            continue;
                        }

                        if (!slotinfodict.ContainsKey(location))
                        {
                            slotinfodict[location] = new Accessory_States.Slotdata();
                        }

                        slotinfodict[location].Binding = binder;
                        if (!ClothFound)
                        {
                            slotinfodict[location].States[0] = states;
                        }
                    }
                }
            }

            foreach (var item in Coordinate)
            {
                item.Value.CleanUp();
            }
            var nulldata = Coordinate.All(x => x.Value.Slotinfo.Count == 0);


            SavedData.data.Add("CoordinateData", MessagePackSerializer.Serialize(Coordinate));

            SetExtendedData("Accessory_States", (nulldata) ? null : SavedData, ChaControl);
        }

        private void ControllerReload_Loop(string Controller_Name, ChaControl ChaControl)
        {
            var Controller = Type.GetType(Controller_Name, false);
            if (Controller != null)
            {
                var temp = ChaControl.GetComponent(Controller);
                var Input_Parameter = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
                Traverse.Create(temp).Method("OnReload", Input_Parameter).GetValue();
            }
            else
            {
                Settings.Logger.LogError($"Controller {Controller_Name} not found");
            }
        }

        private void ControllerCoordReload_Loop(string Controller_Name, ChaControl ChaControl, ChaFileCoordinate coordinate)
        {
            var Controller = Type.GetType(Controller_Name, false);
            if (Controller != null)
            {
                var temp = ChaControl.GetComponent(Controller);
                var Input_Parameter = new object[2] { coordinate, false };
                Traverse.Create(temp).Method("OnCoordinateBeingLoaded", Input_Parameter).GetValue();
            }
            else
            {
                Settings.Logger.LogError($"Controller {Controller_Name} not found");
            }
        }

        public void SetExtendedData(string IDtoSET, PluginData data, ChaControl ChaControl)
        {
            ExtendedSave.SetExtendedDataById(ChaControl.chaFile, IDtoSET, data);
            ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);
#if !KKS
            if (ThisOutfitData.heroine != null)
            {
                ExtendedSave.SetExtendedDataById(ThisOutfitData.heroine.charFile, IDtoSET, data);
            }
#endif
        }
    }
}
