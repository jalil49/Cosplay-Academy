using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    [Serializable]
    [MessagePackObject]
    public class HFolderStruct
    {
        [Key("_hfol")]
        public List<FolderData> FolderData { get; private set; }

        public HFolderStruct() { FolderData = new List<FolderData>(); }

        [SerializationConstructor]
        public HFolderStruct(List<FolderData> _hfol) { FolderData = _hfol; }

        public List<FolderData> GetAllFolders()
        {
            var result = new List<FolderData>();
            foreach (var item in FolderData)
            {
                result.AddRange(item.GetAllFolders());
            }
            return result;
        }

        internal List<CardData> GetAllCards()
        {
            var result = new List<CardData>();
            foreach (var folder in FolderData)
            {
                result.AddRange(folder.GetAllCards());
            }
            return result;
        }

        public void CleanUp()
        {
            for (var j = FolderData.Count - 1; j > -1; j--)
            {
                var folder = FolderData[j];
                if (!Directory.Exists(folder.FolderPath))
                {
                    FolderData.RemoveAt(j);
                    continue;
                }
                folder.CleanUp();
                if (folder.Cards.Count == 0)
                {
                    FolderData.RemoveAt(j);
                }
            }
        }

        public void Update()
        {
            foreach (var folder in FolderData)
            {
                folder.Update();
            }
        }

        public void Populate(string path)
        {
            var sep = Path.DirectorySeparatorChar;

            var subdirectories = DirectoryFinder.Grab_Folder_Directories(path, true);
            foreach (var directory in subdirectories)
            {
                var endsinsets = directory.EndsWith(sep + "Sets");
                if (endsinsets)
                {
                    var setdirectories = DirectoryFinder.Grab_Folder_Directories(directory, false);
                    foreach (var set in setdirectories)
                    {
                        if (FolderData.Any(x => x.FolderPath == set))
                            continue;

                        FolderData.Add(new FolderData(set));
                    }
                    continue;
                }

                if (FolderData.Any(x => x.FolderPath == directory))
                    continue;

                FolderData.Add(new FolderData(directory));
            }
        }
    }
}
