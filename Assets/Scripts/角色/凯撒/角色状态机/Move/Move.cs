using System;
using UnityEngine;

public class Move : CharacterState<Caesar_Controller.Caesar_StateID>
{

    private Animator M_animator;
    private int M_MoveAnimaeHash;

    public Move(Animator animator, string MoveAnimae) : base()
    {
        M_animator = animator;
        M_MoveAnimaeHash = Animator.StringToHash(MoveAnimae);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        M_animator.Play(M_MoveAnimaeHash);
    }

    public override void OnLogic()
    {
        base.OnLogic();
    }

}
