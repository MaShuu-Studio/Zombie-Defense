using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    public IBTNode rootNode;

    public BehaviourTree(IBTNode root)
    {
        rootNode = root;
    }

    public void Operate()
    {
        rootNode.Evaluate();
    }
}

public interface IBTNode
{
    public enum NodeState
    {
        Running,
        Success,
        Failure,
    }

    public NodeState Evaluate();
}

public sealed class ActionNode : IBTNode
{
    Func<IBTNode.NodeState> onUpdate = null;

    public ActionNode(Func<IBTNode.NodeState> onUpdate)
    {
        this.onUpdate = onUpdate;
    }
    // ?? �����ڴ� null�� ��� �������� ��ȯ��.
    // ?. ������ ���� ��������. null�� �ƴ� ��쿡 Invoke�� ȣ��. �װ� �ƴ϶�� null�� ������.
    // onUpdate�� null�̶�� ?? ������ null�̹Ƿ� Failure �ƴ϶�� Invoke��.
    // onUpdate�� ���ٸ� ������ Failure���� Invoke�ϰ� �Ǹ� ���°� �ٲ��
    public IBTNode.NodeState Evaluate() => onUpdate?.Invoke() ?? IBTNode.NodeState.Failure;
}

public sealed class SelectorNode : IBTNode
{
    List<IBTNode> actions;

    public SelectorNode(List<IBTNode> actions)
    {
        this.actions = actions;
    }

    public IBTNode.NodeState Evaluate()
    {
        if (actions == null) return IBTNode.NodeState.Failure;

        foreach (var action in actions)
        {
            // Selector�� ��� ������ �� �ִٸ� �ٸ� ���� üũ���� �ʱ� ������
            // Failure�� �ƴ϶�� return�ص� ������.
            IBTNode.NodeState result = action.Evaluate();
            if (result != IBTNode.NodeState.Failure) return result;
        }

        return IBTNode.NodeState.Failure;
    }
}

public sealed class SequenceNode : IBTNode
{
    List<IBTNode> actions;

    public SequenceNode(List<IBTNode> actions)
    {
        this.actions = actions;
    }

    // Sequence�� ��쿡�� Success�� �� �� ���� Running�� ��� �����ϰ� �־�� ��.
    // 
    public IBTNode.NodeState Evaluate()
    {
        if (actions == null) return IBTNode.NodeState.Failure;

        foreach (var action in actions)
        {
            IBTNode.NodeState result = action.Evaluate();
            switch (result)
            {
                case IBTNode.NodeState.Running: return IBTNode.NodeState.Running;
                case IBTNode.NodeState.Success: continue;
                case IBTNode.NodeState.Failure: return IBTNode.NodeState.Failure;
            }
        }

        return IBTNode.NodeState.Failure;
    }
}