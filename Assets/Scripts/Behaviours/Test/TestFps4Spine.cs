using UnityEngine;
using System.Collections;

public class TestFps4Spine : MonoBehaviour {
    public GameObject target;
    

	// Use this for initialization
	void Start () {
        
	}

    void AddSpine()
    {
        Vector3 spawnPoint = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
        GameObject spineGO = (GameObject)GameObject.Instantiate(target, target.transform.position, Quaternion.identity);
        spineGO.transform.parent = transform;
        spineGO.transform.localScale = new Vector3(1f, 1f, 1f);
        spineGO.transform.position = spineGO.transform.position + spawnPoint;
    }

	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            AddSpine();
        }
	}
}
