using UnityEngine;
using System.Collections;

public class DrawPoint : MonoBehaviour {

	// Use this for initialization

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, .5f);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
