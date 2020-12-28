using UnityEngine;
using UnityEngine.UI;

public class TestCoroutine : MonoBehaviour
{
    public Animator ElimAnim;

    public Button ConfirmSkipRateButton;

    public InputField inputField;

    // Use this for initialization
    void Start()
    {
        ConfirmSkipRateButton.onClick.AddListener(delegate() 
        {
            int sr = int.Parse(inputField.text);
            SkeletonAnimator.DefaultSkipRate = sr;
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
