using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

/// <summary>
/// 掛在靜態文字，或者靜態文本圖片
/// </summary>
public class LanguageUIHelper : MonoBehaviour
{
    Graphic target;

	// Use this for initialization
	void Start () {
        target = GetComponent<Graphic>();

        if (target is Image)
        {
            //如果是圖片，則重置圖片的寬高
            (target as Image).SetNativeSize();
            //TODO判斷語言系統並切換貼圖，以下範例
            /*
            Debug.Log(target.name);
            Sprite x= Resources.Load<Sprite>("Textures/UIUnitDialog/"+ target.name);
            if (x!=null) {
                target.GetComponent<Image>().sprite = x;
            }
            */
        }




        else if (target is Text)
        {
            //靜態文字，則去取對應語言的文字
            Text tTarget = target as Text;
            tTarget.text = Utils.GetText(tTarget.text);
            //Debug.Log(tTarget.name);
        }
	}
 
}
