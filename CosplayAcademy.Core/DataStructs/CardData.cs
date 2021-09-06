using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cosplay_Academy
{
    [Serializable]
    [MessagePackObject]
    public class CardData
    {
        [IgnoreMember]
        public FolderData ParentFolder { get; private set; }

        [Key("_name")]
        public string Filepath { get; private set; }

        [Key("_defined")]
        public bool DefinedData { get; private set; }

        [Key("_personality")]
        public Dictionary<int, int> RestrictedPersonality { get; private set; }

#if KK
#elif KKS
        [Key("_interest")]
        public Dictionary<int, int> RestrictedInterest { get; private set; }
#endif
        [Key("_height")]
        public bool[] Height_Restriction { get; private set; }

        [Key("_breast")]
        public bool[] Breastsize_Restriction { get; private set; }

        [Key("_restrict")]
        public ChaFileParameter.Attribute Restricted { get; private set; }

        [Key("_allow")]
        public ChaFileParameter.Attribute Allowed { get; private set; }

        internal CardData(string _name) { Filepath = _name; }

        public CardData(string _name, FolderData parent)
        {
            Filepath = _name;
            SetParent(parent);
        }

        public CardData(string _name, FolderData parent, Additional_Card_Info.RestrictionInfo coordinfo)
        {
            Filepath = _name;
            SetParent(parent);
            DefinedData = true;
            RestrictedPersonality = coordinfo.PersonalityType_Restriction;
            Restricted = new ChaFileParameter.Attribute();
            Allowed = new ChaFileParameter.Attribute();
            Height_Restriction = coordinfo.Height_Restriction;
            Breastsize_Restriction = coordinfo.Breastsize_Restriction;
#if KK
            foreach (var item in coordinfo.TraitType_Restriction)
            {
                var restricted = item.Value == -1;
                switch (item.Key)
                {

                    case 0:
                        Restricted.hinnyo = restricted;
                        Allowed.hinnyo = !restricted;
                        break;
                    case 1:
                        Restricted.harapeko = restricted;
                        Allowed.harapeko = !restricted;
                        break;
                    case 2:
                        Restricted.donkan = restricted;
                        Allowed.donkan = !restricted;
                        break;
                    case 3:
                        Restricted.choroi = restricted;
                        Allowed.choroi = !restricted;

                        break;
                    case 4:
                        Restricted.bitch = restricted;
                        Allowed.bitch = !restricted;
                        break;
                    case 5:
                        Restricted.mutturi = restricted;
                        Allowed.mutturi = !restricted;
                        break;
                    case 6:
                        Restricted.dokusyo = restricted;
                        Allowed.dokusyo = !restricted;
                        break;
                    case 7:
                        Restricted.ongaku = restricted;
                        Allowed.ongaku = !restricted;
                        break;
                    case 8:
                        Restricted.kappatu = restricted;
                        Allowed.kappatu = !restricted;
                        break;
                    case 9:
                        Restricted.ukemi = restricted;
                        Allowed.ukemi = !restricted;
                        break;
                    case 10:
                        Restricted.friendly = restricted;
                        Allowed.friendly = !restricted;
                        break;
                    case 11:
                        Restricted.kireizuki = restricted;
                        Allowed.kireizuki = !restricted;
                        break;
                    case 12:
                        Restricted.taida = restricted;
                        Allowed.taida = !restricted;
                        break;
                    case 13:
                        Restricted.sinsyutu = restricted;
                        Allowed.sinsyutu = !restricted;
                        break;
                    case 14:
                        Restricted.hitori = restricted;
                        Allowed.hitori = !restricted;
                        break;
                    case 15:
                        Restricted.undo = restricted;
                        Allowed.undo = !restricted;
                        break;
                    case 16:
                        Restricted.majime = restricted;
                        Allowed.majime = !restricted;
                        break;
                    case 17:
                        Restricted.likeGirls = restricted;
                        Allowed.likeGirls = !restricted;
                        break;
                }
            }

#elif KKS
            foreach (var item in coordinfo.TraitType_Restriction)
            {
                var restricted = item.Value == -1;
                switch (item.Key)
                {
                    case 0:
                        Restricted.harapeko = restricted;
                        Allowed.harapeko = !restricted;
                        return;
                    case 1:
                        Restricted.choroi = restricted;
                        Allowed.choroi = !restricted;
                        return;
                    case 2:
                        Restricted.dokusyo = restricted;
                        Allowed.dokusyo = !restricted;
                        return;
                    case 3:
                        Restricted.ongaku = restricted;
                        Allowed.ongaku = !restricted;
                        return;
                    case 4:
                        Restricted.okute = restricted;
                        Allowed.okute = !restricted;
                        return;
                    case 5:
                        Restricted.friendly = restricted;
                        Allowed.friendly = !restricted;
                        return;
                    case 6:
                        Restricted.kireizuki = restricted;
                        Allowed.kireizuki = !restricted;
                        return;
                    case 7:
                        Restricted.sinsyutu = restricted;
                        Allowed.sinsyutu = !restricted;
                        return;
                    case 8:
                        Restricted.hitori = restricted;
                        Allowed.hitori = !restricted;
                        return;
                    case 9:
                        Restricted.active = restricted;
                        Allowed.active = !restricted;
                        return;
                    case 10:
                        Restricted.majime = restricted;
                        Allowed.majime = !restricted;
                        return;
                    case 11:
                        Restricted.info = restricted;
                        Allowed.info = !restricted;
                        return;
                    case 12:
                        Restricted.love = restricted;
                        Allowed.love = !restricted;
                        return;
                    case 13:
                        Restricted.talk = restricted;
                        Allowed.talk = !restricted;
                        return;
                    case 14:
                        Restricted.nakama = restricted;
                        Allowed.nakama = !restricted;
                        return;
                    case 15:
                        Restricted.nonbiri = restricted;
                        Allowed.nonbiri = !restricted;
                        return;
                    case 16:
                        Restricted.hinnyo = restricted;
                        Allowed.hinnyo = !restricted;
                        return;
                    case 17:
                        Restricted.likeGirls = restricted;
                        Allowed.likeGirls = !restricted;
                        return;
                    case 18:
                        Restricted.bitch = restricted;
                        Allowed.bitch = !restricted;
                        return;
                    case 19:
                        Restricted.mutturi = restricted;
                        Allowed.mutturi = !restricted;
                        return;
                    case 20:
                        Restricted.lonely = restricted;
                        Allowed.lonely = !restricted;
                        return;
                    default:
                        return;
                }
            }

            RestrictedInterest = coordinfo.Interest_Restriction;
#endif
        }

        [SerializationConstructor]
#if KK
        public CardData(string _name, bool _defined, Dictionary<int, int> _personality, ChaFileParameter.Attribute _restrict, ChaFileParameter.Attribute _allow, bool[] _height, bool[] _breast)
#elif KKS
        public CardData(string _name, bool _defined, Dictionary<int, int> _personality, ChaFileParameter.Attribute _restrict, ChaFileParameter.Attribute _allow, bool[] _height, bool[] _breast, Dictionary<int, int> _interest)
#endif
        {
            Filepath = _name;
            DefinedData = _defined;
            RestrictedPersonality = _personality;
            Restricted = _restrict;
            Allowed = _allow;
            Height_Restriction = _height;
            Breastsize_Restriction = _breast;
#if KK
#elif KKS
            RestrictedInterest = _interest;
#endif
        }

        internal void SetParent(FolderData _parent)
        {
            ParentFolder = _parent;
        }

        public string GetFullPath()
        {
            if (ParentFolder == null)
                return Filepath;
            return ParentFolder.FolderPath + Path.DirectorySeparatorChar + Filepath;
        }
    }
}
