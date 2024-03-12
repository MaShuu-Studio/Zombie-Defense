using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeUI : MonoBehaviour
{
    [SerializeField] private RectTransform turretIconScrollRectTransform;
    [SerializeField] private BuildModeItemIcon turretIconPrefab;
    private List<BuildModeItemIcon> turretIcons = new List<BuildModeItemIcon>();

    [Space]
    [SerializeField] private BuildModeItemIcon[] companionIcons;

    private void Awake()
    {
        turretIconPrefab.gameObject.SetActive(false);
    }

    public void Init()
    {
        turretIcons.ForEach(turretIcon => Destroy(turretIcon.gameObject));
        turretIcons.Clear();

        int count = 0;
        foreach (var turret in TurretManager.Turrets)
        {
            var turretIcon = Instantiate(turretIconPrefab, turretIconScrollRectTransform);
            turretIcon.Init(turret.key);
            turretIcon.gameObject.SetActive(true);
            turretIcons.Add(turretIcon);
            count++;
        }
    }

    public void BuildMode(bool b)
    {
        gameObject.SetActive(b);
        if (b)
        {
            selectedCompanionIndex = -1;
            UpdateCompanions();
        }
    }

    private int selectedCompanionIndex;
    private Vector2 companionPatrolStartPos;
    private Vector2 companionPatrolEndPos;

    private void Update()
    {
        if (GameController.Instance.GameStarted == false
            || GameController.Instance.Pause) return;

        float axisX = Input.GetAxis("Horizontal");
        float axisY = Input.GetAxis("Vertical");
        Vector3 movePos = CameraController.Instance.Cam.transform.position + new Vector3(axisX, axisY) * Time.deltaTime * 10;
        CameraController.Instance.MoveCamera(movePos, movePos);


        Vector3 mousePos = CameraController.Instance.Cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = MapGenerator.PosToGrid(MapGenerator.RoundToInt(mousePos));
        TurretController.Instance.MoveTurretPointer(pos);

        // �ͷ� ���� �� ����
        if (Input.GetMouseButton(0) && !UIController.PointOverUI())
        {
            TurretController.Instance.BuildTurret(pos);
        }

        // �ͷ� ����
        if (Input.GetMouseButton(1))
        {
            TurretController.Instance.StoreTurret(pos);
        }

        // ����Ʈ
        if (Input.GetKeyDown(KeyCode.Q) && TurretController.Instance.SelectTurret(pos))
        {
            // ����Ʈ�� �ٷ� �Ѿ�� �� �ƴ� Floating Dropdown�� ���.
            UIController.Instance.ShowMountWeaponUI(true, pos);
        }

        // �ͷ��̶� ��ġ�� �ʵ��� �ؾ���.
        if (selectedCompanionIndex != -1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                companionPatrolStartPos = pos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                companionPatrolEndPos = pos;
                CompanionController.Instance.SetCompanionPatrol(selectedCompanionIndex, new List<Vector2>() { companionPatrolStartPos, companionPatrolEndPos });
            }
        }
    }

    public void SelectCompanion(BuildModeItemIcon icon)
    {
        selectedCompanionIndex = -1;
        for (int i = 0; i < companionIcons.Length; i++)
        {
            if (companionIcons[i] == icon)
            {
                selectedCompanionIndex = i;
                break;
            }
        }
    }

    private void UpdateCompanions()
    {
        foreach (var icon in companionIcons) icon.gameObject.SetActive(false);

        for (int i = 0; i < CompanionController.Instance.Companions.Count; i++)
        {
            var data = CompanionController.Instance.Companions[i];
            companionIcons[i].gameObject.SetActive(true);
            companionIcons[i].Init("COMPANION.COMPANION");
        }
    }
}
