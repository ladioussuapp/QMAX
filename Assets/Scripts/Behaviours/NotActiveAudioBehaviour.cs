using UnityEngine;
using System.Collections;

public class NotActiveAudioBehaviour : MonoBehaviour {


	void Start () {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.playOnAwake)
        {
            AudioClip clip = audio.clip;
            if (clip != null)
            {
                GameController.Instance.AudioManager.PlayAudio(clip);
            }
        }
	}
	
}
