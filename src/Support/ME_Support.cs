//using ExtensibleSaveFormat;
//using KKAPI;
//using KKAPI.Chara;
//using KKAPI.Maker;
//using MaterialEditorAPI;
//using MessagePack;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using UniRx;
//using UnityEngine;

//namespace Cosplay_Academy.Support
//{
//    public void ME_RePack(UnityEngine.Component MEController)
//    {
//        List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
//        List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
//        List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
//        List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
//        List<MaterialShader> MaterialShaderList = new List<MaterialShader>();



//        #region UnPackCoordinates
//        for (int outfitnum = 0; outfitnum < ChaControl.chaFile.coordinate.Length; outfitnum++)
//        {
//            var data = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
//            if (data?.data != null)
//            {
//                var importDictionary = new Dictionary<int, int>();

//                if (data.data.TryGetValue(nameof(TextureDictionary), out var texDic) && texDic != null)
//                    foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
//                        importDictionary[x.Key] = SetAndGetTextureID(x.Value);

//                if (data.data.TryGetValue(nameof(MaterialShaderList), out var materialShaders) && materialShaders != null)
//                {
//                    var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])materialShaders);
//                    for (var i = 0; i < properties.Count; i++)
//                    {
//                        var loadedProperty = properties[i];
//                        //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
//                        MaterialShaderList.Add(new MaterialShader(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
//                    }
//                }

//                if (data.data.TryGetValue(nameof(RendererPropertyList), out var rendererProperties) && rendererProperties != null)
//                {
//                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
//                    for (var i = 0; i < properties.Count; i++)
//                    {
//                        var loadedProperty = properties[i];
//                        //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
//                        RendererPropertyList.Add(new RendererProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
//                    }
//                }

//                if (data.data.TryGetValue(nameof(MaterialFloatPropertyList), out var materialFloatProperties) && materialFloatProperties != null)
//                {
//                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
//                    for (var i = 0; i < properties.Count; i++)
//                    {
//                        var loadedProperty = properties[i];
//                        //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
//                        MaterialFloatPropertyList.Add(new MaterialFloatProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
//                    }
//                }

//                if (data.data.TryGetValue(nameof(MaterialColorPropertyList), out var materialColorProperties) && materialColorProperties != null)
//                {
//                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
//                    for (var i = 0; i < properties.Count; i++)
//                    {
//                        var loadedProperty = properties[i];
//                        //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
//                        MaterialColorPropertyList.Add(new MaterialColorProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
//                    }
//                }

//                if (data.data.TryGetValue(nameof(MaterialTexturePropertyList), out var materialTextureProperties) && materialTextureProperties != null)
//                {
//                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
//                    for (var i = 0; i < properties.Count; i++)
//                    {
//                        var loadedProperty = properties[i];
//                        //if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
//                        {
//                            int? texID = null;
//                            if (loadedProperty.TexID != null)
//                                texID = importDictionary[(int)loadedProperty.TexID];

//                            MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, CurrentCoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
//                            MaterialTexturePropertyList.Add(newTextureProperty);
//                        }
//                    }
//                }
//            }
//        }
//        #endregion
//        #region Pack
//        var SaveData = new PluginData();

//        List<int> IDsToPurge = new List<int>();
//        foreach (int texID in TextureDictionary.Keys)
//            if (MaterialTexturePropertyList.All(x => x.TexID != texID))
//                IDsToPurge.Add(texID);

//        for (var i = 0; i < IDsToPurge.Count; i++)
//        {
//            int texID = IDsToPurge[i];
//            if (TextureDictionary.TryGetValue(texID, out var val)) val.Dispose();
//            TextureDictionary.Remove(texID);
//        }

//        if (TextureDictionary.Count > 0)
//            SaveData.data.Add(nameof(TextureDictionary), MessagePackSerializer.Serialize(TextureDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.Data)));
//        else
//            SaveData.data.Add(nameof(TextureDictionary), null);

//        if (RendererPropertyList.Count > 0)
//            SaveData.data.Add(nameof(RendererPropertyList), MessagePackSerializer.Serialize(RendererPropertyList));
//        else
//            SaveData.data.Add(nameof(RendererPropertyList), null);

//        if (MaterialFloatPropertyList.Count > 0)
//            SaveData.data.Add(nameof(MaterialFloatPropertyList), MessagePackSerializer.Serialize(MaterialFloatPropertyList));
//        else
//            SaveData.data.Add(nameof(MaterialFloatPropertyList), null);

//        if (MaterialColorPropertyList.Count > 0)
//            SaveData.data.Add(nameof(MaterialColorPropertyList), MessagePackSerializer.Serialize(MaterialColorPropertyList));
//        else
//            SaveData.data.Add(nameof(MaterialColorPropertyList), null);

//        if (MaterialTexturePropertyList.Count > 0)
//            SaveData.data.Add(nameof(MaterialTexturePropertyList), MessagePackSerializer.Serialize(MaterialTexturePropertyList));
//        else
//            SaveData.data.Add(nameof(MaterialTexturePropertyList), null);

//        if (MaterialShaderList.Count > 0)
//            SaveData.data.Add(nameof(MaterialShaderList), MessagePackSerializer.Serialize(MaterialShaderList));
//        else
//            SaveData.data.Add(nameof(MaterialShaderList), null);

//        SetExtendedData("com.deathweasel.bepinex.materialeditor", SaveData);

//        #endregion
//    }
//    #region Stuff ME_RePack Needs
//    int CurrentCoordinateIndex => ChaControl.fileStatus.coordinateType;
//    public Dictionary<int, TextureContainer> TextureDictionary = new Dictionary<int, TextureContainer>();
//    public int SetAndGetTextureID(byte[] textureBytes)
//    {
//        int highestID = 0;
//        foreach (var tex in TextureDictionary)
//            if (tex.Value.Data.SequenceEqual(textureBytes))
//                return tex.Key;
//            else if (tex.Key > highestID)
//                highestID = tex.Key;

//        highestID++;
//        TextureDictionary.Add(highestID, new TextureContainer(textureBytes));
//        return highestID;
//    }
//    public enum ObjectType
//    {
//        /// <summary>
//        /// Unknown type, things should never be of this type
//        /// </summary>
//        Unknown,
//        /// <summary>
//        /// Clothing
//        /// </summary>
//        Clothing,
//        /// <summary>
//        /// Accessory
//        /// </summary>
//        Accessory,
//        /// <summary>
//        /// Hair
//        /// </summary>
//        Hair,
//        /// <summary>
//        /// Parts of a character
//        /// </summary>
//        Character
//    };
//    public sealed class TextureContainer : IDisposable
//    {
//        private byte[] _data;
//        private Texture2D _texture;

//        /// <summary>
//        /// Load a byte array containing texture data.
//        /// </summary>
//        /// <param name="data"></param>
//        public TextureContainer(byte[] data)
//        {
//            Data = data ?? throw new ArgumentNullException(nameof(data));
//        }

//        /// <summary>
//        /// Load the texture at the specified file path.
//        /// </summary>
//        /// <param name="filePath">Path of the file to load</param>
//        public TextureContainer(string filePath)
//        {
//            var data = LoadTextureBytes(filePath);
//            Data = data ?? throw new ArgumentNullException(nameof(data));
//        }

//        /// <summary>
//        /// Byte array containing the texture data.
//        /// </summary>
//        public byte[] Data
//        {
//            get => _data;
//            set
//            {
//                Dispose();
//                _data = value;
//            }
//        }

//        /// <summary>
//        /// Texture data. Created from the Data byte array when accessed.
//        /// </summary>
//        public Texture2D Texture
//        {
//            get
//            {
//                if (_texture == null)
//                    if (_data != null)
//                        _texture = TextureFromBytes(_data);

//                return _texture;
//            }
//        }

//        /// <summary>
//        /// Dispose of the texture data. Does not dispose of the byte array. Texture data will be recreated when accessing the Texture property, if needed.
//        /// </summary>
//        public void Dispose()
//        {
//            if (_texture != null)
//            {
//                UnityEngine.Object.Destroy(_texture);
//                _texture = null;
//            }
//        }

//        /// <summary>
//        /// Read the specified file and return a byte array.
//        /// </summary>
//        /// <param name="filePath">Path of the file to load</param>
//        /// <returns>Byte array with texture data</returns>
//        private static byte[] LoadTextureBytes(string filePath)
//        {
//            return File.ReadAllBytes(filePath);
//        }

//        /// <summary>
//        /// Convert a byte array to Texture2D.
//        /// </summary>
//        /// <param name="texBytes">Byte array containing the image</param>
//        /// <param name="format">TextureFormat</param>
//        /// <param name="mipmaps">Whether to generate mipmaps</param>
//        /// <returns></returns>
//        private static Texture2D TextureFromBytes(byte[] texBytes, TextureFormat format = TextureFormat.ARGB32, bool mipmaps = true)
//        {
//            if (texBytes == null || texBytes.Length == 0) return null;

//            //LoadImage automatically resizes the texture so the texture size doesn't matter here
//            var tex = new Texture2D(2, 2, format, mipmaps);

//            tex.LoadImage(texBytes);
//            return tex;
//        }
//    }
//    public class RendererProperty
//    {
//        /// <summary>
//        /// Type of the object
//        /// </summary>
//        [Key("ObjectType")]
//        public ObjectType ObjectType;
//        /// <summary>
//        /// Coordinate index, always 0 except in Koikatsu
//        /// </summary>
//        [Key("CoordinateIndex")]
//        public int CoordinateIndex;
//        /// <summary>
//        /// Slot of the accessory, hair, or clothing
//        /// </summary>
//        [Key("Slot")]
//        public int Slot;
//        /// <summary>
//        /// Name of the renderer
//        /// </summary>
//        [Key("RendererName")]
//        public string RendererName;
//        /// <summary>
//        /// Property type
//        /// </summary>
//        [Key("Property")]
//        public RendererProperties Property;
//        /// <summary>
//        /// Value
//        /// </summary>
//        [Key("Value")]
//        public string Value;
//        /// <summary>
//        /// Original value
//        /// </summary>
//        [Key("ValueOriginal")]
//        public string ValueOriginal;

//        public enum ShaderPropertyType
//        {
//            /// <summary>
//            /// Texture
//            /// </summary>
//            Texture,
//            /// <summary>
//            /// Color, Vector4, Vector3, Vector2
//            /// </summary>
//            Color,
//            /// <summary>
//            /// Float, Int, Bool
//            /// </summary>
//            Float
//        }
//        public enum RendererProperties
//        {
//            /// <summary>
//            /// Whether the renderer is enabled
//            /// </summary>
//            Enabled,
//            /// <summary>
//            /// ShadowCastingMode of the renderer
//            /// </summary>
//            ShadowCastingMode,
//            /// <summary>
//            /// Whether the renderer will receive shadows cast by other objects
//            /// </summary>
//            ReceiveShadows
//        }
//        /// <summary>
//        /// Data storage class for renderer properties
//        /// </summary>
//        /// <param name="objectType">Type of the object</param>
//        /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
//        /// <param name="slot">Slot of the accessory, hair, or clothing</param>
//        /// <param name="rendererName">Name of the renderer</param>
//        /// <param name="property">Property type</param>
//        /// <param name="value">Value</param>
//        /// <param name="valueOriginal">Original</param>
//        public RendererProperty(ObjectType objectType, int coordinateIndex, int slot, string rendererName, RendererProperties property, string value, string valueOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            RendererName = rendererName.Replace("(Instance)", "").Trim();
//            Property = property;
//            Value = value;
//            ValueOriginal = valueOriginal;
//        }
//    }
//    public class MaterialFloatProperty
//    {
//        /// <summary>
//        /// Type of the object
//        /// </summary>
//        [Key("ObjectType")]
//        public ObjectType ObjectType;
//        /// <summary>
//        /// Coordinate index, always 0 except in Koikatsu
//        /// </summary>
//        [Key("CoordinateIndex")]
//        public int CoordinateIndex;
//        /// <summary>
//        /// Slot of the accessory, hair, or clothing
//        /// </summary>
//        [Key("Slot")]
//        public int Slot;
//        /// <summary>
//        /// Name of the material
//        /// </summary>
//        [Key("MaterialName")]
//        public string MaterialName;
//        /// <summary>
//        /// Name of the property
//        /// </summary>
//        [Key("Property")]
//        public string Property;
//        /// <summary>
//        /// Value
//        /// </summary>
//        [Key("Value")]
//        public string Value;
//        /// <summary>
//        /// Original value
//        /// </summary>
//        [Key("ValueOriginal")]
//        public string ValueOriginal;

//        /// <summary>
//        /// Data storage class for float properties
//        /// </summary>
//        /// <param name="objectType">Type of the object</param>
//        /// <param name="coordinateIndex">Coordinate index, always 0 except in Koikatsu</param>
//        /// <param name="slot">Slot of the accessory, hair, or clothing</param>
//        /// <param name="materialName">Name of the material</param>
//        /// <param name="property">Name of the property</param>
//        /// <param name="value">Value</param>
//        /// <param name="valueOriginal">Original value</param>
//        public MaterialFloatProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, string value, string valueOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            Property = property;
//            Value = value;
//            ValueOriginal = valueOriginal;
//        }
//    }
//    public class MaterialColorProperty
//    {
//        [Key("ObjectType")]
//        public ObjectType ObjectType;
//        [Key("CoordinateIndex")]
//        public int CoordinateIndex;
//        [Key("Slot")]
//        public int Slot;
//        [Key("MaterialName")]
//        public string MaterialName;
//        [Key("Property")]
//        public string Property;
//        [Key("Value")]
//        public Color Value;
//        [Key("ValueOriginal")]
//        public Color ValueOriginal;

//        public MaterialColorProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, Color value, Color valueOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            Property = property;
//            Value = value;
//            ValueOriginal = valueOriginal;
//        }
//    }
//    public class MaterialTextureProperty
//    {
//        [Key("ObjectType")]
//        public ObjectType ObjectType;
//        [Key("CoordinateIndex")]
//        public int CoordinateIndex;
//        [Key("Slot")]
//        public int Slot;
//        [Key("MaterialName")]
//        public string MaterialName;
//        [Key("Property")]
//        public string Property;
//        [Key("TexID")]
//        public int? TexID;
//        [Key("Offset")]
//        public Vector2? Offset;
//        [Key("OffsetOriginal")]
//        public Vector2? OffsetOriginal;
//        [Key("Scale")]
//        public Vector2? Scale;
//        [Key("ScaleOriginal")]
//        public Vector2? ScaleOriginal;

//        public MaterialTextureProperty(ObjectType objectType, int coordinateIndex, int slot, string materialName, string property, int? texID = null, Vector2? offset = null, Vector2? offsetOriginal = null, Vector2? scale = null, Vector2? scaleOriginal = null)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            Property = property;
//            TexID = texID;
//            Offset = offset;
//            OffsetOriginal = offsetOriginal;
//            Scale = scale;
//            ScaleOriginal = scaleOriginal;
//        }

//        public bool NullCheck() => TexID == null && Offset == null && Scale == null;
//    }
//    public class MaterialShader
//    {
//        [Key("ObjectType")]
//        public ObjectType ObjectType;
//        [Key("CoordinateIndex")]
//        public int CoordinateIndex;
//        [Key("Slot")]
//        public int Slot;
//        [Key("MaterialName")]
//        public string MaterialName;
//        [Key("ShaderName")]
//        public string ShaderName;
//        [Key("ShaderNameOriginal")]
//        public string ShaderNameOriginal;
//        [Key("RenderQueue")]
//        public int? RenderQueue;
//        [Key("RenderQueueOriginal")]
//        public int? RenderQueueOriginal;

//        public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, string shaderName, string shaderNameOriginal, int? renderQueue, int? renderQueueOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            ShaderName = shaderName;
//            ShaderNameOriginal = shaderNameOriginal;
//            RenderQueue = renderQueue;
//            RenderQueueOriginal = renderQueueOriginal;
//        }
//        public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, string shaderName, string shaderNameOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            ShaderName = shaderName;
//            ShaderNameOriginal = shaderNameOriginal;
//        }
//        public MaterialShader(ObjectType objectType, int coordinateIndex, int slot, string materialName, int? renderQueue, int? renderQueueOriginal)
//        {
//            ObjectType = objectType;
//            CoordinateIndex = coordinateIndex;
//            Slot = slot;
//            MaterialName = materialName.Replace("(Instance)", "").Trim();
//            RenderQueue = renderQueue;
//            RenderQueueOriginal = renderQueueOriginal;
//        }
//        public bool NullCheck() => ShaderName.IsNullOrEmpty() && RenderQueue == null;

//    }
//    #endregion


//}
