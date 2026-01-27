using UnityEngine;

public class Move : CharacterState<Caesar_Controller.Caesar_StateID>
{

    private Animator M_animator;
    private int M_MoveAnimaeHash;

    public Move(Animator animator, int MoveAnimaeHash) : base()
    {
        M_animator = animator;
        M_MoveAnimaeHash = MoveAnimaeHash;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入移动状态");
        M_animator.Play(M_MoveAnimaeHash);
    }

    public override void OnLogic()
    {
        base.OnLogic();
    }

}
