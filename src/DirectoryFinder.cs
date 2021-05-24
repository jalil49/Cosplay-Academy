using ExtensibleSaveFormat;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    static class DirectoryFinder
    {
        static readonly List<string> Choosen;
        static readonly List<string> FoldersPath;
        static DirectoryFinder()
        {
            Choosen = new List<string>();
            FoldersPath = new List<string>();
        }

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
            string[] files = System.IO.Directory.GetFiles(coordinatepath + @"\Unorganized", "*.png");
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
                        Result = coordinatepath + Constants.InputStrings[12] + Constants.InputStrings2[HstateType_Restriction] + SubPath;
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
                    Result = coordinatepath + Constants.InputStrings[CoordinateType] + Constants.InputStrings2[HstateType_Restriction] + SubPath;
                    if (!Directory.Exists(Result))
                        Directory.CreateDirectory(Result);
                    Result += FileName;
                    File.Copy(Coordinate, Result, true);
                    File.Delete(Coordinate);
                }
            }
        }

        public static List<string> Grab_All_Files(string input)
        {
            FoldersPath.Clear();
            FoldersPath.Add(input);
            string[] folders = System.IO.Directory.GetDirectories(input, "*", System.IO.SearchOption.AllDirectories); //grab child folders
            List<string> FolderLists = folders.ToList();
            int index = FolderLists.FindIndex(a => a.EndsWith(@"\Sets"));
            FolderLists.RemoveAt(index);
            FoldersPath.AddRange(FolderLists);

            return FoldersPath;
        }
        public static List<string> Get_Set_Paths(string Narrow)
        {
            Choosen.Clear();
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            string[] folders = System.IO.Directory.GetDirectories(coordinatepath + "coordinate", "*", System.IO.SearchOption.AllDirectories); //grab child folders
            foreach (string folder in folders)
            {
                if (folder.Contains(Narrow))
                { Choosen.Add(folder); }
            }
            return Choosen;
        }
        public static List<string> Get_Outfits_From_Path(string FilePath, bool RemoveSets = true)
        {
            //ExpandedOutfit.Logger.LogDebug("Searching " + FilePath);
            Choosen.Clear();
            List<string> Paths = new List<string>
            {
                FilePath //add parent folder to list
            };
            //ExpandedOutfit.Logger.LogDebug(coordinatepath + "coordinate" + Narrow);
            string[] folders = System.IO.Directory.GetDirectories(FilePath, "*", System.IO.SearchOption.AllDirectories); //grab child folders
            if (folders.Length > 0)
            {
                Paths.AddRange(folders);
            }
            //step through each folder and grab files
            foreach (string path in Paths)
            {
                if (RemoveSets && path.Contains(@"\Sets"))
                {
                    continue;
                }
                string[] files = System.IO.Directory.GetFiles(path, "*.png");
                Choosen.AddRange(files);
            }
            if (Choosen.Count == 0)
            {
                Choosen.Add("Default");
                Settings.Logger.LogWarning("No files found in :" + FilePath);
            }
            Settings.Logger.LogDebug($"Files found in : {FilePath} + {Choosen.Count}");
            return Choosen;
        }
    }
}


