/********************************************************************************
** auth： johnsonybq
** date： 2015/7/9 星期四 16:05:54
** FileName：BattleTools
** desc： 戰鬥內輔助工具
** Ver.:  V1.0.0
*********************************************************************************/

using Com4Love.Qmax;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleTools
{

    public static int IDLERATE = 30;
    /// <summary>
    /// 創建飛出來的獎勵物品
    /// </summary>
    /// <param name="icon">獎勵圖標</param>
    /// <param name="Layer">在哪一層上飛</param>
    /// <param name="anchorMax"></param>
    /// <param name="anchorMin"></param>
    /// <param name="anchoredPosition3D"></param>
    /// <param name="randomX">目標X軸</param>
    /// <param name="awardNum">獎勵數量</param>
    /// <param name="scaleInit">初始縮放值</param>
    /// <param name="scaleRate">目標縮放值</param>
    public static void CreateFlyAward(Sprite icon, RectTransform Layer, Vector2 anchorMax, Vector2 anchorMin,
        Vector3 anchoredPosition3D,
        float randomX, int awardNum,
        float scaleInit = 1.0f, int scaleRate = 500)
    {
        GameObject awardPrefab;
        awardPrefab = Resources.Load<GameObject>("Prefabs/RewardFly");
        RectTransform target = GameObject.Instantiate(awardPrefab).transform as RectTransform;

        target.SetParent(Layer);
        target.localScale = new Vector3(1, 1, 1);
        //設置為跟參考的Tile位置相同
        if (anchorMax.x != -1)
        {
            target.anchorMax = anchorMax;
            target.anchorMin = anchorMin;
            target.anchoredPosition3D = anchoredPosition3D;
        }
        else
        {
            target.localPosition = new Vector3(0, 0, 0);
        }

        RewardFlyBehaviour reward1 = target.GetComponent<RewardFlyBehaviour>();
        reward1.SetData(icon, awardNum);

        //動畫
        target.gameObject.SetActive(true);
        Vector3 targetPos = new Vector3(randomX, target.localPosition.y + 150, 0.0f);
        float initY = target.localPosition.y;
        LeanTween.moveLocal(target.gameObject, targetPos, 1.0f).setEase(LeanTweenType.linear).setOnUpdate(delegate(Vector3 val)
        {
            float scale = scaleInit + (val.y - initY) / scaleRate;
            target.localScale = new Vector3(scale, scale, scale);
        }).setOnComplete(delegate()
        {
            GameObject.Destroy(target.gameObject);
        });
    }

    /// <summary>
    /// 動畫需求，將棋盤部分的獎勵動畫形式改掉，保留怪物身上出現獎勵的動畫形式
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="Layer"></param>
    /// <param name="anchorMax"></param>
    /// <param name="anchorMin"></param>
    /// <param name="anchoredPosition3D"></param>
    /// <param name="randomX"></param>
    /// <param name="awardNum"></param>
    /// <param name="scaleInit"></param>
    /// <param name="scaleRate"></param>
    public static void CreateBoardFlyAward(Sprite icon, RectTransform Layer, Vector2 anchorMax, Vector2 anchorMin,
     Vector3 anchoredPosition3D,
     float randomX, int awardNum,
     float scaleInit = 1.0f, int scaleRate = 500)
    {
        GameObject awardPrefab;
        awardPrefab = Resources.Load<GameObject>("Prefabs/RewardFlyType2");
        RectTransform target = GameObject.Instantiate(awardPrefab).transform as RectTransform;

        target.SetParent(Layer);
        target.localScale = new Vector3(1, 1, 1);
        //設置為跟參考的Tile位置相同
        if (anchorMax.x != -1)
        {
            target.anchorMax = anchorMax;
            target.anchorMin = anchorMin;
            target.anchoredPosition3D = anchoredPosition3D;
        }
        else
        {
            target.localPosition = new Vector3(0, 0, 0);
        }

        RewardFlyBehaviour reward1 = target.GetComponent<RewardFlyBehaviour>();
        reward1.SetData(icon, awardNum);

        //動畫
        target.gameObject.SetActive(true);
        //awardPrefab 中有美術那邊的動畫，幀尾發事件出來在RewardFlyBehaviour中被刪除

    }



    /// <summary>
    /// 通過ID獲得獎勵圖標
    /// 獎勵 1鑰 2 黃毛球 3 藍毛球 4 鑽石 (id為PlayerValueType中的值)
    /// </summary>
    /// <param name="id">獎勵物品的ID</param>
    /// <returns></returns>
    public static Sprite GetBattleAwardIconByID(RewardType type)
    {
        Sprite awardIcon = null;
        switch (type)
        {
            case RewardType.Key:
                awardIcon = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "AddKey");
                break;
            case RewardType.UpgradeA:
                awardIcon = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "AddUpgradeA");
                break;
            case RewardType.UpgradeB:
                awardIcon = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "AddUpgradeB");
                break;
            case RewardType.Gem:
                awardIcon = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "AddGem");
                break;
            case RewardType.Coin:
                awardIcon = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "AddCoin");
                break;
        }
        return awardIcon;
    }

    /// <summary>
    /// 播放戰鬥獎勵音效
    /// </summary>
    /// <param name="type"></param>
    public static void PlayBattleAwarAudioByID(RewardType type)
    {
        switch (type)
        {
            case RewardType.Key:
                break;
            case RewardType.UpgradeA:
                break;
            case RewardType.UpgradeB:
                break;
            case RewardType.Gem:
                break;
            case RewardType.Coin:
                GameController.Instance.AudioManager.PlayAudio("SD_remove_coin");
                break;
        }
    }

    public static string GetBattleAwardSoundByID(RewardType type)
    {
        string soundUrl = "SD_remove_fruit";
        switch (type)
        {
            case RewardType.Key:
            case RewardType.UpgradeA:
            case RewardType.UpgradeB:
                soundUrl = "SD_remove_fruit";
                break;
            case RewardType.Gem:
            case RewardType.Coin:
                soundUrl = "SD_remove_diamond";
                break;
        }
        return soundUrl;
    }

    public static List<Com4Love.Qmax.Data.Config.StageConfig.Goal> ParseGoal(string value)
    {
        List<Com4Love.Qmax.Data.Config.StageConfig.Goal> ret = new List<Com4Love.Qmax.Data.Config.StageConfig.Goal>();
        //1,0,6|2,222,3|2,227,1
        string[] arr = value.Split('|');
        for (int i = 0, n = arr.Length; i < n; i++)
        {
            if (arr[i] == "")
            {
                continue;
            }

            string[] arr2 = arr[i].Split(',');
            Com4Love.Qmax.Data.Config.StageConfig.Goal g = new Com4Love.Qmax.Data.Config.StageConfig.Goal();

            g.Type = (BattleGoal)Convert.ToInt32(arr2[0]);
            g.RelativeID = Convert.ToInt32(arr2[1]);
            g.Num = Convert.ToInt32(arr2[2]);
            ret.Add(g);
        }
        return ret;
    }
}
