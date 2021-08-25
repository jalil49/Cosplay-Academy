using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    public static partial class OutfitDecider
    {
        private readonly static char sep = Path.DirectorySeparatorChar;
        private static readonly OutfitData[] outfitData;

        private static ChaDefault ThisOutfitData;
        private static int HExperience;
        private static int RandHExperience;

        static OutfitDecider()
        {
            outfitData = new OutfitData[Constants.InputStrings.Length];
            for (int i = 0, n = outfitData.Length; i < n; i++)
            {
                outfitData[i] = new OutfitData();
            }
        }

        public static void ResetDecider()
        {
            foreach (OutfitData data in outfitData)
            {
                data.Clear();
            }
            foreach (var item in CharaEvent.ChaDefaults)
            {
                item.processed = false;
            }
#if KK
            ChaDefault.LastClub = -1;
#endif
            OutfitData.Anger = false;
            Get_Outfits();
            foreach (var data in outfitData)
            {
                data.Coordinate();
            }
        }

        public static void Get_Outfits()
        {
            int hstatelen = Constants.InputStrings2.Length;
            for (int sets = 0, setslen = Constants.InputStrings.Length; sets < setslen; sets++)
            {
                for (int hstate = 0; hstate < hstatelen; hstate++)
                {
                    var hstatefolder = DataStruct.DefaultFolder[sets].FolderData[hstate];

                    if (Settings.ListOverrideBool[sets].Value)
                    {
                        var overridepath = Settings.ListOverride[sets].Value;
                        var find = hstatefolder.GetAllFolders().Find(x => x.FolderPath == overridepath);
                        if (find == null)
                        {
                            hstatefolder.Populate(overridepath);
                            find = hstatefolder.GetAllFolders().Find(x => x.FolderPath == overridepath);
                            if (find == null)
                            {
                                outfitData[sets].Insert(hstate, new List<CardData>(), false);//assign "is" set and store data
                                continue;
                            }
                        }
                        outfitData[sets].Insert(hstate, find.GetAllCards(), true);//assign "is" set and store data
                        continue;
                    }

                    if (outfitData[sets].IsSet(hstate))//Skip set items
                    {
                        continue;
                    }

                    if (Settings.EnableSets.Value)
                    {
                        var AllFolder = hstatefolder.GetAllFolders();
#if KK
                        Grabber(ref AllFolder, sets, hstate);
#endif
                        if (AllFolder.Count == 0)
                        {
                            outfitData[sets].Insert(hstate, new List<CardData>(), false);
                            continue;
                        }

                        var selectedfolder = AllFolder[UnityEngine.Random.Range(0, AllFolder.Count)];

                        Settings.Logger.LogWarning($"Selected folder for {Constants.InputStrings[sets]}/{Constants.InputStrings2[hstate]}: {selectedfolder.FolderPath}");

                        bool isset = selectedfolder.FolderPath.Contains($"{sep}Sets{sep}");

                        outfitData[sets].Insert(hstate, selectedfolder.GetAllCards(), isset);

                        if (!Settings.IndividualSets.Value && isset)
                        {
                            Setsfunction(selectedfolder);
                        }
                        continue;
                    }

                    outfitData[sets].Insert(hstate, hstatefolder.GetAllCards(), false);
                }
            }
        }

        public static void Decision(string name, ChaDefault cha)
        {
            ThisOutfitData = cha;
#if !KKS
            SaveData.Heroine person = ThisOutfitData.heroine;

            if (person != null)
            {
                OutfitData.Anger = person.isAnger;
                HExperience = (int)person.HExperience;
            }
            else
            {
#endif
                OutfitData.Anger = false;
                HExperience = (int)Settings.MakerHstate.Value;
#if !KKS
            }
#endif
            RandHExperience = UnityEngine.Random.Range(0, HExperience + 1);
            for (int i = 0; i < Constants.InputStrings.Length; i++)
            {
                Generalized_Assignment(Settings.MatchGeneric[i].Value, i, i);
            }

            SpecialProcess();
#if !KKS
            if (person != null)
            {
                Settings.Logger.LogDebug(name + " is processed.");
            }
#endif
        }

        private static void Setsfunction(FolderData folderData)
        {
            var sep = Path.DirectorySeparatorChar;
            string split = sep + folderData.FolderPath.Split(new string[] { sep + "Sets" + sep }, System.StringSplitOptions.RemoveEmptyEntries).Last();
            for (int sets = 0, n = outfitData.Length; sets < n; sets++)
            {
                for (int hexp = 0; hexp < 4; hexp++)
                {
                    if (Settings.FullSet.Value && outfitData[sets].IsSet(hexp) || Settings.ListOverrideBool[sets].Value)
                    {
                        continue;
                    }
                    var find = DataStruct.DefaultFolder[sets].FolderData[hexp].GetAllFolders().Find(x => x.FolderPath.EndsWith(split));
                    if (find == null)
                    {
                        continue;
                    }
                    List<CardData> temp = find.GetAllCards();
                    outfitData[sets].Insert(hexp, find.GetAllCards(), true);
                }
            }
        }

        private static void Generalized_Assignment(bool uniform_type, int Path_Num, int Data_Num)
        {
            var status = ThisOutfitData.ChaControl.fileParam;
            switch (Settings.H_EXP_Choice.Value)
            {
                case Hexp.RandConstant:
                    ThisOutfitData.Hvalue = RandHExperience;
                    ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].Random(RandHExperience, uniform_type, false, status.personality, status.attribute, ThisOutfitData.ChaControl.GetBustCategory(), ThisOutfitData.ChaControl.GetHeightCategory());
                    break;
                case Hexp.Maximize:
                    ThisOutfitData.Hvalue = HExperience;
                    ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].Random(HExperience, uniform_type, false, status.personality, status.attribute, ThisOutfitData.ChaControl.GetBustCategory(), ThisOutfitData.ChaControl.GetHeightCategory());
                    break;
                default:
                    ThisOutfitData.Hvalue = UnityEngine.Random.RandomRangeInt(0, HExperience + 1);
                    ThisOutfitData.alloutfitpaths[Path_Num] = outfitData[Data_Num].RandomSet(ThisOutfitData.Hvalue, uniform_type, false, status.personality, status.attribute, ThisOutfitData.ChaControl.GetBustCategory(), ThisOutfitData.ChaControl.GetHeightCategory());
                    break;
            }
        }
    }
}


