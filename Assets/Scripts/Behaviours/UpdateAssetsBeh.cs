using Com4Love.Qmax;
using Com4Love.Qmax.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAssetsBeh : MonoBehaviour
{
    public RectTransform ProgressBar;
    public Text TextField;

    private AssetBundleManager assetBundleMrg;

    void Start()
    {
        assetBundleMrg = GameController.Instance.AssetBundleMrg;
        StartLoad();
    }


    private void StartLoad()
    {
        //ProgressBar.value = 0;
        Vector2 size = ProgressBar.sizeDelta;
        Vector2 oldSize = size;
        float width = size.x;
        ProgressBar.sizeDelta = new Vector2(0, oldSize.y);

        int total = 0;
        TextField.text = "檢測更新資源";

        Action<int, AssetBundleManager.Code, string, AssetBundle, float> loadOneCallback = null;
        loadOneCallback = delegate(int index, AssetBundleManager.Code code, string abName, AssetBundle assetBundle, float assetSize)
        {
            //ProgressBar.value = (float)index / total;
            size.x = width * (float)index / total;
            ProgressBar.sizeDelta = size;
            TextField.text = string.Format("正在更新素材...({0}/{1})", index, total);
            if (code != AssetBundleManager.Code.Success)
            {
                Q.Log("Load {0} Fail", abName);
                return;
            }

            Q.Log("Load {0} complete", abName);
        };

        Action<List<string>, List<AssetBundleManager.Code>> allComplete = null;
        allComplete = delegate(List<string> arg0, List<AssetBundleManager.Code> arg1)
        {
            Q.Log("全部加載完成");
            TextField.text = "全部加載完成";
            assetBundleMrg.UpdateProgressEvent -= loadOneCallback;
            assetBundleMrg.UpdateCompleteEvent -= allComplete;
            AllDone();
        };

        assetBundleMrg.CheckAssetStatus(
            delegate(AssetBundleManager.Code code)
            {
                Q.Log("check complete {0}", code);

                if (assetBundleMrg.NeedUpdateAssetCount == 0)
                {
                    TextField.text = "檢測更新資源";
                    ProgressBar.sizeDelta = oldSize;
                    Invoke("AllDone", 0.5f);
                    return;
                }

                total = assetBundleMrg.NeedUpdateAssetCount;
                assetBundleMrg.UpdateProgressEvent += loadOneCallback;
                assetBundleMrg.UpdateCompleteEvent += allComplete;

                Q.Log("更新資源數量{0}", assetBundleMrg.NeedUpdateAssetCount);
                assetBundleMrg.UpdateAll();
            }
        );
    }


    private void AllDone()
    {
        GameController.Instance.Model.LoadConfigs(delegate(bool result)
        {
            if (!result)
            {
                //TODO 

                //GameController.Instance.Popup.Open()
                return;
            }
            GameController.Instance.SceneCtr.LoadLevel(Scenes.LoginScene);
        });
    }
}
