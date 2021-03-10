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
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            ChaDefault setting = Constants.ChaDefaults.Find(x => x.ChaID == ChaControl.name);
            if (setting == null)
            {
                ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} made new default");

                setting = new ChaDefault();
                Constants.ChaDefaults.Add(setting);
                setting.ChaID = ChaControl.name;
            }
            ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} has id of {ChaControl.name}");
            self = this;
            if (!ExpandedOutfit.EnableSetting.Value || !ExpandedOutfit.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode || Repeat_stoppper && GameMode.Maker != currentGameMode/*|| !ExpandedOutfit.Makerview.Value && GameMode.Unknown == currentGameMode*/)
            {
                Repeat_stoppper = false;
                return;
            }//if disabled don't run
            if (GameMode.Maker == currentGameMode)
            {
                setting.firstpass = true;
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
                OutfitDecider.ResetDecider();
            }
            if (setting.firstpass) //Save all accessories to avoid duplicating head accessories each load and be reuseable
            {
                List<ChaFileAccessory.PartsInfo>[] CoordinatePartsQueue = new List<ChaFileAccessory.PartsInfo>[Constants.outfitpath.Length];
                List<CharaEvent.HairAccessoryInfo>[] HairAccQueue = new List<CharaEvent.HairAccessoryInfo>[Constants.outfitpath.Length];

                WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
                Dictionary<int, CharaEvent.HairAccessoryInfo> temp2;
                #region Queue accessories to keep
                for (int outfitnum = 0, n = Constants.outfitpath.Length; outfitnum < n; outfitnum++)
                {
                    List<ChaFileAccessory.PartsInfo> import = new List<ChaFileAccessory.PartsInfo>();
                    List<HairAccessoryInfo> Subimport = new List<HairAccessoryInfo>();


                    var InputToSave = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
                    temp2 = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
                    if (InputToSave != null)
                        if (InputToSave.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                            temp2 = MessagePackSerializer.Deserialize<Dictionary<int, HairAccessoryInfo>>((byte[])loadedHairAccessories);


                    if (_accessoriesByChar.TryGetValue(ChaControl.chaFile, out var SaveAccessory) == false)
                    {
                        SaveAccessory = new MoreAccessories.CharAdditionalData();
                        _accessoriesByChar.Add(ChaControl.chaFile, SaveAccessory);
                    }
                    if (SaveAccessory.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> acclist) == false)
                    {
                        acclist = new List<ChaFileAccessory.PartsInfo>();
                    }
                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(acclist);
                    Intermediate.AddRange(ChaControl.chaFile.coordinate[outfitnum].accessory.parts);
                    for (int i = 0; i < 20; i++)
                    {
                        ExpandedOutfit.Logger.LogWarning($"Coord {outfitnum}: {ChaControl.chaFile.coordinate[outfitnum].accessory.parts[i].id} parent: {ChaControl.chaFile.coordinate[outfitnum].accessory.parts[i].parentKey}");
                    }

                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (Constants.Inclusion.Contains(Intermediate[i].parentKey))
                        {
                            if (!temp2.TryGetValue(i, out HairAccessoryInfo ACCdata))
                            {
                                ACCdata = null;
                            }
                            import.Add(Intermediate[i]);
                            Subimport.Add(ACCdata);
                        }
                    }
                    CoordinatePartsQueue[outfitnum] = import;
                    HairAccQueue[outfitnum] = Subimport;
                    ExpandedOutfit.Logger.LogWarning($"import count {import.Count}\t subimport count {Subimport.Count} ");

                }
                setting.firstpass = false;
                setting.CoordinatePartsQueue = CoordinatePartsQueue;
                setting.HairAccQueue = HairAccQueue;
                //foreach (var coord in ChaControl.chaFile.coordinate)
                //{
                //    OutfitList.Add(coord);
                //}
                #endregion
            }

            //use Chacontrol.name instead of ChaControl.fileParam.fullname to probably avoid same name conflicts
            if (ChaControl.sex == 1 && (GameMode.Maker == currentGameMode || !OutfitDecider.ProcessedNames.Contains(ChaControl.name)))//run the following if female and unprocessed
            {
                if (currentGameMode == GameMode.MainGame || ExpandedOutfit.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    OutfitDecider.Decision(ChaControl.fileParam.fullname);//Generate outfits
                    OutfitDecider.ProcessedNames.Add(ChaControl.name);//character is processed
                    if (!ExpandedOutfit.PermChangeOutfit.Value)
                    {
                        ExpandedOutfit.ChangeOutfit.Value = false;
                    }
                    int HoldOutfit = ChaControl.fileStatus.coordinateType;
                    ClothingLoader.FullLoad(ChaControl, self, setting);//Load outfits
                    ChaControl.fileStatus.coordinateType = HoldOutfit;
                }
                ChaInfo temp = (ChaInfo)ChaControl;
                ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
                if (Repeat_stoppper)//stop any potential endless loops in maker
                {
                    Repeat_stoppper = false;
                    return;
                }
                //object[] OnReloadArray = new object[2] { currentGameMode, false };
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
                Finish();
            }
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
            if (heroine != null && ChaControl.sex == 1)
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
            if (heroine != null && ChaControl.sex == 1)
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

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!ExpandedOutfit.AccKeeper.Value)
            {
                return;
            }//if disabled don't run
            ChaDefault setting = Constants.ChaDefaults.Find(x => x.ChaID == ChaControl.name);
            if (setting == null)
            {
                ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} No ChaId Found");
                return;
            }
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
            ExpandedOutfit.Logger.LogWarning($"Status: {(CoordinateType)ChaFileControl.status.coordinateType} ");
            //Apply pre-existing Accessories in any open slot or final slots.
            #region Reassign Exisiting Accessories
            var Inputdata = ExtendedSave.GetExtendedDataById(ChaControl.nowCoordinate, "com.deathweasel.bepinex.hairaccessorycustomizer");
            Dictionary<int, CharaEvent.HairAccessoryInfo> Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);

            if (_accessoriesByChar.TryGetValue(ChaControl.chaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(ChaControl.chaFile, data);
            }
            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(setting.CoordinatePartsQueue[ChaFileControl.status.coordinateType]);
            var HairQueue = new Queue<CharaEvent.HairAccessoryInfo>(setting.HairAccQueue[ChaFileControl.status.coordinateType]);
            ExpandedOutfit.Logger.LogWarning($"CPQ: {setting.CoordinatePartsQueue[ChaFileControl.status.coordinateType].Count}");
            ExpandedOutfit.Logger.LogWarning($"HAQ: {setting.HairAccQueue[ChaFileControl.status.coordinateType].Count}");

            int ACCpostion = 0;

            for (int n = ChaControl.nowCoordinate.accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                if (ChaControl.nowCoordinate.accessory.parts[ACCpostion].type == 120) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        ChaControl.nowCoordinate.accessory.parts[ACCpostion] = PartsQueue.Dequeue();
                        ExpandedOutfit.Logger.LogWarning(ChaControl.fileParam.fullname + $" Deque<20");
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
            }
            for (int n = data.nowAccessories.Count; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                if (data.nowAccessories[ACCpostion].type == 120) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        ExpandedOutfit.Logger.LogWarning(ChaControl.fileParam.fullname + $" Deque>20");
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
                        Temp.Add(ACCpostion, HairQueue.Dequeue());
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
            //base.OnCoordinateBeingLoaded(coordinate);
        }
        private IEnumerator WaitForCoords()
        {
            int frames = 0;
            for (int j = 0; j < Constants.outfitpath.Length - 1; j++)
            {
                var Parts = ChaControl.chaFile.coordinate[j].accessory.parts;
                for (int i = 0, n = Parts.Length; i < n; i++)
                {
                    while (Parts[i].type != 120 && Parts[i].id == -1)
                    {
                        yield return 0;
                        ExpandedOutfit.Logger.LogWarning($"Waited {++frames} frames");
                    }
                }
            }
            ExpandedOutfit.Logger.LogWarning($"Coroutine ended {frames} frames");
        }
        private void DangerLoop()
        {
            int frames = 0;
            for (int j = 0; j < Constants.outfitpath.Length - 1; j++)
            {
                var Parts = ChaControl.chaFile.coordinate[j].accessory.parts;
                for (int i = 0, n = Parts.Length; i < n; i++)
                {
                    while (Parts[i].type != 120 && Parts[i].id == 0)
                    {
                        ExpandedOutfit.Logger.LogWarning($"Waited {++frames} Ticks");
                        if (frames % 10 == 0)
                        {
                            ExpandedOutfit.Logger.LogWarning($"at {++frames} Ticks id: {Parts[i].id}");
                        }
                    }
                }
            }
            ExpandedOutfit.Logger.LogWarning($"Coroutine ended {frames} Ticks");
        }
    }
}