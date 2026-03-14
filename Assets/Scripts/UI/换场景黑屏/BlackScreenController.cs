using UnityEngine;

public class BlackScreenController : MonoBehaviour
{
    [SerializeField, ChineseLabel("黑屏UI")] private GameObject blackScreenUI;
    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        if(blackScreenUI)
        {
            blackScreenUI.SetActive(false);
        }
        gameManager.SetBlackScreen(blackScreenUI);
    }
}
