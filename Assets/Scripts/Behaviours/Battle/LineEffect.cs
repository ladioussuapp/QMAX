using UnityEngine;
using System.Collections;

public class LineEffect : MonoBehaviour {
    public Renderer lineRenderer;
 
    public Material GetShareMaterial()
    {
        return lineRenderer.sharedMaterial;
    }

}
