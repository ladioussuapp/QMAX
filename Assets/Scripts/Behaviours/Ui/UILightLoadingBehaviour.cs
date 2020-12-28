using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

public class UILightLoadingBehaviour : MonoBehaviour
{
    public Canvas renderCanvas;
    public Animator Animator;

    //public Transform LoadingImage;
    // Use this for initialization
    void Start()
    {
        renderCanvas.enabled = false;
        Animator.enabled = false;

        //简单点就直接延迟调用了
        Invoke("Display" , 1f);
    }

    void Display()
    {
        Animator.enabled = renderCanvas.enabled = true;
    }

}
