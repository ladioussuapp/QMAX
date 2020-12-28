using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Com4Love.Qmax;

public class MapThingManager : MonoBehaviour
{
    public const string LVLBUTTON_STATEUNLOCK_PREFAB = "Prefabs/Map/LvlButtonUnLock";
    public const string LVLBUTTON_STATE_LOCKED_PREFAB = "Prefabs/Map/LvlButtonLocked";
    public const string LVLBUTTON_STATE_SELECTED__PREFAB = "Prefabs/Map/LvlButtonSelected";

    //按鈕選中的特效
    Transform ButtonSelectEffect;
    Transform MapButtonUnlockEffect;
    RectTransform InfoBoardCover;
    public RectTransform InfoBoardContainer;

    public static MapThingManager Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
        GameController.Instance.PoolManager.Despool("Prefabs/Map/ButtonInfoBoardPrefab");
    }

    public Transform GetButtonEffect()
    {
        if (ButtonSelectEffect == null)
        {
            //選中特效
            ButtonSelectEffect = GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Effects/EffectGuankaxuanze");
        }

        return ButtonSelectEffect;
    }

    public Transform GetMapButtonUnlockEffect()
    {
        //TODO 邏輯待修改    1.允許多個解鎖特效    2 在地圖被隱藏後將所有解鎖特效移除以防止多次播放  

        //if (MapButtonUnlockEffect == null)
        //{
        //    MapButtonUnlockEffect = GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Effects/MapButtonUnlockEffect");
        //}

        DurationTimeDispatcher timeDispatcher = null;
        Action playOver = null;

        playOver = delegate ()
        {
            timeDispatcher.OnTimeOut -= playOver;
            GameObject.Destroy(timeDispatcher.gameObject);
        };

        Transform effect = GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Effects/MapButtonUnlockEffect");
        Q.Assert(effect != null, "解鎖特效未找到");

        timeDispatcher = effect.GetComponent<DurationTimeDispatcher>();
        timeDispatcher.OnTimeOut += playOver;

        return timeDispatcher.transform;
    }
 
    public Dictionary<Transform, int> InfoBoardSort = new Dictionary<Transform, int>();

    //直接按關卡排序
    public RectTransform CreateButtonInfoBoard(Vector2 anchoredPosition, int stageId)
    {
        RectTransform rT = GameController.Instance.PoolManager.PrefabSpawn("Prefabs/Map/ButtonInfoBoardPrefab") as RectTransform;
        rT.SetParent(InfoBoardContainer);
        rT.localScale = new Vector3(1, 1, 1);
        rT.anchoredPosition3D = anchoredPosition;
        rT.localRotation = Quaternion.identity;
        SortButtonInfoBoard(rT, stageId);
        InfoBoardSort.Add(rT, stageId);
        return rT;
    }

    //創建星星
    public RectTransform CreateStarInfoBoard(Vector2 anchoredPosition, int stageId)
    {
        RectTransform rT = GameController.Instance.PoolManager.PrefabSpawn("Prefabs/Map/MapStarInfoBoard") as RectTransform;
        rT.SetParent(InfoBoardContainer);
        rT.localScale = new Vector3(1, 1, 1);
        rT.anchoredPosition3D = anchoredPosition;
        rT.localRotation = Quaternion.identity;
        return rT;
    }

    public void RemoveButtonInfoBoard(Transform t)
    {
        if (t != null)
        {
            InfoBoardSort.Remove(t);

            GameController.Instance.PoolManager.Despawn(t);
        }
    }

    public void RemoveStarInfoBoard(Transform t)
    {
        if (t != null)
        {
            GameController.Instance.PoolManager.Despawn(t);
        }
    }


    public RectTransform GetBoardCover()
    {
        if (InfoBoardCover == null)
        {
            InfoBoardCover = InfoBoardContainer.Find("Cover").GetComponent<RectTransform>();
        }

        InfoBoardCover.SetSiblingIndex(0);

        return InfoBoardCover;
    }

    public void SortButtonInfoBoard(Transform t, int sortMark)
    {
        int index = InfoBoardContainer.childCount - 1;
        Transform c;

        for (int i = InfoBoardContainer.childCount - 1; i >= 0; i--)
        {
            c = InfoBoardContainer.GetChild(i);
            if (t == c || !InfoBoardSort.ContainsKey(c))
            {
                continue;
            }

            int sortMarkOld = InfoBoardSort[c];

            //關卡越小，越在上層。索引越大
            if (sortMark > sortMarkOld)
            {
                //關卡越高越往上排
                index = i;
            }
        }

        t.SetSiblingIndex(index);
    }

    protected List<string> planeKeysInPool = new List<string>();

    public Transform CreateMapPlane(string key)
    {
        if (planeKeysInPool.Contains(key))
        {
            planeKeysInPool.Remove(key);
        }

        Transform plane = GameController.Instance.PoolManager.PopFromInstancePool(key, true);

        return plane;
    }

    //丟進對像池
    public void RemovePlane(string key, Transform plane)
    {
        if (plane == null)
        {
            //有可能已經被unity銷毀
            return;
        }

        if (!planeKeysInPool.Contains(key))
        {
            planeKeysInPool.Add(key);
        }

        GameController.Instance.PoolManager.PushToInstancePool(key, plane);
    }

    //將所有在對像池中的地圖塊銷毀
    public void RemovePlanesInPool()
    {
        for (int i = 0; i < planeKeysInPool.Count; i++)
        {
            GameController.Instance.PoolManager.RemoveAtInstancePool(planeKeysInPool[i]);
        }

        planeKeysInPool.Clear();
    }
}

