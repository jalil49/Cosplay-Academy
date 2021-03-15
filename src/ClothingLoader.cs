using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Chara;
using MessagePack;
using MoreAccessoriesKOI;
using System.Collections.Generic;
using ToolBox;
using UnityEngine;
namespace Cosplay_Academy
{
    public partial class CharaEvent : CharaCustomFunctionController
    {
        private static Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>> HairAccessories;
        private static List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
        private static List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
        private static List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
        private static List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
        private static List<MaterialShader> MaterialShaderList = new List<MaterialShader>();


        public void FullLoad()
        {
            var HairPlugin = new PluginData();
            HairAccessories = new Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>>();
            ME_ListClear();


            for (int i = 0; i < Constants.outfitpath.Length; i++)
            {
                GeneralizedLoad(i);
                ExpandedOutfit.Logger.LogDebug($"loaded {i} " + ThisOutfitData.outfitpath[i]);
            }

            HairPlugin.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            CharaEvent.self.SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", HairPlugin);
            ThisOutfitData.ME_Work = true;
        }


        private void GeneralizedLoad(int outfitnum)
        {
            //queue Accessorys to keep
            #region Queue accessories to keep

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
            #endregion

            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            if (_accessoriesByChar.TryGetValue(ChaFileControl, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaFileControl, data);
            }

            //Load new outfit
            ChaControl.fileStatus.coordinateType = outfitnum;
            ChaFileControl.coordinate[outfitnum].LoadFile(ThisOutfitData.outfitpath[outfitnum]);
            //var checkdata = ExtendedSave.GetAllExtendedData(ChaFileControl.coordinate[outfitnum]);

            //Apply pre-existing Accessories in any open slot or final slots.
            #region Reassign Exisiting Accessories

            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }
            var Inputdata = ExtendedSave.GetExtendedDataById(ChaFileControl.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
            var Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);

            List<RendererProperty> Renderer = new List<RendererProperty>();
            List<MaterialFloatProperty> MaterialFloat = new List<MaterialFloatProperty>();
            List<MaterialColorProperty> MaterialColor = new List<MaterialColorProperty>();
            List<MaterialTextureProperty> MaterialTexture = new List<MaterialTextureProperty>();
            List<MaterialShader> MaterialShade = new List<MaterialShader>();

            #region ME Acc Import
            var MaterialEditorData = ExtendedSave.GetExtendedDataById(ChaFileControl.coordinate[outfitnum], "com.deathweasel.bepinex.materialeditor");

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
#if Debug
            ExpandedOutfit.Logger.LogWarning("Start loading accessories");
#endif

            int ACCpostion = 0;
            bool Empty;
            for (int n = ChaFileControl.coordinate[outfitnum].accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = ChaFileControl.coordinate[outfitnum].accessory.parts[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    ChaFileControl.coordinate[outfitnum].accessory.parts[ACCpostion] = PartsQueue.Dequeue();
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
                        if (!ThisOutfitData.ME_Work && ME_Info.TexID != null)
                        {
                            if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                            {
                                ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
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
            for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion - 20 < n; ACCpostion++)
            {
                Empty = NewRAW[ACCpostion - 20].type == 120;
                if (Empty) //120 is empty/default
                {

                    NewRAW[ACCpostion - 20] = PartsQueue.Dequeue();
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
                        if (!ThisOutfitData.ME_Work && ME_Info.TexID != null)
                        {
                            if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                            {
                                ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                            }
                        }
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
                NewRAW.Add(PartsQueue.Dequeue());
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
                    if (!ThisOutfitData.ME_Work && ME_Info.TexID != null)
                    {
                        if (ThisOutfitData.importDictionaryQueue[ME_Info.CoordinateIndex].TryGetValue((int)ME_Info.TexID, out byte[] imgbyte))
                        {
                            ME_Info.TexID = ME_Support.SetAndGetTextureID(imgbyte);
                        }
                    }
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
            //ExpandedOutfit.Logger.LogWarning("add range");
#endif

            ThisOutfitData.ReturnMaterialColor.AddRange(MaterialColor);

            ThisOutfitData.ReturnMaterialFloat.AddRange(MaterialFloat);

            ThisOutfitData.ReturnMaterialShade.AddRange(MaterialShade);

            ThisOutfitData.ReturnMaterialTexture.AddRange(MaterialTexture);

            ThisOutfitData.ReturnRenderer.AddRange(Renderer);
#if Debug
            ExpandedOutfit.Logger.LogWarning("finish");
#endif

            #endregion
        }
        private void ME_ListClear()
        {
            RendererPropertyList.Clear();
            MaterialFloatPropertyList.Clear();
            MaterialColorPropertyList.Clear();
            MaterialTexturePropertyList.Clear();
            MaterialShaderList.Clear();
            //ImportList.Clear();
            ThisOutfitData.ReturnimportDictionary.Clear();
            ThisOutfitData.ReturnMaterialTexture.Clear();
        }
    }
}