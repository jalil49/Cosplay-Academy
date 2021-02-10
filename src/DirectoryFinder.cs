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
            string[] InputStrings = { @"\School Uniform" , @"\AfterSchool", @"\Gym" , @"\Swimsuit" , @"\Club\Swim" ,
            @"\Club\Manga", @"\Club\Cheer", @"\Club\Tea", @"\Club\Track", @"\Casual" , @"\Nightwear", @"\Club\Koi" };
            string[] InputStrings2 = { @"\FirstTime", @"\Amateur", @"\Pro", @"\Lewd" };
            string[] InputStrings3 = { @"\Sets", "" };
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if (!Directory.Exists(coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j] + InputStrings3[k]))
                        {
                            ExpandedOutfit.Logger.LogWarning("File not found, creating directory at " + "coordinate" + InputStrings[i] + InputStrings2[j] + InputStrings3[k]);
                            Directory.CreateDirectory(coordinatepath + "coordinate" + InputStrings[i] + InputStrings2[j] + InputStrings3[k]);
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
            return FoldersPath;
        }
        public static List<string> Get_Set_Paths(string Narrow)
        {
            Choosen.Clear();
            string coordinatepath = new DirectoryInfo(UserData.Path).FullName;
            string[] folders = System.IO.Directory.GetDirectories(coordinatepath + "coordinate", "*", System.IO.SearchOption.AllDirectories); //grab child folders
            for (int i = 0; i < folders.Length; i++)
            {
                if (folders[i].Contains(Narrow))
                { Choosen.Add(folders[i]); }
            }
            return Choosen;
        }
        public static List<string> Get_Outfits_From_Path(string FilePath)
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
            for (int i = 0; i < Paths.Count; i++)
            {
                string[] files = System.IO.Directory.GetFiles(Paths[i]);
                Choosen.AddRange(files);
            }
            if (Choosen.Count == 0)
            {
                Choosen.Add("Default");
                ExpandedOutfit.Logger.LogWarning("No files found in :" + FilePath);
            }
            ExpandedOutfit.Logger.LogDebug("Files found in :" + FilePath + Choosen.Count);
            return Choosen;
        }
    }
}


