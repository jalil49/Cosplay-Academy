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
using MoreAccessoriesKOI;
using System.Collections.Generic;
#if TRACE
using System.Diagnostics;
#endif
using System.Linq;
using ToolBox;

namespace Cosplay_Academy
{
    public class CharaEvent : CharaCustomFunctionController
    {
        private ChaDefault ThisOutfitData;
        private static bool Firstpass = true;
        private static readonly WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = (WeakKeyDictionary<ChaFile, MoreAccessories.CharAdditionalData>)Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue();

#if TRACE
        private static readonly Stopwatch Time = new Stopwatch();
        private static readonly List<long> Average = new List<long>();
#endif
        public static bool inH = false; //hopefully code that will work if additional heroines are loaded in H actively. such as in Kplug, Not tested.
        public static void MakerAPI_MakerExiting()
        {
            Firstpass = true;
            if (!MakerAPI.IsInsideClassMaker())
            {
                Constants.ChaDefaults.Clear();
                OutfitDecider.ResetDecider();
            }
        }

        public static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var owner = Settings.Instance;
            e.AddSidebarControl(new SidebarToggle("Enable Cosplay Academy", Settings.Makerview.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.Makerview.Value,
                (controller, value) => Settings.Makerview.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand outfits", Settings.ChangeOutfit.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.ChangeOutfit.Value,
                (controller, value) => Settings.ChangeOutfit.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Rand Underwear", Settings.RandomizeUnderwear.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.RandomizeUnderwear.Value,
                (controller, value) => Settings.RandomizeUnderwear.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Reset Sets", Settings.ResetMaker.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.ResetMaker.Value,
                (controller, value) => Settings.ResetMaker.Value = value);
            e.AddSidebarControl(new SidebarToggle("CA: Only Underwear", Settings.RandomizeUnderwearOnly.Value, owner)).BindToFunctionController<CharaEvent, bool>(
                (controller) => Settings.RandomizeUnderwearOnly.Value,
                (controller, value) => Settings.RandomizeUnderwearOnly.Value = value);
        }

        protected override void OnReload(GameMode currentGameMode, bool MaintainState) //from KKAPI.Chara when characters enter reload state
        {
            if (ChaControl.sex == 0)
            {
                return;
            }
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
            bool IsMaker = currentGameMode == GameMode.Maker;
            if (IsMaker || ThisOutfitData == null || !ThisOutfitData.processed)
            {
                Process(currentGameMode);

                ThisOutfitData.ClothingLoader.Reload_RePacks(ChaControl, inH);
            }
            else if (ThisOutfitData.processed && GameAPI.InsideHScene)
            {
                ThisOutfitData.Chafile = ChaFileControl;
                ThisOutfitData.ClothingLoader.Run_Repacks(ChaControl);

                ThisOutfitData.ClothingLoader.Reload_RePacks(ChaControl, inH);
            }
            if (IsMaker && Firstpass || inH)
            {
                ChaControl.ChangeCoordinateTypeAndReload();
                Firstpass = false;
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
            ThisOutfitData = Constants.ChaDefaults.Find(x => ChaControl.fileParam.personality == x.Personality && x.FullName == ChaControl.fileParam.fullname && x.BirthDay == ChaControl.fileParam.strBirthDay);
            if (ThisOutfitData == null)
            {
                //ExpandedOutfit.Logger.LogWarning($"{ChaControl.fileParam.fullname} made new default; chano {ChaControl.fileParam.strBirthDay} name {ChaControl.fileParam.personality}");
                ThisOutfitData = new ChaDefault
                {
                    FullName = ChaControl.fileParam.fullname,
                    BirthDay = ChaControl.fileParam.strBirthDay,
                    Personality = ChaControl.fileParam.personality,
                    heroine = ChaControl.GetHeroine()
                };
                Constants.ChaDefaults.Add(ThisOutfitData);
            }
            ThisOutfitData.ChaControl = ChaControl;
            ThisOutfitData.Chafile = ChaFileControl;
        }

        public void Process(GameMode currentGameMode)
        {
            ThisOutfitDataProcess();

            if (ThisOutfitData.heroine != null && ThisOutfitData.heroine.isTeacher && !Settings.TeacherDress.Value)
            {
                return;
            }

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
                var Chafile_ME_Data = new ME_List(MaterialEditorData, ThisOutfitData);
                #endregion

                #region Queue accessories to keep

                #region ACI Data
                List<int>[] HairKeep = new List<int>[Constants.Outfit_Size];
                List<int>[] ACCKeep = new List<int>[Constants.Outfit_Size];

                for (int i = 0; i < Constants.Outfit_Size; i++)
                {
                    HairKeep[i] = new List<int>();
                    ACCKeep[i] = new List<int>();
                }

                bool Cosplay_Academy_Ready = false;
                var Required_Support = ExtendedSave.GetExtendedDataById(ThisOutfitData.Chafile, "Additional_Card_Info");
                if (Required_Support != null)
                {
                    if (Required_Support.data.TryGetValue("HairAcc", out var Bytedata))
                    {
                        HairKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])Bytedata);
                    }
                    if (Required_Support.data.TryGetValue("AccKeep", out Bytedata))
                    {
                        ACCKeep = MessagePackSerializer.Deserialize<List<int>[]>((byte[])Bytedata);
                    }
                    if (Required_Support.data.TryGetValue("Cosplay_Academy_Ready", out Bytedata))
                    {
                        ThisOutfitData.ClothingLoader.Character_Cosplay_Ready = Cosplay_Academy_Ready = MessagePackSerializer.Deserialize<bool>((byte[])Bytedata);
                    }
                }
                #endregion

                var ObjectTypeList = new List<ObjectType>() { ObjectType.Accessory };
                for (int outfitnum = 0, n = Constants.Outfit_Size; outfitnum < n; outfitnum++)
                {
                    ThisOutfitData.Original_Coordinates[outfitnum] = CloneCoordinate(ChaFileControl.coordinate[outfitnum]);

                    if (CharaHair.TryGetValue(outfitnum, out Dictionary<int, HairSupport.HairAccessoryInfo> HairInfo) == false)
                    {
                        HairInfo = new Dictionary<int, HairSupport.HairAccessoryInfo>();
                    }

                    if (_accessoriesByChar.TryGetValue(ThisOutfitData.Chafile, out var SaveAccessory) == false)
                    {
                        SaveAccessory = new MoreAccessories.CharAdditionalData();
                        _accessoriesByChar.Add(ThisOutfitData.Chafile, SaveAccessory);
                    }

                    if (SaveAccessory.rawAccessoriesInfos.TryGetValue(outfitnum, out List<ChaFileAccessory.PartsInfo> acclist) == false)
                    {
                        acclist = new List<ChaFileAccessory.PartsInfo>();
                    }

                    var Intermediate = new List<ChaFileAccessory.PartsInfo>(ThisOutfitData.Chafile.coordinate[outfitnum].accessory.parts);
                    Intermediate.AddRange(new List<ChaFileAccessory.PartsInfo>(acclist));//create intermediate as it seems that acclist is a reference

                    var ME_ACC_Storage = ThisOutfitData.Original_Accessory_Data[outfitnum];
                    for (int i = 0; i < Intermediate.Count; i++)
                    {
                        //ExpandedOutfit.Logger.LogWarning($"ACC :{i}\tID: {data.nowAccessories[i].id}\tParent: {data.nowAccessories[i].parentKey}");
                        if (Settings.ExtremeAccKeeper.Value && !Cosplay_Academy_Ready || Constants.Generic_Inclusion.Contains(Intermediate[i].parentKey) && !Cosplay_Academy_Ready || HairKeep[outfitnum].Contains(i) || ACCKeep[outfitnum].Contains(i))
                        {
                            if (!HairInfo.TryGetValue(i, out HairSupport.HairAccessoryInfo ACCdata))
                            {
                                ACCdata = new HairSupport.HairAccessoryInfo
                                {
                                    HairLength = -999
                                };
                            }

                            #region ME_Data
                            ME_ACC_Storage.MaterialColorProperty.AddRange(Chafile_ME_Data.Color_FindAll(ObjectTypeList, i, outfitnum));
                            ME_ACC_Storage.MaterialFloatProperty.AddRange(Chafile_ME_Data.Float_FindAll(ObjectTypeList, i, outfitnum));
                            ME_ACC_Storage.MaterialShader.AddRange(Chafile_ME_Data.Shader_FindAll(ObjectTypeList, i, outfitnum));
                            ME_ACC_Storage.MaterialTextureProperty.AddRange(Chafile_ME_Data.Texture_FindAll(ObjectTypeList, i, outfitnum));
                            ME_ACC_Storage.RendererProperty.AddRange(Chafile_ME_Data.Render_FindAll(ObjectTypeList, i, outfitnum));
                            #endregion

                            ThisOutfitData.CoordinatePartsQueue[outfitnum].Add(Intermediate[i]);
                            ThisOutfitData.HairAccQueue[outfitnum].Add(ACCdata);

                            #region ACI_Data
                            ThisOutfitData.HairKeepQueue[outfitnum].Add(HairKeep[outfitnum].Contains(i));
                            ThisOutfitData.ACCKeepQueue[outfitnum].Add(ACCKeep[outfitnum].Contains(i));
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
                ThisOutfitData.ClothingLoader.FullLoad(ChaControl, ChaFileControl);//Load outfits; has to run again for story mode les scene at least
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
            ThisOutfitData.ClothingLoader.CoordinateLoad(coordinate, ChaControl);
        }

        private ChaFileCoordinate CloneCoordinate(ChaFileCoordinate OriginalCoordinate)
        {
            ChaFileCoordinate Temp = new ChaFileCoordinate
            {
                clothes = OriginalCoordinate.clothes,
                makeup = OriginalCoordinate.makeup,
                enableMakeup = OriginalCoordinate.enableMakeup
            };
            return Temp;
        }
    }
}