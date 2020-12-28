using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com4Love.Qmax.TileBehaviour
{
    /// <summary>
    /// 基本的地形物行為類
    /// </summary>
    public class BaseTileBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 行號
        /// </summary>
        public int Row { get { return Data.Row; } }

        /// <summary>
        /// 列號
        /// </summary>
        public int Col { get { return Data.Col; } }


        [HideInInspector]
        public BoardBehaviour BoardBehaviour;

        /// <summary>
        /// 可否掉落
        /// </summary>
        public bool Dropable { get { return Data.Config.Dropable; } }


        protected bool _isLinked = false;
        /// <summary>
        /// 是否被連接
        /// </summary>
        public virtual bool IsLinked
        {
            get { return _isLinked; }
            set
            {
                if (value == _isLinked)
                    return;
                //Q.Log("Change Linked");
                _isLinked = value;
                Q.Assert(_data != null);

                SetHighLight(_isLinked);
            }
        }

        protected TileObject _data;
        public virtual TileObject Data
        {
            get { return _data; }
            set
            {
                _data = value;
                RefreshData();
            }
        }

        public TileObjectConfig Config { get { return _data != null ? _data.Config : null; } }
                
        protected PlayingRuleCtr playingRuleCtr;

        protected ViewEventSystem viewEvtSys;

        protected BattleModel batModel;

        /// <summary>
        /// 被連接時調用該函數。
        /// 如果無法被連接返回-1；可以被連接返回0，並把自身加入linkQueue中；如果回退，返回1，並將自身從linkQueue移除
        /// </summary>
        /// <param name="linkQueue"></param>
        /// <returns>返回0，表示允許被連接；返回-1，表示不允許被連接；返回1，,表示回退</returns>
        public virtual int Link(List<Position> linkQueue)
        {
            return -1;
        }

        /// <summary>
        /// 清理狀態，在取消連接時會被調用。
        /// 所有在linkQueue，affectedQueue中的消除物都會被調用
        /// </summary>
        public virtual void CancelLink(List<Position> linkQueue)
        {
            IsLinked = false;
        }

        /// <summary>
        /// 在消除隊列裡的地形物會被調用。
        /// </summary>
        /// <returns>如果消除完該Tile消失，則返回true，否則返回false。</returns>
        public virtual bool Eliminate(TileObject newData) { return false; }

        public virtual void Dispose()
        {
            IsLinked = false;
            _data = null;
            GetComponent<Image>().sprite = null;
        }


        protected virtual void Start()
        {
            playingRuleCtr = GameController.Instance.PlayingRuleCtr;
            viewEvtSys = GameController.Instance.ViewEventSystem;
            batModel = GameController.Instance.Model.BattleModel;
        }



        /// <summary>
        /// 刷新data
        /// </summary>
        protected virtual void RefreshData()
        {
            if (_data == null)
                GetComponent<Image>().sprite = null;
            else
                SetHighLight(_isLinked);
                //ChangeTexture(_data.Config.ResourceIcon, _isLinked);
            //_ColTest = data == null ? -1 : data.Col;
            //_RowTest = data == null ? -1 : data.Row;
            UpdateGameObjectName();
        }


        protected virtual void UpdateGameObjectName()
        {
            gameObject.name = _data == null ? "Tile" : (_data.Config.ObjectType.ToString() + Row + "_" + Col + "_" + _data.Config.ColorType);
        }

        /// <summary>
        /// 設置材質高光
        /// </summary>
        /// <param name="highLight"></param>
        public void SetHighLight(bool highLight)
        {
            AtlasManager atlasMgr = GameController.Instance.AtlasManager;
            Image img = GetComponent<Image>();
            string spriteName = highLight ? _data.Config.ResourceIcon + "HL" : _data.Config.ResourceIcon;
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
