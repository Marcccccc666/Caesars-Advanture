using UnityEngine;

public static class AnimatorTool
{
    /// <summary>
    /// 检测动画是否播完
    /// </summary>
    public static bool IsAnimationFinished(Animator animator, int animationHashName, int layerIndex = 0)
    {
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == animationHashName)
        {
            return animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime >= 0.98f;
        }
        return false;
    }

    /// <summary>
    /// 检测动画是否正在特定的动画状态中
    /// </summary>
    public static bool IsInAnimationState(Animator animator, int animationHashName, int layerIndex = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == animationHashName;
    }

    /// <summary>
    /// 获取动画长度
    /// </summary>
    /// <param name="animator">动画组件</param>
    /// <param name="animationHashName">动画状态的Hash值</param>
    public static float GetAnimationLength(Animator animator, int animationHashName, int layerIndex = 0)
    {
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == animationHashName)
        {
            return animator.GetCurrentAnimatorStateInfo(layerIndex).length;
        }
        return 0f;
    }
}
