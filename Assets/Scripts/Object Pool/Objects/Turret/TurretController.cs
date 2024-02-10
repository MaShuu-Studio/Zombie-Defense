using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public static TurretController Instance { get { return instance; } }
    private static TurretController instance;

    private static Vector2[] directions;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        directions = new Vector2[]
        {
            new Vector2(-1,1),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(-1,0),
            new Vector2(1,-1),
            new Vector2(0,-1),
            new Vector2(-1,-1),
        };
    }

    [SerializeField] private Transform turretPointer;

    private Dictionary<string, int> turretAmounts = new Dictionary<string, int>();
    private Dictionary<Vector2, TurretObject> turrets = new Dictionary<Vector2, TurretObject>();

    public void StartGame()
    {
        foreach (var turret in turrets.Values)
        {
            turret.DestroyTurret();
        }
        turrets.Clear();

        turretAmounts.Clear();
        foreach (var turret in TurretManager.Turrets)
        {
            turretAmounts.Add(turret.key, 0);
        }
    }

    void Update()
    {
        if (GameController.Instance.GameStarted == false || GameController.Instance.Pause) return;

        turretPointer.position = GetDirection(Player.Instance.transform.position, CameraController.Instance.Cam.ScreenToWorldPoint(Input.mousePosition));

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildTurret(turretPointer.position, "TURRET.BARRICADE");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuildTurret(turretPointer.position, "TURRET.TURRET");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BuildTurret(turretPointer.position, "TURRET.SCANTOWER");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (turrets.ContainsKey(turretPointer.position))
            {
                var turret = (AttackTurretObject)turrets[turretPointer.position];
                if (turret)
                {
                    turret.Mount(WeaponController.Instance.CurWeapon);
                }
            }
        }
    }

    public void AddTurret(string key)
    {
        if (turretAmounts.ContainsKey(key) == false) return;
        turretAmounts[key]++;
        UIController.Instance.UpdateTurretAmount(key, turretAmounts[key]);
    }

    public void BuildTurret(Vector2 pos, string name)
    {
        Vector2Int mapPos = MapGenerator.ConvertToMapPos(MapGenerator.RoundToInt(pos));

        Turret data = TurretManager.GetTurret(name);
        if (data == null
            || turretAmounts.ContainsKey(data.key) == false || turretAmounts[data.key] <= 0
            || turrets.ContainsKey(pos)) return;
        if (MapGenerator.Instance.Map[mapPos.x, mapPos.y] == MapGenerator.WALL) return;

        Poolable obj = PoolController.Pop(name);
        TurretObject turret = obj.GetComponent<TurretObject>();
        if (turret == null)
        {
            PoolController.Push(name, obj);
            return;
        }
        turretAmounts[data.key]--;
        UIController.Instance.UpdateTurretAmount(data.key, turretAmounts[data.key]);

        turret.SetData(data, pos);
        turret.gameObject.name = name;
        turret.transform.parent = transform;
        turret.transform.position = pos;
        turrets.Add(pos, turret);
    }

    public void RemoveTurret(Vector2 pos)
    {
        if (turrets.ContainsKey(pos)) turrets.Remove(pos);
    }

    public static Vector2 GetDirection(Vector2 charPos, Vector3 mouseWorldPos)
    {
        /* [-1, 1] [0, 1] [1, 1]
         * [-1, 0] [0, 0] [1, 0]
         * [-1,-1] [0,-1] [1,-1]
         */

        /* Ÿ���� grid.cellSize��ŭ �������� ����
         * ���� worldPos���� ������ ���� �簢���� ���� �Ʒ� �������� ����.
         * ���� cellSize�� ���ݸ�ŭ ��ġ�� �������ָ� Ÿ���� ��ġ�� ��.
         */

        /* ĳ���͸� �������� Ÿ���� ���� �� �ִ� ������ ���� ��ġ�� ������ 8���� �� ĭ���� �ϰ� ����.
         * �켱, ĳ���͸� �������� �ֺ� Ÿ���� üũ��.
         * �� �� ���� ���콺�� ��ġ�� �Ÿ��� ���ؼ� �ش� ��ġ�� ������ ����
         */
        charPos = MapGenerator.RoundToInt(charPos);

        Vector2 buildPos = PosToGrid(charPos + directions[0]);
        float minDistance = Vector2.Distance(buildPos, mouseWorldPos);
        for (int i = 1; i < directions.Length; i++)
        {
            Vector2 cmpPos = PosToGrid(charPos + directions[i]);
            float distance = Vector2.Distance(cmpPos, mouseWorldPos);

            if (minDistance > distance)
            {
                minDistance = distance;
                buildPos = cmpPos;
            }
        }

        return buildPos;
        /*
        Vector3 dir = mousePos - charPos;
        float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (deg > 180 - 22.5f) return directions[7];
        else if (deg > 180 - 22.5f * 3) return directions[0];
        else if (deg > 180 - 22.5f * 5) return directions[1];
        else if (deg > 180 - 22.5f * 7) return directions[2];
        else if (deg > 180 - 22.5f * 9) return directions[3];
        else if (deg > 180 - 22.5f * 11) return directions[4];
        else if (deg > 180 - 22.5f * 13) return directions[5];
        else if (deg > 180 - 22.5f * 15) return directions[6];
        else return directions[7];*/
    }

    private static Vector2 PosToGrid(Vector2 pos)
    {
        float x = Mathf.FloorToInt(pos.x);
        float y = Mathf.FloorToInt(pos.y);
        return new Vector2(x, y);
    }
}
