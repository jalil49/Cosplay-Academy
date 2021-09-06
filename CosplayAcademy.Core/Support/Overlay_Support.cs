using MessagePack;

namespace Cosplay_Academy.Support
{
    #region Stuff KCOX_RePack Needs
    [MessagePackObject]
    public class ClothesTexData
    {
        [Key(0)]
        public byte[] TextureBytes;

        [Key(1)]
        public bool Override;
    }

    #endregion
}
