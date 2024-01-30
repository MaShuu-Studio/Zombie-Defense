using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    private void Awake()
    {
        SpriteManager.Init();
        EnemyManager.Init();
        WeaponManager.Init();
        TurretManager.Init();
        RoundManager.Init();
    }

    private void Start()
    {
        GameController.Instance.GoTo(SceneController.Scene.TITLE);
    }
}
