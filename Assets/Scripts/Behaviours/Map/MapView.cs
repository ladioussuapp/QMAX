using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Unit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{
    //模型数量，从配置中取
    int planeModelCount = 0;
    int startModelIndex = 0;
    int planeLayer;
    private const string SKY_TEXTURE_ROOT = "Textures/MapView/";

    //组成场景的所有planes  每个plane下有一个地图块的模型
    public List<Plane> planes;

    public HeroRun heroRun;
    public MapSkyBehaviour sky;
    //地图块地面到圆心的半径
    public float Radius;

    List<StageModelSettingConfig> modelConfigs;

    //当前打到关卡所在的地图块索引   用来做拖动限制，不允许拖动到未开启关卡以外的区域太多
    int cutStageModelIndex = 0;

    //当前地面
    Plane cutPlane;
    //上方地面
    Plane NextPlane;
    //下方底面
    Plane PrePlane;
    //背面
    Plane BackPlane;

    Collider inCollider;
    Collider currentCollider;
    //Collider nextCollider;
    //Collider preCollider;

    public MapMove mapMove;
    public Camera mapCamera;

    //上方plane检测的方向物体
    public Transform RayUpDir;

    Ray rayCenter;
    Ray rayBg;
    Ray rayUp;
    Ray rayDown;

    /// <summary>
    /// 超过此dots的值则表示再摄像机范围之外
    /// </summary>
    public float visibleDots;
    public Vector3 visibleDownDir;

    public bool IsBusy
    {
        get
        {
            return isBusy;
        }
    }

    bool isBusy = false;

    public void Awake()
    {
        Instance = this;
    }

    public static MapView Instance;

    /// <summary>
    /// 计算当前相关的关卡数据信息
    /// </summary>
    /// <param name="lastPassedLvl">上一次战斗所通过的关卡  -1表示上一次没有通一个新关卡</param>
    /// <param name="cutLvl">当前需要打的关卡  </param>
    void GenLastPassStageLvls(out int lastPassedLvl, out int cutLvl)
    {
        lastPassedLvl = cutLvl = -1;
        if (GameController.Instance.Model.IsStagePassedInLastFight)
        {
            lastPassedLvl = GameController.Instance.Model.PrePassStageId + 1;
            cutLvl = GameController.Instance.PlayerCtr.PlayerData.passStageId + 1;
        }
        else
        {
            cutLvl = GameController.Instance.PlayerCtr.PlayerData.passStageId + 1;
        }

        if (cutLvl > GameController.Instance.Model.LastStageId)
        {
            //已经通关   
            cutLvl = GameController.Instance.Model.LastStageId;
        }
    }

    /// <summary>
    /// 初始化地图块，如果上回合通关了一个新关卡，会定位到此关卡上。否则定位到当前要打的关卡上。 
    /// 如果定位到了上回合通过的关卡上，则需要由外部或者回调去执行解开下一关的动作，并滚动到下一关。
    /// </summary>
    void Init()
    {
        int lastPassedLvl;
        int cutLvl;
        GenLastPassStageLvls(out lastPassedLvl, out cutLvl);
        StageModelSettingConfig cutModelConfig;

        if (lastPassedLvl > 0)
        {
            cutModelConfig = GetStageModelConfig(lastPassedLvl);
        }
        else
        {
            cutModelConfig = GetStageModelConfig(cutLvl);
        }

        planeModelCount = GameController.Instance.Model.StageModelSettingConfigs.Count;
        startModelIndex = cutModelConfig.MapId - 1;     //配置从1开始
        cutStageModelIndex = startModelIndex;       //当前显示的地图块索引

        //根据最高通关关卡数，设置起始的模型索引
        for (int i = 0; i < planes.Count; i++)
        {
            Plane plane = planes[i];
            plane.planeIndex = i;
            plane.MapCamera = mapCamera;

            int modelIndex = startModelIndex + i;

            if (modelIndex >= planeModelCount)
            {
                modelIndex = 0;
            }

            plane.ModelIndex = modelIndex;
        }

        CheckVisiblePlane();        //找按钮之前刷新下地图块
        MapLvlButton defaultLvlButton;
        MapLvlButton LockLvlButton;

        if (lastPassedLvl > 0)
        {
            //需要滚动到下一关的按钮上
            mapMove.TouchAble = false;      //滚动完成后解锁滚动触碰(MoveToNextLvlButton)
            defaultLvlButton = GetLvlButton(lastPassedLvl, out cutStageModelIndex);
            //  defaultLvlButton  此按钮是上一次通关的关卡按钮，需要滚动到此按钮，  LockLvlButton其实是需要打的关卡按钮，需要手动先锁住
            LockLvlButton = GetLvlButton(cutLvl, out cutStageModelIndex);
            LockLvlButton.SetLock(true);
        }
        else
        {
            defaultLvlButton = GetLvlButton(cutLvl, out cutStageModelIndex);
            defaultLvlButton.SetSelect(true);
        }

        //Debug.Log("默认按钮：" + defaultLvlButton.Stage.ID);
        StartCoroutine(mapMove.LookButton(defaultLvlButton.transform, false));
        CheckVisiblePlane();        //手动刷新地图块
    }

    public void MoveToLvlButton(int lvl)
    {
        if (isBusy)
        {
            return;
        }

        //GetStageModelConfig(lvl)
        MapLvlButton btn = GetLvlButton(lvl);

        if (btn != null)
        {
            StartCoroutine(MoveToLvlButton(btn));
        }
        else
        {
            StartCoroutine(MoveToOtherModelLvlButton(lvl));
        }
    }

    IEnumerator MoveToLvlButton(MapLvlButton btn)
    {
        isBusy = true;
        yield return StartCoroutine(mapMove.LookButton(btn.transform, true));
        isBusy = false;
        //Debug.Log("当前按钮所在 modelIndex:" + cutPlane.ModelIndex);
    }

    IEnumerator MoveToOtherModelLvlButton(int lvl)
    {
        isBusy = true;

        StageModelSettingConfig modelConfig = GetStageModelConfig(lvl);
        Q.Assert(modelConfig != null, "MapView::MoveToOtherModelLvlButton({0})", lvl);
        int targetModelIndex = modelConfig.MapId - 1;
        //int modelDis = cutPlane.ModelIndex - targetModelIndex;
        float speed = 300f;

        if (cutPlane.ModelIndex < targetModelIndex)
        {
            speed = -300f;

            while (NextPlane.ModelIndex != targetModelIndex && !mapMove.TouchDown)
            {
                mapMove.AutoMove(speed * Time.deltaTime);
                yield return 0;
            }
        }
        else
        {
            speed = 300;

            while (PrePlane.ModelIndex != targetModelIndex && !mapMove.TouchDown)
            {
                mapMove.AutoMove(speed * Time.deltaTime);
                yield return 0;
            }
        }

        //Debug.Log("targetModelIndex:" + targetModelIndex + "  |  cutModelIndex:" + cutPlane.ModelIndex);

        MapLvlButton btn = GetLvlButton(lvl);

        if (btn != null)
        {
            yield return StartCoroutine(mapMove.LookButton(btn.transform, true));
        }

        //Debug.Log("targetModelIndex:" + targetModelIndex + "  |  cutModelIndex:" + cutPlane.ModelIndex);
        isBusy = false;
    }

    public void MoveToNextLvlButton()
    {
        StartCoroutine(BeginMoveToNextLvlButton());
    }

    IEnumerator BeginMoveToNextLvlButton()
    {
        isBusy = true;

        int lastPassedLvl;
        int cutLvl;

        GenLastPassStageLvls(out lastPassedLvl, out cutLvl);
        MapLvlButton nextButton = GetLvlButton(cutLvl, out cutStageModelIndex);

        yield return StartCoroutine(mapMove.LookButton(nextButton.transform, true));
        //button.SetLock(false);
        nextButton.PlayUnLockEffect();
        yield return 0;
        nextButton.SetSelect(true);

        StageConfig activeStage = GameController.Instance.StageCtr.GetActiveStage(lastPassedLvl);

        if (activeStage != null)
        {
            //有活动关卡同时被开启
            MapLvlButton nextActiveButton = GetLvlButton(activeStage.ID, out cutStageModelIndex);
            nextActiveButton.PlayUnLockEffect();
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
            MapUiBehaviour uiBehaviour = UnityEngine.Object.FindObjectOfType<MapUiBehaviour>();
            uiBehaviour.OpenLvl(cutLvl);            //有了活动关卡之后  是否还需要打开此关
        }

        mapMove.TouchAble = true;

        if (GameController.Instance.ViewEventSystem.OnMoveToNextLvlComplete != null)
        {
            GameController.Instance.ViewEventSystem.OnMoveToNextLvlComplete();
        }

        isBusy = false;
    }


    protected StageModelSettingConfig GetStageModelConfig(int lvl)
    {
        //passStageId从0开始  配置从1开始
        string lvl_ = lvl.ToString();

        for (int i = 0; i < modelConfigs.Count; i++)
        {
            StageModelSettingConfig config = modelConfigs[i];

            if (Array.IndexOf(config.levelRange, lvl_) > -1)
            {
                return config;
            }
        }

        return null;
    }

    void ModelEventSystem_BeforeSceneChangeEvent(Scenes arg1, Scenes arg2)
    {
        //跳转场景   所有模型返回对象池
        foreach (Plane plane in planes)
        {
            plane.DestoryAll();
        }
    }

    // Use this for initialization
    void Start()
    {
        GameController.Instance.ModelEventSystem.BeforeSceneChangeEvent += ModelEventSystem_BeforeSceneChangeEvent;
        GameController.Instance.ModelEventSystem.OnStageUnlocked += ModelEventSystem_OnStageUnlocked;

        sky.CutTextureName = GetSkyTextureName(0);
        modelConfigs = GameController.Instance.Model.StageModelSettingConfigs;
        planeLayer = 1 << LayerMask.NameToLayer("Plane");
        InitCameraInfo();
        mapMove = GetComponent<MapMove>();
        mapMove.Camera = mapCamera;
        mapMove.onMapMove += mapMove_onMapMove;

        Init();
    }


    public void OnDestroy()
    {
        this.StopAllCoroutines();
        mapMove.onMapMove -= mapMove_onMapMove;
        GameController.Instance.ModelEventSystem.BeforeSceneChangeEvent -= ModelEventSystem_BeforeSceneChangeEvent;
        GameController.Instance.ModelEventSystem.OnStageUnlocked -= ModelEventSystem_OnStageUnlocked;
    }

    void ModelEventSystem_OnStageUnlocked(int lvl)
    {
        //解锁了一个特殊关卡。
        MapLvlButton button = GetLvlButton(lvl);

        if (button != null)
        {
            button.UpdateSkinNow();
        }
    }

    void InitCameraInfo()
    {
        Vector3 cameraPosition = mapCamera.transform.position;
        Vector3 cameraForward = mapCamera.transform.forward;
        rayCenter = new Ray(cameraPosition, cameraForward);

        //纵向角度  增加3度缓冲
        float vFov = mapCamera.fieldOfView * .5f + 3f;
        Vector3 downOrigin = cameraPosition;
        visibleDownDir = Quaternion.Euler(Vector3.right * vFov) * cameraForward;
        visibleDots = Vector3.Dot(visibleDownDir, cameraForward);

        rayDown = new Ray(downOrigin, visibleDownDir);
        rayBg = new Ray(cameraPosition, Quaternion.Euler(Vector3.right * -4) * cameraForward);
        rayUp = new Ray(RayUpDir.position, RayUpDir.forward);
    }

    //地图旋转的时候，动态加载不同的地图模型
    void mapMove_onMapMove(float deltaY)
    {
        CheckVisiblePlane();
    }

    public MapLvlButton GetLvlButton(int lvl)
    {
        MapLvlButton button = cutPlane.FindLvlButton(lvl);

        if (button == null)
        {
            button = NextPlane.FindLvlButton(lvl);
        }
        if (button == null)
        {
            button = PrePlane.FindLvlButton(lvl);
        }

        return button;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lvl"></param>
    /// <param name="targetModelIndex">当前找到按钮的地图块</param>
    /// <returns></returns>
    public MapLvlButton GetLvlButton(int lvl, out int targetModelIndex)
    {
        MapLvlButton button = cutPlane.FindLvlButton(lvl);
        targetModelIndex = cutPlane.ModelIndex;

        if (button == null)
        {
            button = NextPlane.FindLvlButton(lvl);
            targetModelIndex = NextPlane.ModelIndex;
        }
        if (button == null)
        {
            button = PrePlane.FindLvlButton(lvl);
            targetModelIndex = PrePlane.ModelIndex;
        }

        Q.Assert(button != null, string.Concat("地图按钮未找到   lvl:", lvl));

        return button;
    }


    RaycastHit hitInfo;

    protected void CheckVisiblePlane()
    {
        //当地图拖动时，每帧触发4条射线检测，如果改成间隔帧触发，则会在检测上下地图块可见性的时候，有延迟 TODO 性能优化
        if (!mapMove.MoveLimit && Physics.Raycast(rayCenter, out hitInfo, 30f, planeLayer))
        {
            if (hitInfo.collider.tag == Tags.Plane.ToString())
            {
                Debug.DrawLine(rayCenter.origin, hitInfo.point, Color.green, .3f);

                if (currentCollider != hitInfo.collider)
                {
                    currentCollider = hitInfo.collider;
                    ChangeCutPlane();
                }
            }
        }

        //检测上方边缘的射线
        if (Physics.Raycast(rayUp, out hitInfo, 30f, planeLayer))
        {
            if (hitInfo.collider.CompareTag(Tags.Plane.ToString()))
            {
                Debug.DrawLine(rayUp.origin, hitInfo.point, Color.red, .3f);

                //上方地面有可能不可见，此时同时看到中间和下方的地面
                NextPlaneVisibleChange(currentCollider != hitInfo.collider);
            }
        }

        if (Physics.Raycast(rayDown, out hitInfo, 30f, planeLayer))
        {
            if (hitInfo.collider.CompareTag(Tags.Plane.ToString()))
            {
                Debug.DrawLine(rayDown.origin, hitInfo.point, Color.red, .3f);

                //下方地面有可能不可见，此时同时看到中间和上方的地面
                PrePlaneVisibleChange(currentCollider != hitInfo.collider);
            }
        }

        //检测替换背景的射线
        if (!mapMove.MoveLimit && Physics.Raycast(rayBg, out hitInfo, 30f, planeLayer))
        {
            if (hitInfo.collider.CompareTag(Tags.Plane.ToString()))
            {
                Debug.DrawLine(rayBg.origin, hitInfo.point, Color.blue, .3f);
                if (inCollider != hitInfo.collider)
                {
                    inCollider = hitInfo.collider;
                    ChangeCutPlaneBg();
                }
            }
        }

        //Debug.DrawRay(cameraPosition, cameraForward * 50f, Color.green, 30f, true);
        //Debug.DrawRay(rayUp.origin, rayUp.direction * 50f, Color.red, 30f, true);
        //Debug.DrawRay(rayDown.origin, rayDown.direction, Color.grey, 30f, true);

        //Debug.Log("CheckVisiblePlane");
    }

    protected void ChangeCutPlane()
    {
        Plane plane = currentCollider.GetComponent<Plane>();

        if (cutPlane != plane)
        {
            cutPlane = plane;

            //待更改   上方地图可见的时候，隐藏下方地图。 下方地图可见时隐藏上方地图 TO DO
            cutPlane.SetVisible(true);
            NextPlane = GetNextPlane();
            PrePlane = GetPrePlane();
            BackPlane = GetBackPlane();

            BackPlane.SetVisible(false);
            NextPlane.SetVisible(true);
            PrePlane.SetVisible(true);

            NextPlane.ModelIndex = cutPlane.ModelIndex + 1;
            PrePlane.ModelIndex = cutPlane.ModelIndex - 1;
            //暂时在切换当前块的时候更新上下两个块
            //动态加载不同地图关内容暂屏蔽
        }
    }

    public void OnEnable()
    {
        this.isBusy = false;
    }

    Plane GetBackPlane()
    {
        int planeIndex = NextPlane.planeIndex;
        planeIndex += 1;

        if (planeIndex >= planes.Count)
        {
            planeIndex = planes.Count - planeIndex;
        }

        return planes[planeIndex];
    }

    Plane GetNextPlane()
    {
        int planeIndex = cutPlane.planeIndex;
        int upIndex = planeIndex == planes.Count - 1 ? 0 : planeIndex + 1;
        return planes[upIndex];
    }

    Plane GetPrePlane()
    {
        int planeIndex = cutPlane.planeIndex;
        int downIndex = planeIndex == 0 ? planes.Count - 1 : planeIndex - 1;
        return planes[downIndex];
    }

    private string GetSkyTextureName(int modelIndex)
    {
        if (modelIndex < 0)
        {
            modelIndex = 0;
        }
        else if (planeModelCount > 0 && modelIndex > planeModelCount - 1)
        {
            modelIndex = planeModelCount - 1;
        }

        StageModelSettingConfig modelConfig = GameController.Instance.Model.StageModelSettingConfigs[modelIndex];

        if (modelConfig.SkyTexture != "")
        {
            return SKY_TEXTURE_ROOT + modelConfig.SkyTexture;
        }
        else
        {
            return "";
        }
    }

    private void ChangeCutPlaneBg()
    {
        Plane plane = inCollider.GetComponent<Plane>();
        string tName = GetSkyTextureName(plane.ModelIndex);

        if (tName != "")
        {
            sky.ChangeTexture(tName);
        }
    }

    protected void NextPlaneVisibleChange(bool visible)
    {
        //当上方的plane是最后一个plane时，限制拖动
        //当上方的plane是当前通关关卡的下一个关卡时，限制拖动

        //Debug.Log(cutStageModelIndex + " | " + cutPlane.ModelIndex);
        //限制两个球
        if (visible)
        {
            if (cutPlane.ModelIndex == planeModelCount - 1 || cutStageModelIndex < cutPlane.ModelIndex - 1)
            {
                //锁定不让继续往下拖
                mapMove.MoveDownLimit = true;
            }
        }
        else
        {
            mapMove.MoveDownLimit = false;
        }
    }

    protected void PrePlaneVisibleChange(bool visible)
    {
        //NextPlane.SetVisible(!visible);

        if (visible)
        {
            if (cutPlane.ModelIndex == 0)
            {
                mapMove.MoveUpLimit = true;
            }
        }
        else
        {
            mapMove.MoveUpLimit = false;
        }
    }
}