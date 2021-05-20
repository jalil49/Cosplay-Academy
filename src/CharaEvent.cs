using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using Manager;
using MessagePack;
using MoreAccessoriesKOI;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UnityEngine;

namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        private ChaDefault ThisOutfitData;
        private static bool ClearData;
        private static bool Firstpass = true;

        public static void MakerAPI_MakerExiting()
        {
            ClearData = false;
            Firstpass = true;
            if (!MakerAPI.IsInsideClassMaker())
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            e.AddSidebarControl(new SidebarToggle("Enable Cosplay Academy", Settings.Makerview.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.Makerview.Value,
                (controller, value) => Settings.Makerview.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand outfits", Settings.ChangeOutfit.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.ChangeOutfit.Value,
                (controller, value) => Settings.ChangeOutfit.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand Underwear", Settings.RandomizeUnderwear.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.RandomizeUnderwear.Value,
                (controller, value) => Settings.RandomizeUnderwear.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Reset Process", Settings.ResetMaker.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.ResetMaker.Value,
                (controller, value) => Settings.ResetMaker.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Only Underwear", Settings.RandomizeUnderwearOnly.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.RandomizeUnderwearOnly.Value,
                (controller, value) => Settings.RandomizeUnderwearOnly.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Clear Existing", false, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => ClearData,
                (controller, value) => ClearData = value);
        }

        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            if (currentGameMode == GameMode.Studio)
            {
                return;
            }
            bool IsMaker = currentGameMode == GameMode.Maker;
            if (IsMaker || !IsMaker && (ThisOutfitData == null || !ThisOutfitData.processed) || GameAPI.InsideHScene)
            {
                if (ClearData && IsMaker)
                {
                    Constants.ChaDefaults.Clear();
                }

                Process(currentGameMode);
                ThisOutfitData.ClothingLoader.Reload_RePacks(ChaControl);
                if (IsMaker && Firstpass)
                {
                    ChaControl.ChangeCoordinateTypeAndReload();
                    Firstpass = false;
                }
            }
            //Time.Stop();

            //ExpandedOutfit.Logger.LogWarning($"Time is {Time.ElapsedMilliseconds}");
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //unused mandatory function 
        }

        public void Process(GameMode currentGameMode)
        {
            //ExpandedOutfit.Logger.LogWarning("Started process for " + ChaControl.fileParam.fullname);
            ThisOutfitData = Constants.ChaDefaults.Find(x => ChaControl.fileParam.personality == x.Personality && x.FullName == ChaControl.fileParam.fullname && x.BirthDay == ChaControl.fileParam.strBirthDay);
            if (ThisOutfitData == null)
            {
                //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} made new default; chano {ChaControl.fileParam.strBirthDay} name {ChaControl.fileParam.personality}");
                ThisOutfitData = new ChaDefault();
                Constants.ChaDefaults.Add(ThisOutfitData);
                ThisOutfitData.FullName = ChaControl.fileParam.fullname;
                ThisOutfitData.BirthDay = ChaControl.fileParam.strBirthDay;
                ThisOutfitData.Personality = ChaControl.fileParam.personality;
                Game _gamemgr = Game.Instance;
                foreach (SaveData.Heroine Heroine in _gamemgr.HeroineList)
                {
                    if (Heroine.parameter.personality == ChaControl.fileParam.personality && Heroine.parameter.fullname == ChaControl.fileParam.fullname && Heroine.parameter.strBirthDay == ChaControl.fileParam.strBirthDay)
                    {
                        ThisOutfitData.heroine = Heroine;
                        break;
                    }
                }
            }
            if (ChaControl.sex != 0 && ThisOutfitData.heroine.isTeacher && !Settings.TeacherDress.Value)
            {
                return;
            }
            ThisOutfitData.ChaControl = ChaControl;
            ThisOutfitData.Chafile = ChaFileControl;
            if (GameMode.Maker == currentGameMode)
            {
                ThisOutfitData.Chafile = MakerAPI.LastLoadedChaFile;
                if (Settings.ResetMaker.Value)
                {
                    OutfitDecider.ResetDecider();
                }
            }

            if (ThisOutfitData.firstpass) //Save all accessories to avoid duplicating head accessories each load and be reuseable
            {
                ThisOutfitData.Clear_ME();

                WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

                Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> CharaHair = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();

                PluginData HairExtendedData;
                HairExtendedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.hairaccessorycustomizer");

                if (HairExtendedData != null && HairExtendedData.data.TryGetValue("HairAccessories", out var AllHairAccessories) && AllHairAccessories != null)
                    CharaHair = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>>((byte[])AllHairAccessories);
                PluginData MaterialEditorData;
                MaterialEditorData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");

                #region ME Acc Import

                List<RendererProperty>[] RendererPropertyQueue = new List<RendererProperty>[Constants.Outfit_Size];
                List<MaterialFloatProperty>[] MaterialFloatPropertyQueue = new List<MaterialFloatProperty>[Constants.Outfit_Size];
                List<MaterialColorProperty>[] MaterialColorPropertyQueue = new List<MaterialColorProperty>[Constants.Outfit_Size];
                List<MaterialTextureProperty>[] MaterialTexturePropertyQueue = new List<MaterialTextureProperty>[Constants.Outfit_Size];
                List<MaterialShader>[] MaterialShaderQueue = new List<MaterialShader>[Constants.Outfit_Size];
                Dictionary<int, byte[]> importedTextDic = new Dictionary<int, byte[]>();

                for (int i = 0; i < Constants.Outfit_Size; i++)
                {
                    RendererPropertyQueue[i] = new List<RendererProperty>();
                    MaterialFloatPropertyQueue[i] = new List<MaterialFloatProperty>();
                    MaterialColorPropertyQueue[i] = new List<MaterialColorProperty>();
                    MaterialTexturePropertyQueue[i] = new List<MaterialTextureProperty>();
                    MaterialShaderQueue[i] = new List<MaterialShader>();
                }

                var ImportDictionary = new Dictionary<int, int>();

                if (MaterialEditorData?.data != null)
                {
                    if (MaterialEditorData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        Dictionary<int, byte[]> Des = MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic);
                        foreach (var x in Des)
                        {
                            importedTextDic.Add(x.Key, x.Value);
                            //ImportDictionary[x.Key] = ME_Support.SetAndGetTextureID(x.Value);
                        }
                    }

                    if (MaterialEditorData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Accessory)
                            {
                                MaterialShaderQueue[loadedProperty.CoordinateIndex].Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                            }
                        }
                    }

                    if (MaterialEditorData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Accessory)
                            {
                                //ExpandedOutfit.Logger.LogWarning($"Renderer index: {loadedProperty.CoordinateIndex},\tSlot: {loadedProperty.Slot},\tName: {loadedProperty.RendererName}");
                                RendererPropertyQueue[loadedProperty.CoordinateIndex].Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }

                    if (MaterialEditorData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Accessory)
                            {
                                MaterialFloatPropertyQueue[loadedProperty.CoordinateIndex].Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }

                    if (MaterialEditorData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Accessory)
                            {
                                MaterialColorPropertyQueue[loadedProperty.CoordinateIndex].Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }

                    if (MaterialEditorData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType == ObjectType.Accessory)
                            {
                                //int? texID = null;
                                if (loadedProperty.TexID != null)
                                {
                                    if (!ThisOutfitData.importDictionaryQueue[loadedProperty.CoordinateIndex].ContainsKey((int)loadedProperty.TexID))
                                    {
                                        ThisOutfitData.importDictionaryQueue[loadedProperty.CoordinateIndex].Add((int)loadedProperty.TexID, importedTextDic[(int)loadedProperty.TexID]);
                                    }
                                    //texID = ImportDictionary[(int)loadedProperty.TexID];
                                }

                                MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.TexID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                MaterialTexturePropertyQueue[loadedProperty.CoordinateIndex].Add(newTextureProperty);
                            }
                        }
                    }
                }
                #endregion

                #region Queue accessories to keep
                List<int>[] HairKeep = new List<int>[Constants.Outfit_Size];
                List<int>[] ACCKeep = new List<int>[Constants.Outfit_Size];
                for (int i = 0; i < Constants.Outfit_Size; i++)
                {
                    HairKeep[i] = new List<int>();
                    ACCKeep[i] = new List<int>();
                }

                bool Cosplay_Academy_Ready = false;
                var Required_Support = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
                if (Required_Support != null)
                {
                    if (Required_Support.data.TryGetValue("HairAcc", out var Bytedata))
                    {
                        HairKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])Bytedata);
                    }
                    if (Required_Support.data.TryGetValue("AccKeep", out Bytedata))
                    {
                        ACCKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])Bytedata);
                    }
                    if (Required_Support.data.TryGetValue("Cosplay_Academy_Ready", out Bytedata))
                    {
                        Cosplay_Academy_Ready = MessagePackSerializer.Deserialize<bool>((byte[])Bytedata);
                    }
                }

                for (int outfitnum = 0, n = Constants.Outfit_Size; outfitnum < n; outfitnum++)
                {
                    ThisOutfitData.Original_Coordinates[outfitnum] = CloneCoordinate(ChaFileControl.coordinate[outfitnum]);

                    List<ChaFileAccessory.PartsInfo> AccImport = new List<ChaFileAccessory.PartsInfo>();
                    List<HairSupport.HairAccessoryInfo> HairImport = new List<HairSupport.HairAccessoryInfo>();
                    if (CharaHair.TryGetValue(outfitnum, out Dictionary<int, HairSupport.HairAccessoryInfo> HairInfo) == false)
                    {
                        HairInfo = new Dictionary<int, HairSupport.HairAccessoryInfo>();
                    }

                    if (_accessoriesByChar.TryGetValue(ThisOutfitData.Chafile, out var SaveAccessory) == false)
                    {
                        SaveAccessory = new MoreAccessories.CharAdditionalData();
                        _accessoriesByChar.Add(ThisOutfitData.Chafile, SaveAccessory);
                    }

                    if (SaveAccessory.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> acclist) == false)
                    {
                        acclist = new List<ChaFileAccessory.PartsInfo>();
                    }

                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(ThisOutfitData.Chafile.coordinate[outfitnum].accessory.parts);
                    Intermediate.AddRange(new List<ChaFileAccessory.PartsInfo>(acclist));//create intermediate as it seems that acclist is a reference

                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (Settings.ExtremeAccKeeper.Value || Constants.Generic_Inclusion.Contains(Intermediate[i].parentKey) && !Cosplay_Academy_Ready || HairKeep[outfitnum].Contains(i) || ACCKeep[outfitnum].Contains(i))
                        {
                            if (!HairInfo.TryGetValue(i, out HairSupport.HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairSupport.HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }
                            ThisOutfitData.HairKeepQueue[outfitnum].Add(HairKeep[outfitnum].Contains(i));
                            ThisOutfitData.ACCKeepQueue[outfitnum].Add(ACCKeep[outfitnum].Contains(i));
                            var ColorList = MaterialColorPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var FloatList = MaterialFloatPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var ShaderList = MaterialShaderQueue[outfitnum].FindAll(x => x.Slot == i);
                            var TextureList = MaterialTexturePropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var RenderList = RendererPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            if (ColorList.Count == 0)
                            {
                                Color color = new Color(0, 0, 0);
                                ColorList.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, i, "", "", color, color));
                            }
                            if (FloatList.Count == 0)
                            {
                                FloatList.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, i, "", "", "", ""));
                            }
                            if (ShaderList.Count == 0)
                            {
                                ShaderList.Add(new MaterialShader(ObjectType.Unknown, outfitnum, i, "", 0, 0));
                            }
                            if (TextureList.Count == 0)
                            {
                                TextureList.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, i, "", ""));
                            }
                            if (RenderList.Count == 0)
                            {
                                RenderList.Add(new RendererProperty(ObjectType.Unknown, outfitnum, i, "", RendererProperties.Enabled, "", ""));
                            }

                            ThisOutfitData.MaterialColorPropertyQueue[outfitnum].AddRange(ColorList);
                            ThisOutfitData.MaterialFloatPropertyQueue[outfitnum].AddRange(FloatList);
                            ThisOutfitData.MaterialShaderQueue[outfitnum].AddRange(ShaderList);
                            ThisOutfitData.MaterialTexturePropertyQueue[outfitnum].AddRange(TextureList);
                            ThisOutfitData.RendererPropertyQueue[outfitnum].AddRange(RenderList);
                            ThisOutfitData.CoordinatePartsQueue[outfitnum].Add(Intermediate[i]);
                            ThisOutfitData.HairAccQueue[outfitnum].Add(ACCdata);
                        }
                    }
                }
                #endregion
                if (currentGameMode != GameMode.Maker)
                {
                    ThisOutfitData.firstpass = false;
                }

            }
            if (!Settings.EnableSetting.Value && GameMode.MainGame == currentGameMode || !Settings.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode /*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            {
                return;
            }//if disabled don't run

            //use Chacontrol.name instead of ChaControl.fileParam.fullname to probably avoid same name conflicts
            if (ChaControl.sex == 1)//run the following if female
            {
                if (currentGameMode == GameMode.MainGame || Settings.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    if (!ThisOutfitData.processed)//run if unprocessed
                    {
                        OutfitDecider.Decision(ChaControl.fileParam.fullname, ThisOutfitData);//Generate outfits
                        ThisOutfitData.processed = true;
                    }
                }
                int HoldOutfit = ChaControl.fileStatus.coordinateType; //requried for Cutscene characters to wear correct outfit such as sakura's first cutscene
                ThisOutfitData.ClothingLoader.FullLoad(ThisOutfitData, ChaControl, ChaFileControl);//Load outfits; has to run again for story mode les scene at least
                ChaControl.fileStatus.coordinateType = HoldOutfit;
                ChaInfo temp = (ChaInfo)ChaControl;
                ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!Settings.AccKeeper.Value)
            {
                return;
            }//if disabled don't run
            ThisOutfitData.ClothingLoader.CoordinateLoad(ThisOutfitData, coordinate, ChaControl);
        }

        private ChaFileCoordinate CloneCoordinate(ChaFileCoordinate OriginalCoordinate)
        {
            ChaFileCoordinate Temp = new ChaFileCoordinate();
            var TempClothPart = Temp.clothes.parts;
            var SourceClothPart = OriginalCoordinate.clothes.parts;

            for (int i = 0; i < TempClothPart.Length; i++)
            {
                int idpass = SourceClothPart[i].id;
                TempClothPart[i].id = idpass;
#if Party
                    //doesnt exist in party

                for (int j = 0; j < TempClothPart[i].hideOpt.Length; j++) //doesnt exist in party
                {
                    bool hideoptpass = SourceClothPart[i].hideOpt[j];
                    TempClothPart[i].hideOpt[j] = hideoptpass;
                }
                
                    int Emblem2 = SourceClothPart[i].emblemeId2;
                    TempClothPart[i].emblemeId2 = Emblem2;
                    int passsleave = SourceClothPart[i].sleevesType;
                    TempClothPart[i].sleevesType = passsleave;                
#endif
                for (int j = 0; j < TempClothPart[i].colorInfo.Length; j++)
                {
                    Color PassBaseColor = SourceClothPart[i].colorInfo[j].baseColor;
                    TempClothPart[i].colorInfo[j].baseColor = PassBaseColor;
                    Color PassPatColor = SourceClothPart[i].colorInfo[j].patternColor;
                    TempClothPart[i].colorInfo[j].patternColor = PassPatColor;
                    int PassPattern = SourceClothPart[j].colorInfo[j].pattern;
                    TempClothPart[i].colorInfo[j].pattern = PassPattern;
                    Vector2 Passtiling = SourceClothPart[j].colorInfo[j].tiling;
                    TempClothPart[i].colorInfo[j].tiling = Passtiling;
                }
                int Emblem1 = SourceClothPart[i].emblemeId;
                TempClothPart[i].emblemeId = Emblem1;
            }
            return Temp;
        }
    }
}