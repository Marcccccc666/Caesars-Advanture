using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

public class TransferScenes : MonoBehaviour
{
    [SerializeField, ChineseLabel("要传送到的场景名称"), Readonly] private string sceneName;

    [SerializeField, ChineseLabel("玩家子弹")] private BulletAttack playerBulletPrefab;

    private PoolManager poolManager => PoolManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TransferScene();
        }
    }

    private void TransferScene()
    {
        if(playerBulletPrefab)
        {
            poolManager.ReleasePool(playerBulletPrefab);
        }
        gameManager.ChangeScene(sceneName);
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
