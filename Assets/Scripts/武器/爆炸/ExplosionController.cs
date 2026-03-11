using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ExplosionController : MonoBehaviour, IPoolable
{
    #region 爆炸设置
    [SerializeField, ChineseLabel("爆炸动画")] private Animator explosionAnimator;
    [SerializeField, ChineseLabel("爆炸动画名")] private string explosionAnimationName = "Explosion";

    [SerializeField, ChineseLabel("碰撞箱")] private Collider2D explosionCollider;
    private int explosionAnimationHash = 0;

    /// <summary>
    /// 爆炸伤害值，默认为20
    /// </summary>
    private int explosionDamage = 2;

    private EnemyManager enemyManager => EnemyManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        explosionAnimationHash = Animator.StringToHash(explosionAnimationName);
    }

    void OnEnable()
    {
        explosionCollider.enabled = true;
        explosionAnimator.Play(explosionAnimationHash);
        StartCoroutine(WaitForAnimationEnd());
    }

    void OnDisable()
    {
        StopCoroutine(WaitForAnimationEnd());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            Dictionary<int, EnemyData> enemyDataDict = enemyManager.GetEnemyDataDict;

            if(!enemyDataDict.TryGetValue(enemyID, out EnemyData enemyData))
            {
                Debug.LogError("未找到敌人数据");
                return;
            }
            enemyData.Damage(explosionDamage);
        }
    }

    public void Initialize(int damage)
    {
        explosionDamage = damage;
    }

    private IEnumerator WaitForAnimationEnd()
    {
        float animationLength = AnimatorTool.GetAnimationLength(explosionAnimator, explosionAnimationHash);
        // 等待动画结束
        yield return new WaitForSeconds(animationLength);

        gameObject.SetActive(false);
        //Release();
    }



    #endregion
    #region 对象池设置
    private IMyPool pool;

	public void SetPool(IMyPool pool)
	{
		this.pool = pool;
	}

	public void Release()
	{
		pool?.Release(this);
	}
    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (explosionAnimator == null)
        {
            explosionAnimator = GetComponent<Animator>();
        }

        if (explosionCollider == null)
        {
            explosionCollider = GetComponent<Collider2D>();
        }
    }
#endif
}
