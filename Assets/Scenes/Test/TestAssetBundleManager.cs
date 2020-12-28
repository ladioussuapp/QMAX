using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Com4Love.Qmax.Data;
using System.IO;

public class TestAssetBundleManager : MonoBehaviour
{
    public Button Btn0;
    public Button Btn1;
    public Button Btn2;
    public Button Btn3;
    public Slider ProgressBar;
    public Text TextField;

    public Transform AssetLayer;


    private AssetBundleManager assetBundleMrg;

    //public TestAssetBundle2 SubScript;

    void Start()
    {
        //正式外网
        //const string ASSET_BUNDLE_URL = "http://app1104772395.imgcache.qzoneapp.com/app1104772395";
        //测试外网
        //const string ASSET_BUNDLE_URL = "http://app1104772395.imgcache.qzoneapp.com/app1104772395";
        //本地
        //file://D:/Work/Qmax/Code_Client/Qmax/Assets\../../assetbundles/assetbundles_android/assetbundles_android/reslist.json

        assetBundleMrg = new AssetBundleManager(PackageConfig.DEV_CDN_URL + "/assetbundles", Application.platform, this);
        //assetBundleMrg = new AssetBundleManager(PackageConfig.DEV_CDN_URL + "/assetbundles", RuntimePlatform.Android, this);

        string[] a = {
            "加载全部AssetBundle",
            "加载Unit",  
            "加载UnitImg",
            "测试一个Bug",
            //"加载Bgm"
        };
        Button[] b = { Btn0, Btn1, Btn2, Btn3 };
        for (int i = 0, n = a.Length; i < n; i++)
        {
            b[i].transform.Find("Text").GetComponent<Text>().text = a[i];
        }

        Btn0.onClick.AddListener(CheckAndUpdateAll);
        Btn1.onClick.AddListener(LoadUnit);
        Btn2.onClick.AddListener(LoadUnitImg);
        Btn3.onClick.AddListener(delegate() { StartCoroutine(TestKeng()); });

        //Caching.CleanCache();
    }


    private void CheckAndUpdateAll()
    {
        ProgressBar.value = 0;
        TextField.text = string.Format("正在更新小伙伴素材...");

        int total = 0;
        Action<int, AssetBundleManager.Code, string, AssetBundle, float> loadOneCallback =
            delegate(int index, AssetBundleManager.Code code, string abName, AssetBundle assetBundle, float assetSize)
            {
                ProgressBar.value = (float)index / total;
                if (code != AssetBundleManager.Code.Success)
                {
                    Q.Log("Load {0} Fail", abName);
                    return;
                }
                Q.Log("Load {0} complete", abName);
            };

        Action<List<string>, List<AssetBundleManager.Code>> allComplete =
            delegate(List<string> arg0, List<AssetBundleManager.Code> arg1)
            {
                Q.Log("全部加载完成");
                TextField.text = "全部加载完成";
            };

        assetBundleMrg.CheckAssetStatus(
            delegate(AssetBundleManager.Code code)
            {
                Q.Log("check complete {0}", code);
                Q.Assert(assetBundleMrg.AssetCount > 0);

                if (assetBundleMrg.NeedUpdateAssetCount == 0)
                {
                    TextField.text = "没有需要更新的资源";
                    return;
                }

                total = assetBundleMrg.NeedUpdateAssetCount;
                Q.Log("{0}个资源需要更新", assetBundleMrg.NeedUpdateAssetCount);

                assetBundleMrg.UpdateProgressEvent += loadOneCallback;
                assetBundleMrg.UpdateCompleteEvent += allComplete;
                assetBundleMrg.UpdateAll();
            }
        );
    }


    private void LoadUnit()
    {
        string resourceName = "Fox1";
        AssetBundleManager.LoadCallback loadOneCallback =
            delegate(AssetBundleManager.Code code, string abName, AssetBundle assetBundle)
            {
                if (code != AssetBundleManager.Code.Success)
                {
                    Q.Log("Load {0} Fail", abName);
                    return;
                }

                string[] allAbNames = assetBundle.GetAllAssetNames();
                for (int i = 0, n = allAbNames.Length; i < n; i++)
                {
                    Q.Log("allAbNames[{0}]={1}", i, allAbNames[i]);
                }

                string assetPath = string.Format("Assets/ExternalRes/Unit/{0}/{1}.prefab", resourceName, resourceName);
                GameObject prefab = assetBundle.LoadAsset<GameObject>(assetPath);
                GameObject ins = Instantiate<GameObject>(prefab);
                Q.Assert(prefab.GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>() == null);
                Q.Assert(ins.GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>() != null);
                ins.transform.SetParent(AssetLayer);
                ins.transform.localPosition = Vector3.zero;
                ins.transform.localScale = new Vector3(1, 1, 1);

                Q.Log("Load {0} complete", abName);
                //Q.Assert(assetBundleMrg.AssetVerDict.Count - assetBundleMrg.NeedUpdateAsset.Count == 1);
            };

        assetBundleMrg.LoadUnit(resourceName, loadOneCallback);
    }

    private void LoadUnitImg()
    {
        string resourceName = "Fox1";
        AssetBundleManager.LoadCallback loadOneCallback =
            delegate(AssetBundleManager.Code code, string abName, AssetBundle assetBundle)
            {
                if (code != AssetBundleManager.Code.Success)
                {
                    Q.Log("Load {0} Fail", abName);
                    return;
                }

                string[] allAbNames = assetBundle.GetAllAssetNames();
                for (int i = 0, n = allAbNames.Length; i < n; i++)
                {
                    Q.Log("allAbNames[{0}]={1}", i, allAbNames[i]);
                }

                //string assetPath = string.Format("Assets/ExternalRes/Unit/{0}/{1}.prefab", resourceName, resourceName);
                //GameObject prefab = assetBundle.LoadAsset<GameObject>(assetPath);
                //GameObject ins = Instantiate<GameObject>(prefab);
                //Q.Assert(prefab.GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>() == null);
                //Q.Assert(ins.GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>() != null);
                //ins.transform.SetParent(AssetLayer);
                //ins.transform.localPosition = Vector3.zero;
                //ins.transform.localScale = new Vector3(1, 1, 1);

                //Q.Log("Load {0} complete", abName);
                //Q.Assert(assetBundleMrg.AssetVerDict.Count - assetBundleMrg.NeedUpdateAsset.Count == 1);
            };

        assetBundleMrg.LoadUnitImg(resourceName, loadOneCallback);
    }


    private IEnumerator TestKeng()
    {
        Caching.CleanCache();
        //string url = "http://app1104772395.imgcache.qzoneapp.com/app1104772395/dev/assetbundles/Windows/unit/fox1";
        string url = "http://app1104772395.imgcache.qzoneapp.com/app1104772395/dev/assetbundles/Android/unit/fox1";
        WWW www = WWW.LoadFromCacheOrDownload(url, 1);

        yield return www;

        if(!string.IsNullOrEmpty(www.error))
        {
            Q.Warning(www.error);
            yield break;
        }

        Debug.LogFormat("cache={0}, url={1}", Caching.IsVersionCached(url, 1), url);
        url = "http://app1104772395.imgcache.qzoneapp.com/app1104772395/dev/assetbundles/Windows/unitimg/fox1";
        Debug.LogFormat("cache={0}, url={1}", Caching.IsVersionCached(url, 1), url);
        url = "http://fox1";
        Debug.LogFormat("cache={0}, url={1}", Caching.IsVersionCached(url, 1), url);
    }
}
