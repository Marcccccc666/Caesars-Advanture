using UnityEngine;
using UnityEngine.SceneManagement;
public class CheckoutPage : MonoBehaviour
{
    [SerializeField, ChineseLabel("成功页面")] private GameObject successPanel;
    [SerializeField, ChineseLabel("失败页面")] private GameObject gameOverPanel;
    [SerializeField, ChineseLabel("下一关页面")] private GameObject nextLevelPanel;
    [SerializeField, ChineseLabel("结束按钮")] private GameObject restartButton;
    private GameManager gameManager => GameManager.Instance;

    private EnemyManager enemyManager => EnemyManager.Instance;

    private CharacterManager characterManager => CharacterManager.Instance;

    private LevelManager levelManager => LevelManager.Instance;

    void Awake()
    {
        gameManager.GameCheckout += checkPageOpen;
    }

    public void RestartGame()
    {
        gameManager.IsGameOver = false;
        enemyManager.ClearEnemyData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void checkPageOpen()
    {
        
        if(CanShowSuccessPage())
        {
            restartButton.SetActive(true);
        }
        else if(CanNextLevel())
        {
            nextLevelPanel.SetActive(true);
        }
        else if(characterManager.GetCurrentPlayerCharacterData.CurrentHealth <= 0)
        {
            restartButton.SetActive(true);
        }
    }

    bool CanNextLevel()
    {
        return characterManager.GetCurrentPlayerCharacterData.CurrentHealth > 0 && enemyManager.GetEnemyDataDict.Count == 0 && !levelManager.IsLastLevel();
    }

    bool CanShowSuccessPage()
    {
        return characterManager.GetCurrentPlayerCharacterData.CurrentHealth > 0 && enemyManager.GetEnemyDataDict.Count == 0 && levelManager.IsLastLevel();
    }
}
