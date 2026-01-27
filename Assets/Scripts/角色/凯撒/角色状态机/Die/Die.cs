using UnityEngine;

public class Die : CharacterState<Caesar_Controller.Caesar_StateID>
{
    public Die() : base()
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入死亡状态");
    }
}
