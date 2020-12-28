using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Com4Love.Qmax.Tools
{
    /// <summary>
    /// 統一的特效接口  會在gameCtr中引用
    /// </summary>
    public class EffectProxy
    {
        /// <summary>
        /// 數字滾動特效
        /// </summary>
        /// <param name="text">對應的text 组件</param>
        /// <param name="from">開始時數字</param>
        /// <param name="to">目標數字</param>
        /// <param name="time">時間</param>
        /// <param name="AudioPlay">是否播放音效  音效文件是指定的</param>
        /// <param name="endCallback">結束回調</param>
        public void ScrollText(Text text, int from, int to, float time, bool AudioPlay = false, Action endCallback = null)
        {
            TextScrollEffect effect = text.gameObject.GetComponent<TextScrollEffect>();
            AudioClip audioClip = null;
            Action func = delegate ()
            {
                if (effect == null)
                {
                    effect = text.gameObject.AddComponent<TextScrollEffect>();
                }

                effect.text = text;
                effect.from = from;
                effect.to = to;
                effect.time = time;
                effect.AudioClip = audioClip;
                effect.Run(endCallback);
            };

            if (AudioPlay)
            {
                GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
                    UIAudioConfig.NUMBER_ROLLING_LOOP, delegate (AudioClip clip)
                    {
                        audioClip = clip;
                        func();
                    });
            }
            else
            {
                func();
            }
        }

        /// <summary>
        /// 數字滾動特效，根據插值計算時間。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="AudioPlay"></param>
        public void ScrollText(Text text, int from, int to, bool AudioPlay = false, Action endCallback = null)
        {
            int dis = Mathf.Abs(to - from);
            float time = .3f + (dis / 15) * .1f;     //20以内都是.3秒

            if (time > 2f)
            {
                time = 2f;
            }

            ScrollText(text, from, to, time, AudioPlay, endCallback);
        }

        public void ZoomScene(Camera PerspectiveCamera , float fovVal , Action OnComplete)
        {
            float initFieldView = PerspectiveCamera.fieldOfView - fovVal;
            float newFiledView = PerspectiveCamera.fieldOfView;
            LTDescr lTDescr;

            lTDescr = LeanTween.value(PerspectiveCamera.gameObject, initFieldView, newFiledView, 0.6f)
                .setOnUpdate(delegate (float val)
            {
                PerspectiveCamera.fieldOfView = val;
            }).setEase(LeanTweenType.easeOutQuad);

            if (OnComplete != null)
            {
                lTDescr.setOnComplete(OnComplete);
            }
        }

        public RenderTexture SnapshotTexture;

        /// <summary>
        /// 屏幕截圖 然後保存起來。
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="material"></param>
        /// <param name="scale"></param>
        public void  SnapshotCache(Camera camera , int scale)
        {
            SnapshotTexture = RenderTexture.GetTemporary(Screen.width / scale, Screen.height / scale);
            camera.targetTexture = SnapshotTexture;
            camera.Render();
            camera.targetTexture = null;
        }

        public void ClearSnapshotCache()
        {
            if (SnapshotTexture != null)
            {
                RenderTexture.ReleaseTemporary(SnapshotTexture);
            }
        }
    }
}
