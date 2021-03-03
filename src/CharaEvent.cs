using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KoiClothesOverlayX;
using Manager;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CoordinateType = ChaFileDefine.CoordinateType;

namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        bool Repeat_stoppper = false;
        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode || Repeat_stoppper/*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            {
                Repeat_stoppper = false;
                return;
            }//if disabled don't run
            if (GameMode.Maker == currentGameMode && ExpandedOutfit.ResetMaker.Value)
            {
                OutfitDecider.Reset = true;
                if (!ExpandedOutfit.PermReset.Value)
                {
                    ExpandedOutfit.ResetMaker.Value = false;
                }
            }
            //use Chacontrol.name instead of ChaControl.fileParam.fullname to probably avoid same name conflicts
            if (ChaControl.sex == 1 && (GameMode.Maker == currentGameMode || OutfitDecider.Reset || !OutfitDecider.ProcessedNames.Contains(ChaControl.name)))//run the following if female and unprocessed
            {
                if (currentGameMode == GameMode.MainGame || ExpandedOutfit.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    OutfitDecider.Decision(ChaControl.fileParam.fullname);//Generate outfits
                    OutfitDecider.ProcessedNames.Add(ChaControl.name);//character is processed
                    if (!ExpandedOutfit.PermChangeOutfit.Value)
                    {
                        ExpandedOutfit.ChangeOutfit.Value = false;
                    }
                }
                int HoldOutfit = ChaControl.fileStatus.coordinateType;
                ClothingLoader.FullLoad(ChaControl);//Load outfits
                ChaControl.fileStatus.coordinateType = HoldOutfit;
                ChaInfo temp = (ChaInfo)ChaControl;
                ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
            }

            if (GameMode.MainGame != currentGameMode)//stop any potential loops in maker since this isn't a maker mod
            {
                return;
            }
            object[] OnReloadArray = new object[2] { currentGameMode, false };
            //Reassign materials for Clothes
            var C_OverlayX = Type.GetType("KoiClothesOverlayX.KoiClothesOverlayController, KK_OverlayMods", false);
            if (C_OverlayX != null)
            {
                //UnityEngine.Component test = ChaControl.gameObject.GetComponent(C_OverlayX);
                KCOX_RePack();
                //Traverse.Create(test).Method("RePack").GetValue();
                //Traverse.Create(test).Method("OnReload", OnReloadArray).GetValue();
            }
            //Reassign materials for accessories
            var ME_OverlayX = Type.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController, KK_MaterialEditor", false);
            if (ME_OverlayX != null)
            {
                //UnityEngine.Component test = ChaControl.gameObject.GetComponent(ME_OverlayX);
                ME_RePack();
                //Traverse.Create(test).Method("RePack").GetValue();
                //Traverse.Create(test).Method("OnReload", OnReloadArray).GetValue();
            }
            //var HairAccessory = Type.GetType("KK_Plugins.HairAccessoryCustomizer, KK_HairAccessoryCustomizer", false);
            //if (HairAccessory != null)
            //{
            //    ExpandedOutfit.Logger.LogWarning("Entered Hair");
            //    //UnityEngine.Component test = ChaControl.gameObject.GetComponent(HairAccessory);
            //    HairAccessory_RePack();
            //    //Traverse.Create(test).Method("RePack").GetValue();
            //    //Traverse.Create(test).Method("OnReload", OnReloadArray).GetValue();
            //}
            Finish();
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



            #region UnPackCoordinates
            for (int outfitnum = 0; outfitnum < ChaControl.chaFile.coordinate.Length; outfitnum++)
            {
                int CurrentCoordinateIndex = outfitnum;
                var data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
                if (data?.data != null)
                {
                    var importDictionary = new Dictionary<int, int>();

                    if (data.data.TryGetValue(nameof(TextureDictionary), out var texDic) && texDic != null)
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionary[x.Key] = SetAndGetTextureID(x.Value);

                    if (data.data.TryGetValue(nameof(MaterialShaderList), out var materialShaders) && materialShaders != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])materialShaders);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialShaderList.Add(new MaterialShader(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }

                    if (data.data.TryGetValue(nameof(RendererPropertyList), out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            RendererPropertyList.Add(new RendererProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (data.data.TryGetValue(nameof(MaterialFloatPropertyList), out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialFloatPropertyList.Add(new MaterialFloatProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (data.data.TryGetValue(nameof(MaterialColorPropertyList), out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            MaterialColorPropertyList.Add(new MaterialColorProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (data.data.TryGetValue(nameof(MaterialTexturePropertyList), out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            {
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionary[(int)loadedProperty.TexID];

                                MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                MaterialTexturePropertyList.Add(newTextureProperty);
                            }
                        }
                    }
                }
            }
            #endregion
            #region Pack
            var SaveData = new PluginData();

            List<int> IDsToPurge = new List<int>();
            foreach (int texID in TextureDictionary.Keys)
                if (MaterialTexturePropertyList.All(x => x.TexID != texID))
                    IDsToPurge.Add(texID);

            for (var i = 0; i < IDsToPurge.Count; i++)
            {
                int texID = IDsToPurge[i];
                if (TextureDictionary.TryGetValue(texID, out var val)) val.Dispose();
                TextureDictionary.Remove(texID);
            }

            if (TextureDictionary.Count > 0)
                SaveData.data.Add(nameof(TextureDictionary), MessagePackSerializer.Serialize(TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
            else
                SaveData.data.Add(nameof(TextureDictionary), null);

            if (RendererPropertyList.Count > 0)
                SaveData.data.Add(nameof(RendererPropertyList), MessagePackSerializer.Serialize(RendererPropertyList));
            else
                SaveData.data.Add(nameof(RendererPropertyList), null);

            if (MaterialFloatPropertyList.Count > 0)
                SaveData.data.Add(nameof(MaterialFloatPropertyList), MessagePackSerializer.Serialize(MaterialFloatPropertyList));
            else
                SaveData.data.Add(nameof(MaterialFloatPropertyList), null);

            if (MaterialColorPropertyList.Count > 0)
                SaveData.data.Add(nameof(MaterialColorPropertyList), MessagePackSerializer.Serialize(MaterialColorPropertyList));
            else
                SaveData.data.Add(nameof(MaterialColorPropertyList), null);

            if (MaterialTexturePropertyList.Count > 0)
                SaveData.data.Add(nameof(MaterialTexturePropertyList), MessagePackSerializer.Serialize(MaterialTexturePropertyList));
            else
                SaveData.data.Add(nameof(MaterialTexturePropertyList), null);

            if (MaterialShaderList.Count > 0)
                SaveData.data.Add(nameof(MaterialShaderList), MessagePackSerializer.Serialize(MaterialShaderList));
            else
                SaveData.data.Add(nameof(MaterialShaderList), null);

            SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData);

            #endregion
        }
        #region Stuff ME_RePack Needs
        //int CurrentCoordinateIndex => ChaControl.fileStatus.coordinateType;
        public Dictionary<int, TextureContainer> TextureDictionary = new Dictionary<int, TextureContainer>();
        public int SetAndGetTextureID(byte[] textureBytes)
        {
            int highestID = 0;
            foreach (var tex in TextureDictionary)
                if (tex.Value.Data.SequenceEqual(textureBytes))
                    return tex.Key;
                else if (tex.Key > highestID)
                    highestID = tex.Key;

            highestID++;
            TextureDictionary.Add(highestID, new TextureContainer(textureBytes));
            return highestID;
        }
        public enum ObjectType
        {
            /// <summary>
            /// Unknown type, things should never be of this type
            /// </summary>
            Unknown,
            /// <summary>
            /// Clothing
            /// </summary>
            Clothing,
            /// <summary>
            /// Accessory
            /// </summary>
            Accessory,
            /// <summary>
            /// Hair
            /// </summary>
            Hair,
            /// <summary>
            /// Parts of a character
            /// </summary>
            Character
        };
        public sealed class TextureContainer : IDisposable
        {
            private byte[] _data;
            private Texture2D _texture;

            /// <summary>
            /// Load a byte array containing texture data.
            /// </summary>
            /// <param name="data"></param>
            public TextureContainer(byte[] data)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
            }

            /// <summary>
            /// Load the texture at the specified file path.
            /// </summary>
            /// <param name="filePath">Path of the file to load</param>
            public TextureContainer(string filePath)
            {
                var data = LoadTextureBytes(filePath);
                Data = data ?? throw new ArgumentNullException(nameof(data));
            }

            /// <summary>
            /// Byte array containing the texture data.
            /// </summary>
            public byte[] Data
            {
                get => _data;
                set
                {
                    Dispose();
                    _data = value;
                }
            }

            /// <summary>
            /// Texture data. Created from the Data byte array when accessed.
            /// </summary>
            public Texture2D Texture
            {
                get
                {
                    if (_texture == null)
                        if (_data != null)
                            _texture = TextureFromBytes(_data);

                    return _texture;
                }
            }

            /// <summary>
            /// Dispose of the texture data. Does not dispose of the byte array. Texture data will be recreated when accessing the Texture property, if needed.
            /// </summary>
            public void Dispose()
            {
                if (_texture != null)
                {
                    UnityEngine.Object.Destroy(_texture);
                    _texture = null;
                }
            }

            /// <summary>
            /// Read the specified file and return a byte array.
            /// </summary>
            /// <param name="filePath">Path of the file to load</param>
            /// <returns>Byte array with texture data</returns>
            private static byte[] LoadTextureBytes(string filePath)
            {
                return File.ReadAllBytes(filePath);
            }

            /// <summary>
            /// Convert a byte array to Texture2D.
            /// </summary>
            /// <param name="texBytes">Byte array containing the image</param>
            /// <param name="format">TextureFormat</param>
            /// <param name="mipmaps">Whether to generate mipmaps</param>
            /// <returns></returns>
            private static Texture2D TextureFromBytes(byte[] texBytes, TextureFormat format = TextureFormat.ARGB32, bool mipmaps = true)
            {
                if (texBytes == null || texBytes.Length == 0) return null;

                //LoadImage automatically resizes the texture so the texture size doesn't matter here
                var tex = new Texture2D(2, 2, format, mipmaps);

                tex.LoadImage(texBytes);
                return tex;
            }
        }
        [Serializable]
        [MessagePackObject]
        public class RendererProperty
        {
            /// <summary>
            /// Type of the object
            /// </summary>
            [Key("ObjectType")]
            public ObjectType ObjectType;
            /// <summary>
            /// Coordinate index, always 0 except in Koikatsu
            /// </summary>
            [Key("CoordinateIndex")]
            public int CoordinateIndex;
            /// <summary>
            /// Slot of the accessory, hair, or clothing
            /// </summary>
            [Key("Slot")]
            public int Slot;
            /// <summary>
            /// Name of the renderer
            /// </summary>
            [Key("RendererName")]
            public string RendererName;
            /// <summary>
            /// Property type
            /// </summary>
            [Key("Property")]
            public RendererProperties Property;
            /// <summary>
            /// Value
            /// </summary>
            [Key("Value")]
            public string Value;
            /// <summary>
            /// Original value
            /// </summary>
            [Key("ValueOriginal")]
            public string ValueOriginal;

            /// <summary>
            /// Data storage class for renderer properties
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="rendererName">Name of the renderer</param>
            /// <param name="property">Property type</param>
            /// <param name="value">Value</param>
            /// <param name="valueOriginal">Original</param>
            public RendererProperty(ObjectType objectType, int coordinateIndex, int slot, string rendererName, RendererProperties property, string value, string valueOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                RendererName = rendererName.Replace("(Instance)", "").Trim();
                Property = property;
                Value = value;
                ValueOriginal = valueOriginal;
            }
        }
        [Serializable]
        [MessagePackObject]
        public class MaterialFloatProperty
        {
            /// <summary>
            /// Type of the object
            /// </summary>
            [Key("ObjectType")]
            public ObjectType ObjectType;
            /// <summary>
            /// Coordinate index, always 0 except in Koikatsu
            /// </summary>
            [Key("CoordinateIndex")]
            public int CoordinateIndex;
            /// <summary>
            /// Slot of the accessory, hair, or clothing
            /// </summary>
            [Key("Slot")]
            public int Slot;
            /// <summary>
            /// Name of the material
            /// </summary>
            [Key("MaterialName")]
            public string MaterialName;
            /// <summary>
            /// Name of the property
            /// </summary>
            [Key("Property")]
            public string Property;
            /// <summary>
            /// Value
            /// </summary>
            [Key("Value")]
            public string Value;
            /// <summary>
            /// Original value
            /// </summary>
            [Key("ValueOriginal")]
            public string ValueOriginal;

            /// <summary>
            /// Data storage class for float properties
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="property">Name of the property</param>
            /// <param name="value">Value</param>
            /// <param name="valueOriginal">Original value</param>
            public MaterialFloatProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, string value, string valueOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                Property = property;
                Value = value;
                ValueOriginal = valueOriginal;
            }
        }
        [Serializable]
        [MessagePackObject]
        public class MaterialColorProperty
        {
            /// <summary>
            /// Type of the object
            /// </summary>
            [Key("ObjectType")]
            public ObjectType ObjectType;
            /// <summary>
            /// Coordinate index, always 0 except in Koikatsu
            /// </summary>
            [Key("CoordinateIndex")]
            public int CoordinateIndex;
            /// <summary>
            /// Slot of the accessory, hair, or clothing
            /// </summary>
            [Key("Slot")]
            public int Slot;
            /// <summary>
            /// Name of the material
            /// </summary>
            [Key("MaterialName")]
            public string MaterialName;
            /// <summary>
            /// Name of the property
            /// </summary>
            [Key("Property")]
            public string Property;
            /// <summary>
            /// Value
            /// </summary>
            [Key("Value")]
            public Color Value;
            /// <summary>
            /// Original value
            /// </summary>
            [Key("ValueOriginal")]
            public Color ValueOriginal;

            /// <summary>
            /// Data storage class for color properties
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="property">Name of the property</param>
            /// <param name="value">Value</param>
            /// <param name="valueOriginal">Original value</param>
            public MaterialColorProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, Color value, Color valueOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                Property = property;
                Value = value;
                ValueOriginal = valueOriginal;
            }
        }
        [Serializable]
        [MessagePackObject]
        public class MaterialTextureProperty
        {
            /// <summary>
            /// Type of the object
            /// </summary>
            [Key("ObjectType")]
            public ObjectType ObjectType;
            /// <summary>
            /// Coordinate index, always 0 except in Koikatsu
            /// </summary>
            [Key("CoordinateIndex")]
            public int CoordinateIndex;
            /// <summary>
            /// Slot of the accessory, hair, or clothing
            /// </summary>
            [Key("Slot")]
            public int Slot;
            /// <summary>
            /// Name of the material
            /// </summary>
            [Key("MaterialName")]
            public string MaterialName;
            /// <summary>
            /// Name of the property
            /// </summary>
            [Key("Property")]
            public string Property;
            /// <summary>
            /// ID of the texture as stored in the texture dictionary
            /// </summary>
            [Key("TexID")]
            public int? TexID;
            /// <summary>
            /// Texture offset value
            /// </summary>
            [Key("Offset")]
            public Vector2? Offset;
            /// <summary>
            /// Texture offset original value
            /// </summary>
            [Key("OffsetOriginal")]
            public Vector2? OffsetOriginal;
            /// <summary>
            /// Texture scale value
            /// </summary>
            [Key("Scale")]
            public Vector2? Scale;
            /// <summary>
            /// Texture scale original value
            /// </summary>
            [Key("ScaleOriginal")]
            public Vector2? ScaleOriginal;

            /// <summary>
            /// Data storage class for texture properties
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="property">Name of the property</param>
            /// <param name="texID">ID of the texture as stored in the texture dictionary</param>
            /// <param name="offset">Texture offset value</param>
            /// <param name="offsetOriginal">Texture offset original value</param>
            /// <param name="scale">Texture scale value</param>
            /// <param name="scaleOriginal">Texture scale original value</param>
            public MaterialTextureProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, int? texID = null, Vector2? offset = null, Vector2? offsetOriginal = null, Vector2? scale = null, Vector2? scaleOriginal = null)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                Property = property;
                TexID = texID;
                Offset = offset;
                OffsetOriginal = offsetOriginal;
                Scale = scale;
                ScaleOriginal = scaleOriginal;
            }

            /// <summary>
            /// Check if the TexID, Offset, and Scale are all null. Safe to remove this data if true.
            /// </summary>
            /// <returns></returns>
            public bool NullCheck() => TexID == null && Offset == null && Scale == null;
        }
        [Serializable]
        [MessagePackObject]
        public class MaterialShader
        {
            /// <summary>
            /// Type of the object
            /// </summary>
            [Key("ObjectType")]
            public ObjectType ObjectType;
            /// <summary>
            /// Coordinate index, always 0 except in Koikatsu
            /// </summary>
            [Key("CoordinateIndex")]
            public int CoordinateIndex;
            /// <summary>
            /// Slot of the accessory, hair, or clothing
            /// </summary>
            [Key("Slot")]
            public int Slot;
            /// <summary>
            /// Name of the material
            /// </summary>
            [Key("MaterialName")]
            public string MaterialName;
            /// <summary>
            /// Name of the shader
            /// </summary>
            [Key("ShaderName")]
            public string ShaderName;
            /// <summary>
            /// Name of the original shader
            /// </summary>
            [Key("ShaderNameOriginal")]
            public string ShaderNameOriginal;
            /// <summary>
            /// Render queue
            /// </summary>
            [Key("RenderQueue")]
            public int? RenderQueue;
            /// <summary>
            /// Original render queue
            /// </summary>
            [Key("RenderQueueOriginal")]
            public int? RenderQueueOriginal;

            /// <summary>
            /// Data storage class for shader data
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="shaderName">Name of the shader</param>
            /// <param name="shaderNameOriginal">Name of the original shader</param>
            /// <param name="renderQueue">Render queue</param>
            /// <param name="renderQueueOriginal">Original render queue</param>
            public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, string shaderName, string shaderNameOriginal, int? renderQueue, int? renderQueueOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                ShaderName = shaderName;
                ShaderNameOriginal = shaderNameOriginal;
                RenderQueue = renderQueue;
                RenderQueueOriginal = renderQueueOriginal;
            }
            /// <summary>
            /// Data storage class for shader data
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="shaderName">Name of the shader</param>
            /// <param name="shaderNameOriginal">Name of the original shader</param>
            public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, string shaderName, string shaderNameOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                ShaderName = shaderName;
                ShaderNameOriginal = shaderNameOriginal;
            }
            /// <summary>
            /// Data storage class for shader data
            /// </summary>
            /// <param name="objectType">Type of the object</param>
            /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
            /// <param name="slot">Slot of the accessory, hair, or clothing</param>
            /// <param name="materialName">Name of the material</param>
            /// <param name="renderQueue">Render queue</param>
            /// <param name="renderQueueOriginal">Original render queue</param>
            public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, int? renderQueue, int? renderQueueOriginal)
            {
                ObjectType = objectType;
                CoordinateIndex = coordinateIndex;
                Slot = slot;
                MaterialName = materialName.Replace("(Instance)", "").Trim();
                RenderQueue = renderQueue;
                RenderQueueOriginal = renderQueueOriginal;
            }

            /// <summary>
            /// Check if the shader name and render queue are both null. Safe to delete this data if true.
            /// </summary>
            /// <returns></returns>
            public bool NullCheck() => ShaderName.IsNullOrEmpty() && RenderQueue == null;
        }
        public enum RendererProperties
        {
            /// <summary>
            /// Whether the renderer is enabled
            /// </summary>
            Enabled,
            /// <summary>
            /// ShadowCastingMode of the renderer
            /// </summary>
            ShadowCastingMode,
            /// <summary>
            /// Whether the renderer will receive shadows cast by other objects
            /// </summary>
            ReceiveShadows
        }

        #endregion

        public void HairAccessory_RePack()
        {
            Dictionary<int, object> PluginData = new Dictionary<int, object>();
            Dictionary<int, Dictionary<int, HairAccessoryInfo>> HairAccessories = new Dictionary<int, Dictionary<int, HairAccessoryInfo>>();
            Dictionary<int, HairAccessoryInfo> Temp;
            for (int i = 0; i < ChaControl.chaFile.coordinate.Length; i++)
            {
                PluginData plugin = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.hairaccessorycustomizer");
                if (plugin != null && plugin.data != null)
                {
                    ExpandedOutfit.Logger.LogWarning($"Hair is not null\t{plugin.data}");

                    if (plugin.data.TryGetValue("HairAccessories", out var loadedHairAccessories2) && loadedHairAccessories2 != null)
                        PluginData = MessagePackSerializer.Deserialize<Dictionary<int, object>>((byte[])loadedHairAccessories2);
                    else
                    {
                        ExpandedOutfit.Logger.LogWarning($"Hair is supposedly empty");

                    }
                    for (int j = 0; j < PluginData.Count; j++)
                    {
                        ExpandedOutfit.Logger.LogWarning($"Coordinate {i}: {PluginData.ElementAt(j).Key}\t\t\t{PluginData.ElementAt(j).Value}");
                    }
                }
                else
                {
                    ExpandedOutfit.Logger.LogWarning($"Hair is null");

                }



                var Inputdata = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "com.deathweasel.bepinex.hairaccessorycustomizer");
                Temp = new Dictionary<int, HairAccessoryInfo>();
                if (Inputdata != null)
                    if (Inputdata.data.TryGetValue("HairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                        Temp = MessagePackSerializer.Deserialize<Dictionary<int, HairAccessoryInfo>>((byte[])loadedHairAccessories);
                for (int j = 0; j < Temp.Count; j++)
                {
                    ExpandedOutfit.Logger.LogWarning($"Coordinate {i}: {Temp.ElementAt(j).Key}\t\t\t{Temp.ElementAt(j).Value}");
                }
                HairAccessories.Add(i, Temp);
            }
            var data = new PluginData();
            data.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", data);
        }

        #region Stuff Hair Accessories needs
        [Serializable]
        [MessagePackObject]
        private class HairAccessoryInfo
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

        private void SetExtendedData(string IDtoSET, PluginData data)
        {
            ExtendedSave.SetExtendedDataById(ChaFileControl, IDtoSET, data);

            object[] Reload = new object[1] { KoikatuAPI.GetCurrentGameMode() };

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
            if (heroine != null && ChaControl.sex == 0)
            {
                ExtendedSave.SetExtendedDataById(ChaFileControl, IDtoSET, data);
                ExtendedSave.SetExtendedDataById(heroine.charFile, IDtoSET, data);
                if (ChaControl.name == heroine.chaCtrl.name)
                {
                    ExtendedSave.SetExtendedDataById(heroine.chaCtrl.chaFile, IDtoSET, data);
                    return;
                }

                //var npc = heroine.GetNPC();
                //if (npc != null && npc.chaCtrl != null && npc.chaCtrl.name == ChaControl.name)
                //{
                //    ExpandedOutfit.Logger.LogWarning("is npc");

                //    ExtendedSave.SetExtendedDataById(npc.chaCtrl.chaFile, IDtoSET, data);
                //    ExpandedOutfit.Logger.LogWarning("NPC control matches");

                //    // Update other instance to reflect the new ext data
                //    var other = npc.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                //    if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
                //    return;
                //}
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
            if (heroine != null && ChaControl.sex == 0)
            {
                if (ChaControl.name == heroine.chaCtrl.name)
                {
                    // Update other instance to reflect the new ext data
                    var other = heroine.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) Traverse.Create(other).Method("OnReloadInternal", Reload).GetValue();
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
        }
    }
}