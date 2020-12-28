using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Com4Love.Qmax.Tools
{
    public class RichtextUtil
    {
        public static string ColorText(string msg, int color)
        {
            return string.Format("<color=#{0}>{1}</color>", color.ToString("X"), msg);
        }

        public static string SizeText(string msg, int fontSize)
        {
            return string.Format("<size={0}>{1}</size>", fontSize, msg);
        }
    }
}
