using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffNode : BTActionNode
{
    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {

    }

    protected override State OnUpdate()
    {
        if (context.buffObject == null) return State.Failure;

        // ���� �غ� ���°� �ƴϰ� Ž���� �����ߴٸ� ���� ����.
        if (!context.buffObject.WaitBuff && context.buffObject.DetectBuffTarget())
        {
            context.buffObject.GiveBuff();
            return State.Success;
        }
        return State.Failure;
    }
}
