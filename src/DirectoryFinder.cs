using System.Collections.Generic;
using System.IO;

namespace Cosplay_Academy
{
    static class DirectoryFinder
    {
        static List<string> Choosen;
        static List<string> FoldersPath;
        static bool Reset;
        static DirectoryFinder()
        {
            Reset = true;
            Choosen = new List<string>();
            FoldersPath = new List<string>();
            CheckMissingFiles();
        }
        public static void CheckMissingFiles()
        {
            if (!Reset)
            {
                return;
            }
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
                            ExpandedOutfit.Logger.LogWarning("File not found, creating directory at " + "coordinate" + input + input2 + input3);
                            Directory.CreateDirectory(coordinatepath + "coordinate" + input + input2 + input3);
                        }
                    }
                }
            }
            Reset = false;
        }

        public static List<string> Grab_All_Files(string input)
        {
            FoldersPath.Clear();
            FoldersPath.Add(input);
            string[] folders = System.IO.Directory.GetDirectories(input, "*", System.IO.SearchOption.AllDirectories); //grab child folders
            FoldersPath.AddRange(folders);
            int index = FoldersPath.FindIndex(a => a.EndsWith(@"\Sets"));
            FoldersPath.RemoveAt(index);
#if Debug
            foreach (var item in FoldersPath)
            {
                ExpandedOutfit.Logger.LogError(item);
            }
#endif
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
            ExpandedOutfit.Logger.LogDebug("Searching " + FilePath);
            Choosen.Clear();
            List<string> Paths = new List<string>();
            Paths.Add(FilePath); //add parent folder to list
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
                string[] files = System.IO.Directory.GetFiles(path);
                Choosen.AddRange(files);

            }
            if (Choosen.Count == 0)
            {
                Choosen.Add("Default");
                ExpandedOutfit.Logger.LogWarning("No files found in :" + FilePath);
            }
            ExpandedOutfit.Logger.LogDebug($"Files found in : {FilePath} + {Choosen.Count}");
            return Choosen;
        }
    }
}


