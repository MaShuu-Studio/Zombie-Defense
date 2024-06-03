using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeUI : MonoBehaviour
{
    [SerializeField] private RectTransform buildingIconScrollRectTransform;
    [SerializeField] private BuildModeItemIcon buildingIconPrefab;
    private List<BuildModeItemIcon> buildingIcons = new List<BuildModeItemIcon>();

    [Space]
    [SerializeField] private BuildModeItemIcon[] companionIcons;

    private void Awake()
    {
        buildingIconPrefab.gameObject.SetActive(false);
    }

    public void Init()
    {
        buildingIcons.ForEach(buildingIcon => Destroy(buildingIcon.gameObject));
        buildingIcons.Clear();

        int count = 0;
        foreach (var building in BuildingManager.Buildings)
        {
            var buildingIcon = Instantiate(buildingIconPrefab, buildingIconScrollRectTransform);
            buildingIcon.Init(building.key);
            buildingIcon.gameObject.SetActive(true);
            buildingIcons.Add(buildingIcon);
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
        BuildingController.Instance.MoveBuildingPointer(pos);

        if (selectedCompanionIndex != -1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                companionPatrolStartPos = pos;
            }

            if (Input.GetMouseButtonUp(0) && !UIController.PointOverUI())
            {
                companionPatrolEndPos = pos;
                CompanionController.Instance.SetCompanionPatrol(selectedCompanionIndex, new List<Vector2>() { companionPatrolStartPos, companionPatrolEndPos });
                selectedCompanionIndex = -1;
                BuildingController.Instance.SelectBuildingOnBuildMode("");
            }
        }
        // ��Ʈ�� ���� �߿��� �ͷ� ���尡 �� �ǵ���
        else
        {
            // �ͷ� ���� �� ����
            if (Input.GetMouseButton(0) && !UIController.PointOverUI())
            {
                BuildingController.Instance.Build(pos);
            }

            // �ͷ� ����
            if (Input.GetMouseButton(1))
            {
                BuildingController.Instance.Store(pos);
            }

            // ����Ʈ
            if (Input.GetKeyDown(KeyCode.Q) && BuildingController.Instance.SelectBuilding(pos))
            {
                // ����Ʈ�� �ٷ� �Ѿ�� �� �ƴ� Floating Dropdown�� ���.
                UIController.Instance.ShowMountWeaponUI(true, pos);
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
                CompanionController.Instance.SetCompanionPatrol(selectedCompanionIndex, new List<Vector2>() { companionPatrolStartPos, companionPatrolEndPos });
                BuildingController.Instance.SelectBuildingOnBuildMode(CompanionController.Instance.Companions[selectedCompanionIndex].Key);
                // ���� ������ �̹��� �ٲ�
                // ���� ��Ʈ�ѷ����� �����ؾ���
                break;
            }
        }
    }

    public void UpdateCompanions()
    {
        foreach (var icon in companionIcons) icon.gameObject.SetActive(false);

        for (int i = 0; i < CompanionController.Instance.Companions.Count; i++)
        {
            var data = CompanionController.Instance.Companions[i];
            companionIcons[i].gameObject.SetActive(true);
            companionIcons[i].Init(data.Key);
        }
    }
}
