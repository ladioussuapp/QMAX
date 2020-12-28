using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{

    public class SortElement : IComparable
    {
        public GameObject elementGa;
        public int row, col;

        public int CompareTo(object obj)
        {
            int result;
            try
            {
                SortElement element = obj as SortElement;
                if (this.row < element.row) // 行
                    result = -1;
                else if (this.col < element.col && this.row < element.row) // 列
                    result = -1;
                else
                    result = 1;
                return result;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }

    /// <summary>
    /// 玩家一段時間沒有操作，顯示提示
    /// 
    /// </summary>
    public class ShowBoardTipsHelper
    {
        static public void Play(BoardBehaviour boardBeh, GameObject[,] elements, TileObject[] tipsPath, Action callback)
        {
            if (tipsPath == null || tipsPath.Length == 0)
            {
                if (callback != null)
                    callback();
                return;
            }

            Transform tipEle = boardBeh.transform.Find("TipElements");
            TipElementBehaviour teb = tipEle.gameObject.GetComponent<TipElementBehaviour>();
            if (teb != null)
            { 
                List<SortElement> sortList = new List<SortElement>();
                for (int i = 0, n = tipsPath.Length; i < n; i++)
                {
                    GameObject ga = elements[tipsPath[i].Row, tipsPath[i].Col];
                    if (ga == null) {
                        continue;
                    }
                    SortElement se = new SortElement();
                    se.elementGa = ga;
                    se.row = tipsPath[i].Row;
                    se.col = tipsPath[i].Col;
                    sortList.Add(se);
                }

                sortList.Sort();
                Q.Assert(sortList.Count >= 3);
                for (int i = 0; i < sortList.Count; i++)
                {
                    GameObject ga = sortList[i].elementGa;
                    teb.StartCoroutine(Utils.DelayToInvokeDo(
                        delegate()
                        {
                            teb.AddTipElement(ga);
                        }, 0.15f * i
                    ));
                }
            }
        }

        /// <summary>
        /// 停止操作提示
        /// </summary>
        /// <param name="boardBeh"></param>
        static public void Stop(BoardBehaviour boardBeh)
        {
            Transform tipEle = boardBeh.transform.Find("TipElements");
            TipElementBehaviour teb = tipEle.gameObject.GetComponent<TipElementBehaviour>();
            if (teb != null)
            {
                teb.StopAllCoroutines();
                teb.StopAllTipElement();
            }
        }

        static private int CompareTileObject(TileObject a, TileObject b)
        {
            if (a.Row == b.Row && a.Col == b.Col)
            {
                return 0;
            }

            if (a.Row < b.Row)
                return -1;
            else if (a.Row > b.Row)
                return 1;

            if (a.Col < b.Col)
                return -1;
            else if (a.Col > b.Col)
                return 1;

            return 0;
        }
    }
}
