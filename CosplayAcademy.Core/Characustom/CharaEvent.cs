using Cosplay_Academy.Hair;
using Cosplay_Academy.ME;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using MessagePack;
using System.Collections.Generic;
using UniRx;
using Extensions;
#if TRACE
using System.Diagnostics;
#endif
using System.Linq;
#if !KKS
using MoreAccessoriesKOI;
using ToolBox;
#endif


namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        public static List<ChaDefault> ChaDefaults = new List<ChaDefault>();

        internal ChaDefault ThisOutfitData;
        private ClothingLoader ClothingLoader => ThisOutfitData.ClothingLoader;
        internal static int Firstpass = 0;
#if !KKS
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();
#endif
#if TRACE
        private static readonly Stopwatch Time = new Stopwatch();
        private static readonly List<long> Average = new List<long>();
#endif
        internal static bool inH = false; //hopefully code that will work if additional heroines are loaded in H actively. such as in Kplug, Not tested.

        internal static void MakerAPI_MakerExiting()
        {
            Firstpass = 0;
#if !KKS
            if (!MakerAPI.IsInsideClassMaker())
            {
                ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
#endif
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            e.AddSidebarControl(new SidebarToggle("Enable Cosplay Academy", Settings.Makerview.Value, owner)).ValueChanged.Subscribe(value => Settings.Makerview.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand outfits", Settings.ChangeOutfit.Value, owner)).ValueChanged.Subscribe(value => Settings.ChangeOutfit.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand Underwear", Settings.RandomizeUnderwear.Value, owner)).ValueChanged.Subscribe(value => Settings.RandomizeUnderwear.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Reset Sets", Settings.ResetMaker.Value, owner)).ValueChanged.Subscribe(value => Settings.ResetMaker.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Only Underwear", Settings.RandomizeUnderwearOnly.Value, owner)).ValueChanged.Subscribe(value => Settings.RandomizeUnderwearOnly.Value = value);
        }

        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            if (ChaControl.sex == 0)
            {
                return;
            }
            bool IsMaker = currentGameMode == GameMode.Maker;
#if TRACE
            var Start = Time.ElapsedMilliseconds;
            if (ThisOutfitData == null || !ThisOutfitData.processed || currentGameMode == GameMode.Maker)
            {
                Time.Start();
            }
#endif
            if (currentGameMode == GameMode.Studio)
            {
                return;
            }
            if (IsMaker || !IsMaker && (ThisOutfitData == null || ThisOutfitData != null && !ThisOutfitData.processed))
            {
                Process(currentGameMode);

                ThisOutfitData.ClothingLoader.Reload_RePacks(ChaControl, inH);
            }
            else if (ThisOutfitData != null && ThisOutfitData.processed
#if !KKS
                && GameAPI.InsideHScene
#endif
                )
            {
                ThisOutfitData.Chafile = ChaFileControl;
                ThisOutfitData.ClothingLoader.Run_Repacks(ChaControl);

                ThisOutfitData.ClothingLoader.Reload_RePacks(ChaControl, inH);
            }

            if (IsMaker && Firstpass++ == 0 || inH)
            {
                ChaControl.ChangeCoordinateTypeAndReload();
            }
#if TRACE
            if (Time.IsRunning)
            {
                Time.Stop();
                var temp = Time.ElapsedMilliseconds - Start;
                Average.Add(temp);
                Settings.Logger.LogWarning($"Total elapsed time {Time.ElapsedMilliseconds}ms\nRun {Average.Count}: {temp}ms\nAverage: {Average.Average()}ms");
            }
#endif
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            //unused mandatory function 
        }

        private void ThisOutfitDataProcess()
        {
            if (ThisOutfitData != null && MakerAPI.InsideMaker)
            {
                return;
            }
            ThisOutfitData = ChaDefaults.Find(x => x.Parameter.Compare(ChaControl.fileParam));
            if (ThisOutfitData == null)
            {
                ThisOutfitData = new ChaDefault(ChaControl)
                {
                    Parameter = ChaControl.fileParam,
                    Chafile = ChaFileControl,
                    ChaControl = ChaControl,
#if KK
                    heroine = ChaControl.GetHeroine()
#endif
                };
                ChaDefaults.Add(ThisOutfitData);
                return;
            }
            ThisOutfitData.ChaControl = ChaControl;
            ThisOutfitData.Chafile = ChaFileControl;
        }

        public void Process(GameMode currentGameMode)
        {
            ThisOutfitDataProcess();
#if KK
            if (ThisOutfitData.heroine != null && ThisOutfitData.heroine.isTeacher && !Settings.TeacherDress.Value)
            {
                return;
            }
#endif
            if (GameMode.Maker == currentGameMode)
            {
                ThisOutfitData.firstpass = true;
                ThisOutfitData.Chafile = MakerAPI.LastLoadedChaFile;
                if (Settings.ResetMaker.Value)
                {
                    OutfitDecider.ResetDecider();
                }
            }

            if (ThisOutfitData.firstpass) //Save all accessories to avoid duplicating head accessories each load and be reuseable
            {
                ThisOutfitData.Clear_Firstpass();

                Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>> CharaHair = new Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>();

                PluginData HairExtendedData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.hairaccessorycustomizer");

                if (HairExtendedData != null && HairExtendedData.data.TryGetValue("HairAccessories", out var AllHairAccessories) && AllHairAccessories != null)
                    CharaHair = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<int, HairSupport.HairAccessoryInfo>>>((byte[])AllHairAccessories);

                PluginData MaterialEditorData = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "com.deathweasel.bepinex.materialeditor");

                #region ME Acc Import
                var Chafile_ME_Data = ThisOutfitData.Finished = new ME_List(MaterialEditorData, ThisOutfitData);
                #endregion

                #region Queue accessories to keep

                #region ACI Data
                Additional_Card_Info.DataStruct ACI_data = new Additional_Card_Info.DataStruct();

                for (int i = 0, n = ThisOutfitData.Outfit_Size; i < n; i++)
                {
                    ACI_data.Createoutfit(i);
                }

                bool Cosplay_Academy_Ready = false;
                var Required_Support = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
                if (Required_Support != null)
                {
                    switch (Required_Support.version)
                    {
                        case 0:
                            Additional_Card_Info.Migrator.MigrateV0(Required_Support, ref ACI_data);
                            break;
                        case 1:
                            if (Required_Support.data.TryGetValue("CardInfo", out var ByteData) && ByteData != null)
                            {
                                ACI_data.CardInfo = MessagePackSerializer.Deserialize<Additional_Card_Info.Cardinfo>((byte[])ByteData);
                            }
                            if (Required_Support.data.TryGetValue("CoordinateInfo", out ByteData) && ByteData != null)
                            {
                                ACI_data.CoordinateInfo = MessagePackSerializer.Deserialize<Dictionary<int, Additional_Card_Info.CoordinateInfo>>((byte[])ByteData);
                            }
                            break;
                        default:
                            Settings.Logger.LogWarning("New version of Additional Card Info found, please update");
                            break;
                    }
                }
                var CardInfo = ACI_data.CardInfo;
                var CoordinateInfo = ACI_data.CoordinateInfo;

                ClothingLoader.CardInfo = CardInfo;
                Cosplay_Academy_Ready = CardInfo.CosplayReady;
                ClothingLoader.MakeUpKeep = CoordinateInfo.ToDictionary(x => x.Key, x => x.Value.MakeUpKeep);
                ClothingLoader.CharacterClothingKeep_Coordinate = CoordinateInfo.ToDictionary(x => x.Key, x => x.Value.CoordinateSaveBools);
                #endregion

                var ObjectTypeList = new List<ObjectType>() { ObjectType.Accessory };

#if !KKS
                MoreAccessories.CharAdditionalData SaveAccessory;
                if (MakerAPI.InsideMaker)
                {
                    SaveAccessory = MoreAccessories._self._charaMakerData;
                }
                else
                {
                    if (_accessoriesByChar.TryGetValue(ThisOutfitData.Chafile, out SaveAccessory) == false)
                    {
                        SaveAccessory = new MoreAccessories.CharAdditionalData();
                        _accessoriesByChar.Add(ThisOutfitData.Chafile, SaveAccessory);
                    }
                }
#endif
                for (int outfitnum = 0, n = ThisOutfitData.Outfit_Size; outfitnum < n; outfitnum++)
                {
                    ThisOutfitData.Original_Coordinates[outfitnum] = CloneCoordinate(ChaFileControl.coordinate[outfitnum]);
                    var HairKeep = new List<int>();
                    var ACCKeep = new List<int>();
                    if (CoordinateInfo.ContainsKey(outfitnum))
                    {
                        HairKeep = CoordinateInfo[outfitnum].HairAcc;
                        ACCKeep = CoordinateInfo[outfitnum].AccKeep;
                    }
                    if (CharaHair.TryGetValue(outfitnum, out Dictionary<int, HairSupport.HairAccessoryInfo> HairInfo) == false)
                    {
                        HairInfo = new Dictionary<int, HairSupport.HairAccessoryInfo>();
                    }
#if !KKS
                    if (SaveAccessory.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> acclist) == false)
                    {
                        acclist = new List<ChaFileAccessory.PartsInfo>();
                    }
#else
                    var acclist = new List<ChaFileAccessory.PartsInfo>();
#endif
                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(ThisOutfitData.Chafile.coordinate[outfitnum].accessory.parts);
                    Intermediate.AddRange(new List<ChaFileAccessory.PartsInfo>(acclist));//create intermediate as it seems that acclist is a reference

                    var ME_ACC_Storage = ThisOutfitData.Original_Accessory_Data[outfitnum];

                    if (!Chafile_ME_Data.Coordinates.TryGetValue(outfitnum, out var coord))
                    {
                        coord = new ME_Coordinate();
                    }
                    var ME_ACC_Data = coord.AccessoryProperties;
                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (Settings.ExtremeAccKeeper.Value && !Cosplay_Academy_Ready || Constants.Generic_Inclusion.Contains(Intermediate[i].parentKey) && !Cosplay_Academy_Ready || HairKeep.Contains(i) || ACCKeep.Contains(i))
                        {
                            if (!HairInfo.TryGetValue(i, out HairSupport.HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairSupport.HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }

                            #region ME_Data

                            #region ME_Data
                            if (!ME_ACC_Data.TryGetValue(i, out var editorProperties))
                            {
                                editorProperties = new MaterialEditorProperties();
                            }
                            ME_ACC_Storage.Add(editorProperties);
                            #endregion

                            ThisOutfitData.CoordinatePartsQueue[outfitnum].Add(Intermediate[i]);
                            ThisOutfitData.HairAccQueue[outfitnum].Add(ACCdata);

                            #region ACI_Data
                            ThisOutfitData.HairKeepQueue[outfitnum].Add(HairKeep.Contains(i));
                            ThisOutfitData.ACCKeepQueue[outfitnum].Add(ACCKeep.Contains(i));
                            #endregion
                        }
                    }
                }

                #endregion
                ThisOutfitData.firstpass = false;
            }

            if (!Settings.EnableSetting.Value && GameMode.MainGame == currentGameMode || !Settings.Makerview.Value && GameMode.Maker == currentGameMode || GameMode.Studio == currentGameMode)
            {
                return;
            }//if disabled don't run

            if (ChaControl.sex == 1)//run the following if female
            {
                if (currentGameMode == GameMode.MainGame && !ThisOutfitData.processed || Settings.ChangeOutfit.Value && GameMode.Maker == currentGameMode)
                {
                    OutfitDecider.Decision(ChaControl.fileParam.fullname, ThisOutfitData);//Generate outfits
                    ThisOutfitData.processed = true;
                }
                int HoldOutfit = ChaControl.fileStatus.coordinateType; //requried for Cutscene characters to wear correct outfit such as sakura's first cutscene
                ThisOutfitData.ClothingLoader.FullLoad(ChaControl, ChaFileControl);
                ChaControl.fileStatus.coordinateType = HoldOutfit;
                ChaInfo temp = (ChaInfo)ChaControl;
                ChaControl.ChangeCoordinateType((ChaFileDefine.CoordinateType)temp.fileStatus.coordinateType, true); //forces cutscene characters to use outfits
            }
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            if (!Settings.AccKeeper.Value)
            {
                return;
            }//if disabled don't run
            ClothingLoader.CoordinateLoad(coordinate, ChaControl);
        }

        private ChaFileCoordinate CloneCoordinate(ChaFileCoordinate OriginalCoordinate)
        {
            return new ChaFileCoordinate
            {
                clothes = OriginalCoordinate.clothes,
                makeup = OriginalCoordinate.makeup,
                enableMakeup = OriginalCoordinate.enableMakeup,
            }; ;
        }
    }
    #endregion
}
