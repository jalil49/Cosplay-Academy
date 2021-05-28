using MessagePack;
using System;
using UnityEngine;

namespace Cosplay_Academy.Hair
{
    #region Stuff Hair Accessories needs
    public class HairSupport
    {
        [Serializable]
        [MessagePackObject]
        public class HairAccessoryInfo
        {
            [Key("HairGloss")]
            public bool HairGloss;
            [Key("ColorMatch")]
            public bool ColorMatch;
            [Key("OutlineColor")]
            public Color OutlineColor;
            [Key("AccessoryColor")]
            public Color AccessoryColor;
            [Key("HairLength")]
            public float HairLength;
        }
        #endregion
    }
}
