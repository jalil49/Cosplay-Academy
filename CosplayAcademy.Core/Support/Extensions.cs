using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static List<T> ToNewList<T>(this List<T> value)
        {
            if (value != null)
            {
                return new List<T>(value);
            }
            return new List<T>();
        }

        public static List<T> ToNewList<T>(this List<T> value, T initialvalue)
        {
            if (value != null)
            {
                return new List<T>(value);
            }
            return new List<T>() { initialvalue };
        }

        public static Dictionary<T1, T2> ToNewDictionary<T1, T2>(this Dictionary<T1, T2> value)
        {
            if (value != null)
            {
                return new Dictionary<T1, T2>(value);
            }

            return new Dictionary<T1, T2>();
        }

        public static bool[] ToNewArray(this bool[] value, int size)
        {
            var array = new bool[size];
            if (value != null)
            {
                for (var i = 0; i < size; i++)
                {
                    array[i] = value[i];
                }
                return array;
            }
            return array;
        }

        public static Color[] ToNewArray(this Color[] value, int size)
        {
            var array = new Color[size];
            if (value != null)
            {
                for (var i = 0; i < size; i++)
                {
                    var color = value[i];
                    array[i] = new Color(color.r, color.g, color.b, color.a);
                }
                return array;
            }
            for (var i = 0; i < size; i++)
            {
                array[i] = new Color();
            }
            return array;
        }

        public static bool AllFalse(this ChaFileParameter.Attribute attribute)
        {
#if KK
            return !(attribute.bitch || attribute.choroi || attribute.dokusyo || attribute.friendly || attribute.harapeko || attribute.hinnyo || attribute.hitori || attribute.kireizuki || attribute.likeGirls || attribute.majime || attribute.mutturi || attribute.ongaku || attribute.sinsyutu || attribute.ukemi || attribute.undo || attribute.donkan || attribute.kappatu || attribute.taida);
#elif KKS
            return !(attribute.bitch || attribute.choroi || attribute.dokusyo || attribute.friendly || attribute.harapeko || attribute.hinnyo || attribute.hitori || attribute.kireizuki || attribute.likeGirls || attribute.majime || attribute.mutturi || attribute.ongaku || attribute.sinsyutu || attribute.ukemi || attribute.undo || attribute.active || attribute.info || attribute.lonely || attribute.love || attribute.nakama || attribute.nonbiri || attribute.okute || attribute.talk);
#endif
        }

        public static bool AnyOverlap(this ChaFileParameter.Attribute attribute, ChaFileParameter.Attribute compare)
        {
#if KK
            if (attribute.bitch && compare.bitch || attribute.choroi && compare.choroi || attribute.dokusyo && compare.dokusyo || attribute.friendly && compare.friendly || attribute.harapeko && compare.harapeko || attribute.hinnyo && compare.hinnyo || attribute.hitori && compare.hitori || attribute.kireizuki && compare.kireizuki || attribute.likeGirls && compare.likeGirls || attribute.majime && compare.majime || attribute.mutturi && compare.mutturi || attribute.ongaku && compare.ongaku || attribute.sinsyutu && compare.sinsyutu || attribute.ukemi && compare.ukemi || attribute.undo && compare.undo || attribute.donkan && compare.donkan || attribute.kappatu && compare.kappatu || attribute.taida && compare.taida)
#elif KKS
            if (attribute.bitch && compare.bitch || attribute.choroi && compare.choroi || attribute.dokusyo && compare.dokusyo || attribute.friendly && compare.friendly || attribute.harapeko && compare.harapeko || attribute.hinnyo && compare.hinnyo || attribute.hitori && compare.hitori || attribute.kireizuki && compare.kireizuki || attribute.likeGirls && compare.likeGirls || attribute.majime && compare.majime || attribute.mutturi && compare.mutturi || attribute.ongaku && compare.ongaku || attribute.sinsyutu && compare.sinsyutu || attribute.ukemi && compare.ukemi || attribute.undo && compare.undo || attribute.active && compare.active || attribute.info && compare.info || attribute.lonely && compare.lonely || attribute.love && compare.love || attribute.nakama && compare.nakama || attribute.nonbiri && compare.nonbiri || attribute.okute && compare.okute || attribute.talk && compare.talk)
#endif
            {
                return true;
            }
            return false;
        }

        public static bool Compare(this ChaFileParameter parameter, ChaFileParameter compare)
        {
            return parameter.fullname == compare.fullname && parameter.strBirthDay == compare.strBirthDay && parameter.personality == compare.personality && parameter.voicePitch == compare.voicePitch;
        }
    }
}
