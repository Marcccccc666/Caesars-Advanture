using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletAttack : MonoBehaviour
{
    /// <summary>
    /// 子弹伤害值，默认为10，可以通过SetBulletDamage方法进行设置
    /// </summary>
    private int bulletDamage = 10;

    /// <summary>
    /// 当前穿透的敌人数量，默认为0，可以通过SetBulletPenetration方法进行设置
    /// </summary>
    private int bulletPenetration = 0;

    [SerializeField] private Rigidbody2D RG2D;

    private GameManager GameManager => GameManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private EnemyManager enemyManager => EnemyManager.Instance;

    private void Awake()
    {
        if (RG2D == null)
        {
            OnValidate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            EnemyData enemyData = enemyManager.GetEnemyDataDict[enemyID];
            enemyData.CurrentHealth -= bulletDamage;

            if(enemyData.CurrentHealth <= 0)
            {
                enemyManager.RemoveEnemyData(enemyID);

            }

            // 如果子弹穿透值大于0，则继续穿透下一个敌人
            if (bulletPenetration > 0)
            {
                bulletPenetration--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if(collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置子弹伤害值
    /// </summary>
    /// <param name="damage">子弹伤害值</param>
    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }

    /// <summary>
    /// 设置子弹穿透值
    /// </summary>
    /// <param name="penetration">子弹穿透值</param>
    public void SetBulletPenetration(int penetration)
    {
        bulletPenetration = penetration;
    }

    public Rigidbody2D GetRG2D => RG2D;

#region UNITY_EDITOR
    private void OnValidate()
    {
        if (RG2D == null)
        {
            RG2D = GetComponent<Rigidbody2D>();
            if (RG2D == null)
            {
                Debug.LogError("BulletAttack脚本未找到Rigidbody2D组件，请检查！");
            }
        }
    }
#endregion
}
