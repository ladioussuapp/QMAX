using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;

public class UISceneLoading : MonoBehaviour {
    //图片尺寸为  638x507 压缩成了512x512
    //public Image ContentImg;
    //public Text msgText;


    public void Start()
    {
        if (GameController.Instance.PlayerCtr.PlayerData == null)
        {
            //GameController.Instance.ModelEventSystem.OnPlayerInfoInit += ModelEventSystem_OnPlayerInfoInit;
        }
        else
        {
            Init();
        }
    }


    void ModelEventSystem_OnPlayerInfoInit(Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse obj)
    {
        Init();
    }

    void Init()
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoInit -= ModelEventSystem_OnPlayerInfoInit;

        //if (GameController.Instance.Model.GameSystemConfig != null)
        //{
        //    StageConfig stageConfig = GameController.Instance.StageCtr.GetNextStage();
        //    int randomTip = Random.Range(0, stageConfig.TipsIds.Length);
        //    LoadingConfig loadingConfig = GameController.Instance.Model.LoadingConfigs[int.Parse(stageConfig.TipsIds[randomTip])];
        //    ContentImg.sprite = GameController.Instance.QMaxAssetsFactory.CreateLoadingTipSprite(loadingConfig);
        //    msgText.text = Utils.GetTextByID(loadingConfig.MsgId);
        //}
    }

    public void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoInit -= ModelEventSystem_OnPlayerInfoInit;
    }

}
