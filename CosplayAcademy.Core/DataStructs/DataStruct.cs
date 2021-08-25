using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if TRACE
using System.Diagnostics;
#endif

namespace Cosplay_Academy
{
    public static class DataStruct
    {
#if TRACE
        private static readonly Stopwatch Stopwatch = new Stopwatch();
#endif
        private static string SavePath;

        public static Dictionary<string, List<FolderStruct>> FolderStructure = new Dictionary<string, List<FolderStruct>>();

        public static List<FolderStruct> DefaultFolder => FolderStructure.ElementAt(Defaultint).Value;

        internal static int Defaultint = 0;

        public static List<CardData> GetAllCards()
        {
            var result = new List<CardData>();
            foreach (var list in FolderStructure.Values)
            {
                foreach (var folder in list)
                {
                    result.AddRange(folder.GetAllCards());
                }
            }
            return result;
        }

        public static List<FolderData> GetAllFolders()
        {
            var result = new List<FolderData>();
            foreach (var list in FolderStructure.Values)
            {
                foreach (var folder in list)
                {
                    result.AddRange(folder.GetAllFolders());
                }
            }
            return result;
        }

        public static void FindNewCards()
        {
            var folders = GetAllFolders();
            foreach (var folder in folders)
            {
                folder.FindCards();
                folder.FindSubFolders();
            }
            SaveFile();
        }

        public static void Init(string path)
        {
#if TRACE
            Settings.Logger.LogWarning($"Starting to load data");
            Stopwatch.Start();
#endif
            SavePath = path;
            if (CreateFile())
            {
                Load(Settings.CoordinatePath.Value);
                OutfitDecider.ResetDecider();
#if TRACE
                Stopwatch.Stop();
                Settings.Logger.LogWarning($"Took {Stopwatch.ElapsedMilliseconds} ms to create data");
#endif
                return;
            }
            ReadFile();
            OutfitDecider.ResetDecider();
#if TRACE
            Stopwatch.Stop();
            Settings.Logger.LogWarning($"Took {Stopwatch.ElapsedMilliseconds} ms to load data");
#endif
        }

        public static void CleanUp()
        {
            foreach (var list in FolderStructure.Values)
            {
                foreach (var folder in list)
                {
                    folder.CleanUp();
                }
            }
        }

        public static List<FolderStruct> Load(string coordinatepath)
        {
            if (!FolderStructure.TryGetValue(coordinatepath, out var list))
            {
                list = FolderStructure[coordinatepath] = new List<FolderStruct>();
            }

            while (list.Count < Constants.InputStrings.Length)
            {
                list.Add(new FolderStruct());
            }

            int set = 0;
            foreach (string coordinatetype in Constants.InputStrings)
            {
                list[set].Populate(coordinatepath + coordinatetype);
                set++;
            }
            SaveFile();
            return list;
        }

        public static void Update()
        {
            CleanUp();
            foreach (var list in FolderStructure.Values)
            {
                foreach (var folder in list)
                {
                    folder.Update();
                }
            }
            SaveFile();
        }

        private static bool CreateFile()
        {
            if (!File.Exists(SavePath))
            {
                File.Create(SavePath).Dispose();
                return true;
            }
            return false;
        }

        private static void ReadFile()
        {
            var data = File.ReadAllBytes(SavePath);
            if (data == null || data.Length == 0)
            {
                return;
            }
            try
            {
                FolderStructure = MessagePackSerializer.Deserialize<Dictionary<string, List<FolderStruct>>>(data);
#if TRACE
                Settings.Logger.LogWarning($"Took {Stopwatch.ElapsedMilliseconds} ms to deserialize data");
#endif
                CleanUp();
                FindNewCards();
            }
            catch
            {
                Load(Settings.CoordinatePath.Value);
            }
        }

        private static void SaveFile()
        {
            CleanUp();
            File.WriteAllBytes(SavePath, MessagePackSerializer.Serialize(FolderStructure));
        }

        public static void Reset()
        {
            FolderStructure = new Dictionary<string, List<FolderStruct>>();
            Load(Settings.CoordinatePath.Value);
            SaveFile();
        }
    }
}
