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
        private static readonly bool ColorMatchDefault = true;
        private static readonly bool HairGlossDefault = true;
        private static Color OutlineColorDefault = Color.black;
        private static Color AccessoryColorDefault = Color.red;
        private static readonly float HairLengthDefault = 0;
        #endregion
    }
}
