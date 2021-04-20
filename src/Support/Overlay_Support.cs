using MessagePack;
using UnityEngine;

namespace Cosplay_Academy.Support
{
    #region Stuff KCOX_RePack Needs
    [MessagePackObject]
    public class ClothesTexData
    {
        [IgnoreMember]
        private byte[] _textureBytes;
        [IgnoreMember]
        private Texture2D _texture;

        [IgnoreMember]
        public Texture2D Texture
        {
            set
            {
                if (value != null && value == _texture) return;
                Object.Destroy(_texture);
                _texture = value;
                _textureBytes = value?.EncodeToPNG();
            }
        }

        [Key(0)]
        public byte[] TextureBytes
        {
            get => _textureBytes;
            set
            {
                Texture = null;
                _textureBytes = value;
            }
        }

        [Key(1)]
        public bool Override;
    }

    #endregion
}
