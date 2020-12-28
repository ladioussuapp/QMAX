using Com4Love.Qmax.Data.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    class InitHelper : MonoBehaviour
    {
        public event Action<ColorType, GameObject> OnProgress;
        public event Action OnComplete;

        public RectTransform UnitLayer;


        public void LoadUnits(List<UnitConfig> unitCfgs, float delay)
        {
            int count = 0;

            if (unitCfgs.Count == 0)
            {
                //有可能會剔除掉spine部分測試
                OnComplete();
            }

            for (int i = 0, n = unitCfgs.Count; i < n; i++)
            {
                UnitConfig cfg = unitCfgs[i];
                GameController.Instance.PoolManager.GetUnitInstance(
                    cfg,
                    delegate(string key, Transform unit)
                    {
                        if (UnitLayer != null)
                            unit.SetParent(UnitLayer.GetChild((int)cfg.UnitColor - 1));
                        unit.localScale = new Vector3(1, 1, 1);
                        unit.localPosition = Vector3.zero;
                        if (OnProgress != null)
                            OnProgress(cfg.UnitColor, unit.gameObject);

                        count++;
                        if (count == unitCfgs.Count && OnComplete != null)
                            OnComplete();
                    }
                );
            }//for
        }
    }
}
