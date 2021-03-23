using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cosplay_Academy
{
    #region Stuff ME_RePack Needs
    //int outfitnum => ChaControl.fileStatus.coordinateType;
    public class ME_Support
    {
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
}