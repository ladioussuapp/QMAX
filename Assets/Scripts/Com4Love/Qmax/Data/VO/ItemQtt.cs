using System;
namespace Com4Love.Qmax.Data.VO
{
    public class ItemQtt
    {
        public RewardType type;
        public int Qtt;

        static public ItemQtt[] ParseMulti(string value)
        {
            ItemQtt[] ret = null;
            if (value != null && value != "")
            {
                string[] arr = value.Split('|');
                ret = new ItemQtt[arr.Length];
                for (int i = 0, n = arr.Length; i < n; i++)
                {
                    string[] arr1 = arr[i].Split(',');
                    ItemQtt itemQtt = new ItemQtt();
                    itemQtt.type = (RewardType)Convert.ToInt32(arr1[0]);
                    itemQtt.Qtt = Convert.ToInt32(arr1[1]);
                    ret[i] = itemQtt;
                }
            }
            else
            {
                ret = new ItemQtt[0];
            }
            return ret;
        }


        static public ItemQtt ParseOne(string value)
        {
            if (value == null || value == "")
                return null;

            string[] arr1 = value.Split(',');
            ItemQtt ret = new ItemQtt();
            ret.type = (RewardType)Convert.ToInt32(arr1[0]);
            ret.Qtt = Convert.ToInt32(arr1[1]);
            return ret;
        }
    }
}
