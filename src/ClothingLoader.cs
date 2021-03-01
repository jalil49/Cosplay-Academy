using System.Collections.Generic;
using System.Linq;

namespace Cosplay_Academy
{
    public static class ClothingLoader
    {
        private static ChaControl chaControl;
        private static readonly string[] Inclusion = { "a_n_headtop", "a_n_headflont", "a_n_head", "a_n_headside", "a_n_waist_b", "a_n_hair_pony", "a_n_hair_twin_L", "a_n_hair_twin_R", "a_n_earrings_R", "a_n_earrings_L", "a_n_megane", "a_n_nose", "a_n_mouth", "a_n_hair_pin", "a_n_hair_pin_R" };
        public static void FullLoad(ChaControl input)
        {
            chaControl = input;
            UniformLoad();
            AfterSchoolLoad();
            GymLoad();
            SwimLoad();
            ClubLoad();
            CasualLoad();
            NightwearLoad();
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
            Queue<ChaFileAccessory.PartsInfo> import = new Queue<ChaFileAccessory.PartsInfo>();
            foreach (ChaFileAccessory.PartsInfo part in chaControl.chaFile.coordinate[outfitnum].accessory.parts)
            {
                if (Inclusion.Contains(part.parentKey))
                {
                    import.Enqueue(part);
                }
            }
            //Load new outfit
            chaControl.fileStatus.coordinateType = outfitnum;
            chaControl.chaFile.coordinate[outfitnum].LoadFile(Constants.outfitpath[outfitnum]);
            //Apply pre-existing Accessories in any open slot or final slots.
            bool Force;
            bool Empty;
            for (int i = 0, n = chaControl.chaFile.coordinate[outfitnum].accessory.parts.Length; i < n; i++)
            {
                if (import.Count == 0)//if queue empty break
                {
                    break;
                }
                Force = (import.Count + i == n);
                Empty = chaControl.chaFile.coordinate[outfitnum].accessory.parts[i].type == 120;
                if (Empty || Force) //120 is empty/default
                {
                    if (!Empty && Force)
                    {
                        ExpandedOutfit.Logger.LogDebug($"Overwriting Accessory (ID:{chaControl.chaFile.coordinate[outfitnum].accessory.parts[i].id}) at {i + 1} with default head accessory");
                    }
                    chaControl.chaFile.coordinate[outfitnum].accessory.parts[i] = import.Dequeue();
                }
            }
        }
    }
}