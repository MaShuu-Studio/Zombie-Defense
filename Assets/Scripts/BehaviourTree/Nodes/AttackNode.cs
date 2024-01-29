using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackNode : BTActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.attackObject == null) return State.Failure;

        // Ž���� �����ߴٸ� ���ݵ����� ���ȿ��� running
        // �����̰� �ƴ϶�� ���� ���� �� ���� ����
        if (context.attackObject.DetectTarget())
        {
            if (context.attackObject.WaitAttack) return State.Running;

            context.attackObject.Attack();
            return State.Success;
        }
        return State.Failure;
    }
}
