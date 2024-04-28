using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    [SerializeField] private EnemyObject enemy;
    int layerMask;
    public void Init(Enemy data)
    {
        layerMask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Turret");
        // ���Ÿ� ������ ��� Trap�� ������ �� ����.
        if (data.range >= 3f) layerMask |= 1 << LayerMask.NameToLayer("Trap");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((1 << collision.gameObject.layer & layerMask) > 0)
        {
            enemy.Damaging(collision.gameObject);
        }
    }
}
