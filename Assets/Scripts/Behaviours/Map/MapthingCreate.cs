using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Stage;
using System.Collections.Generic;
using UnityEngine;

public class MapthingCreate : MonoBehaviour
{
    public MapThingData[] things;
    protected Transform lvlRoot;

    bool Initialization = false;

    [HideInInspector]
    //public List<MapLvlButton> buttons = new List<MapLvlButton>();
    public Dictionary<int, MapLvlButton> buttons = new Dictionary<int, MapLvlButton>();

    public string LVL_NAME_PREFIX = "LVL";
    public string ACTIVELVL_NAME_PREFIX = "ACTIVELVL";

    protected void CreateButton()
    {
        for (int i = 0; i < lvlRoot.childCount; i++)
        {
            Transform c = lvlRoot.GetChild(i);
            CreateLvlButton(c);
        }
    }

 
    bool CreateLvlButton(Transform c)
    {
        string buttonName = c.name.ToUpper();
        int matchIndex = buttonName.IndexOf(LVL_NAME_PREFIX);
 
        if (matchIndex == -1)
        {
            return false;
        }

        MapLvlButton button;
        //抓取關卡數字--
        string lvlString = buttonName.Substring(matchIndex + LVL_NAME_PREFIX.Length);
        int lvl = int.Parse(lvlString);

        if (!GameController.Instance.Model.StageConfigs.ContainsKey(lvl))
        {
            return false;
        }

        StageConfig stageConfig = GameController.Instance.Model.StageConfigs[lvl];
 
        if (GameController.Instance.StageCtr.IsActivityStage(stageConfig))
        {
            //需要檢查是否屬於可以打的活動關卡，如果不是則不用創建
            if (GameController.Instance.StageCtr.GetStageData(stageConfig.ID) != null)
            {
                button = c.gameObject.AddComponent<MapActiveLvlButton>();
                button.SetData(stageConfig);
                button.Create();
                buttons.Add(lvl, button);
            }
        }
        else
        {
            button = c.gameObject.AddComponent<MapLvlButton>();
            button.SetData(stageConfig);
            button.Create();
            buttons.Add(lvl, button);

            //玩家已經獲得了這個類型的寵物
            if (stageConfig.UnitHeadId != -1 && !GameController.Instance.UnitCtr.HasTypeUnit(stageConfig.UnitHeadId))
            {
                MapButtonInfoBoard InfoBoard = button.gameObject.AddComponent<MapButtonInfoBoard>();
                InfoBoard.SetData(stageConfig);
            }

            // 添加星星       放在按鈕上方案
            //Stage stage = GameController.Instance.StageCtr.GetStageData(stageConfig.ID);

            //if (stage != null && button.state != MapThing.ThingState.STATE_LOCKED && stage.star != 0 && !button.isGemLocked)
            //{
            //    MapButtonStars stats = button.gameObject.AddComponent<MapButtonStars>();
            //    stats.SetData(stage);
            //}

            //添加星星
            if (GameController.Instance.PlayerCtr.PlayerData.passStageId + 1 >= stageConfig.ID)
            {
                MapStarInfoBoard starInfo = button.gameObject.AddComponent<MapStarInfoBoard>();
                if (GameController.Instance.StageCtr.StarChangeLevel != 0)
                {
                    if (GameController.Instance.StageCtr.StarChangeLevel == stageConfig.ID)
                    {
                        starInfo.SetData(stageConfig, true);
                    }
                    else
                    {
                        starInfo.SetData(stageConfig);
                    }
                }
                else
                {
                    starInfo.SetData(stageConfig);
                }
            }

        }
 
        return true;
    }

    /// <summary>
    /// 通過關卡號獲取舞台按鈕
    /// </summary>
    /// <param name="lvl"></param>
    /// <returns></returns>
    public MapLvlButton GetButton(int lvl)
    {
        if (buttons.ContainsKey(lvl))
        {
            return buttons[lvl];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 只執行一次
    /// </summary>
    public void Create()
    {
        if (Initialization)
        {
            return;
        }

        Initialization = true;

        foreach (MapThingData item in things)
        {
            if (item == null || item.body == null)
            {
                continue;
            }

#if EFFECT_HIDE
            if (item.type == ThingType.Effect)
            {
                continue;
            }
#endif

            //直接創建
            Transform thing = (Transform)Instantiate(item.body, Vector3.zero, Quaternion.identity);
            thing.parent = transform;
            thing.localPosition = Vector3.zero;
            thing.localRotation = Quaternion.identity;

            if (item.type == ThingType.Audio)
            {
                DelayPlayAudio(thing);
            }
            else if (item.type == ThingType.Plane)
            {
                lvlRoot = thing;
            }
        }

        CreateButton();
    }

    private void DelayPlayAudio(Transform audioThing)
    {
        AudioSource[] audios = audioThing.GetComponentsInChildren<AudioSource>();

        for (int i = 0; i < audios.Length; i++)
        {
            DelayAudioPlay delayPlay = audios[i].GetComponent<DelayAudioPlay>();

            if (delayPlay == null)
            {
                audios[i].gameObject.AddComponent<DelayAudioPlay>();
            }
        }
    }
}
