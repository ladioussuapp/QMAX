using UnityEngine;
using System.Collections;

public class SimpleScaleAnimation : SimpleAnimationBase
{
    public Vector3 ScaleFrom = new Vector3(1,1,1);
    public Vector3 ScaleTo = new Vector3(1, 1, 1);

    protected override void UpdateAnimation(float elapsedRate_)
    {
        base.UpdateAnimation(elapsedRate_);
        transform.localScale = Vector3.Lerp(ScaleFrom, ScaleTo, elapsedRate_);
    }
}
