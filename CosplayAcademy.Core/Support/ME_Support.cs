using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cosplay_Academy.ME
{

    public class MaterialEditorProperties
    {
        public List<RendererProperty> RendererProperty = new List<RendererProperty>();
        public List<MaterialFloatProperty> MaterialFloatProperty = new List<MaterialFloatProperty>();
        public List<MaterialColorProperty> MaterialColorProperty = new List<MaterialColorProperty>();
        public List<MaterialTextureProperty> MaterialTextureProperty = new List<MaterialTextureProperty>();
        public List<MaterialShader> MaterialShader = new List<MaterialShader>();

        public MaterialEditorProperties() { }

        public MaterialEditorProperties(List<RendererProperty> renderer, List<MaterialFloatProperty> floatprob, List<MaterialColorProperty> color, List<MaterialTextureProperty> texture, List<MaterialShader> shader)
        {
            RendererProperty = renderer;
            MaterialColorProperty = color;
            MaterialFloatProperty = floatprob;
            MaterialTextureProperty = texture;
            MaterialShader = shader;
        }

        public void Clear()
        {
            MaterialShader.Clear();
            RendererProperty.Clear();
            MaterialColorProperty.Clear();
            MaterialFloatProperty.Clear();
            MaterialTextureProperty.Clear();
        }

        public void Queue(out Queue<RendererProperty> rendererProperties, out Queue<MaterialFloatProperty> materialFloatProperties, out Queue<MaterialColorProperty> materialColorProperties, out Queue<MaterialShader> materialShaders, out Queue<MaterialTextureProperty> materialTextureProperties)
        {
            rendererProperties = new Queue<RendererProperty>(RendererProperty);
            materialColorProperties = new Queue<MaterialColorProperty>(MaterialColorProperty);
            materialFloatProperties = new Queue<MaterialFloatProperty>(MaterialFloatProperty);
            materialShaders = new Queue<MaterialShader>(MaterialShader);
            materialTextureProperties = new Queue<MaterialTextureProperty>(MaterialTextureProperty);
        }
    }

    public class ME_List
    {
        public Dictionary<int, ME_Coordinate> Coordinates = new Dictionary<int, ME_Coordinate>();

        //Just need empty Lists
        public ME_List(int size)
        {
            for (var i = 0; i < size; i++)
            {
                Coordinates[i] = new ME_Coordinate();
            }
        }

        //Copy
        public ME_List(ME_List Original)
        {
            Coordinates = new Dictionary<int, ME_Coordinate>(Original.Coordinates);
        }

        //Full Chafile Accessory Load
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData)
        {
            for (var i = 0; i < ThisOutfitData.Outfit_Size; i++)
            {
                Coordinates[i] = new ME_Coordinate();
            }
            if (pluginData != null)
            {
                if (pluginData.version == 0)
                {
                    var importDictionaryList = new Dictionary<int, int>();

                    if (pluginData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }

                    if (pluginData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            var coord = Coordinates[loadedProperty.CoordinateIndex];
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            var coord = Coordinates[loadedProperty.CoordinateIndex];
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            var coord = Coordinates[loadedProperty.CoordinateIndex];
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();

                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            var coord = Coordinates[loadedProperty.CoordinateIndex];
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();

                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];
                            var slot = loadedProperty.Slot;
                            var coord = Coordinates[loadedProperty.CoordinateIndex];
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = coord.CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }

                    return;
                }
                else
                {
                    ClothingLoader.OutdatedMessage("Material Editor", true);
                }
            }
        }

        //Add Data to Return (Generalized Load)
        public void LoadCoordinate(PluginData pluginData, ChaDefault ThisOutfitData, int outfitnum)
        {
            if (!Coordinates.TryGetValue(outfitnum, out var coord))
            {
                Coordinates[outfitnum] = coord = new ME_Coordinate();
            }
            coord.SoftClear();
            if (pluginData != null)
            {
                if (pluginData.version == 0)
                {
                    var importDictionaryList = new Dictionary<int, int>();

                    if (pluginData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }

                    if (pluginData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.ClothingProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.AccessoryProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.HairProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.CharacterProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.ClothingProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.AccessoryProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.HairProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.CharacterProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.ClothingProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.AccessoryProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.HairProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.CharacterProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.ClothingProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.AccessoryProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.HairProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        coord.CharacterProperties[slot] = editorProperties = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        properties = properties.Where(x => x.CoordinateIndex < ThisOutfitData.Outfit_Size).ToList();
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            if (loadedProperty.ObjectType != ObjectType.Unknown)
                            {
                                int? texID = null;
                                if (loadedProperty.TexID != null)
                                    texID = importDictionaryList[(int)loadedProperty.TexID];
                                var slot = loadedProperty.Slot;
                                MaterialEditorProperties editorProperties;
                                switch (loadedProperty.ObjectType)
                                {
                                    case ObjectType.Clothing:
                                        if (!coord.ClothingProperties.TryGetValue(slot, out editorProperties))
                                        {
                                            coord.ClothingProperties[slot] = editorProperties = new MaterialEditorProperties();
                                        }
                                        break;
                                    case ObjectType.Accessory:
                                        if (!coord.AccessoryProperties.TryGetValue(slot, out editorProperties))
                                        {
                                            coord.AccessoryProperties[slot] = editorProperties = new MaterialEditorProperties();
                                        }
                                        break;
                                    case ObjectType.Hair:
                                        if (!coord.HairProperties.TryGetValue(slot, out editorProperties))
                                        {
                                            coord.HairProperties[slot] = editorProperties = new MaterialEditorProperties();
                                        }
                                        break;
                                    case ObjectType.Character:
                                        if (!coord.CharacterProperties.TryGetValue(slot, out editorProperties))
                                        {
                                            coord.CharacterProperties[slot] = editorProperties = new MaterialEditorProperties();
                                        }
                                        break;
                                    default:
                                        continue;
                                }

                                var newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                                editorProperties.MaterialTextureProperty.Add(newTextureProperty);
                            }
                        }
                    }
                }
                else
                {
                    ClothingLoader.OutdatedMessage("Material Editor", true);
                }
            }
        }

        public void Clear()
        {
            Coordinates.Clear();
        }

        public void AllProperties(out List<RendererProperty> rendererProperties, out List<MaterialFloatProperty> materialFloatProperties, out List<MaterialColorProperty> materialColorProperties, out List<MaterialShader> materialShaders, out List<MaterialTextureProperty> materialTextureProperties)
        {
            rendererProperties = new List<RendererProperty>();
            materialFloatProperties = new List<MaterialFloatProperty>();
            materialColorProperties = new List<MaterialColorProperty>();
            materialShaders = new List<MaterialShader>();
            materialTextureProperties = new List<MaterialTextureProperty>();

            foreach (var outfit in Coordinates.Values)
            {
                foreach (var slot in outfit.AccessoryProperties.Values)
                {
                    rendererProperties.AddRange(slot.RendererProperty);
                    materialFloatProperties.AddRange(slot.MaterialFloatProperty);
                    materialColorProperties.AddRange(slot.MaterialColorProperty);
                    materialShaders.AddRange(slot.MaterialShader);
                    materialTextureProperties.AddRange(slot.MaterialTextureProperty);
                }
                foreach (var slot in outfit.ClothingProperties.Values)
                {
                    rendererProperties.AddRange(slot.RendererProperty);
                    materialFloatProperties.AddRange(slot.MaterialFloatProperty);
                    materialColorProperties.AddRange(slot.MaterialColorProperty);
                    materialShaders.AddRange(slot.MaterialShader);
                    materialTextureProperties.AddRange(slot.MaterialTextureProperty);
                }
                foreach (var slot in outfit.HairProperties.Values)
                {
                    rendererProperties.AddRange(slot.RendererProperty);
                    materialFloatProperties.AddRange(slot.MaterialFloatProperty);
                    materialColorProperties.AddRange(slot.MaterialColorProperty);
                    materialShaders.AddRange(slot.MaterialShader);
                    materialTextureProperties.AddRange(slot.MaterialTextureProperty);
                }
                foreach (var slot in outfit.CharacterProperties.Values)
                {
                    rendererProperties.AddRange(slot.RendererProperty);
                    materialFloatProperties.AddRange(slot.MaterialFloatProperty);
                    materialColorProperties.AddRange(slot.MaterialColorProperty);
                    materialShaders.AddRange(slot.MaterialShader);
                    materialTextureProperties.AddRange(slot.MaterialTextureProperty);
                }
            }
        }

    }

    public class ME_Coordinate
    {
        public Dictionary<int, MaterialEditorProperties> AccessoryProperties = new Dictionary<int, MaterialEditorProperties>();
        public Dictionary<int, MaterialEditorProperties> ClothingProperties = new Dictionary<int, MaterialEditorProperties>();
        public Dictionary<int, MaterialEditorProperties> HairProperties = new Dictionary<int, MaterialEditorProperties>();
        public Dictionary<int, MaterialEditorProperties> CharacterProperties = new Dictionary<int, MaterialEditorProperties>();

        public ME_Coordinate() { }

        public ME_Coordinate(PluginData pluginData, ChaDefault ThisOutfitData, int outfitnum)
        {
            if (pluginData != null)
            {
                if (pluginData.version == 0)
                {
                    var importDictionaryList = new Dictionary<int, int>();

                    if (pluginData.data.TryGetValue("TextureDictionary", out var texDic) && texDic != null)
                    {
                        foreach (var x in MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])texDic))
                            importDictionaryList[x.Key] = ThisOutfitData.ME.SetAndGetTextureID(x.Value);
                    }

                    if (pluginData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                    }

                    if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                    {
                        var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                        for (var i = 0; i < properties.Count; i++)
                        {
                            var loadedProperty = properties[i];
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];
                            var slot = loadedProperty.Slot;
                            MaterialEditorProperties editorProperties;
                            switch (loadedProperty.ObjectType)
                            {
                                case ObjectType.Clothing:
                                    if (!ClothingProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = ClothingProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Accessory:
                                    if (!AccessoryProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = AccessoryProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Hair:
                                    if (!HairProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = HairProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                case ObjectType.Character:
                                    if (!CharacterProperties.TryGetValue(slot, out editorProperties))
                                    {
                                        editorProperties = CharacterProperties[slot] = new MaterialEditorProperties();
                                    }
                                    break;
                                default:
                                    continue;
                            }
                            editorProperties.MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }
                }
                else
                {
                    ClothingLoader.OutdatedMessage("Material Editor", true);
                }
            }
        }

        internal void SoftClear(bool[] clothingkeep)
        {
            AccessoryProperties.Clear();
            for (var i = 0; i < clothingkeep.Length; i++)
            {
                if (!clothingkeep[i])
                    ClothingProperties.Remove(i);
            }
        }

        internal void SoftClear()
        {
            AccessoryProperties.Clear();
            ClothingProperties.Clear();
        }

        internal void HardClear()
        {
            AccessoryProperties.Clear();
            ClothingProperties.Clear();
            HairProperties.Clear();
            CharacterProperties.Clear();
        }

        internal void AddAccessory(int outfitnum, int slot, MaterialEditorProperties materialEditorProperties)
        {
            var color = materialEditorProperties.MaterialColorProperty;
            var floatprop = materialEditorProperties.MaterialFloatProperty;
            var shader = materialEditorProperties.MaterialShader;
            var texture = materialEditorProperties.MaterialTextureProperty;
            var render = materialEditorProperties.RendererProperty;

            foreach (var item in color)
            {
                item.CoordinateIndex = outfitnum;
                item.Slot = slot;
            }
            foreach (var item in floatprop)
            {
                item.CoordinateIndex = outfitnum;
                item.Slot = slot;
            }
            foreach (var item in shader)
            {
                item.CoordinateIndex = outfitnum;
                item.Slot = slot;
            }
            foreach (var item in texture)
            {
                item.CoordinateIndex = outfitnum;
                item.Slot = slot;
            }
            foreach (var item in render)
            {
                item.CoordinateIndex = outfitnum;
                item.Slot = slot;
            }

            AccessoryProperties[slot] = new MaterialEditorProperties(render, floatprop, color, texture, shader);
        }

        internal void ChangeCoord(int outfitnum)
        {
            foreach (var prop in ClothingProperties.Values)
            {
                foreach (var slot in prop.RendererProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialColorProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialFloatProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialTextureProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialShader)
                {
                    slot.CoordinateIndex = outfitnum;
                }
            }
            foreach (var prop in HairProperties.Values)
            {
                foreach (var slot in prop.RendererProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialColorProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialFloatProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialTextureProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialShader)
                {
                    slot.CoordinateIndex = outfitnum;
                }
            }
            foreach (var prop in CharacterProperties.Values)
            {
                foreach (var slot in prop.RendererProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialColorProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialFloatProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialTextureProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialShader)
                {
                    slot.CoordinateIndex = outfitnum;
                }
            }
            foreach (var prop in AccessoryProperties.Values)
            {
                foreach (var slot in prop.RendererProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialColorProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialFloatProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialTextureProperty)
                {
                    slot.CoordinateIndex = outfitnum;
                }
                foreach (var slot in prop.MaterialShader)
                {
                    slot.CoordinateIndex = outfitnum;
                }
            }
        }

        internal void AllProperties(out List<RendererProperty> rendererProperties, out List<MaterialFloatProperty> materialFloatProperties, out List<MaterialColorProperty> materialColorProperties, out List<MaterialShader> materialShaders, out List<MaterialTextureProperty> materialTextureProperties)
        {
            rendererProperties = new List<RendererProperty>();
            materialFloatProperties = new List<MaterialFloatProperty>();
            materialColorProperties = new List<MaterialColorProperty>();
            materialShaders = new List<MaterialShader>();
            materialTextureProperties = new List<MaterialTextureProperty>();

            foreach (var slot in AccessoryProperties)
            {
                rendererProperties.AddRange(slot.Value.RendererProperty);
                materialFloatProperties.AddRange(slot.Value.MaterialFloatProperty);
                materialColorProperties.AddRange(slot.Value.MaterialColorProperty);
                materialShaders.AddRange(slot.Value.MaterialShader);
                materialTextureProperties.AddRange(slot.Value.MaterialTextureProperty);
            }
            foreach (var slot in ClothingProperties)
            {
                rendererProperties.AddRange(slot.Value.RendererProperty);
                materialFloatProperties.AddRange(slot.Value.MaterialFloatProperty);
                materialColorProperties.AddRange(slot.Value.MaterialColorProperty);
                materialShaders.AddRange(slot.Value.MaterialShader);
                materialTextureProperties.AddRange(slot.Value.MaterialTextureProperty);
            }
            foreach (var slot in HairProperties)
            {
                rendererProperties.AddRange(slot.Value.RendererProperty);
                materialFloatProperties.AddRange(slot.Value.MaterialFloatProperty);
                materialColorProperties.AddRange(slot.Value.MaterialColorProperty);
                materialShaders.AddRange(slot.Value.MaterialShader);
                materialTextureProperties.AddRange(slot.Value.MaterialTextureProperty);
            }
            foreach (var slot in CharacterProperties)
            {
                rendererProperties.AddRange(slot.Value.RendererProperty);
                materialFloatProperties.AddRange(slot.Value.MaterialFloatProperty);
                materialColorProperties.AddRange(slot.Value.MaterialColorProperty);
                materialShaders.AddRange(slot.Value.MaterialShader);
                materialTextureProperties.AddRange(slot.Value.MaterialTextureProperty);
            }
        }
    }

    #region Original Stuff
    public class ME_Support
    {
        public Dictionary<int, TextureContainer> TextureDictionary = new Dictionary<int, TextureContainer>();
        public int SetAndGetTextureID(byte[] textureBytes)
        {
            var highestID = 0;
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

    public sealed class TextureContainer
    {
        public byte[] Data;
        public TextureContainer(byte[] data)
        {
            Data = data;
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