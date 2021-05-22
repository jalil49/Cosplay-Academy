using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cosplay_Academy.ME
{
    public class ME_List
    {
        public bool NoData = false;

        public List<MaterialShader> MaterialShader = new List<MaterialShader>();
        public List<RendererProperty> RendererProperty = new List<RendererProperty>();
        public List<MaterialColorProperty> MaterialColorProperty = new List<MaterialColorProperty>();
        public List<MaterialFloatProperty> MaterialFloatProperty = new List<MaterialFloatProperty>();
        public List<MaterialTextureProperty> MaterialTextureProperty = new List<MaterialTextureProperty>();

        //Just need empty Lists
        public ME_List()
        { }

        //Copy
        public ME_List(ME_List Original)
        {
            MaterialShader = new List<MaterialShader>(Original.MaterialShader);
            RendererProperty = new List<RendererProperty>(Original.RendererProperty);
            MaterialColorProperty = new List<MaterialColorProperty>(Original.MaterialColorProperty);
            MaterialFloatProperty = new List<MaterialFloatProperty>(Original.MaterialFloatProperty);
            MaterialTextureProperty = new List<MaterialTextureProperty>(Original.MaterialTextureProperty);
        }

        //Full Chafile Accessory Load
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData)
        {
            if (pluginData?.data != null)
            {
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        //var temp = new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal);
                        if (loadedProperty.ObjectType == ObjectType.Accessory)
                        {
                            MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                        }
                        //ThisOutfitData.Finished.MaterialShader.Add(temp);
                    }
                }


                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        //var temp = new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal);
                        if (loadedProperty.ObjectType == ObjectType.Accessory)
                        {
                            RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                        //ThisOutfitData.Finished.RendererProperty.Add(temp);
                    }
                }
                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];

                        //var temp = new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal);
                        if (loadedProperty.ObjectType == ObjectType.Accessory)
                        {
                            MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        }
                        //ThisOutfitData.Finished.MaterialFloatProperty.Add(temp);
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        //var temp = new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal);
                        if (loadedProperty.ObjectType == ObjectType.Accessory)
                            MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                        //ThisOutfitData.Finished.MaterialColorProperty.Add(temp);
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
                        //var temp = new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                        if (loadedProperty.ObjectType == ObjectType.Accessory)
                        {
                            MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                        //ThisOutfitData.Finished.MaterialTextureProperty.Add(temp);
                    }
                }
            }
            else
            {
                NoData = true;
            }
        }

        //
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData, List<ObjectType> objectTypes)
        {
            if (pluginData?.data != null)
            {
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        if (objectTypes.Contains(loadedProperty.ObjectType))
                            MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypes.Contains(loadedProperty.ObjectType))
                            RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypes.Contains(loadedProperty.ObjectType))
                            MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypes.Contains(loadedProperty.ObjectType))
                            MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypes.Contains(loadedProperty.ObjectType))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }
                }
            }
            else
            {
                NoData = true;
            }
        }

        //Coordinate Load
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData, int outfitnum)
        {
            if (pluginData?.data != null)
            {
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                            MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing)
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }
                }
            }
            else
            {
                NoData = true;
            }
        }

        //Load Data for all outfits (Underwear), but don't apply
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData, bool LoadAll, int outfitnum = -1)
        {
            if (pluginData?.data != null)
            {
                List<ObjectType> objectTypesToLoad = new List<ObjectType>
                    {
                        ObjectType.Accessory,
                        ObjectType.Character,
                        ObjectType.Clothing,
                        ObjectType.Hair
                    };
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            if (LoadAll)
                                for (int outfitnum_loop = 0; i < Constants.Outfit_Size; i++)
                                    MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum_loop, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));

                            else if (outfitnum > -1)
                            {
                                MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                            }
                            else
                            {
                                MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                            }
                        }
                    }
                }

                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            if (LoadAll)
                                for (int outfitnum_loop = 0; i < Constants.Outfit_Size; i++)
                                    RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum_loop, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));

                            else if (outfitnum > -1)
                            {
                                RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                            else
                            {
                                RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }
                }

                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            if (LoadAll)
                                for (int outfitnum_loop = 0; i < Constants.Outfit_Size; i++)
                                    MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum_loop, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            else if (outfitnum > -1)
                            {
                                MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                            else
                            {
                                MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            if (LoadAll)
                                for (int outfitnum_loop = 0; i < Constants.Outfit_Size; i++)
                                    MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum_loop, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            else if (outfitnum > -1)
                            {
                                MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                            else
                            {
                                MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                            }
                        }
                    }
                }

                if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];
                            if (LoadAll)
                                for (int outfitnum_loop = 0; i < Constants.Outfit_Size; i++)
                                    MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum_loop, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            else if (outfitnum > -1)
                            {
                                MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            }
                            else
                            {
                                MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                            }
                        }
                    }
                }
            }
            else
            {
                NoData = true;
            }
        }

        //ClothingLoader.Original_ME_Data()
        public ME_List(PluginData pluginData, ChaDefault ThisOutfitData, List<int>[] KeepCloth)
        {
            if (pluginData?.data != null)
            {
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth[loadedProperty.CoordinateIndex].Contains(loadedProperty.Slot))
                            MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth[loadedProperty.CoordinateIndex].Contains(loadedProperty.Slot))
                            RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth[loadedProperty.CoordinateIndex].Contains(loadedProperty.Slot))
                            MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth[loadedProperty.CoordinateIndex].Contains(loadedProperty.Slot))
                            MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (loadedProperty.ObjectType == ObjectType.Clothing && KeepCloth[loadedProperty.CoordinateIndex].Contains(loadedProperty.Slot))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            MaterialTextureProperty.Add(new MaterialTextureProperty(loadedProperty.ObjectType, loadedProperty.CoordinateIndex, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal));
                        }
                    }
                }
            }
            else
            {
                NoData = true;
            }
        }

        //Add Data to Return (Generalized Load)
        public static void PrimaryData(PluginData pluginData, ChaDefault ThisOutfitData, int outfitnum)
        {
            if (pluginData?.data != null)
            {
                List<ObjectType> objectTypesToLoad = new List<ObjectType>
                {
                    ObjectType.Accessory,
                    ObjectType.Character,
                    ObjectType.Clothing,
                    ObjectType.Hair
                };
                Dictionary<int, int> importDictionaryList = new Dictionary<int, int>();

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
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.Finished.MaterialShader.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.Finished.RendererProperty.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.Finished.MaterialFloatProperty.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.Finished.MaterialColorProperty.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (pluginData.data.TryGetValue("MaterialTexturePropertyList", out var materialTextureProperties) && materialTextureProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialTextureProperty>>((byte[])materialTextureProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                        {
                            int? texID = null;
                            if (loadedProperty.TexID != null)
                                texID = importDictionaryList[(int)loadedProperty.TexID];

                            MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                            ThisOutfitData.Finished.MaterialTextureProperty.Add(newTextureProperty);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            MaterialShader.Clear();
            RendererProperty.Clear();
            MaterialColorProperty.Clear();
            MaterialFloatProperty.Clear();
            MaterialTextureProperty.Clear();
        }

        #region Return List of Properties
        //specific Accessory
        public List<RendererProperty> Render_FindAll(List<ObjectType> ObjectList, int slot, int outfitnum)
        {
            var List = RendererProperty.FindAll(x => ObjectList.Contains(x.ObjectType) && x.Slot == slot && x.CoordinateIndex == outfitnum);
            if (List.Count == 0)
            {
                List.Add(new RendererProperty(ObjectType.Unknown, outfitnum, slot, "", RendererProperties.Enabled, "", ""));
            }
            return List;
        }
        public List<MaterialFloatProperty> Float_FindAll(List<ObjectType> ObjectList, int slot, int outfitnum)
        {
            var List = MaterialFloatProperty.FindAll(x => ObjectList.Contains(x.ObjectType) && x.Slot == slot && x.CoordinateIndex == outfitnum);
            if (List.Count == 0)
            {
                List.Add(new MaterialFloatProperty(ObjectType.Unknown, outfitnum, slot, "", "", "", ""));
            }
            return List;
        }
        public List<MaterialColorProperty> Color_FindAll(List<ObjectType> ObjectList, int slot, int outfitnum)
        {
            var List = MaterialColorProperty.FindAll(x => ObjectList.Contains(x.ObjectType) && x.Slot == slot && x.CoordinateIndex == outfitnum);
            if (List.Count == 0)
            {
                Color color = new Color(0, 0, 0);
                List.Add(new MaterialColorProperty(ObjectType.Unknown, outfitnum, slot, "", "", color, color));
            }
            return List;
        }
        public List<MaterialTextureProperty> Texture_FindAll(List<ObjectType> ObjectList, int slot, int outfitnum)
        {
            var List = MaterialTextureProperty.FindAll(x => ObjectList.Contains(x.ObjectType) && x.Slot == slot && x.CoordinateIndex == outfitnum);
            if (List.Count == 0)
            {
                List.Add(new MaterialTextureProperty(ObjectType.Unknown, outfitnum, slot, "", ""));
            }
            return List;
        }
        public List<MaterialShader> Shader_FindAll(List<ObjectType> ObjectList, int slot, int outfitnum)
        {
            var List = MaterialShader.FindAll(x => ObjectList.Contains(x.ObjectType) && x.Slot == slot && x.CoordinateIndex == outfitnum);
            if (List.Count == 0)
            {
                List.Add(new MaterialShader(ObjectType.Unknown, outfitnum, slot, "", 0, 0));
            }
            return List;
        }
        #endregion
    }


    #region Original Stuff
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