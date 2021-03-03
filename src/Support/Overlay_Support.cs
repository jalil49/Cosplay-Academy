////using System;
////using System.Diagnostics;
////using System.Runtime.InteropServices;
////using UnityEngine;
////using MessagePack;

////namespace Cosplay_Academy.Support
////{
//public void KCOX_RePack(UnityEngine.Component ClothingController)//Take in an Array of Coordinate PluginData, and set it in Chafile for reload (used in Cosplay Academy to Load clothing textures)
//{
//    PluginData SavedData;
//    var data = new PluginData { version = 1 };
//    Dictionary<string, ClothesTexData> storage;
//    Dictionary<CoordinateType, Dictionary<string, ClothesTexData>> Final = new Dictionary<CoordinateType, Dictionary<string, ClothesTexData>>();
//    for (int i = 0; i < ChaControl.chaFile.coordinate.Length; i++)
//    {
//        SavedData = ExtendedSave.GetExtendedDataById(ChaControl.chaFile.coordinate[i], "KCOX");
//        storage = new Dictionary<string, ClothesTexData>();
//        if (SavedData != null && SavedData.data.TryGetValue("Overlays", out var bytes) && bytes is byte[] byteArr)
//        {
//            var dict = MessagePackSerializer.Deserialize<Dictionary<string, ClothesTexData>>(byteArr);
//            if (dict != null)
//            {
//                foreach (var texData in dict)
//                    storage.Add(texData.Key, texData.Value);
//            }
//        }
//        Final.Add((CoordinateType)i, storage);
//    }
//    data.data.Add("Overlays", MessagePackSerializer.Serialize(Final));
//    SetExtendedData("KCOX", data);
//}
//#region Stuff KCOX_RePack Needs
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
//                    _texture = Util.TextureFromBytes(_textureBytes, KoiSkinOverlayMgr.GetSelectedOverlayTexFormat(false));
//            }
//            return _texture;
//        }
//        set
//        {
//            if (value != null && value == _texture) return;
//            Object.Destroy(_texture);
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
//        Object.Destroy(_texture);
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

//#endregion
////}
