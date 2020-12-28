using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax;

/// <summary>
/// 特效生成  每次激活时会生成一个对应的特效在自己位置上
/// </summary>
public class EffectSpawn : MonoBehaviour
{
    public Transform EffectPrefab;
    public int MaxCount = 1;
    //可以指定另外一个父级对象
    public Transform Parent;
    /// <summary>
    /// 是否使用对象池  不使用的话，则就是单纯的将特效加载进来
    /// </summary>
    public bool UseGOPool = false;       
    List<Transform> effects = new List<Transform>();
    bool SPAWN_FLAG = false;
    string spawnKey;

    [System.NonSerialized]
    [HideInInspector]
    public Transform LastEffectSpawned;

    public void Awake()
    {
        //使用自己的KEY
        if (Parent == null)
        {
            Parent = transform;
        }

        if (UseGOPool)
        {
            spawnKey = string.Concat(EffectPrefab.name, Time.time);
            GameController.Instance.PoolManager.PrePrefabSpawn(EffectPrefab, spawnKey);
        }
    }

    Transform Create()
    {
#if EFFECT_HIDE
        //TO DO
#endif

        if (EffectPrefab == null)
        {
            return null;
        }

        Transform t = UseGOPool ?  GameController.Instance.PoolManager.PrefabSpawn(spawnKey) : GameObject.Instantiate<Transform>(EffectPrefab);
        t.gameObject.SetActive(true);
        t.SetParent(Parent);
        //effectT.localScale = EffectPrefab.localScale;     //设置了缩放后发现特效的效果与直接拖进去的不一致
        t.position = transform.position;
        t.localRotation = Quaternion.identity;
        LastEffectSpawned = t;
 
        return t;
    }

    void Destroy(Transform t)
    {
        if (UseGOPool)
        {
            GameController.Instance.PoolManager.Despawn(t);
        }
        else
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    void  spawn()
    {
        SPAWN_FLAG = false;

        Transform effectT = Create();
        effects.Add(effectT);

        //数量超限
        if (effects.Count > MaxCount)
        {
            Destroy(effects[0]);
            effects.RemoveAt(0);
        }
    }
 
    public void Spawn()
    {
        if (!this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
            return;
        }

        SPAWN_FLAG = true;
    }

    public void OnEnable()
    {
        Spawn();
    }

    public void OnDestroy()
    {
        if (UseGOPool)
        {
            GameController.Instance.PoolManager.Despool(spawnKey);
        }

        LastEffectSpawned = null;
        effects.Clear();
    }

    public void Update()
    {
        if (SPAWN_FLAG)
        {
            spawn();
        }
    }

}
