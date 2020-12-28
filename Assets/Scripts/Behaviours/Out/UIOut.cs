using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

public class UIOut : MonoBehaviour {
    public UIButtonBehaviour CancelButton;
    public UIButtonBehaviour EnterButton;

	// Use this for initialization
	void Start () {
        CancelButton.onClick += OnCancelClick;
        EnterButton.onClick += OnEnterClick;
	}

    void OnCancelClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIOut);
    }

    void OnEnterClick(UIButtonBehaviour button)
    {
        GameController.Instance.QuitGame();
    }
 
}
