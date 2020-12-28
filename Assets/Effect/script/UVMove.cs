using UnityEngine;
using System.Collections;
 
public class UVMove : MonoBehaviour
{
    protected Material Material;
    public float scaleU = 1f;
    public float scaleV = 1f;
    public float speedU = .1f;
    public float speedV = .1f;

    public bool UseShareMaterial = false;
    Vector2 vectScale = new Vector2();
#if UNITY_EDITOR || UNITY_STANDALONE
    void Start()
    {

        Material = GetComponent<Renderer>().material;
        Material.mainTextureScale = new Vector2(scaleU, scaleV);
    }

#else
    void Start () 
    {

        Material = UseShareMaterial ? GetComponent<Renderer>().sharedMaterial: GetComponent<Renderer>().material;
        Material.mainTextureScale = new Vector2(scaleU, scaleV);
    }
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
    

    void Update()
    {
        Material = UseShareMaterial ? GetComponent<Renderer>().sharedMaterial : GetComponent<Renderer>().material;
        //Material.mainTextureScale = new Vector2(scaleU, scaleV);
        vectScale.x = speedU * Time.deltaTime;
        vectScale.y = speedV * Time.deltaTime;
        Material.mainTextureOffset += vectScale;
    }


#else
    void Update()
    {
        vectScale.x = speedU * Time.deltaTime;
        vectScale.y = speedV * Time.deltaTime;
        Material.mainTextureOffset += vectScale;
    }

#endif

}
