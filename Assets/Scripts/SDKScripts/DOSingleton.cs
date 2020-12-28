using UnityEngine;
using System.Collections;

namespace DoPlatform
{

	public class DOSingleton<T> : MonoBehaviour where T : DOSingleton<T>
	{
		protected static T instance = null;
		public static T Instance 
		{
			get 
			{
				if (instance == null)
				{
					instance = FindObjectOfType(typeof(T)) as T;
					if (instance == null)
					{
						GameObject obj = new GameObject(typeof(T).Name);
						//obj.AddComponent<DontUnloadObject>();
						instance = obj.AddComponent<T>();

						GameObject parent = GameObject.Find("Root");
						if(parent != null)
							obj.transform.parent = parent.transform;
					}
					if (instance == null)
						Debug.LogError("Failed to create instance of " + typeof(T).FullName + ".");
				}
				return instance;
			}
		}
	}

}