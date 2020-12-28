using UnityEngine;
using System.Collections;

public class SimplePositionAnimation : SimpleAnimationBase {
    public Vector3 From;
    public Vector3 To;

    [Tooltip ("自身坐标系还是世界坐标系")]
    public Space Space;

    protected override void UpdateAnimation(float elapsedRate_)
    {
        base.UpdateAnimation(elapsedRate_);

        if (Space == Space.Self)
        {
            transform.localPosition = Vector3.Lerp(From, To, elapsedRate_);
        }
        else
        {
            transform.position = Vector3.Lerp(From, To, elapsedRate_);
        }
    }
}
