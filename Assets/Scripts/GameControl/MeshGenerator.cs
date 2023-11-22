using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshGenerator : MonoBehaviour
{
    public SquareGrid grid;
    private List<Vector3> vertices;
    private List<int> triangles;
    public void GenerateMesh(int[,] map, float squareSize)
    {
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
        GetComponent<MeshCollider>().sharedMesh = mesh;
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

        
        /*
        switch (square.configuration)
        {
            case 0: break;

            // 1 points
            case 1:
                MeshFromPoints(square.bottom, square.bottomLeft, square.left);
                break;
            case 2:
                MeshFromPoints(square.bottom, square.bottomRight, square.right);
                break;
            case 4:
                MeshFromPoints(square.top, square.topRight, square.right);
                break;
            case 8:
                MeshFromPoints(square.top, square.topLeft, square.left);
                break;

            // 2 points
            case 3:
                MeshFromPoints(square.left, square.bottomLeft, square.right, square.bottomRight);
                break;
            case 6:
                MeshFromPoints(square.top, square.topRight, square.bottom, square.bottomRight);
                break;
            case 9:
                MeshFromPoints(square.top, square.topLeft, square.bottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.left, square.topLeft, square.right, square.topRight);
                break;
            case 5:
                MeshFromPoints(square.left, square.bottomLeft, square.bottom, square.topRight, square.top, square.right);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.left, square.bottom, square.bottomRight, square.right, square.top);
                break;

            // 3 points
            case 7:
                MeshFromPoints(square.bottomLeft, square.bottomRight, square.topRight, square.left, square.top);
                break;
            case 11:
                MeshFromPoints(square.bottomLeft, square.bottomRight, square.topLeft, square.right, square.top);
                break;
            case 13:
                MeshFromPoints(square.bottomLeft, square.topRight, square.topLeft, square.right, square.bottom);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.bottomRight, square.topRight, square.right, square.bottom);
                break;

            // 4 points
            case 15:
                MeshFromPoints(square.bottomLeft, square.bottomRight, square.topRight, square.topLeft);
                break;
        }*/
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
    }

    private void OnDrawGizmos()
    {/*
        if (grid != null)
        {
            for (int x = 0; x < grid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < grid.squares.GetLength(1); y++)
                {
                    Gizmos.color = (grid.squares[x, y].topLeft.active) ? new Color(.65f, .45f, .4f) : new Color(.2f, .5f, .2f);
                    Gizmos.DrawCube(grid.squares[x, y].topLeft.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[x, y].topRight.active) ? new Color(.65f, .45f, .4f) : new Color(.2f, .5f, .2f);
                    Gizmos.DrawCube(grid.squares[x, y].topRight.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[x, y].bottomRight.active) ? new Color(.65f, .45f, .4f) : new Color(.2f, .5f, .2f);
                    Gizmos.DrawCube(grid.squares[x, y].bottomRight.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[x, y].bottomLeft.active) ? new Color(.65f, .45f, .4f) : new Color(.2f, .5f, .2f);
                    Gizmos.DrawCube(grid.squares[x, y].bottomLeft.position, Vector3.one * .4f);

                    Gizmos.color = new Color(.425f, .475f, .3f);
                    Gizmos.DrawCube(grid.squares[x, y].top.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].right.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].bottom.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].left.position, Vector3.one * .15f);
                }
            }
        }*/
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
