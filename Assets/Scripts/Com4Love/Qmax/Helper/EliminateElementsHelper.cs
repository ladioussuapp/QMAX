using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.TileBehaviour;
using System.Collections.Generic;
using UnityEngine;


namespace Com4Love.Qmax.Helper
{
    public class EliminateElementsHelper
    {
        BoardBehaviour boardbeh;
        PlayingRuleCtr playingRuleCtr;

        public EliminateElementsHelper(BoardBehaviour boardbeh)
        {
            this.boardbeh = boardbeh;
            playingRuleCtr = GameController.Instance.PlayingRuleCtr;
        }


        /// <summary>
        /// 消除同樣顏色的消除物
        /// </summary>
        /// <param name="color"></param>
        public void EliminateColorElements(ColorType color)
        {
            //如果有轉換石，需要視覺上先把轉換石範圍內的消除物變成普通消除物，所以需要一個延遲
            bool hasCovertBlock = false;
            const float CONVERT_DELAY = 0.3f;
            List<Position> linkPath = new List<Position>();
            List<ElementBehaviour> linkElements = new List<ElementBehaviour>();
            GameObject[,] eleViews = boardbeh.GetTypeGameObjects(TileType.Element);

            for (int r = 0, n = eleViews.GetLength(0); r < n; r++)
            {
                for (int c = 0, m = eleViews.GetLength(1); c < m; c++)
                {
                    ElementBehaviour beh = boardbeh.GetLinkableElementAt(r, c);
                    if (beh != null && beh.Config.ColorType == color)
                    {
                        linkPath.Add(new Position(r, c));
                        linkElements.Add(beh);
                        beh.Link(linkPath);

                        if (beh.Type == ElementType.ConvertBlock)
                        {
                            hasCovertBlock = true;
                        }
                    }
                }
            }


            if (!hasCovertBlock)
            {
                boardbeh.Eliminate(linkPath);
            }
            else
            {
                boardbeh.StartCoroutine(Utils.DelayToInvokeDo(delegate()
                {
                    boardbeh.Eliminate(linkPath);
                }, CONVERT_DELAY));
            }

            //Debug.Log("------------------------------------eliminate queue count is " + eliminateQueue.Count);  

        }


        /// <summary>
        /// 消除單個消除物
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool EliminateOneElement(int r, int c)
        {
            ///消除單個物品道具的威力//
            int hurt = (int)GameController.Instance.PropCtr.GetPropValue(PropType.EliminateOne);

            ///當前道具消除的物體可能不是普通元素，可能有障礙物，覆蓋物...並且根據威力來逐個消除///
            ///現在這個方法值提供了元素的消除，其他類型的消除方法待添加///
            ///提供設置消除威力藉口待添加///
            List<Position> linkPath = new List<Position>();
            linkPath.Add(new Position(r, c));
            List<int> hurtlist = new List<int>();
            hurtlist.Add(hurt);

            return playingRuleCtr.Eliminate(linkPath, hurtlist);
        }


        public bool IsCanEliminateByOne(int r, int c)
        {
            return GetOneEliminateObject(r, c).Count != 0;
        }

        private List<BaseTileBehaviour> GetOneEliminateObject(int r, int c)
        {
            GameObject[,] elementViews = boardbeh.GetTypeGameObjects(TileType.Element);
            GameObject[,] coverViews = boardbeh.GetTypeGameObjects(TileType.Cover);
            GameObject[,] obstacleViews = boardbeh.GetTypeGameObjects(TileType.Obstacle);
            GameObject[,] BottomViews = boardbeh.GetTypeGameObjects(TileType.Bottom);

            List<BaseTileBehaviour> tile = new List<BaseTileBehaviour>();

            ///按從上倒下的順序加入列表，如覆蓋物在嘴上先加入...///
            ///後面消除處理會用到這個順序///
            if (coverViews[r, c] != null)
            {
                tile.Add(coverViews[r, c].GetComponent<BaseTileBehaviour>());
            }

            if (obstacleViews[r, c] != null)
            {
                tile.Add(obstacleViews[r, c].GetComponent<BaseTileBehaviour>());
            }

            if (elementViews[r, c] != null)
            {
                tile.Add(elementViews[r, c].GetComponent<BaseTileBehaviour>());
            }

            if (BottomViews[r, c] != null)
            {
                tile.Add(BottomViews[r, c].GetComponent<BaseTileBehaviour>());
            }

            return tile;
        }

    }
}

