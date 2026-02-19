using UnityEngine;

/// <summary>
/// 玩家进入房间
/// </summary>
public class RoomEnter : MonoBehaviour
{
    [SerializeField, ChineseLabel("房间控制器")] private RoomBase roomController;
    [SerializeField, ChineseLabel("进入时直接弹结算成功界面")] private bool showCheckoutOnEnter = false;
    
    private void Awake()
    {
        if(roomController == null)
        {
            roomController = GetComponentInParent<RoomBase>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
        {
            return;
        }

        if (showCheckoutOnEnter)
        {
            ShowCheckoutNextLevelPage();
            return;
        }

        roomController.PlayerEnterRoom();
    }

    private void ShowCheckoutNextLevelPage()
    {
        CheckoutPage checkoutPage = FindAnyObjectByType<CheckoutPage>();
        if (checkoutPage == null)
        {
            Debug.LogWarning("未找到 CheckoutPage，无法打开下一关结算界面。", this);
            return;
        }

        checkoutPage.ShowNextLevelPage();
    }

#region UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    private void OnValidate()
    {
        if(roomController == null)
        {
            roomController = GetComponentInParent<RoomBase>();

            if(roomController == null)
            {
                Debug.LogError("RoomEnter脚本未找到RoomBase组件，请检查！");
            }
        }
    }
#endregion

}
