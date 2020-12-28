using UnityEngine;
using System.Collections;

/// <summary>
/// for test
/// </summary>
public class TestMapMove : MonoBehaviour
{
    public float AutoMoveSpeed = 0f;
    public MapMove MapMove;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (AutoMoveSpeed != 0)
        {
            MapMove.AutoMove(AutoMoveSpeed);
        }
    }
}
