using UnityEngine;
using System.Collections;

//配合QMax/MultiWather shader使用
public class MultiWather : MonoBehaviour
{
    protected Material Material;
    public float speedU = .1f;
    public float speedV = .1f;
    public float speedU2 = .1f;
    public float speedV2 = .1f;

    public bool UseShareMaterial = false;
    Vector2 vect = new Vector2();
 
    void Start () 
    {
        Material = UseShareMaterial ? GetComponent<Renderer>().sharedMaterial: GetComponent<Renderer>().material;
    }
 
    void Update()
    {
        vect = Material.GetTextureOffset("_SurfaceTex");
        vect.x += speedU * Time.deltaTime;
        vect.y += speedV * Time.deltaTime;
        Material.SetTextureOffset("_SurfaceTex", vect);

        vect = Material.GetTextureOffset("_BottomTex");
        vect.x += speedU2 * Time.deltaTime;
        vect.y += speedV2 * Time.deltaTime;
        Material.SetTextureOffset("_BottomTex", vect);
    }
}



