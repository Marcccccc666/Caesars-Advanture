using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    [SerializeField, ChineseLabel("敌人数据")]private EnemyData enemyData;
    private GameManager gameManager => GameManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            characterManager.GetCurrentPlayerCharacterData.CurrentHealth -= enemyData.EnemyBaseData.baseAttack;

            int playerHP = characterManager.GetCurrentPlayerCharacterData.CurrentHealth;
            if (playerHP <= 0)
            {
                gameManager.IsGameOver = true;
            }
        }
    }
}
