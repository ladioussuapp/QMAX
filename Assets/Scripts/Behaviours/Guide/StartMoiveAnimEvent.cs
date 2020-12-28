using UnityEngine;
using System.Collections;

public class StartMoiveAnimEvent : MonoBehaviour {
    public StartMovieBehaviour moveBeh;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void MovieAnimEnd(int index)
    {
        moveBeh.OnMovieExit(index);
    }
}
