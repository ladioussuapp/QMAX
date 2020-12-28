using Com4Love.Qmax;
using UnityEngine;

public class TestPerf : MonoBehaviour
{
    private int logUpdateNum = 10;

    void Awake()
    {
        Q.Log(LogTag.TestPerf, "TestPerf::Awake() {0}", Q.ElapsedSeconds(GameController.Instance.TestPrefID));
    }

    // Use this for initialization
    void Start()
    {
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "TestPerf::Start()");
    }

    // Update is called once per frame
    void Update()
    {
        if (logUpdateNum <= 0)
            return;
        logUpdateNum--;
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "TestPerf::Update()");
    }
}
