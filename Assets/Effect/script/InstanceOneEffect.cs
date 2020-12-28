using UnityEngine;
using System.Collections;

public class InstanceOneEffect : MonoBehaviour {

    public GameObject EffectPrefab;

    GameObject Eff;

    void OnEnable()
    {
        Create();
    }

    void OnDisable()
    {
        Destroy(Eff);
    }

    public void Create()
    {
#if EFFECT_HIDE

#endif

        if (Eff != null)
            Destroy(Eff);

        Eff = Instantiate(EffectPrefab);
        Eff.transform.SetParent(transform);
        Eff.transform.localPosition = Vector3.zero;
        Eff.transform.localScale = Vector3.one;
        Eff.transform.localRotation = Quaternion.identity;
    }

}
