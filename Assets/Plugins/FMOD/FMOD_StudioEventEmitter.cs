using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD.Studio;
using FMOD;


public class FMOD_StudioEventEmitter : MonoBehaviour 
{
	public FMODAsset asset;

    private string _path;

    public string path
    {
        get { return _path; }
        set
        {
            evt = null;
            _path = value;
        }
    }

    public bool isMusic = true;
	public bool startEventOnAwake = true;

    public Parameter[] Params;

    FMOD.Studio.EventInstance evt;
	bool hasStarted = false;
	
	Rigidbody cachedRigidBody;

	[System.Serializable]
	public class Parameter
	{
		public string name;
		public float value;
        public float min;
        public float max;
	}
	
	public void Play()
	{
        if (evt == null || !evt.isValid())
        {
            CacheEventInstance();
        }

        if (evt != null)
		{
            if (isMusic)
                FMOD_Manager.instance.PlayMusic(evt);
            else
                FMOD_Manager.instance.PlayOneShot(evt);
        }
		else
		{
			FMOD.Studio.UnityUtil.LogWarning("Tried to play event without a valid instance: " + path);
			return;			
		}
	}
	
	public void Stop()
	{
		if (evt != null)
		{
			ERRCHECK(evt.stop(STOP_MODE.IMMEDIATE));
		}		
	}	
	
	public FMOD.Studio.ParameterInstance getParameter(string name)
	{
		FMOD.Studio.ParameterInstance param = null;
		ERRCHECK(evt.getParameter(name, out param));
			
		return param;
	}

    public void setParameterValue(string name, float value)
    {
        if (evt == null || !evt.isValid())
            return;

        if (Params != null)
        {
            foreach (var param in Params)
            {
                if (param.name == name)
                {
                    param.value = value;
                    break;
                }
            }
        }
        evt.setParameterValue(name, value);
    }


    public FMOD.Studio.PLAYBACK_STATE getPlaybackState()
	{
		if (evt == null || !evt.isValid())
			return FMOD.Studio.PLAYBACK_STATE.STOPPED;
		
		FMOD.Studio.PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		
		if (ERRCHECK (evt.getPlaybackState(out state)) == FMOD.RESULT.OK)
			return state;
		
		return FMOD.Studio.PLAYBACK_STATE.STOPPED;
	}

	void Start() 
	{
		if (evt == null || !evt.isValid())
		{
			CacheEventInstance();
		}
		
		cachedRigidBody = GetComponent<Rigidbody>();
		
		if (startEventOnAwake)
			StartEvent();
	}
	
	void CacheEventInstance()
	{
		if (asset != null)
		{
			evt = FMOD_StudioSystem.instance.GetEvent(asset.id);				
		}
		else if (!String.IsNullOrEmpty(path))
		{
			evt = FMOD_StudioSystem.instance.GetEvent(path);
		}
	}

    static bool isShuttingDown = false;

	void OnApplicationQuit() 
	{
		isShuttingDown = true;
	}

	void OnDestroy() 
	{
		if (isShuttingDown)
			return;

		FMOD.Studio.UnityUtil.Log("Destroy called");
		if (evt != null && evt.isValid()) 
		{
			if (getPlaybackState () != FMOD.Studio.PLAYBACK_STATE.STOPPED)
			{
				FMOD.Studio.UnityUtil.Log("Release evt: " + path);
				ERRCHECK (evt.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
			}
			
			ERRCHECK(evt.release ());
			evt = null;
		}
	}

	public void StartEvent()
	{		
		if (evt == null || !evt.isValid())
		{
			CacheEventInstance();
		}
		
		// Attempt to release as oneshot
		if (evt != null && evt.isValid())
		{
			Update3DAttributes();

            if (isMusic)
                FMOD_Manager.instance.PlayMusic(evt);
            else
                FMOD_Manager.instance.PlayOneShot(evt);

            //if (evt.release() == FMOD.RESULT.OK) 
            {
				//evt = null;
			}
		}
		else
		{
			FMOD.Studio.UnityUtil.LogError("Event retrieval failed: " + path);
		}

		hasStarted = true;
	}

	public bool HasFinished()
	{
		if (!hasStarted)
			return false;
		if (evt == null || !evt.isValid())
			return true;
		
		return getPlaybackState () == FMOD.Studio.PLAYBACK_STATE.STOPPED;
	}

	void Update() 
	{
		if (evt != null && evt.isValid ()) 
		{
			Update3DAttributes();
		} 
		else 
		{
			evt = null;
		}
        if (Params != null)
        {
            foreach (var param in Params)
            {
                evt.setParameterValue(param.name, param.value);
            }
        }
    }
	
	void Update3DAttributes()
	{
		if (evt != null && evt.isValid ()) 
		{
			var attributes = FMOD.Studio.UnityUtil.to3DAttributes(gameObject, cachedRigidBody);			
			ERRCHECK(evt.set3DAttributes(attributes));
		}
	}    
    
	
	FMOD.RESULT ERRCHECK(FMOD.RESULT result)
	{
		FMOD.Studio.UnityUtil.ERRCHECK(result);
		return result;
	}
}
