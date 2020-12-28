using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Com4Love.TexturePacker
{
    /// <summary>
    /// 解析TexturePacker的*.tpsheet文件的類
    /// </summary>
    public class TPParser
    {
        
        static public TPSheet Parse(string name, string content)
        {
            StringReader reader = new StringReader(content);
            TPSheet ret = new TPSheet();
            ret.Name = name;

            string line = null;
            List<TPSprite> list = new List<TPSprite>();

            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                    break;

                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] arr = line.Split(';');
                if (arr.Length != 7)
                    continue;

                try
                {
                    //TexturePacker的坐標原點在左下角
                    TPSprite s = new TPSprite();
                    s.Name = arr[0];
                    s.Left = Convert.ToInt32(arr[1]);
                    s.Bottom = Convert.ToInt32(arr[2]);
                    s.Width = Convert.ToInt32(arr[3]);
                    s.Height = Convert.ToInt32(arr[4]);
                    s.PivotX = Convert.ToSingle(arr[5]);
                    s.PivotY = Convert.ToSingle(arr[6]);
                    list.Add(s);
                }
                catch (Exception e)
                {
                    //Do nothing.
                    Debug.LogWarning(e.Message);
                }
            }

            ret.sprites = list;
            return ret;
        }

        static public TPSheet ParseByPath(string name, string path)
        {
            TPSheet ret = new TPSheet();
            ret.Name = name;
            ret.Path = path;

            StreamReader reader = new StreamReader(path);
            string line = null;
            List<TPSprite> list = new List<TPSprite>();

            while (true)
            {
                line = reader.ReadLine();
                if(line == null)
                    break;

                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] arr = line.Split(';');
                if (arr.Length != 7)
                    continue;

                try
                {
                    //TexturePacker的坐標原點在左下角
                    TPSprite s = new TPSprite();
                    s.Name = arr[0];
                    s.Left = Convert.ToInt32(arr[1]);
                    s.Bottom = Convert.ToInt32(arr[2]);
                    s.Width = Convert.ToInt32(arr[3]);
                    s.Height = Convert.ToInt32(arr[4]);
                    s.PivotX = Convert.ToSingle(arr[5]);
                    s.PivotY = Convert.ToSingle(arr[6]);
                    list.Add(s);
                }
                catch (Exception e)
                {
                    //Do nothing.
                    Debug.LogWarning(e.Message);
                }
            }

            ret.sprites = list;
            return ret;
        }
    }
}
