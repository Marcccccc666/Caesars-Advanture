using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NextLevelController : InteractionObjectController
{
    [SerializeField, ChineseLabel("下一关场景名称"), Readonly] private string nextLevelSceneName;
    private GameManager gameManager => GameManager.Instance;

    protected override void Interact()
    {
        gameManager.ChangeScene(nextLevelSceneName);
    }

    #if UNITY_EDITOR
    [SerializeField, ChineseLabel("下一个场景")] private SceneAsset nextLevelSceneAsset;
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    private void OnValidate()
    {
        if (nextLevelSceneAsset != null)
        {
            nextLevelSceneName = nextLevelSceneAsset.name;
        }
    }
    #endif
}
