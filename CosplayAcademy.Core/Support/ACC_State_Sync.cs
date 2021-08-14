using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cosplay_Academy.Support
{
    public static class AccStateSync
    {
        [Serializable]
        [MessagePackObject]
        public class TriggerProperty
        {
            [Key("Coordinate")]
            public int Coordinate { get; set; } = -1;

            [Key("Slot")]
            public int Slot { get; set; } = -1;

            [Key("RefKind")]
            public int RefKind { get; set; } = -1;

            [Key("RefState")]
            public int RefState { get; set; } = -1;

            [Key("Visible")]
            public bool Visible { get; set; } = true;

            [Key("Priority")]
            public int Priority { get; set; } = 0;

            [SerializationConstructor]
            public TriggerProperty(int coordinate, int slot, int refkind, int refstate, bool visible, int priority) // this shit is here to avoid msgpack fucking error
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = refkind;
                RefState = refstate;
                Visible = visible;
                Priority = priority;
            }

            public TriggerProperty(int coordinate, int slot, int refkind, int refstate)
            {
                Coordinate = coordinate;
                Slot = slot;
                RefKind = refkind;
                RefState = refstate;
            }
        }

        [Serializable]
        [MessagePackObject]
        public class TriggerGroup
        {
            [Key("Coordinate")]
            public int Coordinate { get; set; } = -1;

            [Key("Kind")]
            public int Kind { get; set; } // TriggerProperty RefGroup

            [Key("State")]
            public int State { get; set; } // Current RefState

            [Key("States")]
            public Dictionary<int, string> States { get; set; }

            [Key("Label")]
            public string Label { get; set; } = "";

            [Key("Startup")]
            public int Startup { get; set; } = 0;

            [Key("Secondary")]
            public int Secondary { get; set; } = -1;

            [Key("GUID")]
            public string GUID { get; set; } = Guid.NewGuid().ToString("D").ToUpper();

            [SerializationConstructor]
            public TriggerGroup(int coordinate, int kind, string label, int state, int startup, int secondary)
            {
                Coordinate = coordinate;
                Kind = kind;
                State = state;
                if (label.Trim().IsNullOrEmpty())
                    label = $"Custom {kind - 8}";
                Label = label;
                Startup = startup;
                Secondary = secondary;

                States = new Dictionary<int, string>() { [0] = "State 1", [1] = "State 2" };
            }
            public TriggerGroup(int coordinate, int kind, string label, int startup, int secondary) : this(coordinate, kind, label, 0, startup, secondary) { }
            public TriggerGroup(int coordinate, int kind, string label = "") : this(coordinate, kind, label, 0, 0, -1) { }

            public void Rename(string label)
            {
                if (label.Trim().IsNullOrEmpty())
                    label = $"Custom {Kind - 8}";
                Label = label;
            }

            public void RenameState(int state, string label)
            {
                if (!States.ContainsKey(state))
                    return;
                if (label.Trim().IsNullOrEmpty())
                    label = $"State {state + 1}";
                States[state] = label;
            }

            public int GetNewStateID()
            {
                return States.OrderByDescending(x => x.Key).FirstOrDefault().Key + 1;
            }

            public int AddNewState()
            {
                int state = States.OrderByDescending(x => x.Key).FirstOrDefault().Key + 1;
                return AddNewState(state);
            }
            public int AddNewState(int state)
            {
                string label = $"State {state + 1}";
                States[state] = label;
                return state;
            }
        }

        internal static List<string> _cordNames = new List<string>();
        internal static List<string> _clothesNames = new List<string>() { "Top", "Bottom", "Bra", "Underwear", "Gloves", "Pantyhose", "Legwear", "Indoors", "Outdoors" };
        internal static List<string> _statesNames = new List<string>() { "Full", "Half 1", "Half 2", "Undressed" };
        internal static Dictionary<string, string> _accessoryParentNames = new Dictionary<string, string>();

        static AccStateSync()
        {
            _cordNames = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList();

            foreach (object _key in Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey)))
                _accessoryParentNames[_key.ToString()] = ChaAccessoryDefine.dictAccessoryParent[(int)_key];
        }

        public partial class Migration
        {
            [Serializable]
            [MessagePackObject]
            public class OutfitTriggerInfoV1
            {
                [Key("Index")]
                public int Index { get; set; }
                [Key("Parts")]
                public List<AccTriggerInfo> Parts { get; set; } = new List<AccTriggerInfo>();

                public OutfitTriggerInfoV1(int index) { Index = index; }
            }

            internal static OutfitTriggerInfo UpgradeOutfitTriggerInfoV1(OutfitTriggerInfoV1 _oldOutfitTriggerInfo)
            {
                OutfitTriggerInfo _outfitTriggerInfo = new OutfitTriggerInfo(_oldOutfitTriggerInfo.Index);
                if (_oldOutfitTriggerInfo.Parts.Count() > 0)
                {
                    for (int j = 0; j < _oldOutfitTriggerInfo.Parts.Count(); j++)
                    {
                        AccTriggerInfo Itrigger = _oldOutfitTriggerInfo.Parts[j];
                        if (Itrigger.Kind > -1)
                        {
                            _outfitTriggerInfo.Parts[j] = new AccTriggerInfo(j);
                            CopySlotTriggerInfo(Itrigger, _outfitTriggerInfo.Parts[j]);
                        }
                    }
                }
                return _outfitTriggerInfo;
            }

            [Serializable]
            [MessagePackObject]
            public class AccTriggerInfo
            {
                [Key("Slot")]
                public int Slot { get; set; }
                [Key("Kind")]
                public int Kind { get; set; } = -1;
                [Key("Group")]
                public string Group { get; set; } = "";
                [Key("State")]
                public List<bool> State { get; set; } = new List<bool>() { true, false, false, false };

                public AccTriggerInfo(int slot) { Slot = slot; }
            }

            [Serializable]
            [MessagePackObject]
            public class OutfitTriggerInfo
            {
                [Key("Index")]
                public int Index { get; set; }
                [Key("Parts")]
                public Dictionary<int, AccTriggerInfo> Parts { get; set; } = new Dictionary<int, AccTriggerInfo>();

                public OutfitTriggerInfo(int index) { Index = index; }
            }

            [Serializable]
            [MessagePackObject]
            public class VirtualGroupInfo
            {
                [Key("Kind")]
                public int Kind { get; set; }
                [Key("Group")]
                public string Group { get; set; }
                [Key("Label")]
                public string Label { get; set; }
                [Key("Secondary")]
                public bool Secondary { get; set; } = false;
                [Key("State")]
                public bool State { get; set; } = true;

                public VirtualGroupInfo(string group, int kind, string label = "")
                {
                    Group = group;
                    Kind = kind;
                    if (label.IsNullOrEmpty())
                    {
                        if (kind > 9)
                            label = group.Replace("custom_", "Custom ");
                        else if (kind == 9)
                        {
                            label = Group;
                            if (_accessoryParentNames.ContainsKey(Group))
                                label = _accessoryParentNames[Group];
                        }
                    }
                    Label = label;
                }
            }

            internal static Dictionary<string, string> UpgradeVirtualGroupNamesV1(Dictionary<string, string> _oldVirtualGroupNames)
            {
                Dictionary<string, string> _outfitVirtualGroupInfo = new Dictionary<string, string>();
                if (_oldVirtualGroupNames?.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> _group in _oldVirtualGroupNames)
                        _outfitVirtualGroupInfo[_group.Key] = _group.Value;
                }
                return _outfitVirtualGroupInfo;
            }

            internal static Dictionary<string, VirtualGroupInfo> UpgradeVirtualGroupNamesV2(Dictionary<string, string> _oldVirtualGroupNames)
            {
                Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                if (_oldVirtualGroupNames?.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> _group in _oldVirtualGroupNames)
                    {
                        if (_group.Key.StartsWith("custom_"))
                            _outfitVirtualGroupInfo[_group.Key] = new VirtualGroupInfo(_group.Key, int.Parse(_group.Key.Replace("custom_", "")) + 9, _group.Value);
                    }
                }
                return _outfitVirtualGroupInfo;
            }

            internal static void ConvertCharaPluginData(PluginData _pluginData, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                Dictionary<int, OutfitTriggerInfo> _charaTriggerInfo = new Dictionary<int, OutfitTriggerInfo>();
                Dictionary<int, Dictionary<string, VirtualGroupInfo>> _charaVirtualGroupInfo = new Dictionary<int, Dictionary<string, VirtualGroupInfo>>();

                _pluginData.data.TryGetValue("CharaTriggerInfo", out object _loadedCharaTriggerInfo);
                if (_loadedCharaTriggerInfo == null) return;

                if (_pluginData.version < 2)
                {
                    List<OutfitTriggerInfoV1> _oldCharaTriggerInfo = MessagePackSerializer.Deserialize<List<OutfitTriggerInfoV1>>((byte[])_loadedCharaTriggerInfo);
                    for (int i = 0; i < 7; i++)
                        _charaTriggerInfo[i] = UpgradeOutfitTriggerInfoV1(_oldCharaTriggerInfo[i]);
                }
                else
                    _charaTriggerInfo = MessagePackSerializer.Deserialize<Dictionary<int, OutfitTriggerInfo>>((byte[])_loadedCharaTriggerInfo);

                if (_charaTriggerInfo == null) return;

                if (_pluginData.version < 5)
                {
                    if (_pluginData.data.TryGetValue("CharaVirtualGroupNames", out object _loadedCharaVirtualGroupNames) && _loadedCharaVirtualGroupNames != null)
                    {
                        if (_pluginData.version < 2)
                        {
                            List<Dictionary<string, string>> _oldCharaVirtualGroupNames = MessagePackSerializer.Deserialize<List<Dictionary<string, string>>>((byte[])_loadedCharaVirtualGroupNames);
                            if (_oldCharaVirtualGroupNames?.Count == 7)
                            {
                                for (int i = 0; i < 7; i++)
                                {
                                    Dictionary<string, string> _outfitVirtualGroupNames = UpgradeVirtualGroupNamesV1(_oldCharaVirtualGroupNames[i]);
                                    _charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(_outfitVirtualGroupNames);
                                }
                            }
                        }
                        else
                        {
                            Dictionary<int, Dictionary<string, string>> _charaVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>((byte[])_loadedCharaVirtualGroupNames);
                            for (int i = 0; i < 7; i++)
                                _charaVirtualGroupInfo[i] = UpgradeVirtualGroupNamesV2(_charaVirtualGroupNames[i]);
                        }
                    }
                }
                else
                {
                    if (_pluginData.data.TryGetValue("CharaVirtualGroupInfo", out object _loadedCharaVirtualGroupInfo) && _loadedCharaVirtualGroupInfo != null)
                        _charaVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<string, VirtualGroupInfo>>>((byte[])_loadedCharaVirtualGroupInfo);
                }

                Migrate(_charaTriggerInfo, _charaVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
            }

            internal static void ConvertOutfitPluginData(int _coordinate, PluginData _pluginData, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                OutfitTriggerInfo _outfitTriggerInfo = null;
                Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                _pluginData.data.TryGetValue("OutfitTriggerInfo", out object _loadedOutfitTriggerInfo);
                if (_loadedOutfitTriggerInfo == null) return;

                if (_pluginData.version < 2)
                {
                    OutfitTriggerInfoV1 _oldCharaTriggerInfo = MessagePackSerializer.Deserialize<OutfitTriggerInfoV1>((byte[])_loadedOutfitTriggerInfo);
                    _outfitTriggerInfo = UpgradeOutfitTriggerInfoV1(_oldCharaTriggerInfo);
                }
                else
                    _outfitTriggerInfo = MessagePackSerializer.Deserialize<OutfitTriggerInfo>((byte[])_loadedOutfitTriggerInfo);

                if (_outfitTriggerInfo == null) return;

                if (_pluginData.version < 5)
                {
                    if (_pluginData.data.TryGetValue("OutfitVirtualGroupNames", out object _loadedOutfitVirtualGroupNames) && _loadedOutfitVirtualGroupNames != null)
                    {
                        Dictionary<string, string> _outfitVirtualGroupNames = MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])_loadedOutfitVirtualGroupNames);
                        _outfitVirtualGroupInfo = UpgradeVirtualGroupNamesV2(_outfitVirtualGroupNames);
                    }
                }
                else
                {
                    if (_pluginData.data.TryGetValue("OutfitVirtualGroupInfo", out object _loadedOutfitVirtualGroupInfo) && _loadedOutfitVirtualGroupInfo != null)
                        _outfitVirtualGroupInfo = MessagePackSerializer.Deserialize<Dictionary<string, VirtualGroupInfo>>((byte[])_loadedOutfitVirtualGroupInfo);
                }

                Migrate(_coordinate, _outfitTriggerInfo, _outfitVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
            }

            public static void Migrate(Dictionary<int, OutfitTriggerInfo> _charaTriggerInfo, Dictionary<int, Dictionary<string, VirtualGroupInfo>> _charaVirtualGroupInfo, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                for (int _coordinate = 0; _coordinate < 7; _coordinate++)
                {
                    OutfitTriggerInfo _outfitTriggerInfo = _charaTriggerInfo.ContainsKey(_coordinate) ? _charaTriggerInfo[_coordinate] : new OutfitTriggerInfo(_coordinate);
                    Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo = null;
                    if (!_charaVirtualGroupInfo.ContainsKey(_coordinate) || _charaVirtualGroupInfo[_coordinate]?.Count == 0)
                        _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();
                    else
                        _outfitVirtualGroupInfo = _charaVirtualGroupInfo[_coordinate];
                    Migrate(_coordinate, _outfitTriggerInfo, _outfitVirtualGroupInfo, ref _outputTriggerProperty, ref _outputTriggerGroup);
                }
            }

            public static void Migrate(int _coordinate, OutfitTriggerInfo _outfitTriggerInfo, Dictionary<string, VirtualGroupInfo> _outfitVirtualGroupInfo, ref List<TriggerProperty> _outputTriggerProperty, ref List<TriggerGroup> _outputTriggerGroup)
            {
                if (_outfitTriggerInfo == null) return;
                if (_outfitVirtualGroupInfo == null)
                    _outfitVirtualGroupInfo = new Dictionary<string, VirtualGroupInfo>();

                Dictionary<string, int> _mapping = new Dictionary<string, int>();
                int _refBase = 9;

                List<AccTriggerInfo> _parts = _outfitTriggerInfo.Parts.Values.OrderBy(x => x.Kind).ThenBy(x => x.Group).ThenBy(x => x.Slot).ToList();
                foreach (AccTriggerInfo _part in _parts)
                {
                    if (MathfEx.RangeEqualOn(0, _part.Kind, 8))
                    {
                        for (int i = 0; i < 4; i++)
                            _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _part.Kind, i, _part.State[i], 0));
                    }
                    else if (_part.Kind >= 9)
                    {
                        if (!_mapping.ContainsKey(_part.Group))
                        {
                            _mapping[_part.Group] = _refBase;
                            _refBase++;
                        }

                        _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _mapping[_part.Group], 0, _part.State[0], 0));
                        _outputTriggerProperty.Add(new TriggerProperty(_coordinate, _part.Slot, _mapping[_part.Group], 1, _part.State[3], 0));
                    }
                }

                foreach (KeyValuePair<string, int> x in _mapping)
                {
                    if (!_outfitVirtualGroupInfo.ContainsKey(x.Key))
                    {
                        string _label = _accessoryParentNames.ContainsKey(x.Key) ? _accessoryParentNames[x.Key] : x.Key;
                        _outputTriggerGroup.Add(new TriggerGroup(_coordinate, x.Value, _label));
                    }
                    else
                    {
                        VirtualGroupInfo _group = _outfitVirtualGroupInfo[x.Key];
                        _outputTriggerGroup.Add(new TriggerGroup(_coordinate, x.Value, _group.Label, (_group.State ? 0 : 1), 0, (_group.Secondary ? 1 : -1)));
                    }
                }
            }

            public static void CopySlotTriggerInfo(AccTriggerInfo CopySource, AccTriggerInfo CopyDestination)
            {
                CopyDestination.Slot = CopySource.Slot;
                CopyDestination.Kind = CopySource.Kind;
                CopyDestination.Group = CopySource.Group;
                CopyDestination.State = CopySource.State.ToList();
            }
        }
    }
}
