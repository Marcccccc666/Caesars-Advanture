#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class TransferScenes : MonoBehaviour
{
    [SerializeField, ChineseLabel("要传送到的场景名称"), Readonly] private string sceneName;

    private GameManager gameManager => GameManager.Instance;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TransferScene(true);
        }
    }

    public void TransferScene(bool needReset)
    {
        gameManager.ChangeScene(sceneName, needReset);
    }

#if UNITY_EDITOR
    [SerializeField, ChineseLabel("要传送到的场景")] private SceneAsset scene;

    private void OnValidate()
    {
        if (scene != null)
        {
            sceneName = scene.name;
        }
    }
#endif
}
