using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckoutPage : MonoBehaviour
{
    [SerializeField, ChineseLabel("成功页面")] private GameObject successPanel;
    [SerializeField, ChineseLabel("失败页面")] private GameObject gameOverPanel;
    [SerializeField, ChineseLabel("下一关页面")] private GameObject nextLevelPanel;

    private EnemyManager enemyManager => EnemyManager.Instance;

    private CharacterManager characterManager => CharacterManager.Instance;

    private LevelManager levelManager => LevelManager.Instance;
   

    void Awake()
    {
        EnsureCanvasScale();
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
    }

    private void OnEnable()
    {
        characterManager.GetCurrentPlayerCharacterData.OnDie += ShowGameOverPage;
    }

    private void OnDisable()
    {
        characterManager.GetCurrentPlayerCharacterData.OnDie -= ShowGameOverPage;
    }

    public void ShowSuccessPage()
    {
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
    }

    public void ShowGameOverPage()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }


    public void RestartGame()
    {
        enemyManager.ClearEnemyData();
        ResetSingletons();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void checkPageOpen()
    {
        if (characterManager.GetCurrentPlayerCharacterData == null)
        {
            return;
        }

        if (characterManager.GetCurrentPlayerCharacterData.CurrentHealth <= 0)
        {
          //  ShowGameOverPage();
            return;
        }

        if (CanShowSuccessPage())
        {
          //  ShowSuccessPage();
            return;
        }
        if (CanNextLevel())
        {
           // ShowBuffSelection();
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

    private void ResetSingletons()
    {
        DestroyIfExists<GameManager>();
        DestroyIfExists<LevelManager>();
        DestroyIfExists<EnemyManager>();
        DestroyIfExists<CharacterManager>();
        DestroyIfExists<BuffManager>();
        DestroyIfExists<WeaponManager>();
        DestroyIfExists<InputData>();
        DestroyIfExists<MultiTimerManager>();
    }

    private void DestroyIfExists<T>() where T : MonoBehaviour
    {
        T instance = FindAnyObjectByType<T>();
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
    }

    private void EnsureCanvasScale()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        Vector3 scale = rect.localScale;
        if (Mathf.Abs(scale.x) < 0.0001f || Mathf.Abs(scale.y) < 0.0001f)
        {
            rect.localScale = Vector3.one;
        }
    }
}
