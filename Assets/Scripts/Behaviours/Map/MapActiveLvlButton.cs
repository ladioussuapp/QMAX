using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;

public class MapActiveLvlButton : MapLvlButton
{
    public override void SetSelect(bool val)
    {
        //暫時留空
    }

    public override void SetLock(bool val)
    {
        //無鎖定效果 留空
    }

    public override void PlayUnLockEffect()
    {
        base.PlayUnLockEffect();

        //Debug.Log("PlayUnLockEffect");
    }

    protected override void CreateButton()
    {
        button = GameController.Instance.QMaxAssetsFactory.CreateActiveMapLvlButton(state).gameObject;
        button.transform.parent = transform;
        button.transform.localPosition = Vector3.zero;
        button.transform.rotation = transform.rotation;
        button.name = string.Format("mapbutton_active_lvl{0}", Stage.ID.ToString());
        animator = button.GetComponent<Animator>();

        if (animator != null)
        {
            //點擊後會自動先激活再播放
            animator.enabled = false;
        }
    }

    protected override void Instance_OnTouchUp(SimpleFinger obj)
    {
        if (obj.PickGameObj != button)
        {
            return;
        }
 
        touchDown = false;

        if (animator != null && !animator.GetBool("IsUp"))
        {
            animator.enabled = true;
            animator.SetTrigger("UpTrigger");
            animator.SetBool("IsDown", false);
            animator.SetBool("IsUp", true);
        }
    }

    protected override void Instance_OnTouchDown(SimpleFinger obj)
    {
        if (obj.PickGameObj != button || GameController.Instance.Popup.HasPopup || GuideManager.getInstance().IsGuideRunning)
        {
            return;
        }

        touchDown = true;

        if (animator != null && !animator.GetBool("IsDown"))
        {
            animator.enabled = true;
            animator.SetTrigger("DownTrigger");
            animator.SetBool("IsUp", false);
            animator.SetBool("IsDown", true);
        }
    }

    protected override void Instance_OnSimpleTap(SimpleFinger obj)
    {
        if (obj.PickGameObj != button || GameController.Instance.Popup.HasPopup || GuideManager.getInstance().IsGuideRunning)
        {
            return;
        }

        if (!touchDown)
        {
            return;
        }

        if (state == ThingState.STATE_LOCKED)
        {
            //2358
            StageConfig.StageUnlock unlock = Stage.Unlocks[0];
            int lvl = unlock.Type == 1 ? unlock.param : -1;
            UIAlertBehaviour.Alert( Utils.GetTextByID(2358, lvl), "", "", 2, 0, 0, (byte)UIAlertBehaviour.ButtonStates.ButtonOk);
            return;
        }

        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_BUTTON_CLICK);

        if (GameController.Instance.ViewEventSystem.ClickMapBtnEvent != null)
            GameController.Instance.ViewEventSystem.ClickMapBtnEvent(Stage.ID);
    }
}
