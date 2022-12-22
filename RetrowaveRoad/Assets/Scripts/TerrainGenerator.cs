using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Mesh Size")]
    public int xSize = 20;
    public int zSize = 20;

    [Header("Terrain Properties")]
    [Range(0.01f, 0.99f)] public float scale = 0.3f;
    [Range(0.1f, 5f)] public float height = 2.5f;
    [Range(1f, 10f)] public float sharpness = 3f;
    [Range(-20f, 20f)] public float valleyShift = 7.1f;
    [Range(0.01f, 3f)] public float valleyAverageIncline = 0.4f;
    [Range(0.01f, 10f)] public float valleySubtractIncline = 1.3f;
    [Range(0f, 25f)] public float floorHeight = 1f;
    [Range(0f, 25f)] public float speed = 5f;
    public bool testValuesRealtime = false;

    [Header("Vertex Properties")]
    public bool visualizeVerts = false;
    public GameObject vertVisualizer;
    [Range(0.001f, 0.25f)] public float vertScale = 0.025f;

    [Header("Line Properties")]
    public bool visualizeLines = false;
    public GameObject lineVisualizer;
    [Range(0.001f, 0.1f)] public float lineThickness = 0.025f;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private GameObject vertSpheres;
    private GameObject lineCylinders;

    private int curZLen;
    private int vert;
    private float zEndPos;


    /* TODO:
     * 
     * -Optimize
     * -Add wireframe lines
     * -Adjust terrain shader
     * -Add sun and HDRI sky
     * -Re-color
     * -Finishing touches such as: air particles, fog, vert flicker, etc.
     * -Clean up code
     * -Clean with chat GPT
     * -Finalize code
     * 
     */


    void Start()
    {
        // Create child GO for grandparent vert spheres
        vertSpheres = new GameObject();
        vertSpheres.name = "Vertices";
        vertSpheres.transform.parent = transform;

        // Create child GO for grandparent line cylinders
        lineCylinders = new GameObject();
        lineCylinders.name = "Lines";
        lineCylinders.transform.parent = transform;

        // Init mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        InitTerrain();

        // Set z-axis length trackers
        curZLen = zSize;
        zEndPos = transform.TransformPoint(vertices[vertices.Count - 1]).z;
    }

    void InitTerrain()
    {
        // GENERATE VERTS ---
        vertices = new List<Vector3>();

        Vector3 prevPos = Vector3.zero;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = CalculateHeight(x, z);
                Vector3 newPos = new Vector3(x, y, z);
                vertices.Add(newPos);

                if (visualizeVerts)
                    VisualizeVert(new Vector3(x, y, z));

                if (visualizeLines && x > 0)
                    VisualizeLine(newPos, prevPos);

                prevPos = newPos;
            }
        }

        // GENERATE TRIS ---
        triangles = new List<int>();

        vert = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles.Add(vert + 0);
                triangles.Add(vert + xSize + 1);
                triangles.Add(vert + 1);
                triangles.Add(vert + 1);
                triangles.Add(vert + xSize + 1);
                triangles.Add(vert + xSize + 2);

                vert++;
            }
            vert++;
        }

        UpdateMesh();
    }

    void GenerateNewTerrain()
    {
        curZLen++;

        // ADD NEW VERTS
        Vector3 prevPos = Vector3.zero;
        for (int x = 0; x <= xSize; x++)
        {
            float y = CalculateHeight(x, curZLen);
            Vector3 newPos = new Vector3(x, y, curZLen);
            vertices.Add(newPos);

            if (visualizeVerts)
                VisualizeVert(newPos);

            if (visualizeLines && x > 0)
                VisualizeLine(newPos, prevPos);

            prevPos = newPos;
        }

        // ADD NEW TRIS ---
        for (int x = 0; x < xSize; x++)
        {
            triangles.Add(vert + 0);
            triangles.Add(vert + xSize + 1);
            triangles.Add(vert + 1);
            triangles.Add(vert + 1);
            triangles.Add(vert + xSize + 1);
            triangles.Add(vert + xSize + 2);

            vert++;
        }

        vert++;
    }

    void DegenerateTerrain()
    {
        // DELETE FRONT TRIS ---
        for (int x = 0; x < xSize * 6; x++)
        {
            triangles.RemoveAt(0);
        }

        //// TODO: Gracefully delete verts
        //for (int x = 0; x <= xSize; x++)
        //{
        //    vertices.RemoveAt(0);
        //}
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    void Update()
    {
        if (testValuesRealtime)
            InitTerrain(); //ONLY USE THIS IN UPDATE FOR TESTING VARS

        // TRANSFORM TERRAIN (towards camera) ---
        Vector3 curPos = transform.position;
        curPos.z -= Time.deltaTime * speed;
        transform.position = curPos;

        // GENERATE NEW TERRAIN ---
        if (zEndPos > transform.TransformPoint(vertices[vertices.Count - 1]).z)
        {
            GenerateNewTerrain();
            DegenerateTerrain();
        }

        UpdateMesh();
    }

    float CalculateHeight(int x, int z)
    {
        // Calculate noise height
        float curHeight = Mathf.PerlinNoise(x * scale, z * scale);

        // Avoid any negative perlin vals
        curHeight = curHeight < 0 ? 0 : curHeight;
        
        // Noise height multipliers
        curHeight *= height;
        curHeight = Mathf.Pow(curHeight, sharpness);

        // Slightly indent valley (using parabola)
        float valleyHeight = valleyAverageIncline * Mathf.Pow((x * scale) - valleyShift, 2);
        curHeight = curHeight > valleyHeight ? (valleyHeight + curHeight) / 2f : curHeight;

        // Fully indent valley (using parabola)
        valleyHeight = valleySubtractIncline * Mathf.Pow((x * scale) - valleyShift, 2);
        curHeight = curHeight > valleyHeight ? valleyHeight : curHeight;

        // Assert floor height
        curHeight = curHeight < floorHeight ? floorHeight : curHeight;

        return curHeight;
    }

    void VisualizeVert(Vector3 pos)
    {
        GameObject curSphere = Instantiate(vertVisualizer);
        curSphere.transform.parent = vertSpheres.transform;
        curSphere.transform.position = pos;
        curSphere.transform.localScale = new Vector3(vertScale, vertScale, vertScale);
    }

    void VisualizeLine(Vector3 posA, Vector3 posB)
    {
        GameObject curLine = Instantiate(lineVisualizer);
        curLine.transform.parent = lineCylinders.transform;

        float distance = Vector3.Distance(posA, posB);
        curLine.transform.localScale = new Vector3(
            lineThickness, distance / 2f, lineThickness);

        Vector3 middlePoint = (posA + posB) / 2f;
        curLine.transform.position = middlePoint;

        Vector3 rotationDirection = (posA - posB);
        curLine.transform.up = rotationDirection;

        // Reference: https://www.youtube.com/watch?v=K-Nckj9tppM
    }
}
