using System.Collections.Generic;
namespace Cosplay_Academy
{
    public class OutfitData
    {
        private bool Set_FirstTime;
        private bool Set_Amateur;
        private bool Set_Pro;
        private bool Set_Lewd;
        public string[] FirstTime;
        public string[] Amateur;
        public string[] Pro;
        public string[] Lewd;
        private string Path_FirstTime;
        private string Path_Amateur;
        private string Path_Pro;
        private string Path_Lewd;
        public bool Anger;


        public OutfitData()
        {
            Path_FirstTime = Path_Amateur = Path_Pro = Path_Lewd = "Default";
            FirstTime = new string[0];
            Amateur = new string[0];
            Pro = new string[0];
            Lewd = new string[0];
            Set_FirstTime = Set_Amateur = Set_Pro = Set_Lewd = Anger = false;
        }

        public void Clear()
        {
            FirstTime = new string[0];
            Amateur = new string[0];
            Pro = new string[0];
            Lewd = new string[0];
            Set_FirstTime = false;
            Set_Amateur = false;
            Set_Pro = false;
            Set_Lewd = false;
        }
        public List<string> Sum(int level)//returns list that is the sum of all available lists.
        {
            List<string> temp = new List<string>();
            if (!Anger)
            {
                if (level >= 3)
                    temp.AddRange(Lewd);
                if (level >= 2)
                    temp.AddRange(Pro);
                if (level >= 1)
                    temp.AddRange(Amateur);
            }
            temp.AddRange(FirstTime);
            return temp;
        }
        public void Insert(int level, string[] Data, bool IsSet)//Insert data and append existing data to new list and confirm if this is a set item
        {
            List<string> temp = new List<string>();
            if (Exportarray(level).Length > 0)//append existing data
            {
                string[] temp2 = Exportarray(level);
                temp.AddRange(temp2);
            }
            if (level == 3)
            {
                temp.AddRange(Data);
                Lewd = temp.ToArray();
                //Lewd = Data;
                Set_Lewd = IsSet;
            }
            else if (level == 2)
            {
                temp.AddRange(Data);
                Pro = temp.ToArray();
                //Pro = Data;

                Set_Pro = IsSet;
            }
            else if (level == 1)
            {
                temp.AddRange(Data);
                Amateur = temp.ToArray();
                //Amateur = Data;

                Set_Amateur = IsSet;
            }
            else
            {
                temp.AddRange(Data);
                FirstTime = temp.ToArray();
                //FirstTime = Data;

                Set_FirstTime = IsSet;
            }
        }
        public string Random(int level)//get any random outfit according to experience
        {
            if (ExpandedOutfit.SumRandom.Value)
            {
                string result;
                List<string> temp = Sum(level);
                result = temp[UnityEngine.Random.Range(0, temp.Count)];
                return result;
            }
            if (!Anger)
            {
                if (level == 3)
                    return Lewd[UnityEngine.Random.Range(0, Lewd.Length)];
                else if (level == 2)
                    return Pro[UnityEngine.Random.Range(0, Pro.Length)];
                else if (level == 1)
                    return Amateur[UnityEngine.Random.Range(0, Amateur.Length)];
            }
            return FirstTime[UnityEngine.Random.Range(0, FirstTime.Length)];
        }
        //public bool SetExists(int level)//Does a set exist for this outfit and lewd state
        //{
        //    if (!Anger)
        //    {
        //        if (level == 3)
        //            return (Set_FirstTime || Set_Amateur || Set_Pro || Set_Lewd);
        //        if (level == 2)
        //            return (Set_FirstTime || Set_Amateur || Set_Pro);
        //        if (level == 1)
        //            return (Set_FirstTime || Set_Amateur);
        //    }
        //    return (Set_FirstTime);
        //}
        public string RandomSet(int level, bool Match)//if set exists add its items to pool along with any coordinated outfit and other choices
        {
            List<string> temp = new List<string>();
            if (!Anger)
            {
                if (level >= 3 && (Set_Lewd || !Match))
                    temp.AddRange(Lewd);
                else if (level >= 3)
                    temp.Add(Path_Lewd);

                if (level >= 2 && (Set_Pro || !Match))
                    temp.AddRange(Pro);
                else if (level >= 2)
                    temp.Add(Path_Pro);

                if (level >= 1 && (Set_Amateur || !Match))
                    temp.AddRange(Amateur);
                else if (level >= 1)
                    temp.Add(Path_Amateur);
            }
            if (Set_FirstTime || !Match)
                temp.AddRange(FirstTime);
            else
                temp.Add(Path_FirstTime);

            return temp[UnityEngine.Random.Range(0, temp.Count)];
        }
        public string[] Exportarray(int level)
        {
            if (level == 3)
                return Lewd;
            if (level == 2)
                return Pro;
            if (level == 1)
                return Amateur;
            return FirstTime;
        }
        public void Coordinate()//set a random outfit to coordinate for non-set items
        {
            Path_Lewd = Random(3);
            Path_Pro = Random(2);
            Path_Amateur = Random(1);
            Path_FirstTime = Random(0);
        }
        public bool IsSet(int level)
        {
            if (level == 3)
                return Set_Lewd;
            if (level == 2)
                return Set_Pro;
            if (level == 1)
                return Set_Amateur;
            return Set_FirstTime;
        }
        //public void Path_set(int level, string path) //Testing code
        //{
        //    if (level == 3)
        //        Path_Lewd = path;
        //    else if (level == 2)
        //        Path_Pro = path;
        //    else if (level == 1)
        //        Path_Amateur = path;
        //    else Path_FirstTime = path;
        //}
        //public string Path_print(int level)//Testing code to see if path setting is correct
        //{
        //    if (level == 3)
        //        return (Path_Lewd);
        //    if (level == 2)
        //        return (Path_Pro);
        //    if (level == 1)
        //        return (Path_Amateur);
        //    return (Path_FirstTime);
        //}
    }
}
