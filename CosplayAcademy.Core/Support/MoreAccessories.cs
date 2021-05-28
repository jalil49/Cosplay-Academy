using ExtensibleSaveFormat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;
namespace Cosplay_Academy.Support
{
    public static class MoreAccessories
    {
        private static readonly bool _hasDarkness = typeof(ChaControl).GetMethod("ChangeShakeAccessory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) != null;
        private static readonly Dictionary<MemberKey, PropertyInfo> _propertyCache = new Dictionary<MemberKey, PropertyInfo>();

        public static List<ChaFileAccessory.PartsInfo> Coordinate_Accessory_Extract(ChaFileCoordinate file)
        {
            XmlNode node = null;
            List<ChaFileAccessory.PartsInfo> Accessories = new List<ChaFileAccessory.PartsInfo>();
            PluginData pluginData = ExtendedSave.GetExtendedDataById(file, "moreAccessories");
            if (pluginData != null && pluginData.data.TryGetValue("additionalAccessories", out object xmlData))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml((string)xmlData);
                node = doc.FirstChild;
            }
            if (node != null)
            {
                foreach (XmlNode accessoryNode in node.ChildNodes)
                {
                    ChaFileAccessory.PartsInfo part = new ChaFileAccessory.PartsInfo
                    {
                        type = XmlConvert.ToInt32(accessoryNode.Attributes["type"].Value)
                    };
                    if (part.type != 120)
                    {
                        part.id = XmlConvert.ToInt32(accessoryNode.Attributes["id"].Value);
                        part.parentKey = accessoryNode.Attributes["parentKey"].Value;

                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                part.addMove[i, j] = new Vector3
                                {
                                    x = XmlConvert.ToSingle(accessoryNode.Attributes[$"addMove{i}{j}x"].Value),
                                    y = XmlConvert.ToSingle(accessoryNode.Attributes[$"addMove{i}{j}y"].Value),
                                    z = XmlConvert.ToSingle(accessoryNode.Attributes[$"addMove{i}{j}z"].Value)
                                };
                            }
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            part.color[i] = new Color
                            {
                                r = XmlConvert.ToSingle(accessoryNode.Attributes[$"color{i}r"].Value),
                                g = XmlConvert.ToSingle(accessoryNode.Attributes[$"color{i}g"].Value),
                                b = XmlConvert.ToSingle(accessoryNode.Attributes[$"color{i}b"].Value),
                                a = XmlConvert.ToSingle(accessoryNode.Attributes[$"color{i}a"].Value)
                            };
                        }
                        part.hideCategory = XmlConvert.ToInt32(accessoryNode.Attributes["hideCategory"].Value);
                        if (_hasDarkness)
                            part.SetPrivateProperty("noShake", accessoryNode.Attributes["noShake"] != null && XmlConvert.ToBoolean(accessoryNode.Attributes["noShake"].Value));
                    }
                    Accessories.Add(part);
                }
            }
            return Accessories;
        }

        private static void SetPrivateProperty(this object self, string name, object value)
        {
            MemberKey key = new MemberKey(self.GetType(), name);
            if (!_propertyCache.TryGetValue(key, out PropertyInfo info))
            {
                info = key.type.GetProperty(key.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                _propertyCache.Add(key, info);
            }
            info.SetValue(self, value, null);
        }

        private struct MemberKey
        {
            public readonly Type type;
            public readonly string name;
            private readonly int _hashCode;

            public MemberKey(Type inType, string inName)
            {
                this.type = inType;
                this.name = inName;
                this._hashCode = this.type.GetHashCode() ^ this.name.GetHashCode();
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }
        }
    }
}
