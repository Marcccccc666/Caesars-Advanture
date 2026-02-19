using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckoutPage : MonoBehaviour
{
    [SerializeField, ChineseLabel("成功页面")] private GameObject successPanel;
    [SerializeField, ChineseLabel("失败页面")] private GameObject gameOverPanel;
    [SerializeField, ChineseLabel("下一关页面")] private GameObject nextLevelPanel;

    private EnemyManager enemyManager => EnemyManager.Instance;

    private CharacterManager characterManager => CharacterManager.Instance;

    private CharacterDate subscribedCharacter;
   

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
        characterManager.OnCurrentPlayerCharacterDataChanged += OnCharacterChanged;

        if (characterManager.GetCurrentPlayerCharacterData != null)
        {
            OnCharacterChanged(characterManager.GetCurrentPlayerCharacterData);
        }
    }

    private void OnDisable()
    {
        if(characterManager)
        {
            characterManager.OnCurrentPlayerCharacterDataChanged -= OnCharacterChanged;
        }
        if(subscribedCharacter)
        {
            subscribedCharacter.OnDie -= ShowGameOverPage;
        }
    }

    /// <summary>
    /// 当前玩家控制的角色数据发生变化时调用
    /// <para>取消订阅旧角色的死亡事件，订阅新角色的死亡事件</para>
    /// <para>如果新角色不为null，则订阅其死亡事件以显示游戏结束页面</para>
    /// <para>如果新角色为null，则不订阅任何事件</para>
    /// </summary>
    /// <param name="newCharacter"></param>
    private void OnCharacterChanged(CharacterDate newCharacter)
    {
        if (subscribedCharacter != null)
        {
            subscribedCharacter.OnDie -= ShowGameOverPage;
        }
        subscribedCharacter = newCharacter;
        if (subscribedCharacter != null)
        {
            subscribedCharacter.OnDie += ShowGameOverPage;
        }
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
    }

    

    private void ResetSingletons()
    {
        DestroyIfExists<GameManager>();
        DestroyIfExists<LevelManager>();
        DestroyIfExists<EnemyManager>();
        DestroyIfExists<CharacterManager>();
        DestroyIfExists<BuffManager>();
        DestroyIfExists<WeaponManager>();
        DestroyIfExists<InputManager>();
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
