using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNode : BTActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.movingObject == null) return State.Failure;

        // Ž���� �����ߴٸ� ���ݵ����� ���ȿ��� running
        // �����̰� �ƴ϶�� ���� ���� �� ���� ����
        if (context.movingObject.DetectPath())
        {
            context.movingObject.AdjustMove(true);
            return State.Success;
        }
        return State.Failure;
    }
}
