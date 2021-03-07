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
        private static CharaEvent Controller;
        private static ChaDefault chaDefault;
        public static void FullLoad(ChaControl input, CharaEvent controller, ChaDefault cha)
        {
            chaDefault = cha;
            Controller = controller;
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

            var PartsQueue = new Queue<ChaFileAccessory.PartsInfo>(chaDefault.CoordinatePartsQueue[outfitnum]);
            var HairQueue = new Queue<CharaEvent.HairAccessoryInfo>(chaDefault.HairAccQueue[outfitnum]);

            WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

            if (_accessoriesByChar.TryGetValue(chaControl.chaFile, out MoreAccessories.CharAdditionalData data) == false)
            {
                data = new MoreAccessories.CharAdditionalData();
                _accessoriesByChar.Add(chaControl.chaFile, data);
            }
            #endregion
            //Load new outfit
            chaControl.fileStatus.coordinateType = outfitnum;
            chaControl.chaFile.coordinate[outfitnum].LoadFile(Constants.outfitpath[outfitnum]);
            //Apply pre-existing Accessories in any open slot or final slots.
            #region Reassign Exisiting Accessories

            if (data.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> NewRAW) == false)
            {
                NewRAW = new List<ChaFileAccessory.PartsInfo>();
            }


            var Inputdata = ExtendedSave.GetExtendedDataById(chaControl.chaFile.coordinate[outfitnum], "com.deathweasel.bepinex.hairaccessorycustomizer");
            var Temp = new Dictionary<int, CharaEvent.HairAccessoryInfo>();
            if (Inputdata != null)
                if (Inputdata.data.TryGetValue("CoordinateHairAccessories", out var loadedHairAccessories) && loadedHairAccessories != null)
                    Temp = MessagePackSerializer.Deserialize<Dictionary<int, CharaEvent.HairAccessoryInfo>>((byte[])loadedHairAccessories);


            int ACCpostion = 0;

            bool Empty;
            for (int n = chaControl.chaFile.coordinate[outfitnum].accessory.parts.Length; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
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
                        chaControl.chaFile.coordinate[outfitnum].accessory.parts[ACCpostion] = PartsQueue.Dequeue();

                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
                    }
                }
            }
            for (int n = NewRAW.Count; PartsQueue.Count != 0 && ACCpostion < n; ACCpostion++)
            {
                Empty = NewRAW[ACCpostion].type == 120;
                if (Empty) //120 is empty/default
                {
                    if (!Temp.ContainsKey(ACCpostion))
                    {
                        NewRAW[ACCpostion] = PartsQueue.Dequeue();
                        if (HairQueue.Peek() != null)
                        {
                            Temp.Add(ACCpostion, HairQueue.Dequeue());
                        }
                        else
                        {
                            HairQueue.Dequeue();
                        }
                    }
                }
            }
            bool print = true;
            while (PartsQueue.Count != 0)
            {
                if (print)
                {
                    print = false;
                    ExpandedOutfit.Logger.LogDebug(chaControl.fileParam.fullname + $" Ran out of space for accessories, Making {PartsQueue.Count} space(s) at least (due to potential keys already existing just in case)");
                }
                if (!Temp.ContainsKey(ACCpostion))
                {
                    NewRAW.Add(PartsQueue.Dequeue());
                    if (HairQueue.Peek() != null)
                    {
                        Temp.Add(ACCpostion, HairQueue.Dequeue());
                    }
                    else
                    {
                        HairQueue.Dequeue();
                    }
                }
                else
                {
                    NewRAW.Add(new ChaFileAccessory.PartsInfo());
                }
                //data.infoAccessory.Add(null);
                //data.objAccessory.Add(null);
                //data.objAcsMove.Add(new GameObject[2]);
                //data.cusAcsCmp.Add(null);
                //data.showAccessories.Add(true);
                ACCpostion++;
            }
            //data.rawAccessoriesInfos[outfitnum] = NewRAW;
            HairAccessories.Add(outfitnum, Temp);

            //ExpandedOutfit.Logger.LogWarning($"NA.C:\t{data.nowAccessories.Count}\t {data.infoAccessory.Count}\t {data.objAccessory.Count}\t {data.objAcsMove.Count}\t {data.cusAcsCmp.Count}\t {data.showAccessories.Count}");

            //while (data.infoAccessory.Count < data.nowAccessories.Count)
            //    data.infoAccessory.Add(null);
            //while (data.objAccessory.Count < data.nowAccessories.Count)
            //    data.objAccessory.Add(null);
            //while (data.objAcsMove.Count < data.nowAccessories.Count)
            //    data.objAcsMove.Add(new GameObject[2]);
            //while (data.cusAcsCmp.Count < data.nowAccessories.Count)
            //    data.cusAcsCmp.Add(null);
            //while (data.showAccessories.Count < data.nowAccessories.Count)
            //    data.showAccessories.Add(true);
            //ExpandedOutfit.Logger.LogWarning($"NA.C:\t{data.nowAccessories.Count}\t {data.infoAccessory.Count}\t {data.objAccessory.Count}\t {data.objAcsMove.Count}\t {data.cusAcsCmp.Count}\t {data.showAccessories.Count}");

            //Traverse.Create(MoreAccessories._self).Method("UpdateUI").GetValue();

            //Traverse.Create(_accessoriesByChar).Method("Purge").GetValue();
            #endregion
        }
    }
}