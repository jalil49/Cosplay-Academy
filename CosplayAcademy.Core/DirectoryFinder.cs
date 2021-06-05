using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    static class DirectoryFinder
    {
        public static void CheckMissingFiles()
        {
            string[] InputStrings3 = { @"\Sets", "" };
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            foreach (string input in Constants.InputStrings)
            {
                foreach (string input2 in Constants.InputStrings2)
                {
                    foreach (string input3 in InputStrings3)
                    {
                        if (!Directory.Exists(coordinatepath + "coordinate" + input + input2 + input3))
                        {
                            Settings.Logger.LogWarning("Folder not found, creating directory at " + "coordinate" + input + input2 + input3);
                            Directory.CreateDirectory(coordinatepath + "coordinate" + input + input2 + input3);
                        }
                    }
                }
            }
            if (!Directory.Exists(coordinatepath + @"coordinate\Unorganized"))
            {
                Settings.Logger.LogWarning("Folder not found, creating directory at " + @"coordinate\Unorganized");
                Directory.CreateDirectory(coordinatepath + @"coordinate\Unorganized");
            }
        }

        public static void Organize()
        {
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName + "coordinate";
            var folders = Grab_All_Directories(coordinatepath + @"\Unorganized");
            foreach (var item in folders)
            {
                var files = Get_Outfits_From_Path(item, null, false);

                foreach (var Coordinate in files)
                {
                    ChaFileCoordinate Organizer = new ChaFileCoordinate();
                    Organizer.LoadFile(Coordinate);
                    var ACI_Data = ExtendedSave.GetExtendedDataById(Organizer, "Additional_Card_Info");

                    if (ACI_Data?.data != null)
                    {

                        var CoordinateSubType = MessagePackSerializer.Deserialize<int>((byte[])ACI_Data.data["CoordinateSubType"]);
                        if (CoordinateSubType != 0 && CoordinateSubType != 10)
                        {
                            continue;
                        }
                        int CoordinateType = MessagePackSerializer.Deserialize<int>((byte[])ACI_Data.data["CoordinateType"]);
                        string SubSetNames = MessagePackSerializer.Deserialize<string>((byte[])ACI_Data.data["SubSetNames"]);
                        string SetNames = MessagePackSerializer.Deserialize<string>((byte[])ACI_Data.data["Set_Name"]);
                        int HstateType_Restriction = MessagePackSerializer.Deserialize<int>((byte[])ACI_Data.data["HstateType_Restriction"]);
                        string Result;
                        string ClubResult = "";
                        string SubPath = @"\";
                        if (SetNames.Length > 0)
                        {
                            SubPath += @"Sets\" + SetNames;
                        }
                        if (SubSetNames.Length > 0)
                        {
                            if (!SubPath.EndsWith(@"\"))
                            {
                                SubPath += @"\";
                            }
                            SubPath += SubSetNames;
                        }
                        var FileName = @"\" + Coordinate.Split('\\').Last();
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
        }

        public static List<string> Grab_All_Directories(string OriginalPath, string AlternativePath = null)
        {
            List<string> FoldersPath = new List<string>
            {
                OriginalPath
            };
            FoldersPath.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            if (AlternativePath != null && Settings.UseAlternativePath.Value && Directory.Exists(AlternativePath))
            {
                FoldersPath.Add(AlternativePath);
                FoldersPath.AddRange(Directory.GetDirectories(AlternativePath, "*", SearchOption.AllDirectories));
            }
            for (int i = 0; i < FoldersPath.Count; i++)
            {
                if (FoldersPath[i].EndsWith(@"\Sets"))
                {
                    FoldersPath.RemoveAt(i--);
                    continue;
                }
                if (Directory.GetFiles(FoldersPath[i], "*.png").Length == 0)
                {
                    FoldersPath.RemoveAt(i--);
                }
            }
            if (FoldersPath.Count == 0)
            {
                FoldersPath.Add(OriginalPath);
            }

            return FoldersPath;
        }

        public static List<string> Get_Set_Paths(string Narrow)
        {
            List<string> Choosen = new List<string>();
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            string AlternativePath = Settings.AlternativePath.Value + "coordinate";
            var folders = Directory.GetDirectories(coordinatepath + "coordinate", "*", SearchOption.AllDirectories).ToList(); //grab child folders
            if (Settings.UseAlternativePath.Value && Directory.Exists(AlternativePath))
            {
                folders.AddRange(Directory.GetDirectories(AlternativePath, "*", SearchOption.AllDirectories));
            }
            foreach (string folder in folders)
            {
                if (folder.Contains(Narrow))
                { Choosen.Add(folder); }
            }
            return Choosen;
        }

        public static List<string> Get_Outfits_From_Path(string FilePath, string AlternativePath = null, bool RemoveSets = true)
        {
            List<string> Choosen = new List<string>();
            List<string> Paths = new List<string>
            {
                FilePath //add parent folder to list
            };
            Paths.AddRange(Directory.GetDirectories(FilePath, "*", SearchOption.AllDirectories)); //grab child folders
            if (AlternativePath != null && Settings.UseAlternativePath.Value && Directory.Exists(AlternativePath))
            {
                Paths.Add(AlternativePath);
                var Files = Directory.GetDirectories(AlternativePath, "*", SearchOption.AllDirectories);
                if (Files.Length > 0)
                    Paths.AddRange(Files);
            }
            //step through each folder and grab files
            foreach (string path in Paths)
            {
                if (RemoveSets && path.Contains(@"\Sets"))
                {
                    continue;
                }
                string[] files = Directory.GetFiles(path, "*.png");
                Choosen.AddRange(files);
            }
            if (Choosen.Count == 0 && !FilePath.Contains(@"\Unorganized"))
            {
                Choosen.Add("Default");
                Settings.Logger.LogWarning("No files found in :" + FilePath);
            }
            Settings.Logger.LogDebug($"Files found in : {FilePath} + {Choosen.Count}");
            return Choosen;
        }
    }
}


