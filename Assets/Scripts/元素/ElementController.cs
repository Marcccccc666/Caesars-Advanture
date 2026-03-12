using System.Threading;
using UnityEngine;

public class ElementController : MonoBehaviour
{
    #region 火元素
    [SerializeField, ChineseLabel("火元素等级")] private int Fire_Level = 0;
    private Coroutine FireDamageCoroutine;

    #endregion

    #region 冰元素
    [SerializeField, ChineseLabel("冰元素等级")] private int Ice_Level = 0;
    #endregion

    #region 雷元素
    [SerializeField, ChineseLabel("雷元素等级")] private int Thunder_Level = 0;
    #endregion


}
