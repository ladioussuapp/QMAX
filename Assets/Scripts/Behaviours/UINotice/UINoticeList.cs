using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.VO;
 
public class UINoticeList : MonoBehaviour {
    //方便美術什麼的修改
    public RectTransform ItemPrefab;
    public RectTransform Content;

    public void Awake()
    {
        ItemPrefab.gameObject.SetActive(false);

        //Invoke("TestAddItem", 1f);
        //Invoke("TestAddItem", 2f);
    }

    public void AddItem(NoticeInfo  data)
    {
        RectTransform itemT = GameObject.Instantiate(ItemPrefab);

        itemT.gameObject.SetActive(true);
        itemT.SetParent(Content);
        itemT.anchoredPosition3D = Vector3.zero;
        itemT.localScale = new Vector3(1, 1, 1);
        UINoticeListItem item = itemT.GetComponent<UINoticeListItem>();
        item.SetDatas(data.Title, data.Info, data.SourceImg);
    }

    void TestAddItem()
    {
        RectTransform itemT = GameObject.Instantiate(ItemPrefab);
 
        itemT.gameObject.SetActive(true);
        itemT.SetParent(Content);
        itemT.anchoredPosition3D = Vector3.zero;
        itemT.localScale = new Vector3(1, 1, 1);

        //Debug.Log("Item大小：" + itemT.transform.b);
    }

    public struct Data
    {
        public string Title;
        public string Info;
        public string SourceImg;
        public float time;
    }
}
