using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoTestDisplay : MonoBehaviour
{
    private int count = 0;

    void Start()
    {
        Button btn = GetComponent<Button>();
        Text txt = transform.GetChild(0).GetComponent<Text>();
        if (GameController.Instance.Model.SdkData.userId != "")
        {
            if (GameController.Instance.Model.LoginData != null)
            {
                txt.text += GameController.Instance.Model.LoginData.actorId;
            }
        }

        count = 0;

        btn.onClick.AddListener(delegate() 
        {
            if(++count>=100)
            {
                txt.text = "Com4Loves.Shenzhen Games ©";
            }
        });
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}
