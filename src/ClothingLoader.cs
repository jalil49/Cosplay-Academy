
using ExtensibleSaveFormat;
using HarmonyLib;
using MessagePack;
using MoreAccessoriesKOI;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UnityEngine;
namespace Cosplay_Academy
{
    public static class ClothingLoader
    {
        private static ChaControl chaControl;
        private static Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>> HairAccessories;
        private static int SomeInt = 0;
        private static ChaDefault ThisOutfitData;
#if ME_Support
        private static List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
        private static List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
        private static List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
        private static List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
        private static List<MaterialShader> MaterialShaderList = new List<MaterialShader>();
        private static Dictionary<int, int> ImportList = new Dictionary<int, int>();
#endif
        public static void FullLoad(ChaControl input, ChaDefault cha)
        {
            ThisOutfitData = cha;
            var HairPlugin = new PluginData();
            var ME_Plugin = new PluginData();
            HairAccessories = new Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>>();
            chaControl = input;
#if ME_Support
            ME_ListClear();
#endif

            UniformLoad();
            AfterSchoolLoad();
            GymLoad();
            SwimLoad();
            ClubLoad();
            CasualLoad();
            NightwearLoad();

            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            CharaEvent.self.SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin);
#if ME_Support
            ThisOutfitData.ME_Work = true;
#endif
        }
        private static void UniformLoad()
        {
            Generalized(0);
            ExpandedOutfit.Logger.LogDebug("loaded 0 " + ThisOutfitData.outfitpath[0]);
        }
        private static void AfterSchoolLoad()
        {
            Generalized(1);
            ExpandedOutfit.Logger.LogDebug("loaded 1 " + ThisOutfitData.outfitpath[1]);
        }
        private static void GymLoad()
        {
            Generalized(2);
            ExpandedOutfit.Logger.LogDebug("loaded 2 " + ThisOutfitData.outfitpath[2]);
        }
        private static void SwimLoad()
        {
            Generalized(3);
            ExpandedOutfit.Logger.LogDebug("loaded 3 " + ThisOutfitData.outfitpath[3]);
        }
        private static void ClubLoad()
        {
            Generalized(4);
            ExpandedOutfit.Logger.LogDebug("loaded 4 " + ThisOutfitData.outfitpath[4]);
        }
        private static void CasualLoad()
        {
            Generalized(5);
            ExpandedOutfit.Logger.LogDebug("loaded 5 " + ThisOutfitData.outfitpath[5]);
        }
        private static void NightwearLoad()
        {
            Generalized(6);
            ExpandedOutfit.Logger.LogDebug("loaded 6 " + ThisOutfitData.outfitpath[6]);
        }
        private static void Generalized(int outfitnum)
        {
            //queue Accessorys to keep
            #region Queue accessories to keep

            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(ThisOutfitData.CoordinatePartsQueue[outfitnum]);
            var HairQueue = new Queue<CharaEvent.HairAccessoryInfo>(ThisOutfitData.HairAccQueue[outfitnum]);

#if ME_Support
            var RenderQueue = new Queue<RendererProperty>(ThisOutfitData.RendererPropertyQueue[outfitnum]);
            var FloatQueue = new Queue<MaterialFloatProperty>(ThisOutfitData.MaterialFloatPropertyQueue[outfitnum]);
            var ColorQueue = new Queue<MaterialColorProperty>(ThisOutfitData.MaterialColorPropertyQueue[outfitnum]);
            var TextureQueue = new Queue<MaterialTextureProperty>(ThisOutfitData.MaterialTexturePropertyQueue[outfitnum]);
            var ShaderQueue = new Queue<MaterialShader>(ThisOutfitData.MaterialShaderQueue[outfitnum]);
#if Debug
            ExpandedOutfit.Logger.LogWarning($"Render: {RenderQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Float: {FloatQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"tColor: {ColorQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Texture: {TextureQueue.Count}");
            ExpandedOutfit.Logger.LogWarning($"Shader: {ShaderQueue.Count}");
#endif

#endif
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

            if (_accessoriesByChar.TryGetValue(chaControl.chaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(chaControl.chaFile, data);
            }
            #endregion


            //Load new outfit
            chaControl.fileStatus.coordinateType = outfitnum;
            chaControl.chaFile.coordinate[outfitnum].LoadFile(ThisOutfitData.outfitpath[outfitnum]);


            //Apply pre-existing Accessories in any open slot or final slots.
            #region Reassign Exisiting Accessories

            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }

            var Inputdata = ExtendedSave.GetExtendedDataById(chaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
            var Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);
#if ME_Support
            List<RendererProperty> Renderer = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloat = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColor = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexture = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShade = new List<MaterialShader>();
            //Dictionary<int, int> importDictionary = new Dictionary<int, int>();

            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(chaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");
            //for (int i = 0; i < MaterialEditorData.data.Count; i++)
            //{
            //    ExpandedOutfit.Logger.LogWarning($"Key: {MaterialEditorData.data.ElementAt(i).Key} Value: {MaterialEditorData.data.ElementAt(i).Value}");
            //}
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
                        ThisOutfitData.ReturnimportDictionary[x.Key] = ME_Support.SetAndGetTextureID(x.Value);
                }

                if (MaterialEditorData.data.TryGetValue("MaterialShaderList", out var shaderProperties) && shaderProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialShader>>((byte[])shaderProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialShade.Add(new MaterialShader(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.ShaderName, loadedProperty.ShaderNameOriginal, loadedProperty.RenderQueue, loadedProperty.RenderQueueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("RendererPropertyList", out var rendererProperties) && rendererProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<RendererProperty>>((byte[])rendererProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnRenderer.Add(new RendererProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.RendererName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialFloatPropertyList", out var materialFloatProperties) && materialFloatProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialFloatProperty>>((byte[])materialFloatProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialFloat.Add(new MaterialFloatProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
                    }
                }

                if (MaterialEditorData.data.TryGetValue("MaterialColorPropertyList", out var materialColorProperties) && materialColorProperties != null)
                {
                    var properties = MessagePackSerializer.Deserialize<List<MaterialColorProperty>>((byte[])materialColorProperties);
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var loadedProperty = properties[i];
                        if (objectTypesToLoad.Contains(loadedProperty.ObjectType))
                            ThisOutfitData.ReturnMaterialColor.Add(new MaterialColorProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, loadedProperty.Value, loadedProperty.ValueOriginal));
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
                                texID = ThisOutfitData.ReturnimportDictionary[(int)loadedProperty.TexID];

                            MaterialTextureProperty newTextureProperty = new MaterialTextureProperty(loadedProperty.ObjectType, outfitnum, loadedProperty.Slot, loadedProperty.MaterialName, loadedProperty.Property, texID, loadedProperty.Offset, loadedProperty.OffsetOriginal, loadedProperty.Scale, loadedProperty.ScaleOriginal);
                            ThisOutfitData.ReturnMaterialTexture.Add(newTextureProperty);
                        }
                    }
                }
            }
            #endregion
#endif
#if Debug
            ExpandedOutfit.Logger.LogWarning("Start loading accessories");
#endif

            int ACCpostion = 0;
            bool Empty;
            for (int n = chaControl.chaFile.coordinate[outfitnum].accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = chaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        chaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion] = PartsQueue.Dequeue();

                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
#if ME_Support

                        if (RenderQueue.Count > 0 && RenderQueue.Count > 0 && RenderQueue.Peek() != null)
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
                        ExpandedOutfit.Logger.LogWarning("Render Pass");
#endif

                        if (ColorQueue.Count > 0 && ColorQueue.Peek() != null)
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
                        ExpandedOutfit.Logger.LogWarning("Color Pass");
                        ExpandedOutfit.Logger.LogWarning($"Texture: {TextureQueue.Count}");
#endif

                        if (TextureQueue.Peek() != null)
                        {
                            MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                            if (ME_Info.TexID != null)
                            {
                                if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                                {
                                    ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                                    ExpandedOutfit.Logger.LogWarning("well shit it works");
                                }
                                else
                                {
                                    ExpandedOutfit.Logger.LogWarning("well something unintended is going on");
                                }
                            }
                            ME_Info.Slot = ACCpostion;
                            MaterialTexture.Add(ME_Info);
                        }
                        else
                        {
                            TextureQueue.Dequeue();
                        }
#if Debug
                        ExpandedOutfit.Logger.LogWarning("Texture Pass");
#endif

                        if (FloatQueue.Count > 0 && FloatQueue.Peek() != null)
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
                        ExpandedOutfit.Logger.LogWarning("Float Pass");
#endif
                        if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null)
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
                        ExpandedOutfit.Logger.LogWarning("Shader Pass");
#endif

#endif
                    }
                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
#if Debug
                ExpandedOutfit.Logger.LogWarning("Force Color Pass");
#endif

            }
#if Debug
            ExpandedOutfit.Logger.LogWarning("Start extra accessories");
#endif
            for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = NewRAW[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        NewRAW[ACCpostion] = PartsQueue.Dequeue();
                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
#if ME_Support
                        if (RenderQueue.Count > 0 && RenderQueue.Peek() != null)
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

                        if (ColorQueue.Count > 0 && ColorQueue.Peek() != null)
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

                        if (TextureQueue.Peek() != null)
                        {
                            MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                            if (ME_Info.TexID != null)
                            {
                                if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                                {
                                    ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                                    ExpandedOutfit.Logger.LogWarning("well shit it works");
                                }
                                else
                                {
                                    ExpandedOutfit.Logger.LogWarning("well something unintended is going on");
                                }
                            }
                            ME_Info.Slot = ACCpostion;

                            ME_Info.Slot = ACCpostion;
                            MaterialTexture.Add(ME_Info);
                        }
                        else
                        {
                            TextureQueue.Dequeue();
                        }

                        if (FloatQueue.Count > 0 && FloatQueue.Peek() != null)
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

                        if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null)
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
#endif 
                    }
                }
                if (ExpandedOutfit.HairMatch.Value && Temp.TryGetValue(ACCpostion, out var info))
                {
                    info.ColorMatch = true;
                }
            }
#if Debug
            ExpandedOutfit.Logger.LogWarning("Start making extra accessories");
#endif

            bool print = true;

            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    print = false;
                }
                if (!Temp.ContainsKey(ACCpostion))
                {
                    NewRAW.Add(PartsQueue.Dequeue());
                    if (HairQueue.Peek() != null)
                    {
                        var HairInfo = HairQueue.Dequeue();
                        if (ExpandedOutfit.HairMatch.Value)
                        {
                            HairInfo.ColorMatch = true;
                        }
                        Temp.Add(ACCpostion, HairInfo);
                    }
                    else
                    {
                        HairQueue.Dequeue();
                    }
#if ME_Support
                    if (RenderQueue.Count > 0 && RenderQueue.Peek() != null)
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
                    if (ColorQueue.Count > 0 && ColorQueue.Peek() != null)
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
                    if (TextureQueue.Peek() != null)
                    {
                        MaterialTextureProperty ME_Info = TextureQueue.Dequeue();
                        if (ME_Info.TexID != null)
                        {
                            if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                            {
                                ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                                ExpandedOutfit.Logger.LogWarning("well shit it works");
                            }
                            else
                            {
                                ExpandedOutfit.Logger.LogWarning("well something unintended is going on");
                            }
                        }
                        ME_Info.Slot = ACCpostion;


                        ME_Info.Slot = ACCpostion;
                        MaterialTexture.Add(ME_Info);
                    }
                    else
                    {
                        TextureQueue.Dequeue();
                    }
                    if (FloatQueue.Count > 0 && FloatQueue.Peek() != null)
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
                    if (ShaderQueue.Count > 0 && ShaderQueue.Peek() != null)
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
#endif
                }
                else
                {
                    NewRAW.Add(new ChaFileAccessory.PartsInfo());
                }
                ACCpostion++;
            }
            data.rawAccessoriesInfos[outfitnum] = NewRAW;

            HairAccessories.Add(outfitnum, Temp);
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
            ExpandedOutfit.Logger.LogWarning("add range");
#endif


#if ME_Support
            ThisOutfitData.ReturnMaterialColor.AddRange(MaterialColor);

            ThisOutfitData.ReturnMaterialFloat.AddRange(MaterialFloat);

            ThisOutfitData.ReturnMaterialShade.AddRange(MaterialShade);

            ThisOutfitData.ReturnMaterialTexture.AddRange(MaterialTexture);

            ThisOutfitData.ReturnRenderer.AddRange(Renderer);
#endif
#if Debug
            ExpandedOutfit.Logger.LogWarning("finish");
#endif

            #endregion
        }
#if ME_Support
        private static void ME_ListClear()
        {
            RendererPropertyList.Clear();
            MaterialFloatPropertyList.Clear();
            MaterialColorPropertyList.Clear();
            MaterialTexturePropertyList.Clear();
            MaterialShaderList.Clear();
            ImportList.Clear();
            ThisOutfitData.ReturnimportDictionary.Clear();
            ThisOutfitData.ReturnMaterialTexture.Clear();
        }
#endif
    }
}