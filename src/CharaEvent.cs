using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
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
    public partial class CharaEvent : CharaCustomFunctionController
    {
        public static CharaEvent self;
        private bool Repeat_stoppper = false;
        private ChaDefault ThisOutfitData;

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
                ThisOutfitData.ChaControl = ChaControl;
                ThisOutfitData.Chafile = ChaFileControl;
            }
            self = this;
            if (GameMode.Maker == currentGameMode)
            {
                ThisOutfitData.Chafile = MakerAPI.LastLoadedChaFile;
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
                ExpandedOutfit.Logger.LogDebug("Reset passed");
                OutfitDecider.ResetDecider();
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} Started First Pass");
#endif
            if (ThisOutfitData.firstpass) //Save all accessories to avoid duplicating head accessories each load and be reuseable
            {
                ThisOutfitData.Clear();

                WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

                Dictionary<int, Dictionary<int, HairAccessoryInfo>> CharaHair = new Dictionary<int, Dictionary<int, HairAccessoryInfo>>();

                PluginData HairExtendedData;
                HairExtendedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.hairaccessorycustomizer");

                if (HairExtendedData != null && HairExtendedData.data.TryGetValue("HairAccessories", out var AllHairAccessories) && AllHairAccessories != null)
                    CharaHair = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, HairAccessoryInfo>>>((byte[])AllHairAccessories);
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
                else
                {
                    ExpandedOutfit.Logger.LogError($"material editor null");
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
                    List<HairAccessoryInfo> HairImport = new List<HairAccessoryInfo>();
                    if (CharaHair.TryGetValue(outfitnum, out Dictionary<int, HairAccessoryInfo> HairInfo) == false)
                    {
                        HairInfo = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
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
                        if (Constants.Inclusion.Contains(Intermediate[i].parentKey))
                        {
                            if (!HairInfo.TryGetValue(i, out HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }

                            var ColorList = MaterialColorPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var FloatList = MaterialFloatPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var ShaderList = MaterialShaderQueue[outfitnum].FindAll(x => x.Slot == i);
                            var TextureList = MaterialTexturePropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            var RenderList = RendererPropertyQueue[outfitnum].FindAll(x => x.Slot == i);
                            if (ColorList.Count == 0)
                            {
                                Color color = new Color(0, 0, 0);
                                ColorList.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, i, "", "", color, color));
                                //ColorList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("Color null");
                            }
                            if (FloatList.Count == 0)
                            {
                                FloatList.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, i, "", "", "", ""));
                                //FloatList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("FloatList null");
                            }
                            if (ShaderList.Count == 0)
                            {
                                ShaderList.Add(new MaterialShader(ObjectType.Unknown, outfitnum, i, "", 0, 0));
                                //ShaderList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("ShaderList null");
                            }
                            if (TextureList.Count == 0)
                            {
                                TextureList.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, i, "", ""));
                                //TextureList.Add(null);
                                //ExpandedOutfit.Logger.LogWarning("TextureList null");
                            }
                            if (RenderList.Count == 0)
                            {
                                RenderList.Add(new RendererProperty(ObjectType.Unknown, outfitnum, i, "", new RendererProperties(), "", ""));
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
                ThisOutfitData.TexturePrint();
#endif
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
                    //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} chano {ChaFileControl.loadProductNo} name {ChaFileControl.loadVersion} {ChaFileControl.facePngData}");
                    int HoldOutfit = ChaControl.fileStatus.coordinateType;
                    FullLoad();//Load outfits; has to run again for story mode les scene at least
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

                    ChaControl.SetAccessoryStateAll(true);

                    //Finish();
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
            for (int i = 0; i < ThisOutfitData.Chafile.coordinate.Length; i++)
            {
                SavedData = ExtendedSave.GetExtendedDataById(ChaFileControl.coordinate[i], "KCOX");
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
            var KoiOverlay = typeof(KoiClothesOverlayController);
            if (KoiOverlay != null)
            {
                //ExpandedOutfit.Logger.LogWarning("Coordinate Load: Hair Acc");
                var temp = ChaControl.GetComponent(KoiOverlay);
                object[] KoiInput = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
                Traverse.Create(temp).Method("OnReload", KoiInput).GetValue();
            }
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
            if (!ThisOutfitData.ME_Work)
            {
                for (int outfitnum = 0; outfitnum < ThisOutfitData.Chafile.coordinate.Length; outfitnum++)
                {
                    var data = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
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
            var ME_OverlayX = Type.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", false);
            if (ME_OverlayX != null)
            {
                UnityEngine.Component ME_Controller = ChaControl.gameObject.GetComponent(ME_OverlayX);
                //Traverse.Create(test).Method("RePack").GetValue();
                object[] OnReloadArray = new object[2] { KoikatuAPI.GetCurrentGameMode(), false };
                Traverse.Create(ME_Controller).Method("OnReload", OnReloadArray).GetValue();
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
            ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);

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
                ExtendedSave.SetExtendedDataById(ThisOutfitData.Chafile, IDtoSET, data);
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
        //private void Finish()
        //{

        //    Game _gamemgr = Game.Instance;
        //    List<SaveData.Heroine> Heroines = _gamemgr.HeroineList;
        //    if (ChaControl.name != null && ChaControl.sex == 1)
        //    {
        //        foreach (SaveData.Heroine Heroine in Heroines)
        //        {
        //            if (Heroine.chaCtrl != null && ChaControl.name == Heroine.chaCtrl.name)
        //            {
        //                if (ChaControl.name == Heroine.chaCtrl.name)
        //                {
        //                    heroine = Heroine;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    object[] Reload = new object[1] { KoikatuAPI.GetCurrentGameMode() };
        //    Repeat_stoppper = true;
        //    if (heroine != null && ChaControl.sex == 1)
        //    {
        //        if (ChaControl.name == heroine.chaCtrl.name)
        //        {
        //            // Update other instance to reflect the new ext data
        //            var other = heroine.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
        //            if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
        //            heroine = null;
        //            return;
        //        }
        //    }

        //    var player = ChaControl.GetPlayer();
        //    if (player != null)
        //    {
        //        if (ChaControl != player.chaCtrl)
        //        {
        //            // Update other instance to reflect the new ext data
        //            var other = player.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
        //            if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
        //        }
        //    }
        //    heroine = null;
        //}

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!ExpandedOutfit.AccKeeper.Value)
            {
                return;
            }//if disabled don't run

            #region Queue accessories to keep
            int outfitnum = ChaControl.fileStatus.coordinateType;

            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            var HairQueue = new Queue<CharaEvent.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

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
            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(ChaControl.nowCoordinate, "com.deathweasel.bepinex.materialeditor");
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
                        importDictionary[x.Key] = ME_Support.SetAndGetTextureID(x.Value);
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
            if (_accessoriesByChar.TryGetValue(ThisOutfitData.Chafile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ThisOutfitData.Chafile, data);
            }


            #region Reassign Exisiting Accessories

            //if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            //{
            //    NewRAW = new List<ChaFileAccessory.PartsInfo>();
            //}
            var Inputdata = ExtendedSave.GetExtendedDataById(ChaControl.nowCoordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            var Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);
            //Dictionary<int, int> importDictionary = new Dictionary<int, int>();
            int ACCpostion = 0;
            bool Empty;
            for (int n = ChaControl.nowCoordinate.accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = ChaControl.nowCoordinate.accessory.parts[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    ChaControl.nowCoordinate.accessory.parts[ACCpostion] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        Temp[ACCpostion] = HairQueue.Dequeue();
                    }
                    else
                    {
                        HairQueue.Dequeue();
                    }

                    if (RenderQueue.Count > 0 && RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = RenderQueue.Peek().Slot;
                        while (RenderQueue.Count > 0 && RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().Slot == slot)
                        {

                            RendererProperty ME_Info = RenderQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            Renderer.Add(ME_Info);
                        }
                    }
                    else
                    {
                        RenderQueue.Dequeue();
                    }
#if Debug
                    //ExpandedOutfit.Logger.LogWarning("Render Pass");
#endif

                    if (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = ColorQueue.Peek().Slot;
                        while (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().Slot == slot)
                        {
                            MaterialColorProperty ME_Info = ColorQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialColor.Add(ME_Info);
                        }
                    }
                    else
                    {
                        ColorQueue.Dequeue();
                    }
#if Debug
                    //ExpandedOutfit.Logger.LogWarning("Color Pass");
#endif

                    if (TextureQueue.Peek() != null && TextureQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                        //if (ME_Info.TexID != null)
                        //{
                        //    if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                        //    {
                        //        ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                        //    }
                        //}
                        ME_Info.Slot = ACCpostion;
                        MaterialTexture.Add(ME_Info);
                    }
                    else
                    {
                        TextureQueue.Dequeue();
                    }
#if Debug
                    //ExpandedOutfit.Logger.LogWarning("Texture Pass");
#endif

                    if (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = FloatQueue.Peek().Slot;
                        while (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().Slot == slot)
                        {
                            MaterialFloatProperty ME_Info = FloatQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialFloat.Add(ME_Info);
                        }
                    }
                    else
                    {
                        FloatQueue.Dequeue();
                    }
#if Debug
                    //ExpandedOutfit.Logger.LogWarning("Float Pass");
#endif
                    if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = ShaderQueue.Peek().Slot;
                        while (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().Slot == slot)
                        {
                            MaterialShader ME_Info = ShaderQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialShade.Add(ME_Info);
                        }
                    }
                    else
                    {
                        ShaderQueue.Dequeue();
                    }
#if Debug
                    //ExpandedOutfit.Logger.LogWarning("Shader Pass");
#endif
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
            ExpandedOutfit.Logger.LogWarning($"Start extra accessories at {ACCpostion} {NewRAW.Count}");
#endif
            for (int n = data.nowAccessories.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
            {
                Empty = data.nowAccessories[ACCpostion - 20].type == 120;
                if (Empty) //120 is empty/default
                {

                    data.nowAccessories[ACCpostion - 20] = PartsQueue.Dequeue();
                    if (HairQueue.Peek() != null && HairQueue.Peek().HairLength != -999)
                    {
                        Temp[ACCpostion] = HairQueue.Dequeue();
                    }
                    else
                    {
                        HairQueue.Dequeue();
                    }
                    if (RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int? slot = new int?(RenderQueue.Peek().Slot);
                        while (RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().Slot == slot)
                        {
                            RendererProperty ME_Info = RenderQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            Renderer.Add(ME_Info);
                        }
                    }
                    else
                    {
                        RenderQueue.Dequeue();
                    }

                    if (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = ColorQueue.Peek().Slot;
                        while (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().Slot == slot)
                        {
                            MaterialColorProperty ME_Info = ColorQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialColor.Add(ME_Info);
                        }
                    }
                    else
                    {
                        ColorQueue.Dequeue();
                    }

                    if (TextureQueue.Peek() != null && TextureQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                        //if (ME_Info.TexID != null)
                        //{
                        //    if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                        //    {
                        //        ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                        //    }
                        //}
                        ME_Info.Slot = ACCpostion;

                        ME_Info.Slot = ACCpostion;
                        MaterialTexture.Add(ME_Info);
                    }
                    else
                    {
                        TextureQueue.Dequeue();
                    }

                    if (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = FloatQueue.Peek().Slot;
                        while (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().Slot == slot)
                        {
                            MaterialFloatProperty ME_Info = FloatQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialFloat.Add(ME_Info);
                        }
                    }
                    else
                    {
                        FloatQueue.Dequeue();
                    }

                    if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().ObjectType != ObjectType.Unknown)
                    {
                        int slot = ShaderQueue.Peek().Slot;
                        while (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().Slot == slot)
                        {
                            MaterialShader ME_Info = ShaderQueue.Dequeue();
                            ME_Info.Slot = ACCpostion;
                            MaterialShade.Add(ME_Info);
                        }
                    }
                    else
                    {
                        ShaderQueue.Dequeue();
                    }

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
                data.nowAccessories.Add(PartsQueue.Dequeue());
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

                if (RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().ObjectType != ObjectType.Unknown)
                {
                    int slot = RenderQueue.Peek().Slot;
                    while (RenderQueue.Count > 0 && RenderQueue.Peek() != null && RenderQueue.Peek().Slot == slot)
                    {
                        RendererProperty ME_Info = RenderQueue.Dequeue();
                        ME_Info.Slot = ACCpostion;
                        Renderer.Add(ME_Info);
                    }
                }
                else
                {
                    RenderQueue.Dequeue();
                }
                if (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().ObjectType != ObjectType.Unknown)
                {
                    int slot = ColorQueue.Peek().Slot;
                    while (ColorQueue.Count > 0 && ColorQueue.Peek() != null && ColorQueue.Peek().Slot == slot)
                    {
                        MaterialColorProperty ME_Info = ColorQueue.Dequeue();
                        ME_Info.Slot = ACCpostion;
                        MaterialColor.Add(ME_Info);
                    }
                }
                else
                {
                    ColorQueue.Dequeue();
                }
                if (TextureQueue.Peek() != null && TextureQueue.Peek().ObjectType != ObjectType.Unknown)
                {
                    MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                    //if (ME_Info.TexID != null)
                    //{
                    //    if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                    //    {
                    //        ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                    //    }
                    //}
                    ME_Info.Slot = ACCpostion;
                    MaterialTexture.Add(ME_Info);
                }
                else
                {
                    TextureQueue.Dequeue();
                }
                if (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().ObjectType != ObjectType.Unknown)
                {
                    int slot = FloatQueue.Peek().Slot;
                    while (FloatQueue.Count > 0 && FloatQueue.Peek() != null && FloatQueue.Peek().Slot == slot)
                    {
                        MaterialFloatProperty ME_Info = FloatQueue.Dequeue();
                        ME_Info.Slot = ACCpostion;
                        MaterialFloat.Add(ME_Info);
                    }
                }
                else
                {
                    FloatQueue.Dequeue();
                }
                if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().ObjectType != ObjectType.Unknown)
                {
                    int slot = ShaderQueue.Peek().Slot;
                    while (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null && ShaderQueue.Peek().Slot == slot)
                    {
                        MaterialShader ME_Info = ShaderQueue.Dequeue();
                        ME_Info.Slot = ACCpostion;
                        MaterialShade.Add(ME_Info);
                    }
                }
                else
                {
                    ShaderQueue.Dequeue();
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
#if Debug
            //ExpandedOutfit.Logger.LogWarning("add range");
#endif

            //ThisOutfitData.ReturnMaterialColor.AddRange(MaterialColor);

            //ThisOutfitData.ReturnMaterialFloat.AddRange(MaterialFloat);

            //ThisOutfitData.ReturnMaterialShade.AddRange(MaterialShade);

            //ThisOutfitData.ReturnMaterialTexture.AddRange(MaterialTexture);

            //ThisOutfitData.ReturnRenderer.AddRange(Renderer);
#if Debug
            ExpandedOutfit.Logger.LogWarning("finish");
#endif

            #endregion

            Traverse.Create(MoreAccessories._self).Method("UpdateUI").GetValue();



            RendererPropertyList = Renderer;
            MaterialFloatPropertyList = MaterialFloat;
            MaterialColorPropertyList = MaterialColor;
            MaterialTexturePropertyList = MaterialTexture;
            MaterialShaderList = MaterialShade;

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

            ExtendedSave.SetExtendedDataById(coordinate, "com.deathweasel.bepinex.materialeditor", SaveData);


            #endregion



            var ME_OverlayX = Type.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", false);
            if (ME_OverlayX != null)
            {
                UnityEngine.Component ME_Controller = ChaControl.gameObject.GetComponent(ME_OverlayX);
                object[] OnReloadArray = new object[2] { coordinate, false };
                Traverse.Create(ME_Controller).Method("OnCoordinateBeingLoaded", OnReloadArray).GetValue();
            }

            if (ExpandedOutfit.HairMatch.Value)
            {
                var Plugdata = new PluginData();

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