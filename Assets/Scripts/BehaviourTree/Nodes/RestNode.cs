using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestNode : BTActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        // �����ߴٸ� �޽� ����.
        if (context.restObject.IsRunningAway && context.restObject.IsArrived)
        {
            context.restObject.Rest();
            return State.Success;
        }

        if (context.restObject.IsRunningAway
            || context.restObject.IsHealed) return State.Running;

        // ü���� üũ�ؼ� Ư�� ��ġ ���ϰ� �Ǹ� ����ħ
        if (context.restObject.CheckHPState)
        {
            context.restObject.Runaway();
            return State.Success;
        }
        return State.Failure;
    }
}
