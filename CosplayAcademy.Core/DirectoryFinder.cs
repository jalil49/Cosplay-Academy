using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    static class DirectoryFinder
    {
        private static readonly char sep = Path.DirectorySeparatorChar;

        public static void CheckMissingFiles()
        {
            string[] InputStrings3 = { $"{sep}Sets", "" };
            string coordinatepath = Settings.CoordinatePath.Value;
            foreach (string input in Constants.InputStrings)
            {
                foreach (string input2 in Constants.InputStrings2)
                {
                    foreach (string input3 in InputStrings3)
                    {
                        var path = coordinatepath + input + input2 + input3;
                        if (!Directory.Exists(path))
                        {
                            Settings.Logger.LogWarning("Folder not found, creating directory at " + path);
                            Directory.CreateDirectory(path);
                        }
                    }
                }
            }
            var path2 = coordinatepath + $"{sep}Unorganized";
            if (!Directory.Exists(path2))
            {
                Settings.Logger.LogWarning("Folder not found, creating directory at " + path2);
                Directory.CreateDirectory(path2);
            }
        }

        public static void Organize()
        {
            string coordinatepath = Settings.CoordinatePath.Value;
            var folders = Grab_All_Directories(coordinatepath + $"{sep}Unorganized");
            foreach (var item in folders)
            {
                var files = Get_Outfits_From_Path(item, false);

                foreach (var Coordinate in files)
                {
                    ChaFileCoordinate Organizer = new ChaFileCoordinate();
                    Organizer.LoadFile(Coordinate);
                    var ACI_Data = ExtendedSave.GetExtendedDataById(Organizer, "Additional_Card_Info");

                    if (ACI_Data == null)
                    {
                        continue;
                    }

                    Additional_Card_Info.CoordinateInfo coordiante;

                    if (ACI_Data.version == 1)
                    {
                        if (ACI_Data.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                        {
                            coordiante = MessagePackSerializer.Deserialize<Additional_Card_Info.CoordinateInfo>((byte[])ByteData);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (ACI_Data.version < 1)
                    {
                        coordiante = Additional_Card_Info.Migrator.CoordinateMigrateV0(ACI_Data);
                    }
                    else
                    {
                        Settings.Logger.LogWarning("New version Detected please update Cosplay Academy");
                        continue;
                    }
                    var restriction = coordiante.RestrictionInfo;
                    var CoordinateSubType = restriction.CoordinateSubType;

                    if (CoordinateSubType != 0 && CoordinateSubType != 10)
                    {
                        continue;
                    }

                    var CoordinateType = restriction.CoordinateType;
                    int HstateType_Restriction = restriction.HstateType_Restriction;
                    string SetNames = coordiante.SetNames;
                    string SubSetNames = coordiante.SubSetNames;
                    string Result;
                    string ClubResult = "";
                    string SubPath = $"{sep}";
                    if (SetNames.Length > 0)
                    {
                        SubPath += @"Sets{sep}" + SetNames;
                    }
                    if (SubSetNames.Length > 0)
                    {
                        if (!SubPath.EndsWith($"{sep}"))
                        {
                            SubPath += $"{sep}";
                        }
                        SubPath += SubSetNames;
                    }
                    var FileName = $"{sep}" + Coordinate.Split(sep).Last();
                    if (CoordinateSubType == 10)
                    {
                        Result = coordinatepath + Constants.InputStrings[7] + Constants.InputStrings2[HstateType_Restriction] + SubPath;
                        if (!Directory.Exists(Result))
                            Directory.CreateDirectory(Result);
                        Result += FileName;
                        File.Copy(Coordinate, Result, true);
                        File.Delete(Coordinate);
                        continue;
                    }
                    if (CoordinateType > 0)
                    {
                        CoordinateType++;
                    }
                    if (CoordinateType == 4)
                    {
                        var club = MessagePackSerializer.Deserialize<int>((byte[])ACI_Data.data["ClubType_Restriction"]);
                        if (club < 0)
                        {
                            Settings.Logger.LogWarning($"Coordinate {FileName} is defined as a club type with no club type assigned");
                            continue;
                        }
                        ClubResult = Constants.ClubPaths[club];
                    }
                    Result = coordinatepath + Constants.AllCoordinatePaths[CoordinateType] + ClubResult + Constants.InputStrings2[HstateType_Restriction] + SubPath;
                    if (!Directory.Exists(Result))
                        Directory.CreateDirectory(Result);
                    Result += FileName;
                    File.Copy(Coordinate, Result, true);
                    File.Delete(Coordinate);
                }
            }
        }

        public static List<string> Grab_All_Directories(string OriginalPath)
        {
            List<string> FoldersPath = new List<string>();
            bool originalpathexists = Directory.Exists(OriginalPath);
            if (originalpathexists)
            {
                FoldersPath.Add(OriginalPath);
                FoldersPath.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            }
            for (int i = 0; i < FoldersPath.Count; i++)
            {
                if (FoldersPath[i].EndsWith($"{sep}Sets"))
                {
                    FoldersPath.RemoveAt(i--);
                    continue;
                }
                if (Directory.GetFiles(FoldersPath[i], "*.png").Length == 0)
                {
                    FoldersPath.RemoveAt(i--);
                }
            }
            return FoldersPath;
        }

        public static List<string> Grab_Folder_Directories(string OriginalPath, bool self)
        {
            bool originalpathexists = Directory.Exists(OriginalPath);
            var list = new List<string>();
            if (originalpathexists)
            {
                if (self)
                    list.Add(OriginalPath);
                list.AddRange(Directory.GetDirectories(OriginalPath));//grab direct child folders
            }
            return list;
        }

        public static List<string> Get_Set_Paths(string Narrow)
        {
            List<string> Choosen = new List<string>();
            string coordinatepath = Settings.CoordinatePath.Value;
            if (Directory.Exists(coordinatepath))
            {
                return Choosen;
            }
            var folders = Directory.GetDirectories(coordinatepath, "*", SearchOption.AllDirectories).ToList(); //grab child folders
            foreach (string folder in folders)
            {
                if (folder.Contains(Narrow))
                { Choosen.Add(folder); }
            }
            return Choosen;
        }

        public static List<string> Get_Outfits_From_Path(string OriginalPath, bool RemoveSets = true)
        {
            List<string> Choosen = new List<string>();
            List<string> Paths = new List<string>();
            if (Directory.Exists(OriginalPath))
            {
                Paths.Add(OriginalPath);
                Paths.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            }
            //step through each folder and grab files
            foreach (string path in Paths)
            {
                if (RemoveSets && path.Contains($"{sep}Sets"))
                {
                    continue;
                }
                string[] files = Directory.GetFiles(path, "*.png");
                Choosen.AddRange(files);
            }
            bool choosenempty = Choosen.Count == 0;
            if ((choosenempty || Settings.EnableDefaults.Value) && !OriginalPath.Contains($"{sep}Unorganized"))
            {
                Choosen.Add("Default");
                if (choosenempty)
                    Settings.Logger.LogWarning("No files found in :" + OriginalPath);
            }
            Settings.Logger.LogDebug($"Files found in : {OriginalPath} + {Choosen.Count}");
            return Choosen;
        }
    }
}


