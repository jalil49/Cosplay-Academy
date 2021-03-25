using Cosplay_Academy.Hair;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
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
        public static CharaEvent self;
        private ChaDefault ThisOutfitData;
        protected override void Awake()
        {
            base.Awake();
            Process(KoikatuAPI.GetCurrentGameMode());
        }
        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            if (currentGameMode == GameMode.Studio)
            {
                return;
            }
            if (currentGameMode == GameMode.Maker)
            {
                Process(currentGameMode);
                ClothingLoader.Reload_RePacks(ChaControl);
            }
            else if (ThisOutfitData.heroine == null)
            {
                Game _gamemgr = Game.Instance;
                foreach (SaveData.Heroine Heroine in _gamemgr.HeroineList)
                {
                    if (Heroine.chaCtrl != null && Heroine.chaCtrl.name != null && ChaControl.name == Heroine.chaCtrl.name)
                    {
                        ThisOutfitData.heroine = Heroine;
                        break;
                    }
                }
            }
        }
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //unused mandatory function 
        }
        private void Process(GameMode currentGameMode)
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
                ThisOutfitData.ChaControl = ChaControl;
                ThisOutfitData.Chafile = ChaFileControl;
                Game _gamemgr = Game.Instance;
                foreach (SaveData.Heroine Heroine in _gamemgr.HeroineList)
                {
                    if (Heroine.chaCtrl != null && Heroine.chaCtrl.name != null && ChaControl.name == Heroine.chaCtrl.name)
                    {
                        ThisOutfitData.heroine = Heroine;
                        break;
                    }
                }
            }

            self = this;
            if (GameMode.Maker == currentGameMode)
            {
                ThisOutfitData.ChaControl = ChaControl;
                ThisOutfitData.Chafile = MakerAPI.LastLoadedChaFile;
                //ThisOutfitData.firstpass = true;
                if (ExpandedOutfit.ResetMaker.Value)
                {
                    OutfitDecider.Reset = true;
                    if (!ExpandedOutfit.PermReset.Value)
                    {
                        ExpandedOutfit.ResetMaker.Value = false;
                    }
                }
            }
            if (OutfitDecider.Reset)
            {
                //OutfitList.Clear();
                Constants.ChaDefaults.ForEach(x => x.processed = false);
                ExpandedOutfit.Logger.LogDebug("Reset passed");
                OutfitDecider.ResetDecider();
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} Started First Pass");
#endif
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

                List<RendererProperty>[] RendererPropertyQueue = new List<RendererProperty>[Constants.outfitpath.Length];
                List<MaterialFloatProperty>[] MaterialFloatPropertyQueue = new List<MaterialFloatProperty>[Constants.outfitpath.Length];
                List<MaterialColorProperty>[] MaterialColorPropertyQueue = new List<MaterialColorProperty>[Constants.outfitpath.Length];
                List<MaterialTextureProperty>[] MaterialTexturePropertyQueue = new List<MaterialTextureProperty>[Constants.outfitpath.Length];
                List<MaterialShader>[] MaterialShaderQueue = new List<MaterialShader>[Constants.outfitpath.Length];
                Dictionary<int, byte[]> importedTextDic = new Dictionary<int, byte[]>();

                for (int i = 0; i < Constants.outfitpath.Length; i++)
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
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended TextureDictionary Pass");
#endif
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
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended MaterialShaderList Pass");
#endif

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
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended RendererPropertyList Pass");
#endif

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
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended MaterialFloatPropertyList Pass");
#endif

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
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended MaterialColorPropertyList Pass");
#endif

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
#if Debug
                                    ExpandedOutfit.Logger.LogWarning($"Name: {loadedProperty.MaterialName}\tcoordin: {loadedProperty.CoordinateIndex}\tLoaded:{(int)loadedProperty.TexID}\tMatName:\t{loadedProperty.MaterialName}\tSlot:{loadedProperty.Slot}");
#endif
                                }

                                MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.TexID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                MaterialTexturePropertyQueue[loadedProperty.CoordinateIndex].Add(newTextureProperty);
                            }
                        }
                    }
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended MaterialTexturePropertyList Pass");
#endif
                }
                #endregion
#if Debug
                ExpandedOutfit.Logger.LogWarning("Ended First Pass");
#endif

                //Dictionary<int, HairAccessoryInfo> HairInfo;
                #region Queue accessories to keep
                for (int outfitnum = 0, n = Constants.outfitpath.Length; outfitnum < n; outfitnum++)
                {
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

                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(ThisOutfitData.Chafile.coordinate[outfitnum].accessory.parts); ;
                    Intermediate.AddRange(new List<ChaFileAccessory.PartsInfo>(acclist));//create intermediate as it seems that acclist is a reference
#if Debug
                    ExpandedOutfit.Logger.LogWarning($"Size of input {Intermediate.Count}");
#endif
                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (ExpandedOutfit.ExtremeAccKeeper.Value || Constants.Inclusion.Contains(Intermediate[i].parentKey))
                        {
                            if (!HairInfo.TryGetValue(i, out HairSupport.HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairSupport.HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }
                            //if (ExpandedOutfit.HairMatch.Value)
                            //{
                            //    ACCdata.ColorMatch = true;
                            //}
                            var ColorList = MaterialColorPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var FloatList = MaterialFloatPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var ShaderList = MaterialShaderQueue[outfitnum].FindAll(x => x.Slot == i);
                            var TextureList = MaterialTexturePropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var RenderList = RendererPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            if (ColorList.Count == 0)
                            {
                                Color color = new Color(0, 0, 0);
                                ColorList.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, -1, "", "", color, color));
                                //ColorList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("Color null");
                            }
                            if (FloatList.Count == 0)
                            {
                                FloatList.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, -1, "", "", "", ""));
                                //FloatList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("FloatList null");
                            }
                            if (ShaderList.Count == 0)
                            {
                                ShaderList.Add(new MaterialShader(ObjectType.Unknown, outfitnum, -1, "", 0, 0));
                                //ShaderList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("ShaderList null");
                            }
                            if (TextureList.Count == 0)
                            {
                                TextureList.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, -1, "", ""));
                                //TextureList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("TextureList null");
                            }
                            if (RenderList.Count == 0)
                            {
                                RenderList.Add(new RendererProperty(ObjectType.Unknown, outfitnum, -1, "", RendererProperties.Enabled, "", ""));
                                //RenderList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("Render null");
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
#if Debug
                //ThisOutfitData.TexturePrint();
#endif
                #endregion
                if (currentGameMode != GameMode.Maker)
                {
                    ThisOutfitData.firstpass = false;
                }

            }
            if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode /*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            {
                return;
            }//if disabled don't run

            //use Chacontrol.name instead of ChaControl.fileParam.fullname to probably avoid same name conflicts
            if (ChaControl.sex == 1)//run the following if female
            {

                if (currentGameMode == GameMode.MainGame || ExpandedOutfit.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    if (!ThisOutfitData.processed)//run if unprocessed
                    {
                        OutfitDecider.Decision(ChaControl.fileParam.fullname, ThisOutfitData);//Generate outfits
                        //OutfitDecider.ProcessedNames.Add(ChaControl.name);//character is processed
                        ThisOutfitData.processed = true;
                        if (!ExpandedOutfit.PermChangeOutfit.Value)
                        {
                            ExpandedOutfit.ChangeOutfit.Value = false;
                        }
                    }
                    ClothingLoader clothingLoader = new ClothingLoader();
                    //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} chano {ChaFileControl.loadProductNo} name {ChaFileControl.loadVersion} {ChaFileControl.facePngData}");
                    int HoldOutfit = ChaControl.fileStatus.coordinateType; //requried for Cutscene characters to wear correct outfit such as sakura's first cutscene
                    clothingLoader.FullLoad(ThisOutfitData, ChaControl, ChaFileControl);//Load outfits; has to run again for story mode les scene at least
                    ChaControl.fileStatus.coordinateType = HoldOutfit;
                    ChaControl.SetAccessoryStateAll(true);
                    ChaInfo temp = (ChaInfo)ChaControl;
                    ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
                }
            }

        }
        //public void HairAccessory_RePack()//original
        //{
        //    Dictionary<int, HairAccessoryInfo> PluginData = new Dictionary<int, HairAccessoryInfo>();
        //    Dictionary<int, Dictionary<int, HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairAccessoryInfo>>();
        //    Dictionary<int, HairAccessoryInfo> Temp;
        //    for (int i = 0; i < ChaFileControl.coordinate.Length; i++)
        //    {
        //        var Inputdata = ExtendedSave.GetExtendedDataById(ChaFileControl.coordinate[i], "com.deathweasel.bepinex.hairaccessorycustomizer");
        //        Temp = new Dictionary<int, HairAccessoryInfo>();
        //        if (Inputdata != null)
        //            if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
        //                Temp = MessagePackSerializer.Deserialize<Dictionary<int, HairAccessoryInfo>>((byte[])loadedHairAccessories);
        //        for (int j = 0; j < Temp.Count; j++)
        //        {
        //            ExpandedOutfit.Logger.LogWarning($"Coordinate {i}: {Temp.ElementAt(j).Key}\t\t\t{Temp.ElementAt(j).Value}");
        //        }
        //        HairAccessories.Add(i, Temp);
        //    }
        //    var data = new PluginData();
        //    data.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
        //    SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", data);
        //}

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!ExpandedOutfit.AccKeeper.Value)
            {
                return;
            }//if disabled don't run
            ClothingLoader.ProcessLoad(ThisOutfitData, coordinate, ChaControl);
        }
    }
}