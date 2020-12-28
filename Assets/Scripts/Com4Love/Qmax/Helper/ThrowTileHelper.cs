using Com4Love.Qmax.Data.VO;
using System;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    /// <summary>
    /// 技能CD滿之後，夥伴扔出TileObject到棋盤中
    /// TODO 要考慮同時出發扔多個炸彈的情況
    /// </summary>
    public class ThrowTileHelper
    {
        static public void Play(BoardBehaviour boardBeh,
            TileObject tileObj, Animator unitAnim,
            Action<TileObject> callback)
        {
            //Action<Animator, string> OnAttackFrame = null;
            //OnAttackFrame = delegate(Animator anim, string name)
            //{
            //    unitAnim.GetComponent<ResendAnimEventBehaviour>().EventDelegate -= OnAttackFrame;
            //    Step2(boardBeh, tileObj, unitAnim, callback);
            //};
            //unitAnim.GetComponent<ResendAnimEventBehaviour>().EventDelegate += OnAttackFrame;
            //unitAnim.SetTrigger("TriggerAttack");
            
            float delayTime = 0;

            Action throwEvent = delegate()
            {
                ///強制動作從頭開始播放///
                unitAnim.Play("Attack", 0, 0);

                GameController.Instance.AudioManager.PlayAudio("SD_generate_jewel");
                // 第一次投放技能，播放台詞音效
                if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeSkill, true))
                {
                    PlayerPrefsTools.SetIntValue(OnOff.FirstTimeSkill, 1, true);
                    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_14");
                }

                boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        Step2(boardBeh, tileObj, unitAnim, callback);
                    }, 0.5f
                ));
            };

            AnimatorStateInfo info = unitAnim.GetCurrentAnimatorStateInfo(0);

            ///如果需要等待上個動作播放完執行這裡///
            ///不需要等待則註釋掉///
            if (info.IsName("Attack"))
            {
                //float anilenght =  info.length / info.normalizedTime;
                //delayTime = anilenght - info.length;
                Q.Log("ThrowTileHelper Animation is attack "+ info.normalizedTime);
            }

            if (delayTime == 0)
                throwEvent();
            else{
                boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                    throwEvent
                    ,delayTime
                    ));
            }

        }


        static private void Step2(BoardBehaviour boardBeh,
            TileObject tileObj, Animator unitAnim,
            Action<TileObject> callback)
        {
            RectTransform flyLayer = boardBeh.FlyLayer;
            RectTransform elementLayer = boardBeh.ElementLayer;
            Camera boardCamera = boardBeh.BoardCamera;
            GameObject[,] eleGameObjs = boardBeh.GetTypeGameObjects(TileType.Element);

            SkillLoadingBehaviour skillLoading = boardBeh.SkillLoadings[tileObj.Config.ColorType];
            RectTransform eleRect = boardBeh.GetTileGameObj(tileObj.Row, tileObj.Col, tileObj).transform as RectTransform;
            //飛行層
            eleRect.transform.SetParent(flyLayer);
            Animator anim = eleRect.GetComponent<Animator>();
            anim.enabled = false;
            //消除物初始化是會在棋盤上正確的位置，把這個位置保存，作為飛行的目標位置
            Vector3 oriLocalPos = eleRect.localPosition;

            //將消除物設定到夥伴的位置
            RectTransform targetUnitRect = skillLoading.transform as RectTransform;
            //將夥伴的位置轉換到世界坐標
            Vector3 unitWorldPoint = targetUnitRect.TransformPoint(0, 0, 0);
            //將世界坐標轉換為屏幕坐標
            Vector3 unitScreenPoint = boardCamera.WorldToScreenPoint(unitWorldPoint);
            Vector2 tempPoint;
            //將屏幕坐標轉換為elementLayer的本地坐標Vector2
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                elementLayer, unitScreenPoint, boardCamera, out tempPoint);
            //Vector2 -> Vector3
            Vector3 targetLocalPos = new Vector3(tempPoint.x, tempPoint.y);
            eleRect.localPosition = targetLocalPos;

            Action<object> onChargeComplete = delegate(object rectTransform)
            {
                eleRect.SetParent(elementLayer);
                Utils.SetCurrentPositoinToAnchor(eleRect);
                if (eleGameObjs[tileObj.Row, tileObj.Col] == null)
                {
                    GameObject.Destroy(eleGameObjs[tileObj.Row, tileObj.Col]);
                }
                else
                {
                    boardBeh.GcTileGameObj(eleGameObjs[tileObj.Row, tileObj.Col]);
                }
                
                eleGameObjs[tileObj.Row, tileObj.Col] = eleRect.gameObject;
                anim.enabled = true;
                if (callback != null)
                    callback(tileObj);
            };

            skillLoading.gameObject.SetActive(false);
            Vector3[] arr1 = {
                                   eleRect.localPosition - new Vector3(100, 100, 0),
                                   //eleRect.localPosition - new Vector3(-100, -100, 0),
                                   //eleRect.localPosition - new Vector3(200, -100, 0),
                                   //eleRect.localPosition - new Vector3(-200, -100, 0),
                               };
            Vector3 minPoint = arr1[UnityEngine.Random.Range(0, arr1.Length)];
            Vector3[] arr = { 
                                  eleRect.localPosition,
                                  minPoint,
                                  minPoint,
                                  oriLocalPos, 
                              };
            //飛到棋盤上製定位置
            //如果激活了Animator，且Animator.CullingMode==AnimatorCullingMode.AlwaysAnimate
            //會導致無法設置eleRect的Transfrom。
            //因此運動前先將其enabled=false，運動完再設為true
            LeanTween.moveLocal(eleRect.gameObject, arr, 0.2f)
                .setOnComplete(onChargeComplete)
                .setOnCompleteParam(eleRect);
        }

    }
}
