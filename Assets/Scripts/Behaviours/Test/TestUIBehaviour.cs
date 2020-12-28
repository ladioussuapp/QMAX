using Com4Love.Qmax;
using UnityEngine;
using UnityEngine.UI;

public class TestUIBehaviour : MonoBehaviour
{
    public UIBattleBehaviour UIBattle;
    public UILoseBehaviour UILose;
    public ImageNumberBehaviour ImageNumberBehaviour;

    public Image TestImage;

    public int Number = 0;


    // Use this for initialization
    void Start()
    {
        
    }

    void Update()
    {
        //if (ImageNumberBehaviour.Number != Number)
        //{
        //    ImageNumberBehaviour.Number = Number;
        //}
    }

    private void OnClickPauseButton(object obj)
    {
        Q.Log("OnClickPauseButton");
    }

    void OnDestroy()
    {

    }

}
