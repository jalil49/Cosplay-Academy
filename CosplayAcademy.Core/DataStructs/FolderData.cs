using ExtensibleSaveFormat;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cosplay_Academy
{
    [Serializable]
    [MessagePackObject]
    public class FolderData
    {
        [Key("_folder")]
        public string FolderPath { get; private set; }

        [Key("_set")]
        public bool PartofSet { get; private set; }

        [Key("_sub")]
        public List<FolderData> Subfolderdata { get; private set; }

        [Key("_cards")]
        public List<CardData> Cards { get; private set; }

        [SerializationConstructor]
        public FolderData(string _folder, bool _set, List<CardData> _cards, List<FolderData> _sub)
        {
            FolderPath = _folder;
            Subfolderdata = _sub;
            PartofSet = _set;
            Cards = _cards;
            CleanUp();
            SetParent();
        }

        public FolderData(string path)
        {
            Subfolderdata = new List<FolderData>();
            Cards = new List<CardData>();
            FolderPath = path;
            var sep = Path.DirectorySeparatorChar;
            PartofSet = path.Contains(sep + "Sets" + sep);
            if (Directory.Exists(path))
            {
                FindCards();
                FindSubFolders();
            }
        }

        public void FindCards()
        {
            var files = Directory.GetFiles(FolderPath, "*.png");
            var chafilecoordinate = new ChaFileCoordinate();
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (!Cards.Any(x => x.Filepath == name) && chafilecoordinate.LoadFile(file))
                {
                    var ACI_Data = ExtendedSave.GetExtendedDataById(chafilecoordinate, "Additional_Card_Info");
                    if (ACI_Data == null)
                    {
                        Cards.Add(new CardData(name, this));
                        continue;
                    }

                    var data = new Additional_Card_Info.CoordinateInfo();

                    switch (ACI_Data.version)
                    {
                        case 0:
                            data = Additional_Card_Info.Migrator.CoordinateMigrateV0(ACI_Data);
                            break;
                        case 1:
                            if (ACI_Data.data.TryGetValue("CoordinateInfo", out var ByteData) && ByteData != null)
                            {
                                data = MessagePackSerializer.Deserialize<Additional_Card_Info.CoordinateInfo>((byte[])ByteData);
                            }
                            break;
                        default:
                            Settings.Logger.LogWarning("New version of Additional Card Info found, please update");
                            break;
                    }
                    Cards.Add(new CardData(name, this, data.RestrictionInfo));
                }
            }
            Settings.Logger.LogDebug($"{FolderPath} found {Cards.Count} cards");
        }

        public void FindSubFolders()
        {
            var sublist = DirectoryFinder.Grab_Folder_Directories(FolderPath, false);
            foreach (var subfolder in sublist)
            {
                if (Subfolderdata.Any(X => X.FolderPath == subfolder))
                {
                    continue;
                }
                Subfolderdata.Add(new FolderData(subfolder));
            }
        }

        public List<CardData> GetAllCards()
        {
            var list = new List<CardData>();

            list.AddRange(Cards);

            foreach (var item in Subfolderdata)
            {
                list.AddRange(item.GetAllCards());
            }

            return list;
        }

        public void Update()
        {
            Cards.Clear();
            FindCards();
            foreach (var item in Subfolderdata)
            {
                item.Update();
            }
            FindSubFolders();
        }

        public void CleanUp()
        {
            var foldercheck = Subfolderdata.Select(x => x.FolderPath).ToArray();
            for (var i = foldercheck.Length - 1; i > -1; i--)
            {
                if (!Directory.Exists(foldercheck[i]))
                {
                    Subfolderdata.RemoveAt(i);
                }
            }
            var sep = Path.DirectorySeparatorChar;
            var cardscheck = Cards.Select(x => x.Filepath).ToArray();
            for (var i = cardscheck.Length - 1; i > -1; i--)
            {
                if (!File.Exists(FolderPath + sep + cardscheck[i]))
                {
                    Cards.RemoveAt(i);
                }
            }
        }

        private void SetParent()
        {
            foreach (var item in Cards)
            {
                item.SetParent(this);
            }
        }

        public List<FolderData> GetAllFolders()
        {
            var result = new List<FolderData> { this };
            foreach (var item in Subfolderdata)
            {
                result.AddRange(item.GetAllFolders());
            }
            return result;
        }
    }
}
