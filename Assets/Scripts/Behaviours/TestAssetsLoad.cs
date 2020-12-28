using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using Com4Love.Qmax;


public class TestAssetsLoad : MonoBehaviour
{
    public RectTransform[] placeHolders;
    protected string pathRoot = "http://192.168.103.7/file/";

    // Use this for initialization
    void Start()
    {
        Debug.Log(GameController.Instance);
        StartCoroutine(Load());
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    protected IEnumerator Load()
    {
        
        foreach (RectTransform transform in placeHolders)
        {
            WWW www = new WWW(pathRoot + transform.name + ".png?" + Time.time);
            yield return www;
 
            if (string.IsNullOrEmpty(www.error))
            {
                Texture2D txt2d = new Texture2D(4, 4, TextureFormat.ARGB32, false);
                www.LoadImageIntoTexture(txt2d);

                GameObject.Destroy(transform.GetComponent<SkeletonAnimation>());
                GameObject.Destroy(transform.GetComponent<MeshFilter>());
                GameObject.Destroy(transform.GetComponent<MeshRenderer>());

                yield return 0;

                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.anchoredPosition3D = Vector3.zero;

                transform.gameObject.AddComponent<CanvasRenderer>();
                Image image = transform.gameObject.AddComponent<Image>();

                //SpriteRenderer spriteRenderer = transform.gameObject.AddComponent<SpriteRenderer>();
                Sprite sprite = Sprite.Create(txt2d, new Rect(0f, 0f, txt2d.width, txt2d.height), new Vector2(.5f, .5f));
                //spriteRenderer.sprite = sprite;
                image.sprite = sprite;
                image.SetNativeSize();
            }
            else
            {
                ShowPhoneDebug(www.url + " 未找到!");
            }
        }

        yield return 0;
    }

    protected void ShowPhoneDebug(string msg)
    {
        DebugBehaviour.WriteLine(msg);
    }
}
