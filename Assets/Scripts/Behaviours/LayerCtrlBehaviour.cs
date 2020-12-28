using UnityEngine;

public class LayerCtrlBehaviour : MonoBehaviour
{
    public Transform NormalLayer;
    public Transform PopupLayer;
    public Transform LoadingLayer;
    public Transform FloatLayer;
    public static LayerCtrlBehaviour ActiveLayer;

    public void Awake()
    {
        ActiveLayer = this;
    }
}
