using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get { return instance; } }
    private static MapGenerator instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public const int WALL = 0;
    public const int GRASS = 1;
    public const int PLANT = 2;

    [SerializeField] private AstarPath astar;
    private bool updateCol;

    [Header("TILE")]
    [SerializeField] private Tilemap boundaryTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap wallBottomTilemap;
    [SerializeField] private Tilemap grassTilemap;
    [SerializeField] private Tilemap plantTilemap;
    [SerializeField] private Tilemap buildModeGridTilemap;
    [SerializeField] private RuleTile[] tiles;
    [SerializeField] private RuleTile wallBottomTile;
    [SerializeField] private RuleTile boundaryTile;

    [Header("MapInfo")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private string seed;
    [SerializeField] private bool useSeed;
    [SerializeField] private int smoothing;

    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;

    public Rect MapBoundary { get; private set; }
    public Bounds MapBounds { get { return new Bounds(Vector3.zero, new Vector3(width, height)); } }
    public int[,] Map { get { return map; } }
    private int[,] map;
    private int squareSize = 1;

    public void StartGame()
    {
        GenerateMap();
        BuildMode(false);

        Vector2 bottomLeft = ConvertToWorldPos(0, 0);
        MapBoundary = new Rect(bottomLeft.x, bottomLeft.y, width, height);
    }

    public void BuildMode(bool b)
    {
        buildModeGridTilemap.gameObject.SetActive(b);
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        if (!useSeed) seed = Time.time.ToString();
        RandomFillMap(ref map, seed, randomFillPercent);

        for (int i = 0; i < smoothing; i++)
            SmoothMap(ref map);

        // Ÿ�� ����
        boundaryTilemap.ClearAllTiles();
        grassTilemap.ClearAllTiles();
        plantTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        wallBottomTilemap.ClearAllTiles();

        for (int x = -1; x <= width; x++)
        {
            boundaryTilemap.SetTile((Vector3Int)ConvertToWorldPos(x, -1), boundaryTile);
            boundaryTilemap.SetTile((Vector3Int)ConvertToWorldPos(x, height), boundaryTile);
        }
        for (int y = -1; y <= height; y++)
        {
            boundaryTilemap.SetTile((Vector3Int)ConvertToWorldPos(-1, y), boundaryTile);
            boundaryTilemap.SetTile((Vector3Int)ConvertToWorldPos(width, y), boundaryTile);
        }

        // �̴ϸ� �� ������ �̻��ϰ� ������ �ʵ���
        for (int x = -10; x < width + 10; x++)
        {
            for (int y = -10; y < height + 10; y++)
            {
                Vector3Int pos = (Vector3Int)ConvertToWorldPos(x, y);
                grassTilemap.SetTile(pos, tiles[GRASS]);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = (Vector3Int)ConvertToWorldPos(x, y);
                if (y > 0 && map[x, y] == WALL)
                {
                    wallTilemap.SetTile(pos, tiles[WALL]);
                    if (map[x, y - 1] == GRASS)
                    {
                        // �⺻������ ��ġ�� �ٷ� �Ʒ�
                        pos.y -= 1;
                        wallBottomTilemap.SetTile(pos, wallBottomTile);
                        map[x, y - 1] = WALL;

                        // ���ʿ� ���� �Ʒ��κ��� ������ ������ �ִٸ� ����
                        // ��Ÿ���̱� ������ ��Ȯ�� ����� ���ؼ��� �翷�� ���𰡰� �־�� ��.
                        pos.x -= 1;
                        if (x > 1 && map[x - 1, y - 1] == WALL)
                        {
                            wallBottomTilemap.SetTile(pos, wallBottomTile);
                            map[x - 1, y - 1] = WALL;
                        }
                        pos.x += 2;
                        if (x < width - 1 && map[x + 1, y - 1] == WALL)
                        {
                            wallBottomTilemap.SetTile(pos, wallBottomTile);
                            map[x + 1, y - 1] = WALL;
                        }
                    }
                }
            }
        }
        CreatePlant();

        // ĳ���� ����
        // ���� �߽����� �������� ���ݾ� ���������� ����� Ȱ��.
        {
            Vector2Int center = ConvertToMapPos(Vector2Int.zero);
            Vector2 spawnPoint = Vector2.zero;

            bool[,] visited = new bool[width, height];
            Queue<Vector2Int> q = new Queue<Vector2Int>();
            q.Enqueue(center);
            visited[center.x, center.y] = true;
            bool find = false;

            while (q.Count > 0 && !find)
            {
                int qsize = q.Count;
                for (int i = 0; i < qsize; i++)
                {
                    Vector2Int pos = q.Dequeue();
                    if (map[pos.x, pos.y] == GRASS)
                    {
                        spawnPoint = ConvertToWorldPos(pos);
                        find = true;
                        break;
                    }

                    if (pos.x > 1 && visited[pos.x - 1, pos.y] == false) q.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                    if (pos.x < width - 1 && visited[pos.x + 1, pos.y] == false) q.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                    if (pos.y > 1 && visited[pos.x, pos.y - 1] == false) q.Enqueue(new Vector2Int(pos.x, pos.y - 1));
                    if (pos.y < height - 1 && visited[pos.x, pos.y + 1] == false) q.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                }
            }

            Player.Instance.transform.position = spawnPoint;
        }

        foreach (var graph in astar.graphs)
        {
            ((Pathfinding.GridGraph)graph).SetDimensions((width + 2) * 2, (height + 2) * 2, .5f);
            ((Pathfinding.GridGraph)graph).center = new Vector3(-.5f, 0);
        }
        /*
        ((Pathfinding.GridGraph)astar.graphs[0]).SetDimensions((width + 2) * 2, (height + 2) * 2, .5f);
        ((Pathfinding.GridGraph)astar.graphs[0]).center = new Vector3(-.5f, 0);
        ((Pathfinding.GridGraph)astar.graphs[1]).SetDimensions(width + 2, height + 2, 1);
        ((Pathfinding.GridGraph)astar.graphs[1]).center = new Vector3(-.5f, 0);*/
        astar.graphs[0].Scan();
        StartCoroutine(UpdatingAstar());
    }

    public void CreatePlant()
    {
        // ���� ������ ������ �ʵ��� �õ� ����
        seed = (Time.time * 2).ToString();
        int fill = 92;
        int[,] map = new int[width, height];

        // WALL�� PLANT�� ������ ����.
        RandomFillMap(ref map, seed, fill);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == WALL)
                {
                    Vector3Int pos = (Vector3Int)ConvertToWorldPos(x, y);
                    plantTilemap.SetTile(pos, tiles[PLANT]);
                }
            }
        }
    }

    public Vector2 GetEnemySpawnPos()
    {
        // �ܰ� ������ ����
        // �¿�� ���� �� ��� �������� ���� ���� ��
        // �¿쿡 �����Ѵٸ� y���� ��������, ���Ͽ� �����Ѵٸ� x���� �������� ��.

        int x, y;
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            // �¿쿡 ����
            x = UnityEngine.Random.Range(-1, 1);
            if (x == 0) x = width;
            y = UnityEngine.Random.Range(0, height);
        }
        else
        {
            // ���Ͽ� ����
            y = UnityEngine.Random.Range(-1, 1);
            if (y == 0) y = height;
            x = UnityEngine.Random.Range(0, width);
        }

        return ConvertToWorldPos(x, y);
    }

    IEnumerator UpdatingAstar()
    {
        while (true)
        {
            if (updateCol == false)
            {
                astar.graphs[1].Scan();
                updateCol = true;
                yield return new WaitForSeconds(2);
            }
            yield return null;
        }
    }
    public void UpdateAstar()
    {
        updateCol = false;
    }

    #region Utils
    public static bool ObjectOnBoundary(Vector2 pos)
    {
        Vector2Int mapPos = ConvertToMapPos(RoundToInt(pos));

        return mapPos.x == -1 || mapPos.x == Instance.width || mapPos.y == -1 || mapPos.y == Instance.height;
    }

    public static Vector2Int GetNearestMapBoundary(Vector2Int pos)
    {
        // x�� -1�̶�� 0�� ���� �����
        // x�� width��� width-1�� ���� �����
        // y�� -1�̶�� 0�� ���� �����
        // y�� height��� height-1�� ���� �����

        pos = ConvertToMapPos(pos);

        if (pos.x == -1) pos.x = 0;
        else if (pos.x == Instance.width) pos.x = Instance.width - 1;

        if (pos.y == -1) pos.y = 0;
        else if (pos.y == Instance.height) pos.y = Instance.height - 1;

        pos = ConvertToWorldPos(pos);

        return pos;
    }

    public static bool PosOnWall(Vector2 worldPos)
    {
        Vector2Int mapPos = ConvertToMapPos(RoundToInt(worldPos));
        if (PosOnMap(mapPos) == false) return false;

        return Instance.Map[mapPos.x, mapPos.y] == WALL;
    }

    public static bool PosOnMap(Vector2Int mapPos)
    {
        return mapPos.x >= 0 && mapPos.x < Instance.width
            && mapPos.y >= 0 && mapPos.y < Instance.height;
    }
    public static Vector2Int PosToGrid(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        return new Vector2Int(x, y);
    }

    public static Vector2Int RoundToInt(Vector2 v)
    {
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }

    public static Vector2Int ConvertToWorldPos(int x, int y)
    {
        return new Vector2Int(x - Instance.map.GetLength(0) / 2, y - Instance.map.GetLength(1) / 2) * Instance.squareSize;
    }

    public static Vector2Int ConvertToWorldPos(Vector2Int mapPos)
    {
        return new Vector2Int(mapPos.x - Instance.map.GetLength(0) / 2, mapPos.y - Instance.map.GetLength(1) / 2) * Instance.squareSize;
    }

    public static Vector2Int ConvertToMapPos(Vector2Int worldPos)
    {
        return new Vector2Int(worldPos.x + Instance.map.GetLength(0) / 2, worldPos.y + Instance.map.GetLength(1) / 2) * Instance.squareSize;
    }
    #endregion
    private void RandomFillMap(ref int [,] map, string seed, int randomFillPercent)
    {
        System.Random rand = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // terrain 0: wall, 1: grass
                int terrain = WALL;
                int res = rand.Next();
                if (res % 100 <= randomFillPercent) terrain = GRASS;
                map[x, y] = terrain;
            }
        }
    }

    private void SmoothMap(ref int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int surroundWallAmount = GetSurroundGrassCount(ref map, x, y);

                if (surroundWallAmount > 4) map[x, y] = GRASS;
                else if (surroundWallAmount < 4) map[x, y] = WALL;
            }
        }
    }

    private int GetSurroundGrassCount(ref int[,] map, int gridX, int gridY)
    {
        int grassCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) grassCount++;
                else if (!(x == gridX && y == gridY)) grassCount += map[x, y];
            }
        }

        return grassCount;
    }
}
