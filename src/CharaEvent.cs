using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KoiClothesOverlayX;
using Manager;
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
    public class CharaEvent : CharaCustomFunctionController
    {
        //private List<ChaFileCoordinate> OutfitList = new List<ChaFileCoordinate>();
        public static CharaEvent self;
        private bool Repeat_stoppper = false;
        private ChaDefault ThisOutfitData;
        //private ClothingLoader ClothingLoader = new ClothingLoader();

        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            ThisOutfitData = Constants.ChaDefaults.Find(x => ChaControl.fileParam.personality == x.Personality && x.FullName == ChaControl.fileParam.fullname && x.BirthDay == ChaControl.fileParam.strBirthDay);
            if (ThisOutfitData == null)
            {
                //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} made new default; chano {ChaControl.fileParam.strBirthDay} name {ChaControl.fileParam.personality}");
                ThisOutfitData = new ChaDefault();
                Constants.ChaDefaults.Add(ThisOutfitData);
                ThisOutfitData.FullName = ChaControl.fileParam.fullname;
                ThisOutfitData.BirthDay = ChaControl.fileParam.strBirthDay;
                ThisOutfitData.Personality = ChaControl.fileParam.personality;
            }
            self = this;
            if (GameMode.Maker == currentGameMode)
            {
                ThisOutfitData.firstpass = true;
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
                ExpandedOutfit.Logger.LogWarning("Reset passed");
                OutfitDecider.ResetDecider();
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} Started First Pass");
#endif
            if (ThisOutfitData.firstpass) //Save all accessories to avoid duplicating head accessories each load and be reuseable
            {
                WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

                Dictionary<int, Dictionary<int, HairAccessoryInfo>> CharaHair = new Dictionary<int, Dictionary<int, HairAccessoryInfo>>();
                PluginData HairExtendedData = ExtendedSave.GetExtendedDataById(ChaFileControl, "com.deathweasel.bepinex.hairaccessorycustomizer");
                if (HairExtendedData != null && HairExtendedData.data.TryGetValue("HairAccessories", out var AllHairAccessories) && AllHairAccessories != null)
                    CharaHair = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, HairAccessoryInfo>>>((byte[])AllHairAccessories);
#if ME_Support
                PluginData MaterialEditorData = ExtendedSave.GetExtendedDataById(ChaFileControl, "com.deathweasel.bepinex.materialeditor");

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
                                //ExpandedOutfit.Logger.LogWarning($"Renderer index: {loadedProperty.CoordinateIndex},\tSlot: {loadedProperty.Slot},\tProperty: {loadedProperty.Property},\tName: {loadedProperty.RendererName},\tValue: {loadedProperty.Value}");
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
                                }
                                //ExpandedOutfit.Logger.LogWarning($"Name: {loadedProperty.MaterialName}");
                                //ExpandedOutfit.Logger.LogWarning($"Loaded:{(int)loadedProperty.TexID}\tTexID:\t{texID}\tSlot:{loadedProperty.Slot}");

                                MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.TexID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                MaterialTexturePropertyQueue[loadedProperty.CoordinateIndex].Add(newTextureProperty);
                            }
                        }
                    }
#if Debug
                    ExpandedOutfit.Logger.LogWarning("Ended MaterialTexturePropertyList Pass");
#endif
                }
                else
                {
                    ExpandedOutfit.Logger.LogError($"material editor null");
                }
                #endregion
#endif
#if Debug
                ExpandedOutfit.Logger.LogWarning("Ended First Pass");
#endif

                //Dictionary<int, HairAccessoryInfo> HairInfo;
                #region Queue accessories to keep
                for (int outfitnum = 0, n = Constants.outfitpath.Length; outfitnum < n; outfitnum++)
                {
                    List<ChaFileAccessory.PartsInfo> AccImport = new List<ChaFileAccessory.PartsInfo>();
                    List<HairAccessoryInfo> HairImport = new List<HairAccessoryInfo>();
                    ThisOutfitData.CoordinatePartsQueue[outfitnum].Clear();
                    ThisOutfitData.HairAccQueue[outfitnum].Clear();
                    if (CharaHair.TryGetValue(outfitnum, out Dictionary<int, HairAccessoryInfo> HairInfo) == false)
                    {
                        HairInfo = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
                    }

                    if (_accessoriesByChar.TryGetValue(ChaControl.chaFile, out var SaveAccessory) == false)
                    {
                        SaveAccessory = new MoreAccessories.CharAdditionalData();
                        _accessoriesByChar.Add(ChaControl.chaFile, SaveAccessory);
                    }
                    if (SaveAccessory.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> acclist) == false)
                    {
                        acclist = new List<ChaFileAccessory.PartsInfo>();
                    }

                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(acclist);//create intermediate as it seems that acclist is a reference
                    Intermediate.AddRange(ChaControl.chaFile.coordinate[outfitnum].accessory.parts);

                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (Constants.Inclusion.Contains(Intermediate[i].parentKey))
                        {
                            if (!HairInfo.TryGetValue(i, out HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }
#if ME_Support
                            var ColorList = MaterialColorPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var FloatList = MaterialFloatPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var ShaderList = MaterialShaderQueue[outfitnum].FindAll(x => x.Slot == i);
                            var TextureList = MaterialTexturePropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var RenderList = RendererPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            if (ColorList.Count == 0)
                            {
                                Color color = new Color(0, 0, 0);
                                ColorList.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, i, "", "", color, color));
                                //ExpandedOutfit.Logger.LogWarning("Color null");
                            }
                            if (FloatList.Count == 0)
                            {
                                FloatList.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, i, "", "", "", ""));
                                //ExpandedOutfit.Logger.LogWarning("FloatList null");
                            }
                            if (ShaderList.Count == 0)
                            {
                                ShaderList.Add(new MaterialShader(ObjectType.Unknown, outfitnum, i, "", 0, 0));
                                //ExpandedOutfit.Logger.LogWarning("ShaderList null");
                            }
                            if (TextureList.Count == 0)
                            {
                                TextureList.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, i, "", ""));
                                //ExpandedOutfit.Logger.LogWarning("TextureList null");
                            }
                            if (RenderList.Count == 0)
                            {
                                RenderList.Add(new RendererProperty(ObjectType.Unknown, outfitnum, i, "", new RendererProperties(), "", ""));
                                //ExpandedOutfit.Logger.LogWarning("Render null");
                            }

                            ThisOutfitData.MaterialColorPropertyQueue[outfitnum].AddRange(ColorList);
                            ThisOutfitData.MaterialFloatPropertyQueue[outfitnum].AddRange(FloatList);
                            ThisOutfitData.MaterialShaderQueue[outfitnum].AddRange(ShaderList);
                            ThisOutfitData.MaterialTexturePropertyQueue[outfitnum].AddRange(TextureList);
                            ThisOutfitData.RendererPropertyQueue[outfitnum].AddRange(RenderList);
#endif
                            ThisOutfitData.CoordinatePartsQueue[outfitnum].Add(Intermediate[i]);
                            ThisOutfitData.HairAccQueue[outfitnum].Add(ACCdata);
                        }
                    }
                }
                //ThisOutfitData.Print();
                if (currentGameMode != GameMode.Maker)
                {
                    ThisOutfitData.firstpass = false;
                }
                #endregion


            }
            if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode || Repeat_stoppper && GameMode.Maker != currentGameMode/*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            {
                Repeat_stoppper = false;
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
                    //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} chano {ChaControl.chaFile.loadProductNo} name {ChaControl.chaFile.loadVersion} {ChaControl.chaFile.facePngData}");
                    int HoldOutfit = ChaControl.fileStatus.coordinateType;
                    ClothingLoader.FullLoad(ChaControl, ThisOutfitData);//Load outfits; has to run again for story mode les scene at least
                    ChaControl.fileStatus.coordinateType = HoldOutfit;

                    ChaInfo temp = (ChaInfo)ChaControl;
                    ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits

                    if (Repeat_stoppper)//stop any potential endless loops since I call a refresh
                    {
                        Repeat_stoppper = false;
                        return;
                    }


                    KCOX_RePack();//Reassign materials for Clothes

                    ME_RePack();//Reassign materials for accessories

                    Finish();
                }
            }
            //else
            //{
            //    ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} is already processed did this occur in H Les scene?");
            //}
        }
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //unused mandatory function 
        }
        public void KCOX_RePack()
        {
            PluginData SavedData;
            var data = new PluginData { version = 1 };
            Dictionary<string, ClothesTexData> storage;
            Dictionary<CoordinateType, Dictionary<string, ClothesTexData>> Final = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
            for (int i = 0; i < ChaControl.chaFile.coordinate.Length; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "KCOX");
                storage = new Dictionary<string, ClothesTexData>();
                if (SavedData != null && SavedData.data.TryGetValue("Overlays", out var bytes) && bytes is byte[] byteArr)
                {
                    var dict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(byteArr);
                    if (dict != null)
                    {
                        foreach (var texData in dict)
                            storage.Add(texData.Key, texData.Value);
                    }
                }
                Final.Add((CoordinateType)i, storage);
            }
            data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
            SetExtendedData("KCOX", data);
        }
        #region Stuff KCOX_RePack Needs
        //[MessagePackObject]
        //public class ClothesTexData
        //{
        //    [IgnoreMember]
        //    private byte[] _textureBytes;
        //    [IgnoreMember]
        //    private Texture2D _texture;

        //    [IgnoreMember]
        //    public Texture2D Texture
        //    {
        //        get
        //        {
        //            if (_texture == null)
        //            {
        //                if (_textureBytes != null)
        //                    _texture = TextureFromBytes(_textureBytes, GetSelectedOverlayTexFormat(false));
        //            }
        //            return _texture;
        //        }
        //        set
        //        {
        //            if (value != null && value == _texture) return;
        //            UnityEngine.Object.Destroy(_texture);
        //            _texture = value;
        //            _textureBytes = value?.EncodeToPNG();
        //        }
        //    }

        //    [Key(0)]
        //    public byte[] TextureBytes
        //    {
        //        get => _textureBytes;
        //        set
        //        {
        //            Texture = null;
        //            _textureBytes = value;
        //        }
        //    }

        //    [Key(1)]
        //    public bool Override;

        //    public void Dispose()
        //    {
        //        UnityEngine.Object.Destroy(_texture);
        //        _texture = null;
        //    }

        //    public void Clear()
        //    {
        //        TextureBytes = null;
        //    }

        //    public bool IsEmpty()
        //    {
        //        return !Override && TextureBytes == null;
        //    }
        //}
        //private static TextureFormat GetSelectedOverlayTexFormat(bool isMask)
        //{
        //    //if (isMask)
        //    //    //    return CompressTextures.Value ? TextureFormat.DXT1 : TextureFormat.RG16;
        //    //    //return CompressTextures.Value ? TextureFormat.DXT5 : TextureFormat.ARGB32;
        //    //    return /*CompressTextures.Value ? TextureFormat.DXT1 :*/ TextureFormat.RG16;
        //    //return /*CompressTextures.Value ? TextureFormat.DXT5 :*/ TextureFormat.ARGB32;
        //    //ConfigEntry<bool> CompressTextures = Traverse.CreateWithType("KoiSkinOverlayX.KoiSkinOverlayMgr, KK_OverlayMods").Property("CompressTextures").GetValue();
        //}
        //private static Texture2D TextureFromBytes(byte[] texBytes, TextureFormat format)
        //{
        //    if (texBytes == null || texBytes.Length == 0) return null;

        //    var tex = new Texture2D(2, 2, format, false);
        //    tex.LoadImage(texBytes);
        //    return tex;
        //}

        #endregion

        public void ME_RePack()
        {
            List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShaderList = new List<MaterialShader>();
            Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

            #region UnPackCoordinates
#if ME_Support
            if (!ThisOutfitData.ME_Work)
            {
#endif
                for (int outfitnum = 0; outfitnum < ChaControl.chaFile.coordinate.Length; outfitnum++)
                {
                    var data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
                    if (data?.data != null)
                    {
                        if (data.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                            foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                                importDictionaryList[x.Key] = ME_Support.SetAndGetTextureID(x.Value);

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
#if ME_Support
            }
            else
            {
                RendererPropertyList = ThisOutfitData.ReturnRenderer;
                MaterialFloatPropertyList = ThisOutfitData.ReturnMaterialFloat;
                MaterialColorPropertyList = ThisOutfitData.ReturnMaterialColor;
                MaterialTexturePropertyList = ThisOutfitData.ReturnMaterialTexture;
                MaterialShaderList = ThisOutfitData.ReturnMaterialShade;
            }
#endif
            #endregion
            #region Pack
            var SaveData = new PluginData();

            List<int> IDsToPurge = new List<int>();
            foreach (int texID in ME_Support.TextureDictionary.Keys)
                if (MaterialTexturePropertyList.All(x => x.TexID != texID))
                    IDsToPurge.Add(texID);

            for (var i = 0; i < IDsToPurge.Count; i++)
            {
                int texID = IDsToPurge[i];
                if (ME_Support.TextureDictionary.TryGetValue(texID, out var val)) val.Dispose();
                ME_Support.TextureDictionary.Remove(texID);
            }


            if (ME_Support.TextureDictionary.Count > 0)
                SaveData.data.Add("TextureDictionary", MessagePackSerializer.Serialize(ME_Support.TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
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

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData);

            #endregion
        }

        //public void HairAccessory_RePack()//original
        //{
        //    Dictionary<int, HairAccessoryInfo> PluginData = new Dictionary<int, HairAccessoryInfo>();
        //    Dictionary<int, Dictionary<int, HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairAccessoryInfo>>();
        //    Dictionary<int, HairAccessoryInfo> Temp;
        //    for (int i = 0; i < ChaControl.chaFile.coordinate.Length; i++)
        //    {
        //        var Inputdata = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.hairaccessorycustomizer");
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

        #region Stuff Hair Accessories needs
        [Serializable]
        [MessagePackObject]
        public class HairAccessoryInfo
        {
            [Key("HairGloss")]
            public bool HairGloss = ColorMatchDefault;
            [Key("ColorMatch")]
            public bool ColorMatch = HairGlossDefault;
            [Key("OutlineColor")]
            public Color OutlineColor = OutlineColorDefault;
            [Key("AccessoryColor")]
            public Color AccessoryColor = AccessoryColorDefault;
            [Key("HairLength")]
            public float HairLength = HairLengthDefault;

        }
        private static bool ColorMatchDefault = true;
        private static bool HairGlossDefault = true;
        private static Color OutlineColorDefault = Color.black;
        private static Color AccessoryColorDefault = Color.red;
        private static float HairLengthDefault = 0;
        #endregion

        SaveData.Heroine heroine;

        public void SetExtendedData(string IDtoSET, PluginData data)
        {
            ExtendedSave.SetExtendedDataById(ChaFileControl, IDtoSET, data);

            //object[] Reload = new object[1] { KoikatuAPI.GetCurrentGameMode() };

            Game _gamemgr = Game.Instance;
            List<SaveData.Heroine> Heroines = _gamemgr.HeroineList;
            if (ChaControl.name != null && ChaControl.sex == 1)
            {
                foreach (SaveData.Heroine Heroine in Heroines)
                {
                    if (Heroine.chaCtrl != null && Heroine.chaCtrl.name != null && ChaControl.name == Heroine.chaCtrl.name)
                    {
                        heroine = Heroine;
                        break;
                    }
                }
            }
            if (heroine != null && ChaControl.sex == 1)
            {
                ExtendedSave.SetExtendedDataById(ChaFileControl, IDtoSET, data);
                ExtendedSave.SetExtendedDataById(heroine.charFile, IDtoSET, data);
                if (ChaControl.name == heroine.chaCtrl.name)
                {
                    ExtendedSave.SetExtendedDataById(heroine.chaCtrl.chaFile, IDtoSET, data);
                    return;
                }
            }

            var player = ChaControl.GetPlayer();
            if (player != null)
            {
                ExtendedSave.SetExtendedDataById(player.charFile, IDtoSET, data);
                if (ChaControl != player.chaCtrl)
                {
                    ExtendedSave.SetExtendedDataById(player.chaCtrl.chaFile, IDtoSET, data);
                }
            }
            heroine = null;
        }
        private void Finish()
        {

            Game _gamemgr = Game.Instance;
            List<SaveData.Heroine> Heroines = _gamemgr.HeroineList;
            if (ChaControl.name != null && ChaControl.sex == 1)
            {
                foreach (SaveData.Heroine Heroine in Heroines)
                {
                    if (Heroine.chaCtrl != null && ChaControl.name == Heroine.chaCtrl.name)
                    {
                        if (ChaControl.name == Heroine.chaCtrl.name)
                        {
                            heroine = Heroine;
                            break;
                        }
                    }
                }
            }

            object[] Reload = new object[1] { KoikatuAPI.GetCurrentGameMode() };
            Repeat_stoppper = true;
            if (heroine != null && ChaControl.sex == 1)
            {
                if (ChaControl.name == heroine.chaCtrl.name)
                {
                    // Update other instance to reflect the new ext data
                    var other = heroine.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
                    heroine = null;
                    return;
                }
            }

            var player = ChaControl.GetPlayer();
            if (player != null)
            {
                if (ChaControl != player.chaCtrl)
                {
                    // Update other instance to reflect the new ext data
                    var other = player.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
                }
            }
            heroine = null;
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!ExpandedOutfit.AccKeeper.Value)
            {
                return;
            }//if disabled don't run

            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            //Apply pre-existing Accessories in any open slot or final slots.
            #region Reassign Exisiting Accessories

            var Inputdata = ExtendedSave.GetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            Dictionary<int, CharaEvent.HairAccessoryInfo> Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);
            if (_accessoriesByChar.TryGetValue(ChaControl.chaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaControl.chaFile, data);
            }


            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[ChaFileControl.status.coordinateType]);
            var HairQueue = new Queue<CharaEvent.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[ChaFileControl.status.coordinateType]);
            //ExpandedOutfit.Logger.LogWarning($"CPQ: {ThisOutfitData.CoordinatePartsQueue[ChaFileControl.status.coordinateType].Count}");
            //ExpandedOutfit.Logger.LogWarning($"HAQ: {ThisOutfitData.HairAccQueue[ChaFileControl.status.coordinateType].Count}");

            int ACCpostion = 0;

            for (int n = ChaControl.nowCoordinate.accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                if (ChaControl.nowCoordinate.accessory.parts[ACCpostion].type == 120) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        ChaControl.nowCoordinate.accessory.parts[ACCpostion] = PartsQueue.Dequeue();
                        //ExpandedOutfit.Logger.LogWarning(ChaControl.fileParam.fullname + $" Deque<20");
                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
                    }
                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    //ExpandedOutfit.Logger.LogWarning($"Coordinate Hair Force prev state:{info.ColorMatch}");
                    info.ColorMatch = true;
                }
            }
            for (int n = data.nowAccessories.Count; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                if (data.nowAccessories[ACCpostion].type == 120) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        //ExpandedOutfit.Logger.LogWarning(ChaControl.fileParam.fullname + $" Deque>20");
                        data.nowAccessories[ACCpostion] = PartsQueue.Dequeue();
                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
                    }
                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    //ExpandedOutfit.Logger.LogWarning($"Coordinate Hair Force prev state:{info.ColorMatch}");
                    info.ColorMatch = true;
                }
            }
            bool print = true;
            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    print = false;
                    ExpandedOutfit.Logger.LogWarning(ChaControl.fileParam.fullname + $" Ran out of space for accessories, Making {PartsQueue.Count} space(s) at least (due to potential keys already existing just in case)");
                }
                if (!Temp.ContainsKey(ACCpostion))
                {
                    data.nowAccessories.Add(PartsQueue.Dequeue());
                    if (HairQueue.Peek() != null)
                    {
                        var HairTemp = HairQueue.Dequeue();
                        if (ExpandedOutfit.HairMatch.Value)
                        {
                            //ExpandedOutfit.Logger.LogWarning("Coordinate Hair Force");
                            HairTemp.ColorMatch = true;
                        }
                        Temp.Add(ACCpostion, HairTemp);
                    }
                    else
                    {
                        HairQueue.Dequeue();
                    }
                }
                else
                {
                    data.nowAccessories.Add(new ChaFileAccessory.PartsInfo());
                }
                data.infoAccessory.Add(null);
                data.objAccessory.Add(null);
                data.objAcsMove.Add(new GameObject[2]);
                data.cusAcsCmp.Add(null);
                data.showAccessories.Add(true);
                ACCpostion++;
            }

            //needed it to stop errors in FreeH when swapping
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
            Traverse.Create(_accessoriesByChar).Method("Purge").GetValue();
            #endregion
            Traverse.Create(MoreAccessories._self).Method("UpdateUI").GetValue();

            var Plugdata = new PluginData();

            if (ExpandedOutfit.HairMatch.Value)
            {
                Plugdata.data.Add("CoordinateHairAccessories", MessagePackSerializer.Serialize(Temp));
                ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.hairaccessorycustomizer", Plugdata);

                var HairAccessoryCustomizer = Type.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController, KK_HairAccessoryCustomizer", false);
                if (HairAccessoryCustomizer != null)
                {
                    //ExpandedOutfit.Logger.LogWarning("Coordinate Load: Hair Acc");
                    var temp = ChaControl.GetComponent(HairAccessoryCustomizer);
                    object[] HairInput = new object[2] { coordinate, false };
                    Traverse.Create(temp).Method("OnCoordinateBeingLoaded", HairInput).GetValue();
                }
            }
        }
    }
}