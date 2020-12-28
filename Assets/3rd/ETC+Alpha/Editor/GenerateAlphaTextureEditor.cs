using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEngine.UI;

public class GenerateAlphaTextureEditor : EditorWindow
{
    public class EtcAlphaInfo
    {
        public Texture2D SourceTexture;
        public Texture2D AlphaTexture;
        public string SourcePath;
        public string AlphaPath;
        public TextureImporter SoureTextureImporter;
        public TextureImporter AlphaTextureImporter;
        public int AlphaFormat;
    }

    public EtcAlphaInfo[] _etcAlphaInfos;
    private static Texture2D _arrowTexture2D;
    private string[] _alphaFormats = new string[] { "Alpha using R channel", "Alpha using G channel", "Alpha using B channel" };
    private readonly Rect _textRect = new Rect(85, 45, 250, 50);
    // Use this for initialization
    [MenuItem("Tools/J3Tech/ETC+Alpha/GenerateAlphaTexture")]
    private static void Init()
    {
        GenerateAlphaTextureEditor textureSettingEditor = GetWindow<GenerateAlphaTextureEditor>(false);
        textureSettingEditor.position = new Rect(Screen.currentResolution.width / 2 - 200, Screen.currentResolution.height / 2 - 70, 400, 140);
#if UNITY_5_2
        textureSettingEditor.titleContent = new GUIContent("Create Alpha");
#else
        textureSettingEditor.title = "Create Alpha";
#endif

        textureSettingEditor.minSize = new Vector2(420, 140);
        textureSettingEditor.maxSize = new Vector2(425, 1000);
    }


    [MenuItem("Tools/J3Tech/ETC+Alpha/生成所有选中贴图的Alpha贴图和材质")]
    private static void GenSelectiveAlphaTexAndMaterial()
    {
        GenerateAlphaTextureEditor textureSettingEditor = GetWindow<GenerateAlphaTextureEditor>(false);
        for (int i = 0, n = textureSettingEditor._etcAlphaInfos.Length; i < n; i++)
        {
            GenAlphaTex(textureSettingEditor._etcAlphaInfos[i], true);
        }
    }


    [MenuItem("Tools/J3Tech/ETC+Alpha/生成所有选中贴图的Alpha贴图")]
    private static void GenSelectiveAlphaTex()
    {
        GenerateAlphaTextureEditor textureSettingEditor = GetWindow<GenerateAlphaTextureEditor>(false);
        for (int i = 0, n = textureSettingEditor._etcAlphaInfos.Length; i < n; i++)
        {
            EtcAlphaInfo info = textureSettingEditor._etcAlphaInfos[i];
            Debug.LogFormat("正在处理 {0}", info.SourcePath);
            GenAlphaTex(info, false);
        }
    }



    [MenuItem("Tools/J3Tech/ETC+Alpha/赋值所有Image的Material")]
    static private void TravelAllImage()
    {
        //Object[] objects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable);
        GameObject[] list = Selection.gameObjects;
        Debug.Log(list.Length);

        Action<Transform, string> RecursiveSetImage = null;
        RecursiveSetImage = delegate(Transform node, string path)
        {
            Image[] imgArray = node.GetComponents<Image>();
            for (int i = 0, n = imgArray.Length; i < n; i++)
            {
                Image img = imgArray[i];
                if (img.sprite == null)
                    continue;

                string texPath = AssetDatabase.GetAssetPath(img.sprite.texture);
                string matPath =
                    Path.GetDirectoryName(texPath) + "/" +
                    Path.GetFileNameWithoutExtension(texPath) + ".mat";

                //判断贴图是否是ETC2，是的话才转换
                TextureImporter importer =
                    AssetImporter.GetAtPath(texPath) as TextureImporter;

                if (importer == null)
                {
                    //没有这个Texture，可能是Unity内置Texture
                    continue;
                }


                Debug.Log(path);
                int maxSize = 0;
                TextureImporterFormat tf;
                importer.GetPlatformTextureSettings("Android", out maxSize, out tf);
                //是否需要赋值Material
                bool needMat = false;
                switch (tf)
                {
                    case TextureImporterFormat.RGBA16:
                        needMat = false;
                        break;
                    case TextureImporterFormat.ETC_RGB4:
                    case TextureImporterFormat.ETC2_RGBA8:
                        needMat = true;
                        break;
                    default:
                        needMat = false;
                        break;
                }

                if (!needMat)
                {
                    Debug.LogWarningFormat("{0} format={1}", texPath, tf);
                    continue;
                }

                Object mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material));

                if (mat == null)
                    Debug.LogWarningFormat("{0} is null", matPath);
                else
                    img.material = mat as Material;
            }

            if (node.childCount == 0)
                return;

            for (int i = 0, n = node.childCount; i < n; i++)
            {
                Transform child = node.GetChild(i);
                RecursiveSetImage(child, path + "/" + child.name);
            }
        };

        for (int i = 0, n = list.Length; i < n; i++)
        {
            GameObject ga = list[i];
            //Debug.Log(list[i].name);
            ga.SetActive(true);

            RecursiveSetImage(ga.transform, ga.name);

            //保存到Prefab
            Object prefab = PrefabUtility.GetPrefabParent(ga);
            PrefabUtility.ReplacePrefab(ga, prefab);
        }
    }



    private void OnFocus()
    {
        OnSelectionChange();
    }

    // Update is called once per frame
    void OnSelectionChange()
    {
        Object[] objects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        if (objects.Length > 0)
        {
            _etcAlphaInfos = new EtcAlphaInfo[objects.Length];
            for (int i = 0; i < _etcAlphaInfos.Length; ++i)
            {
                _etcAlphaInfos[i] = new EtcAlphaInfo();
                _etcAlphaInfos[i].SourceTexture = objects[i] as Texture2D;
                _etcAlphaInfos[i].SourcePath = AssetDatabase.GetAssetPath(objects[i]);
                _etcAlphaInfos[i].SoureTextureImporter = AssetImporter.GetAtPath(_etcAlphaInfos[i].SourcePath) as TextureImporter;
                _etcAlphaInfos[i].AlphaPath = Path.GetDirectoryName(_etcAlphaInfos[i].SourcePath) + "/" + Path.GetFileNameWithoutExtension(_etcAlphaInfos[i].SourcePath) + "_Alpha.png";
                if (File.Exists(Application.dataPath.Substring(0, Application.dataPath.Length - 6) + _etcAlphaInfos[i].AlphaPath))
                {
                    _etcAlphaInfos[i].AlphaTexture = AssetDatabase.LoadAssetAtPath(_etcAlphaInfos[i].AlphaPath, typeof(Texture)) as Texture2D;
                    _etcAlphaInfos[i].AlphaTextureImporter = AssetImporter.GetAtPath(_etcAlphaInfos[i].AlphaPath) as TextureImporter;
                    _etcAlphaInfos[i].AlphaFormat = -1;

                    string[] label = AssetDatabase.GetLabels(_etcAlphaInfos[i].AlphaTexture);

                    if (label != null && label.Length > 0)
                    {
                        if (label[0].Contains("ETC+Alpha using R channel"))
                        {
                            _etcAlphaInfos[i].AlphaFormat = 0;
                        }
                        else if (label[0].Contains("ETC+Alpha using G channel"))
                        {
                            _etcAlphaInfos[i].AlphaFormat = 1;
                        }
                        else
                        {
                            _etcAlphaInfos[i].AlphaFormat = 2;
                        }
                    }
                }
            }
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (_etcAlphaInfos != null && _etcAlphaInfos.Length > 0)
        {
            int y = 10;
            for (int i = 0; i < _etcAlphaInfos.Length; ++i)
            {
                DrawGrid.Draw(new Rect(10, y, 100, 100));
                GUI.DrawTexture(new Rect(10, y, 100, 100), _etcAlphaInfos[i].SourceTexture);
                GUI.Label(new Rect(10, y + 100, 100, 20), Path.GetFileName(_etcAlphaInfos[i].SourcePath));
                DrawGrid.Draw(new Rect(160, y, 100, 100));
                if (_etcAlphaInfos[i].AlphaTexture != null)
                {
                    GUI.DrawTexture(new Rect(160, y, 100, 100), _etcAlphaInfos[i].AlphaTexture);
                    GUI.Label(new Rect(160, y + 100, 100, 20), Path.GetFileName(_etcAlphaInfos[i].AlphaPath));
                }
                if (_arrowTexture2D == null)
                {
#if UNIT_5_2 || UNITY_5_1
                    _arrowTexture2D = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3rd/ETC+Alpha/Editor/Texture/arrow.png");
#else
                    _arrowTexture2D = AssetDatabase.LoadAssetAtPath("Assets/3rd/ETC+Alpha/Editor/Texture/arrow.png", typeof(Texture2D)) as Texture2D;
#endif
                }
                GUI.DrawTexture(new Rect(120, y + (100 - _arrowTexture2D.height) / 2, _arrowTexture2D.width, _arrowTexture2D.height), _arrowTexture2D);
                GUI.Label(new Rect(270, y + 20, 120, 20), "Format");
                _etcAlphaInfos[i].AlphaFormat = EditorGUI.Popup(new Rect(270, y + 40, 140, 20), "", _etcAlphaInfos[i].AlphaFormat, _alphaFormats);

                if (GUI.Button(new Rect(270, y + 60, 140, 20), "Generate"))
                {
                    GenAlphaTex(_etcAlphaInfos[i], false);
                }
                y += 120;
            }
        }
        else
        {
            GUI.Label(_textRect, "Please select texture in project view");
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="genMat">是否生成Material</param>
    static private void GenAlphaTex(EtcAlphaInfo info, bool genMat)
    {
        try
        {
            info.SoureTextureImporter.isReadable = true;
            info.SoureTextureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.RGBA32);
            AssetDatabase.ImportAsset(info.SourcePath);

            info.AlphaTexture = new Texture2D(info.SourceTexture.width,
                info.SourceTexture.height, TextureFormat.RGBA32, false);
            Color32[] color32S = info.AlphaTexture.GetPixels32();
            Color32[] srcColor32S = info.SourceTexture.GetPixels32();

            if (info.AlphaFormat == 0)
            {
                for (int n = 0; n < color32S.Length; ++n)
                {
                    color32S[n] = new Color32(srcColor32S[n].a, 0, 0, 0);
                }
            }
            else if (info.AlphaFormat == 1)
            {
                for (int n = 0; n < color32S.Length; ++n)
                {
                    color32S[n] = new Color32(0, srcColor32S[n].a, 0, 0);
                }
            }
            else
            {
                for (int n = 0; n < color32S.Length; ++n)
                {
                    color32S[n] = new Color32(0, 0, srcColor32S[n].a, 0);
                }
            }
            info.AlphaTexture.SetPixels32(color32S);
            info.AlphaTexture.Apply(false);
            string fileName = Application.dataPath.Substring(0, Application.dataPath.Length - 6) +
                              info.AlphaPath;
            File.WriteAllBytes(fileName, info.AlphaTexture.EncodeToPNG());
            while (!File.Exists(fileName)) ;
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            info.AlphaTextureImporter =
                AssetImporter.GetAtPath(info.AlphaPath) as TextureImporter;
            while (info.AlphaTextureImporter == null)
            {
                info.AlphaTextureImporter =
                    AssetImporter.GetAtPath(info.AlphaPath) as TextureImporter;
            }
            info.AlphaTextureImporter.textureType = TextureImporterType.Default;
            info.AlphaTextureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4);
            info.AlphaTextureImporter.mipmapEnabled = false;
            info.SoureTextureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4);

            AssetDatabase.ImportAsset(info.SourcePath);
            AssetDatabase.ImportAsset(info.AlphaPath);

            info.AlphaTexture = AssetDatabase.LoadAssetAtPath(info.AlphaPath, typeof(Texture)) as Texture2D;


            //生成一个Material
            Shader s = null;
            if (info.AlphaFormat == 0)
            {
                AssetDatabase.SetLabels(info.AlphaTexture, new string[] { "ETC+Alpha using R channel for " + Path.GetFileName(info.SourcePath) });
                s = Shader.Find("UI/Default (ETC+Alpha using R channel)");
            }
            else if (info.AlphaFormat == 1)
            {
                AssetDatabase.SetLabels(info.AlphaTexture, new string[] { "ETC+Alpha using G channel for " + Path.GetFileName(info.SourcePath) });
                s = Shader.Find("UI/Default (ETC+Alpha using G channel)");
            }
            else
            {
                AssetDatabase.SetLabels(info.AlphaTexture, new string[] { "ETC+Alpha using B channel for " + Path.GetFileName(info.SourcePath) });
                s = Shader.Find("UI/Default (ETC+Alpha using B channel)");
            }

            if (genMat)
            {
                string matPath = Path.GetDirectoryName(info.SourcePath) + "/" + Path.GetFileNameWithoutExtension(info.SourcePath) + ".mat";
                Material m = new Material(s);
                m.SetTexture("_MainTex", info.SourceTexture);
                m.SetTexture("_AlphaTex", info.AlphaTexture);
                AssetDatabase.CreateAsset(m, matPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }





    private void OnDesroy()
    {
        DrawGrid.Done();
    }
}
