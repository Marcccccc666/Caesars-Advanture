using UnityEngine;

public class Idle : CharacterState<Caesar_Controller.Caesar_StateID>
{
    public Idle(): base()
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入待机状态");
    }
}
