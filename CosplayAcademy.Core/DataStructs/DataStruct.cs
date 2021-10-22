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

        public static Dictionary<string, List<FolderStruct>> FullStructures = new Dictionary<string, List<FolderStruct>>();

        public static Dictionary<string, FolderStruct> IndividualStructures = new Dictionary<string, FolderStruct>();
        public static List<FolderStruct> DefaultFolder => FullStructures.ElementAt(Defaultint).Value;

        internal static int Defaultint = 0;

        public static List<CardData> GetAllCards()
        {
            var result = new List<CardData>();
            foreach (var list in FullStructures.Values)
            {
                foreach (var folder in list)
                {
                    result.AddRange(folder.GetAllCards());
                }
            }
            foreach (var folder in IndividualStructures.Values)
            {
                result.AddRange(folder.GetAllCards());
            }

            return result;
        }

        public static List<FolderData> GetAllFolders()
        {
            var result = new List<FolderData>();
            foreach (var list in FullStructures.Values)
            {
                foreach (var folder in list)
                {
                    result.AddRange(folder.GetAllFolders());
                }
            }
            foreach (var folder in IndividualStructures.Values)
            {
                result.AddRange(folder.GetAllFolders());
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

        public static void SetPath(string path)
        {
            SavePath = path;
        }
        public static void StartUpLoad()
        {
#if TRACE
            Settings.Logger.LogWarning($"Starting to load data");
            Stopwatch.Start();
#endif
            if (CreateFile())
            {
                LoadFullStructure(Settings.CoordinatePath.Value);
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
            foreach (var list in FullStructures.Values)
            {
                foreach (var folder in list)
                {
                    folder.CleanUp();
                }
            }
            foreach (var folder in IndividualStructures.Values)
            {
                folder.CleanUp();
            }
        }

        public static List<FolderStruct> LoadFullStructure(string coordinatepath)
        {
            if (!FullStructures.TryGetValue(coordinatepath, out var list))
            {
                list = FullStructures[coordinatepath] = new List<FolderStruct>();
            }

            while (list.Count < Constants.InputStrings.Length + Constants.ExtraInputStrings.Length)
            {
                list.Add(new FolderStruct());
            }

            var set = 0;
            foreach (var coordinatetype in Constants.InputStrings)
            {
                list[set].Populate(coordinatepath + coordinatetype);
                set++;
            }

            foreach (var coordinatetype in Constants.ExtraInputStrings)
            {
                list[set].Populate(coordinatepath + coordinatetype);
                set++;
            }
            SaveFile();
            return list;
        }
        public static FolderStruct LoadSingleStructure(string coordinatepath)
        {
            if (!IndividualStructures.TryGetValue(coordinatepath, out var folderstruct))
            {
                folderstruct = IndividualStructures[coordinatepath] = new FolderStruct();
            }

            folderstruct.Populate(coordinatepath);

            SaveFile();
            return folderstruct;
        }

        public static void Update()
        {
            CleanUp();
            foreach (var list in FullStructures.Values)
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
                var serializeddict = MessagePackSerializer.Deserialize<Dictionary<string, byte[]>>(data);
                FullStructures = MessagePackSerializer.Deserialize<Dictionary<string, List<FolderStruct>>>(serializeddict["FullStruct"]);
                IndividualStructures = MessagePackSerializer.Deserialize<Dictionary<string, FolderStruct>>(serializeddict["IndividualStructures"]);

#if TRACE
                Settings.Logger.LogWarning($"Took {Stopwatch.ElapsedMilliseconds} ms to deserialize data");
#endif
                CleanUp();
                FindNewCards();
            }
            catch
            {
                LoadFullStructure(Settings.CoordinatePath.Value);
            }
        }

        private static void SaveFile()
        {
            CleanUp();
            var serializedict = new Dictionary<string, byte[]>
            {
                ["FullStruct"] = MessagePackSerializer.Serialize(FullStructures),
                ["IndividualStructures"] = MessagePackSerializer.Serialize(IndividualStructures)
            };
            File.WriteAllBytes(SavePath, MessagePackSerializer.Serialize(serializedict));
        }

        public static void Reset()
        {
            FullStructures = new Dictionary<string, List<FolderStruct>>();
            LoadFullStructure(Settings.CoordinatePath.Value);
            SaveFile();
        }
    }
}
