using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid grid;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    private List<List<int>> outlines = new List<List<int>>();
    private HashSet<int> checkedVertices = new HashSet<int>();

    [SerializeField] private PolygonCollider2D mapCollider;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        grid = new SquareGrid(map, squareSize);
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < grid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < grid.squares.GetLength(1); y++)
            {
                TriangulateSquare(grid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateWallCollider();
    }

    private void CreateWallCollider()
    {
        CalculateMeshOutlines();
        mapCollider.pathCount = outlines.Count;

        for (int pathIndex = 0; pathIndex < outlines.Count; pathIndex++)
        {
            List<int> outline = outlines[pathIndex];
            Vector2[] points = new Vector2[outline.Count];
            for (int i = 0; i < outline.Count; i++)
                points[i] = vertices[outline[i]];

            mapCollider.SetPath(pathIndex, points);
        }
    }

    private void TriangulateSquare(Square square)
    {
        /* ���ǿ����� switch������ �����Ͽ����� �̷��� �Ǹ� �Ǽ��� ���ɼ��� �ְ� 
         * �ڵ� ��ü�� �������� �ʹ� �������� ��.
         * ���� �ٸ� ����� ����.
         * 
         * �⺻������ Mesh�� ���� �� ������ �߿���.
         * ���� bottomLeft���� ���� �ݽð� �������� ���ư��� ���·� ����.
         * control Node�� on/off�� ��Ʈ ������ ���� Ȯ���� �� ����.
         * on/off �� ������ �� �� node�� !b�� ���·� on/off ��.
         * node�� �� ���� ���� �����ٸ� ������ �ǰ� �ϳ��� �����ٸ� ������ ��.
         * 
         * �������� �ݽð�������� ���������� Node�� �־��� �� ToArray�� ���� ��ȯ��Ű��
         * ������ ���߸� ��� ��Ȳ�� �ϳ��� �ڵ�� �����ϰ� �۵���ų �� ����.
         * �ٸ� �� ����� �� ���� �����ս��� �����شٰ�� ���� ���ϰ���.
         * �̴� ���� �ڵ� �������� ���̸� �Ǽ��� ���� �� �ְ� ����.
         */

        List<Node> nodes = new List<Node>();
        bool[] control = new bool[4]; // 0: bottomLeft, 1: bottomRight, 2: topRight, 3: topLeft
        bool[] center = new bool[4]; // 0: left, 1: bottom, 2: right, 3: top
        short mask = 1;
        for (int i = 0; i < 4; i++)
        {
            control[i] = (square.configuration & mask) != 0;

            if (control[i])
            {
                // �� �� ���� ���� index�� index + 1 �� ���� ����.
                // �� �� �������� ��쿡�� 0���� ���ư����ϹǷ� %4�� �� ��.
                int nextSide = (i + 1) % 4;
                center[i] = !center[i];
                center[nextSide] = !center[nextSide];
            }

            mask <<= 1; // ��Ʈ������ ���� �� ĭ�� ������ �о ����ŷ����. 
        }

        // �� �� �߰��� �ϵ��ڵ����� �߰�.
        if (control[0]) nodes.Add(square.bottomLeft);
        if (center[1]) nodes.Add(square.bottom);

        if (control[1]) nodes.Add(square.bottomRight);
        if (center[2]) nodes.Add(square.right);

        if (control[2]) nodes.Add(square.topRight);
        if (center[3]) nodes.Add(square.top);

        if (control[3]) nodes.Add(square.topLeft);
        if (center[0]) nodes.Add(square.left);

        MeshFromPoints(nodes.ToArray());
        if (square.configuration == 15)
        {
            checkedVertices.Add(square.topLeft.vertextIndex);
            checkedVertices.Add(square.topRight.vertextIndex);
            checkedVertices.Add(square.bottomRight.vertextIndex);
            checkedVertices.Add(square.bottomLeft.vertextIndex);
        }
    }

    private void MeshFromPoints(params Node[] points)
    {
        AssigneVertices(points);

        if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
    }

    private void AssigneVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertextIndex == -1)
            {
                points[i].vertextIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertextIndex);
        triangles.Add(b.vertextIndex);
        triangles.Add(c.vertextIndex);

        Triangle triangle = new Triangle(a.vertextIndex, b.vertextIndex, c.vertextIndex);
        AddTriangleDictinary(triangle.vertexIndexA, triangle);
        AddTriangleDictinary(triangle.vertexIndexB, triangle);
        AddTriangleDictinary(triangle.vertexIndexC, triangle);
    }

    private void AddTriangleDictinary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
            triangleDictionary[vertexIndexKey].Add(triangle);
        else
        {
            triangleDictionary.Add(vertexIndexKey, new List<Triangle>());
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
    }

    private void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> triangles = triangleDictionary[vertexIndex];

        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle triangle = triangles[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB == vertexIndex || checkedVertices.Contains(vertexB)) continue;
                if (IsOutlineEdge(vertexIndex, vertexB))
                {
                    return vertexB;
                }
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesA = triangleDictionary[vertexA];
        int sharedTriCount = 0;

        for (int i = 0; i < trianglesA.Count; i++)
        {
            if (trianglesA[i].Contains(vertexB))
            {
                sharedTriCount++;
                if (sharedTriCount > 1) break;
            }
        }

        return sharedTriCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[] { vertexIndexA, vertexIndexB, vertexIndexC };
        }

        public int this[int i] { get { return vertices[i]; } }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int countX = map.GetLength(0);
            int countY = map.GetLength(1);
            float width = countX * squareSize;
            float height = countY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[countX, countY];

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    Vector3 pos = new Vector3(x - countX / 2 + .5f, y - countY / 2 + .5f) * squareSize;
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] != 1, squareSize);
                }
            }

            squares = new Square[countX - 1, countY - 1];

            for (int x = 0; x < countX - 1; x++)
            {
                for (int y = 0; y < countY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node top, right, bottom, left;
        public int configuration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            top = topLeft.right;
            right = bottomRight.above;
            bottom = bottomLeft.right;
            left = bottomLeft.above;

            if (topLeft.active) configuration += 8;
            if (topRight.active) configuration += 4;
            if (bottomRight.active) configuration += 2;
            if (bottomLeft.active) configuration += 1;
        }
    }
    public class Node
    {
        public Vector3 position;
        public int vertextIndex = -1;

        public Node(Vector3 pos)
        {
            position = pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 pos, bool active, float sqaureSize) : base(pos)
        {
            this.active = active;
            above = new Node(pos + Vector3.up * sqaureSize / 2f);
            right = new Node(pos + Vector3.right * sqaureSize / 2f);
        }
    }
}
