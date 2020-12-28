using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com4Love.Qmax.TileBehaviour
{
    public class ElementBehaviour : BaseTileBehaviour
    {
        private GameObject QuestionMark;


        public ElementType Type { get { return Config == null ? ElementType.NotElement : Config.ElementType; } }

        public override TileObject Data
        {
            set
            {
                if (value == base.Data)
                    return;

                // 再賦予值
                base.Data = value;
                if (value != null)
                {
                    this.ResetData();
                }
                isDataDirty = true;
            }
        }

        private UIBattleTipsBehaviour.AnimType animType;
        protected override void Start()
        {
            base.Start();

            GameController.Instance.ModelEventSystem.OnBattleTipsCheck += this.OnTipsCheck;
        }

        protected void ResetData()
        {
            bool rel = false;
            if (Config.ColorType == ColorType.Wood && Type == ElementType.Bomb)
            {
                if (!PlayerPrefsTools.GetBoolValue(UIBattleTipsBehaviour.AnimType.GreenBomb.ToString(), true))
                    rel = true;
                animType = UIBattleTipsBehaviour.AnimType.GreenBomb;
            }
            else if (Config.ColorType == ColorType.Golden && Type == ElementType.ConvertBlock)
            {
                if (!PlayerPrefsTools.GetBoolValue(UIBattleTipsBehaviour.AnimType.YellowCover.ToString(), true))
                    rel = true;
                animType = UIBattleTipsBehaviour.AnimType.YellowCover;
            }
            else if (Config.ObjectName == "BombBlack")
            {
                if (!PlayerPrefsTools.GetBoolValue(UIBattleTipsBehaviour.AnimType.BlackBomb.ToString(), true))
                    rel = true;
                animType = UIBattleTipsBehaviour.AnimType.BlackBomb;
            }

            Q.Assert(GameController.Instance.Model.BattleModel != null,
                "EleBehBeh:ResetData assert1");

            if (GameController.Instance.Model.BattleModel == null)
            {
                rel = false;
            }

            if (GameController.Instance.Model.BattleModel != null &&
                GameController.Instance.Model.BattleModel.CrtStageConfig != null &&
                GameController.Instance.Model.BattleModel.CrtStageConfig.ShowNum <= 3)
            {
                rel = false;
            }

            if (rel)
            {
                if (QuestionMark == null)
                {
                    GameObject ins = Resources.Load<GameObject>("Prefabs/Ui/UIBattleTipsMark");
                    QuestionMark = Instantiate<GameObject>(ins);
                    QuestionMark.transform.SetParent(this.transform);
                    QuestionMark.transform.localPosition = new Vector3(0, 0, -20);
                    QuestionMark.transform.localScale = new Vector3(1, 1, 1);
                }
                QuestionMark.SetActive(true);
            }
            else if (QuestionMark != null)
            {
                Destroy(QuestionMark);
                QuestionMark = null;
            }

            GameObject light = this.transform.Find("Light").gameObject;
            light.SetActive(false);
        }

        private void OnTipsCheck(bool value, UIBattleTipsBehaviour.AnimType type)
        {
            if (type == animType && QuestionMark != null)
            {
                QuestionMark.SetActive(!value);
            }
        }

        void OnDetroy()
        {
            GameController.Instance.ModelEventSystem.OnBattleTipsCheck -= this.OnTipsCheck;
        }

        public override bool Eliminate(TileObject newData)
        {
            Q.Assert(newData == null);
            return true;
        }


        /// <summary>
        /// 被連接時調用該函數。
        /// </summary>
        /// 如果無法被連接返回-1；可以被連接返回0，並把自身加入linkQueue中；
        /// 如果回退，返回1，並將自身從linkQueue移除
        /// <param name="linkQueue"></param>
        /// <returns>返回0，表示允許被連接；返回-1，表示不允許被連接；返回1，表示回退</returns>
        public override int Link(List<Position> linkQueue)
        {
            Q.Assert(linkQueue != null);

            //無顏色無法被連接
            if (Config.ColorType == ColorType.None)
            {
                if (linkQueue.Count == 0 && Config.ObjectName == "BombBlack")
                {
                    UIBattleTipsBehaviour.Show(UIBattleTipsBehaviour.AnimType.BlackBomb);
                }
                return -1;
            }

            //第一連接的消除物不能是轉換石
            if (linkQueue.Count == 0 && this.Type == ElementType.ConvertBlock)
            {
                //點擊轉換石，出現提示
                if (this.Config.ColorType == ColorType.Golden)
                {
                    UIBattleTipsBehaviour.Show(UIBattleTipsBehaviour.AnimType.YellowCover);
                }
                return -1;
            }

            // 第一連接的消除物不能是多態石（多色）
            if (linkQueue.Count == 0 && this.Type == ElementType.MultiColor)
            {
                return -1;
            }

            if (linkQueue.Count == 0)
            {
                IsLinked = true;
                linkQueue.Add(new Position(Row, Col));
                PlayLinkSound(1);
                if (viewEvtSys.TileStatusChangeEvent != null)
                    viewEvtSys.TileStatusChangeEvent(this.Data, ViewEventSystem.TileStatusChangeMode.ToLink, linkQueue);
                return 0;
            }

            Position crtPos = new Position(Row, Col);

            //相同位置，忽略
            if (linkQueue[linkQueue.Count - 1] == crtPos)
                return -1;

            Position lastPos = linkQueue[linkQueue.Count - 1];
            //回退機制
            if (linkQueue.Count >= 2 && crtPos == linkQueue[linkQueue.Count - 2])
            {
                GetEleBehAt(lastPos.Row, lastPos.Col).CancelLink(linkQueue);
                return 1;
            }


            TileObject lastTileData = GetElementDataAt(lastPos.Row, lastPos.Col);
            //判斷可連線的條件            
            //是同樣顏色的消除物
            Position pos = linkQueue[0];
            TileObject firstTileData = GetElementDataAt(pos.Row, pos.Col);
            ColorType mainColor = firstTileData.Config.ColorType;

            Q.Assert(lastTileData != null, "{0},{1}", lastPos.Row, lastPos.Col);
            bool ret = lastTileData.Config.ObjectType == TileType.Element && mainColor == _data.Config.ColorType;
            // 多態石(多色)
            if (!ret && this.Type == ElementType.MultiColor)
            {
                foreach (ColorType ct in _data.Config.AllColors)
                {
                    if (mainColor == ct)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //相鄰位置
            ret = ret && Mathf.Abs(lastPos.Row - Row) <= 1 && Mathf.Abs(lastPos.Col - Col) <= 1;
            //尚未被連中
            ret = ret && !IsLinked;
            if (!ret)
                return -1;

            if (!playingRuleCtr.CheckLinkableBySeperator(lastPos.Row, lastPos.Col, Row, Col))
                return -1;

            //Q.Log("ElementBehaviour.OnLink() 3, r={0}, c={1}", lastEle.Row, lastEle.Col);
            IsLinked = true;
            linkQueue.Add(crtPos);

            //只有在連數量大於3時才會開始連線
            if (linkQueue.Count == 3)
            {
                BoardBehaviour.StartDrawLine(linkQueue[0]);
                BoardBehaviour.DrawToPoint(linkQueue[1]);
                BoardBehaviour.DrawToPoint(linkQueue[2]);
            }
            else if (linkQueue.Count > 3)
            {
                BoardBehaviour.DrawToPoint(crtPos);
            }


            PlayLinkSound(linkQueue.Count);
            if (viewEvtSys.TileStatusChangeEvent != null)
                viewEvtSys.TileStatusChangeEvent(this.Data, ViewEventSystem.TileStatusChangeMode.ToLink, linkQueue);
            return 0;
        }


        public override void CancelLink(List<Position> linkQueue)
        {
            //移除掉自己
            linkQueue.RemoveAt(linkQueue.Count - 1);

            if (linkQueue.Count <= 2)
                BoardBehaviour.ClearDrawLine();
            else if (linkQueue.Count > 0)
                BoardBehaviour.DrawToPoint(linkQueue[linkQueue.Count - 1]);

            if (linkQueue.Count > 0)
                PlayLinkSound(linkQueue.Count);

            // 取消抖動
            PlayLinkShake(-1, 0);
            if (viewEvtSys.TileStatusChangeEvent != null)
                viewEvtSys.TileStatusChangeEvent(this.Data, ViewEventSystem.TileStatusChangeMode.ToUnlink, linkQueue);
            base.CancelLink(linkQueue);
        }


        protected override void RefreshData()
        {
            base.RefreshData();
            Transform tf = transform.Find("Image");
            if (tf != null)
            {
                AtlasManager atlasMgr = GameController.Instance.AtlasManager;
                Image img = tf.gameObject.GetComponent<Image>();
                if (img != null && _data != null)
                {
                    string spriteName = _data.Config.ResourceIcon + "TL";

                    if (img.sprite != null && img.sprite.name == spriteName)
                        return;

                    Sprite s = atlasMgr.GetSprite(Atlas.Tile, spriteName);
                    if (s == null)
                        return;

                    img.sprite = s;
                    img.SetNativeSize();
                }
            }
        }



        /// <summary>
        /// 播放連接音效
        /// </summary>
        /// <param name="linkCount"></param>
        private void PlayLinkSound(int linkCount)
        {
            GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
                "SD_press_step" + Math.Min(28, linkCount),
                delegate(AudioClip clip)
                {
                    AudioSource asour = GetComponent<AudioSource>();
                    if (asour == null || !asour.enabled)
                        return;
                    asour.PlayOneShot(clip);
                });

        }

        /// <summary>
        /// 播放元素抖動的動畫
        /// </summary>
        /// <param name="comboRate"></param>
        /// <param name="time"></param>
        public void PlayLinkShake(float comboRate, float time)
        {
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

            if (comboRate >= 2.0f && !stateInfo.IsName("Shake3"))
            {
                animator.SetBool("ShakeReset", false);
                animator.Play("Shake3", 1, time);
            }
            else if (comboRate >= 1.5f && comboRate < 2.0f && !stateInfo.IsName("Shake2"))
            {
                animator.SetBool("ShakeReset", false);
                animator.Play("Shake2", 1, time);
            }
            else if (comboRate >= 1.2f && comboRate < 1.5f && !stateInfo.IsName("Shake1"))
            {
                animator.SetBool("ShakeReset", false);
                animator.Play("Shake1", 1, time);
            }
            else if (comboRate < 1.2)
            {
                animator.SetBool("ShakeReset", true);
            }
        }

        public void SetBoomEffectEnable(bool enable)
        {
            Transform effect = transform.Find("EffectBoomIcon");
            if (effect == null)
                return;
            bool isActive = effect.gameObject.activeSelf;
            effect.gameObject.SetActive(enable);
            if (isActive != enable)
            {
                BoardBehaviour.BombFuseAudioEffect(enable);
            }
        }


        private bool isDataDirty = false;

        void Update()
        {
            if (isDataDirty)
            {
                if (Data != null && Data.Config != null)
                {
                    SetBoomArrowEnable(Type == ElementType.Bomb, Data.Config);
                }
                isDataDirty = false;
            }
        }

        private void SetBoomArrowEnable(bool enable, TileObjectConfig config)
        {
            Transform arrow = transform.Find("UIArrow");
            if (arrow == null)
                return;
            arrow.gameObject.SetActive(false);

            //             if (!enable)
            //             {
            //                 arrow.gameObject.SetActive(false);
            //                 return;
            //             }
            // 
            //             Animator animator = arrow.GetComponent<Animator>();
            //             ElimRangeMode mode = config.RangeMode;
            //             if (mode == ElimRangeMode.Normal)
            //             {
            //                 arrow.gameObject.SetActive(false);
            //                 return;
            //             }
            // 
            //             if (animator != null)
            //             {
            //                 arrow.gameObject.SetActive(true);
            //                 if (mode == ElimRangeMode.Horizontal)
            //                 {
            //                     animator.Play("Init", 0);
            //                     animator.SetTrigger("TriggerCorss");
            //                 }
            //                 else if (mode == ElimRangeMode.Vertical)
            //                 {
            //                     animator.Play("Init", 0);
            //                     animator.SetTrigger("TriggerVertical");
            //                 }
            //                 else
            //                 {
            //                     animator.Play("Init", 0);
            //                     animator.SetTrigger("TriggerAll");
            //                 }
            //             }
            // 
            //             ArrowBehvaiour arrowBeh = arrow.GetComponent<ArrowBehvaiour>();
            //             if (arrowBeh == null)
            //                 return;
            // 
            //             // 顏色對應的圖片
            //             if (config.ColorType == ColorType.Water)
            //                 arrowBeh.SetArrowImage("Jiantou_01");
            //             else if (config.ColorType == ColorType.Golden)
            //                 arrowBeh.SetArrowImage("Jiantou_02");
            //             else if (config.ColorType == ColorType.Earth)
            //                 arrowBeh.SetArrowImage("Jiantou_03");
            //             else if (config.ColorType == ColorType.Fire)
            //                 arrowBeh.SetArrowImage("Jiantou_04");
            //             else if (config.ColorType == ColorType.Wood)
            //                 arrowBeh.SetArrowImage("Jiantou_05");
        }


        protected TileObject GetElementDataAt(int r, int c)
        {
            return batModel.GetElementAt(r, c);
        }

        protected ElementBehaviour GetEleBehAt(int r, int c)
        {
            return BoardBehaviour.eleViews[r, c].GetComponent<ElementBehaviour>();
        }

    }
}
