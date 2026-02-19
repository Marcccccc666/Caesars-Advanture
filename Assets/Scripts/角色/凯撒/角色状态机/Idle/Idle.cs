using UnityEngine;

public class Idle : CharacterState<Caesar_Controller.Caesar_StateID>
{
    private Animator animator;
    private int idleAnimateNameHash;

    public Idle(Animator animator, string idleAnimateName): base()
    {
        this.animator = animator;
        idleAnimateNameHash = Animator.StringToHash(idleAnimateName);
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // 设置待机动画
        animator.Play(idleAnimateNameHash);
    }
}
