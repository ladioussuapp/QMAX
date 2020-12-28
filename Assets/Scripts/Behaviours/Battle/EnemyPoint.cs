using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using System;
using UnityEngine;

/// <summary>
/// 戰斗場景中的怪點
/// </summary>
public class EnemyPoint : MonoBehaviour
{
    [HideInInspector]
    public Transform Enemy;

    /// <summary>
    /// 標識是第幾隻敵人
    /// </summary>
    [HideInInspector]
    public int Index = -1;

    public UnitConfig _config;
    /// <summary>
    /// 這個怪點的怪物類型。如果為null，表示這個怪點沒有怪。
    /// </summary>
    public UnitConfig Config
    {
        get { return _config; }
        set
        {
            if (_config == value)
                return;
            Clear();
            _config = value;
            BodyChange();
        }
    }

    /// <summary>
    /// 該怪點是否有怪
    /// </summary>
    public bool HasEnemy
    {
        get { return _config != null; }
    }


    void Awake()
    {
        if (transform.childCount > 0)
        {
            Enemy = transform.GetChild(0);
        }
    }

    /// <summary>
    /// 根據提供的技能類型，判斷該敵人是否有此類型的技能
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public SkillConfig GetSkillConfigByType(SkillType type)
    {
        SkillConfig skill = null;
        QmaxModel qmaxModel = GameController.Instance.Model;
        if (_config != null)
        {
            for (int ii = 0; ii < _config.UnitSkillIdArr.Count; ii++)
            {
                if (_config.UnitSkillIdArr[ii] != -1)
                {
                    SkillConfig skillCfg = qmaxModel.SkillConfigs[_config.UnitSkillIdArr[ii]];
                    if (skillCfg.SkillType == type)
                    {
                        skill = skillCfg;
                    }
                }
            }
        }
        else
        {
            skill = null;
        }

        return skill;
    }

    public Animator EnemyAnimator
    {
        get
        {
            return Enemy != null ? Enemy.GetComponent<Animator>() : null;
        }
    }


    public void Clear()
    {
        if (transform.childCount > 0)
        {
            if (Enemy != null)
            {
                //Enemy.GetComponent<Renderer>().enabled = false;
                Destroy(Enemy.gameObject, 0.5f);
                if (_config == null)
                {
                    transform.DetachChildren();
                }

            }
            _config = null;
        }
        Enemy = null;
    }


    public void ClearAfterDieAnim(bool isWeak = false)
    {
        if (transform.childCount > 0)
        {
            if (Enemy != null)
            {
                PlayAnimDieAndClear(Enemy.gameObject, isWeak);
                if (_config == null)
                {
                    transform.DetachChildren();
                }

            }
            _config = null;
        }
        Enemy = null;
    }


    private void PlayAnimDieAndClear(GameObject ga, bool isWeakly)
    {
        Animator anim = ga.GetComponent<Animator>();

        Action<Animator, AnimatorStateInfo, int> OnStateExitDelegate = null;
        OnStateExitDelegate = delegate(Animator enemyAnimator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!stateInfo.IsName("Die") && !stateInfo.IsName("Weak_Die"))
            {
                return;
            }
            enemyAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= OnStateExitDelegate;
            // 死亡動作播放完畢
            // 先隱藏敵人
            MeshRenderer renderer = ga.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;
            // 把SkeletonAnimator停了，太耗性能
            SkeletonAnimator skele = ga.GetComponent<SkeletonAnimator>();
            if (skele != null)
                skele.enabled = false;
            // 延遲1秒銷毀敵人
            StartCoroutine(Utils.DelayToInvokeDo(delegate() { Destroy(ga); }, 1.0f));
        };

        if (!anim.isActiveAndEnabled)
        {
            Q.Assert(false, "EnemyPoint:PlayAnimDieAndClear Assert 1");
            return;
        }

        anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnStateExitDelegate;
        anim.Play(isWeakly ? EnemyAnim.WEAK_DIE : EnemyAnim.DIE);

    }

    private void BodyChange()
    {
        Q.Assert(_config != null, "EnemyPoint:BodyChange Assert 1");
        if(_config ==null)
            return;

        GameController.Instance.PoolManager.GetUnitInstance(
            _config,
            delegate(string key, Transform ins)
            {
                if(ins == null)
                {
                    Q.Assert(false, "EnemyPoint:BodyChange Assert 2");
                    return;
                }
                

                Enemy = Instantiate<GameObject>(ins.gameObject).transform;
                //Transform prefab = GameController.Instance.PoolManager.GetPrefabInstance(data.Config.AssetBundleName);
                Enemy.SetParent(transform);

                Enemy.gameObject.layer = LayerMask.NameToLayer(Layer.Battle);
                //處理敵人會鏡像翻轉的問題
                Enemy.transform.localScale = ins.localScale;
                //Enemy.localPosition = prefab.localPosition;
                Enemy.localPosition = Vector3.zero;

                //除了第一隻敵人，默認都讓其不播放動畫
                //等到鏡頭移動到該位置時，才開始播放動畫
                if (Index == 0)
                    Enemy.GetComponent<Animator>().enabled = true;
                else
                    Enemy.GetComponent<Animator>().enabled = false;

                GameController.Instance.PoolManager.PushToInstancePool(key, ins);
            }
        );
    }
}
