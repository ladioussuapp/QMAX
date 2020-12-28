using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;
using System.Text;

//地面
public class Plane : MonoBehaviour
{
    [HideInInspector]
    public Transform planePrefab;
    [HideInInspector]
    public Camera MapCamera;
    [HideInInspector]
    public int planeIndex = -1;
    [HideInInspector]
    public Transform colliderPosition;
    [HideInInspector]
    public MapthingCreate thingCreate;
    bool visible = false;
    int modelIndex = int.MinValue;
    const string MODEL_FIRST = "ModelFirst";
    const string MODEL_END = "ModelEnd";
    string cutModelName;
    string lastModelName;

    //ModelSettingConfigs的索引  ModelSettingConfigs的mapId从1开始
    public int ModelIndex
    {
        get
        {
            return modelIndex;
        }
        set
        {
            if (value != modelIndex)
            {
                modelIndex = value;

                List<StageModelSettingConfig> ModelSettingConfigs = GameController.Instance.Model.StageModelSettingConfigs;

                if (modelIndex < 0 )
                {
                    cutModelName = MODEL_FIRST;
                }
                else if (modelIndex > ModelSettingConfigs.Count - 1)
                {
                    cutModelName = MODEL_END;
                }
                else
                {
                    StageModelSettingConfig modelConfg = ModelSettingConfigs[modelIndex];
                    cutModelName = modelConfg.MapModel;
                }

                ChangeSkin(cutModelName, lastModelName);
                lastModelName = cutModelName;
            }
        }
    }
 
    protected void ChangeSkin(string cutPrefabName , string lastPrefabName)
    {
        if (planePrefab)
        {
            RemovePlanePrefab(lastPrefabName);
        }

        planePrefab = CreatePlanePrefab(cutPrefabName);
        thingCreate = planePrefab.GetComponent<MapthingCreate>();
        thingCreate.Create();
        planePrefab.parent = this.transform;
        planePrefab.localRotation = Quaternion.identity;
        planePrefab.localScale = Vector3.one;
        planePrefab.localPosition = Vector3.zero;
        planePrefab.gameObject.SetActive(visible);
    }
 
    //创建
    protected Transform CreatePlanePrefab(string prefabName)
    {
        string key = string.Concat("Prefabs/Map/" ,prefabName);

        return MapThingManager.Instance.CreateMapPlane(key);
    }

    //旋转过程中的移除地图块，返回单例对象池中
    protected void RemovePlanePrefab(string prefabName)
    {
        string key = string.Concat("Prefabs/Map/", prefabName);
        MapThingManager.Instance.RemovePlane(key, planePrefab);
    }
 

    public void SetVisible(bool val)
    {
        if (visible != val)
        {
            visible = val;

            if (planePrefab)
            {
                planePrefab.gameObject.SetActive(val);
            }
        }
    }
 
    public void DestoryAll()
    {
        //切换场景  所有地图块都销毁
        MapThingManager.Instance.RemovePlanesInPool();
    }

    public MapLvlButton FindLvlButton(int lvl)
    {
        MapthingCreate thingCreate = planePrefab.GetComponent<MapthingCreate>();
        MapLvlButton button = thingCreate.GetButton(lvl);
 
        return button;
    }

    public override string ToString()
    {
        return string.Concat("plane:", name, " lvl:", ModelIndex);
    }

}
