using UnityEngine;
using System.Collections;

public class AutoRotation : MonoBehaviour
{
    [Tooltip("旋转速度  单位都是秒")]
    public Vector3 RotationSpeed;
    public Space Space;
 
    public void LateUpdate()
    {
        transform.Rotate(RotationSpeed * Time.deltaTime , Space);
    }
}
