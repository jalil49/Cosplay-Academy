using ExtensibleSaveFormat;
using HarmonyLib;
using MessagePack;
using MoreAccessoriesKOI;
using System.Collections.Generic;
using System.Linq;
using ToolBox;
using UnityEngine;
namespace Cosplay_Academy
{
    public static class ClothingLoader
    {
        private static ChaControl chaControl;
        private static Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>> HairAccessories;
        public static void FullLoad(ChaControl input)
        {
            var data = new PluginData();
            HairAccessories = new Dictionary<int, Dictionary<int, CharaEvent.HairAccessoryInfo>>();
            chaControl = input;
            UniformLoad();
            AfterSchoolLoad();
            GymLoad();
            SwimLoad();
            ClubLoad();
            CasualLoad();
            NightwearLoad();
            data.data.Add("HairAccessories", MessagePackSerializer.Serialize(HairAccessories));
            CharaEvent.self.SetExtendedData("com.deathweasel.bepinex.hairaccessorycustomizer", data);
        }
        private static void UniformLoad()
        {
            Generalized(0);
            ExpandedOutfit.Logger.LogDebug("loaded 0 " + Constants.outfitpath[0]);
        }
        private static void AfterSchoolLoad()
        {
            Generalized(1);
            ExpandedOutfit.Logger.LogDebug("loaded 1 " + Constants.outfitpath[1]);
        }
        private static void GymLoad()
        {
            Generalized(2);
            ExpandedOutfit.Logger.LogDebug("loaded 2 " + Constants.outfitpath[2]);
        }
        private static void SwimLoad()
        {
            Generalized(3);
            ExpandedOutfit.Logger.LogDebug("loaded 3 " + Constants.outfitpath[3]);
        }
        private static void ClubLoad()
        {
            Generalized(4);
            ExpandedOutfit.Logger.LogDebug("loaded 4 " + Constants.outfitpath[4]);
        }
        private static void CasualLoad()
        {
            Generalized(5);
            ExpandedOutfit.Logger.LogDebug("loaded 5 " + Constants.outfitpath[5]);
        }
        private static void NightwearLoad()
        {
            Generalized(6);
            ExpandedOutfit.Logger.LogDebug("loaded 6 " + Constants.outfitpath[6]);
        }
        private static void Generalized(int outfitnum)
        {
            //queue Accessorys to keep
            #region Queue accessories to keep
            //PartofHead doesn't work at this stage, I checked.... long after making inclusion
            //Queue<ChaFileAccessory.PartsInfo> import = new Queue<ChaFileAccessory.PartsInfo>();
            Queue<ChaFileAccessory.PartsInfo> import = new Queue<ChaFileAccessory.PartsInfo>();
            Queue<CharaEvent.HairAccessoryInfo> Subimport = new Queue<CharaEvent.HairAccessoryInfo>();
            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

            Dictionary<int, CharaEvent.HairAccessoryInfo> Temp;

            var Inputdata = ExtendedSave.GetExtendedDataById(chaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
            Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);


            if (_accessoriesByChar.TryGetValue(chaControl.chaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(chaControl.chaFile, data);
            }
            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out data.nowAccessories) == false)
            {
                data.nowAccessories = new List<ChaFileAccessory.PartsInfo>();
                //data.rawAccessoriesInfos.Add(outfitnum, data.nowAccessories);
            }
            data.nowAccessories.AddRange(chaControl.chaFile.coordinate[outfitnum].accessory.parts);

            for (int i = 0; i < data.nowAccessories.Count; i++)
            {
                //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                if (Constants.Inclusion.Contains(data.nowAccessories[i].parentKey))
                {
                    if (!Temp.TryGetValue(i, out CharaEvent.HairAccessoryInfo ACCdata))
                    {
                        ACCdata = null;
                    }
                    import.Enqueue(data.nowAccessories[i]);
                    Subimport.Enqueue(ACCdata);
                }
            }
            data.nowAccessories.Clear();
            #endregion
            //Load new outfit
            chaControl.fileStatus.coordinateType = outfitnum;
            chaControl.chaFile.coordinate[outfitnum].LoadFile(Constants.outfitpath[outfitnum]);
            //Apply pre-existing Accessories in any open slot or final slots.
            //bool Force;
            #region Reassign Exisiting Accessories

            Inputdata = ExtendedSave.GetExtendedDataById(chaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
            Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);


            int ACCpostion = 0;

            bool Empty;
            for (int n = chaControl.chaFile.coordinate[outfitnum].accessory.parts.Length; import.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                //Force = (import.Count + i == n);
                Empty = chaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion].type == 120;
                if (Empty/* || Force*/) //120 is empty/default
                {
                    //if (!Empty && Force)
                    //{
                    //    ExpandedOutfit.Logger.LogDebug($"Overwriting Accessory (ID:{chaControl.chaFile.coordinate[outfitnum].accessory.parts[i].id}) at {i + 1} with default head accessory");
                    //}
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        chaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion] = import.Dequeue();

                        if (Subimport.Peek() != null)
                        {
                            Temp.Add(ACCpostion, Subimport.Dequeue());
                        }
                        else
                        {
                            Subimport.Dequeue();
                        }
                    }
                }
            }
            for (int n = data.nowAccessories.Count; import.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = data.nowAccessories[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        data.nowAccessories[ACCpostion] = import.Dequeue();
                        if (Subimport.Peek() != null)
                        {
                            Temp.Add(ACCpostion, Subimport.Dequeue());
                        }
                        else
                        {
                            Subimport.Dequeue();
                        }
                    }
                }
            }
            bool print = true;
            while (import.Count != 0)
            {
                if (print)
                {
                    print = false;
                    ExpandedOutfit.Logger.LogDebug(chaControl.fileParam.fullname + $" Ran out of space for accessories, Making {import.Count} space(s) at least (due to potential keys already existing just in case)");
                }
                if (!Temp.ContainsKey(ACCpostion))
                {
                    data.nowAccessories.Add(import.Dequeue());
                    if (Subimport.Peek() != null)
                    {
                        Temp.Add(ACCpostion, Subimport.Dequeue());
                    }
                    else
                    {
                        Subimport.Dequeue();
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
            HairAccessories.Add(outfitnum, Temp);
            #endregion
        }
    }
}