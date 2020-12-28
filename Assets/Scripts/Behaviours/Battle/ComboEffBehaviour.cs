using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using UnityEngine;
using UnityEngine.UI;

public class ComboEffBehaviour : MonoBehaviour
{
    /// <summary>
    /// 连消*X.X的效果层
    /// </summary>
    public Transform CombatComboLayer;

    /// <summary>
    /// Combo时喊话的特效
    /// </summary>
    public Transform CombatWordLayer;

    /// <summary>
    /// Combo喊话时对应的语音
    /// </summary>
    public AudioClip[] CombatWordBlueAudios;

    /// <summary>
    /// Combo喊话时对应的语音
    /// </summary>
    public AudioClip[] CombatWordGreenAudios;

    /// <summary>
    /// Combo喊话时对应的语音
    /// </summary>
    public AudioClip[] CombatWordOrangeAudios;

    public string[] CombatWordBlues;
    public string[] CombatWordGreens;
    public string[] CombatWordOranges;



    /// <summary>
    /// 设置“连消X.X”的效果是否显示
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="elementCount"></param>
    public void SetCombatCmoboEffect(bool visible, int elementCount = 0)
    {
        CombatComboLayer.gameObject.SetActive(visible);

        if (elementCount <= 0)
            return;

        CombatComboLayer.gameObject.SetActive(true);
        for (int i = 0; i < CombatComboLayer.childCount; i++)
        {
            CombatComboLayer.GetChild(i).gameObject.SetActive(false);
        }

        ComboConfig conf = GameController.Instance.Model.ComboConfigs[elementCount];
        float comboRate = conf.ComboRate;

        Transform combatComboWord = CombatComboLayer.Find("sideword" + comboRate);
        if (combatComboWord != null)
        {
            combatComboWord.gameObject.SetActive(true);
        }
    }


    /// <summary>
    /// 播放combo时的喊话
    /// </summary>
    public void PlayComboSlogan(int elementCount)
    {
        if (elementCount <= 5)
            return;


        string spriteName = null;
        AudioClip clip = null;
        int random = 0;

        if (elementCount >= 6 && elementCount <= 8)
        {
            random = UnityEngine.Random.Range(0, 3);
            clip = CombatWordBlueAudios[random];
            spriteName = CombatWordBlues[random];
        }
        else if (elementCount == 9)
        {
            clip = CombatWordBlueAudios[4];
            spriteName = CombatWordBlues[4];
        }
        else if (elementCount >= 10 && elementCount <= 14)
        {
            random = UnityEngine.Random.Range(0, CombatWordGreens.Length - 1);
            clip = CombatWordGreenAudios[random];
            spriteName = CombatWordGreens[random];
        }
        else if (elementCount >= 15)
        {
            random = UnityEngine.Random.Range(0, CombatWordOranges.Length - 1);
            clip = CombatWordOrangeAudios[random];
            spriteName = CombatWordOranges[random];
        }

        if (clip)
        {
            AudioSource audioSource = CombatWordLayer.GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
        }
        Sprite sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, spriteName);

        Q.Assert(sprite != null, "ComboEffBehaviour:PlayComboSlogan Assert1 spriteName={0}", spriteName);
        GameObject child = CombatWordLayer.GetChild(0).gameObject;
        Image img = child.GetComponent<Image>();
        child.gameObject.SetActive(true);
        img.sprite = sprite;
        img.SetNativeSize();
        CombatWordLayer.GetComponent<Animator>().Play("CombatWord");
    }



    // Use this for initialization
    void Start()
    {
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIBattle);
        CombatWordBlues = new string[5] { "Combat_Combo_Blueword1", "Combat_Combo_Blueword2", "Combat_Combo_Blueword3", "Combat_Combo_Blueword4", "Combat_Combo_Blueword5" };
        CombatWordGreens = new string[2] { "Combat_Combo_Greenword1", "Combat_Combo_Greenword2" };
        CombatWordOranges = new string[2] { "Combat_Combo_Orangeword1", "Combat_Combo_Orangeword2" };
    }

    void OnDestroy()
    {
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIBattle);
    }
}
