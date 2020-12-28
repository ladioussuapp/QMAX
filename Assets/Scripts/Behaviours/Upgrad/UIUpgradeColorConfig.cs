using UnityEngine;
using System.Collections;

public class UIUpgradeColorConfig : MonoBehaviour {
    public Color[] Body_BG_COLORS = new Color[] { Color.white, Color.white, Color.white, Color.white, Color.white };

    public static UIUpgradeColorConfig Instance;

	// Use this for initialization
	void Awake () {
        Instance = this;
	}
 
}
