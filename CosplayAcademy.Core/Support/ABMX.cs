using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoordinateType = ChaFileDefine.CoordinateType;

namespace Cosplay_Academy.Support
{
    public static class ABMX
    {
        [MessagePackObject]
        public sealed class BoneModifier
        {
            internal static readonly int CoordinateCount = Enum.GetValues(typeof(CoordinateType)).Length;

            /// <summary>
            /// Create empty modifier that is not coordinate specific
            /// </summary>
            /// <param name="boneName">Name of the bone transform to affect</param>
            public BoneModifier(string boneName) : this(boneName, new[] { new BoneModifierData() }) { }

            /// <param name="boneName">Name of the bone transform to affect</param>
            /// <param name="coordinateModifiers">
            /// Needs to be either 1 long to apply to all coordinates or 7 to apply to specific
            /// coords
            /// </param>
            public BoneModifier(string boneName, BoneModifierData[] coordinateModifiers)
            {
                if (string.IsNullOrEmpty(boneName))
                    throw new ArgumentException("Invalid boneName - " + boneName, nameof(boneName));
                if (coordinateModifiers == null)
                    throw new ArgumentNullException(nameof(coordinateModifiers));

                BoneName = boneName;
                CoordinateModifiers = coordinateModifiers.ToArray();
            }

            /// <summary>
            /// Name of the targetted bone
            /// </summary>
            [Key(0)]
            public string BoneName { get; }

            /// <summary>
            /// Transform of the targetted bone
            /// </summary>
            [IgnoreMember]
            public Transform BoneTransform { get; internal set; }

            /// <summary>
            /// Actual modifier values, split for different coordinates if required
            /// </summary>
            [Key(1)]
            // Needs a public set to make serializing work
            public BoneModifierData[] CoordinateModifiers { get; set; }

            public BoneModifierData GetModifier(CoordinateType coordinate)
            {
                if (CoordinateModifiers.Length == 1) return CoordinateModifiers[0];
                return CoordinateModifiers[(int)coordinate];
            }

            /// <summary>
            /// Check if this modifier has unique values for each coordinate, or one set of values for all coordinates
            /// </summary>
            public bool IsCoordinateSpecific()
            {
#if AI || HS2
            // No coordinate saving in AIS
            return false;
#else
                return CoordinateModifiers.Length == CoordinateCount;
#endif
            }

            /// <summary>
            /// If this modifier is not coordinate specific, make it coordinate specific (one set of values for each outfit)
            /// </summary>
            public void MakeCoordinateSpecific()
            {
                if (!IsCoordinateSpecific())
                    CoordinateModifiers = Enumerable.Range(0, CoordinateCount).Select(_ => CoordinateModifiers[0].Clone()).ToArray();
            }
        }

        [MessagePackObject]
        public sealed class BoneModifierData
        {
            public static readonly BoneModifierData Default = new BoneModifierData();

            [Key(0)]
            public Vector3 ScaleModifier;

            [Key(1)]
            public float LengthModifier;

            [Key(2)]
            public Vector3 PositionModifier;

            [Key(3)]
            public Vector3 RotationModifier;

            public BoneModifierData() : this(Vector3.one, 1, Vector3.zero, Vector3.zero) { }
            public BoneModifierData(Vector3 scaleModifier, float lengthModifier) : this(scaleModifier, lengthModifier, Vector3.zero, Vector3.zero) { }
            public BoneModifierData(Vector3 scaleModifier, float lengthModifier, Vector3 positionModifier, Vector3 rotationModifier)
            {
                ScaleModifier = scaleModifier;
                LengthModifier = lengthModifier;
                PositionModifier = positionModifier;
                RotationModifier = rotationModifier;
            }

            public BoneModifierData Clone()
            {
                return (BoneModifierData)MemberwiseClone();
            }

            public bool HasLength()
            {
                return LengthModifier != 1;
            }

            public bool HasScale()
            {
                return ScaleModifier.x != 1 || ScaleModifier.y != 1 || ScaleModifier.z != 1;
            }

            public bool HasPosition()
            {
                return PositionModifier.x != 0 || PositionModifier.y != 0 || PositionModifier.z != 0;
            }

            public bool HasRotation()
            {
                return RotationModifier.x != 0 || RotationModifier.y != 0 || RotationModifier.z != 0;
            }

            public bool IsEmpty()
            {
                return ScaleModifier.x == 1 && ScaleModifier.y == 1 && ScaleModifier.z == 1 &&
                    PositionModifier.x == 0 && PositionModifier.y == 0 && PositionModifier.z == 0 &&
                    RotationModifier.x == 0 && RotationModifier.y == 0 && RotationModifier.z == 0 &&
                    LengthModifier == 1;
            }

            public void Clear()
            {
                ScaleModifier = Vector3.one;
                RotationModifier = Vector3.zero;
                PositionModifier = Vector3.zero;
                LengthModifier = 1;
            }
        }

        private const string ExtDataBoneDataKey = "boneData";

        public static List<BoneModifier> MigrateOldExtData(PluginData pluginData)
        {
            if (pluginData == null) return null;
            if (!pluginData.data.TryGetValue(ExtDataBoneDataKey, out var value)) return null;
            if (!(value is string textData)) return null;

            return MigrateOldStringData(textData);
        }

        public static List<BoneModifier> MigrateOldStringData(string textData)
        {
            if (string.IsNullOrEmpty(textData)) return null;
            return DeserializeToModifiers(textData.Split());
        }

        private static List<BoneModifier> DeserializeToModifiers(IEnumerable<string> lines)
        {
            string GetTrimmedName(string[] splitValues)
            {
                // Turn cf_d_sk_top__1 into cf_d_sk_top 
                var boneName = splitValues[1];
                return boneName[boneName.Length - 2] == '_' && boneName[boneName.Length - 3] == '_'
                    ? boneName.Substring(0, boneName.Length - 3)
                    : boneName;
            }

            var query = from lineText in lines
                        let trimmedText = lineText?.Trim()
                        where !string.IsNullOrEmpty(trimmedText)
                        let splitValues = trimmedText.Split(',')
                        where splitValues.Length >= 6
                        group splitValues by GetTrimmedName(splitValues);

            var results = new List<BoneModifier>();

            foreach (var groupedBoneDataEntries in query)
            {
                var groupedOrderedEntries = groupedBoneDataEntries.OrderBy(x => x[1]).ToList();

                var coordinateModifiers = new List<BoneModifierData>(groupedOrderedEntries.Count);

                foreach (var singleEntry in groupedOrderedEntries)
                {
                    try
                    {
                        //var boneName = singleEntry[1];
                        //var isEnabled = bool.Parse(singleEntry[2]);
                        var x = float.Parse(singleEntry[3]);
                        var y = float.Parse(singleEntry[4]);
                        var z = float.Parse(singleEntry[5]);

                        var lenMod = singleEntry.Length > 6 ? float.Parse(singleEntry[6]) : 1f;

                        coordinateModifiers.Add(new BoneModifierData(new Vector3(x, y, z), lenMod));
                    }
                    catch (Exception ex)
                    {
                        Settings.Logger.LogError($"Failed to load legacy line \"{string.Join(",", singleEntry)}\" - {ex.Message}");
                    }
                }

                if (coordinateModifiers.Count == 0)
                    continue;

                if (coordinateModifiers.Count > 7)
                    coordinateModifiers.RemoveRange(0, coordinateModifiers.Count - 7);
                if (coordinateModifiers.Count > 1 && coordinateModifiers.Count < 7)
                    coordinateModifiers.RemoveRange(0, coordinateModifiers.Count - 1);

                results.Add(new BoneModifier(groupedBoneDataEntries.Key, coordinateModifiers.ToArray()));
            }

            return results;
        }
    }
}
