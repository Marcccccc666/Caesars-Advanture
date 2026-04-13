#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class TransferScenes : MonoBehaviour
{
    [SerializeField, ChineseLabel("要传送到的场景名称"), Readonly] private string sceneName;

    [SerializeField, ChineseLabel("提示拿武器")] private GameObject hintObject;
    [SerializeField, ChineseLabel("是否需要重置游戏")] private bool needReset = false;

    private GameManager gameManager => GameManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (hintObject != null)
        {
            hintObject?.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(weaponManager.GetCurrentWeapon == null)
            {
                StartCoroutine(ShowHintTemporarily());
                return;
            }
            TransferScene(needReset);
        }
    }

    public void TransferScene(bool needReset)
    {
        Dictionary<int, EnemyData> dict = EnemyManager.Instance.GetEnemyDataDict;
        dict.Clear();
        gameManager.ChangeScene(sceneName, needReset);
    }

    private IEnumerator ShowHintTemporarily()
    {
        if (hintObject != null)
        {
            hintObject?.SetActive(true);
            yield return new WaitForSeconds(2f); // 显示提示2秒
            hintObject?.SetActive(false);
        }
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
