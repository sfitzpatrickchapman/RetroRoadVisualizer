using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
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
    [Range(0.01f, 100f)] public float valleySubtractIncline = 1.3f;
    [Range(0f, 25f)] public float floorHeight = 1f;
    public bool testValuesRealtime = false;

    [Header("Wireframe Properties")]
    public bool visualizeVerts = false;
    public GameObject vertVisualizer;
    [Range(0.5f, 5f)] public float vertScale = 5f;

    public bool visualizeLines = false;
    public GameObject lineVisualizer;
    [Range(0.5f, 2.5f)] public float lineThickness = 1f;

    public GameObject combinedMeshGO;
    private readonly int combineSize = 250;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private GameObject vertSpheres;
    private GameObject lineCylinders;
    private GameObject combinedMeshes;
    private List<CombineInstance[]> combineInstances;

    private int vert;
    private int backVertZPos;
    private int frontVertZPos;


    void Start()
    {
        // If testing mode is enabled, set camera speed to 0 and disable wireframe
        if (testValuesRealtime)
        {
            Debug.Log("TESTING VALUES - Wireframe disabled and speed set to 0.");
            //CameraController.m_instance.speed = 0;
            Camera.main.GetComponent<CameraController>().enabled = false;
            visualizeVerts = false;
            visualizeLines = false;
        }
        else
        {
            // Create child GO for grandparent wireframe verts
            vertSpheres = new GameObject();
            vertSpheres.name = "Vertices";
            vertSpheres.transform.parent = transform;
            vertSpheres.isStatic = true;

            // Create child GO for grandparent wireframe lines
            lineCylinders = new GameObject();
            lineCylinders.name = "Lines";
            lineCylinders.transform.parent = transform;
            lineCylinders.isStatic = true;

            // Create child GO for combined wireframe meshes
            combinedMeshes = new GameObject();
            combinedMeshes.name = "CombinedMeshes";
            combinedMeshes.transform.parent = transform;
            combinedMeshes.isStatic = true;
        }

        // Init mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        backVertZPos = zSize;
        frontVertZPos = 0;
        InitTerrain();
    }

    void Update()
    {
        if (testValuesRealtime)
            InitTerrain(); //ONLY USE THIS IN UPDATE FOR TESTING VARS

        // GENERATE/DEGENERATE TERRAIN ---
        // If the camera is 5 in front of cur vert on z-axis
        if (Camera.main.transform.position.z - 5f > vertices[frontVertZPos].z)
        {
            GenerateNewTerrain();
            DegenerateTerrain();

            backVertZPos++;
            frontVertZPos++;
        }

        // Update wireframe meshes combine queue(possibly combine)
        if (!testValuesRealtime)
        {
            UpdateCombineQueue(vertSpheres);
            UpdateCombineQueue(lineCylinders);
        }

        UpdateMesh();
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

        // COMBINE WIREFRAME MESHES ---
        if (visualizeVerts)
            CombineInitMeshes(vertSpheres);

        if (visualizeLines)
            CombineInitMeshes(lineCylinders);


        UpdateMesh();
    }

    void GenerateNewTerrain()
    {
        // ADD NEW VERTS ---
        float y;
        Vector3 prevPos = Vector3.zero;

        for (int x = 0; x <= xSize; x++)
        {
            y = CalculateHeight(x, backVertZPos);
            Vector3 newPos = new Vector3(x, y, backVertZPos);

            vertices.Add(newPos);

            if (visualizeVerts)
                VisualizeVert(newPos);

            if (visualizeLines && x > 0)
                VisualizeLine(newPos, prevPos);

            prevPos = newPos;
        }
    }

    void DegenerateTerrain()
    {
        // DELETE OLD VERTS ---
        for (int x = 0; x <= xSize; x++)
            vertices.RemoveAt(0);
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
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

        // Partially indent a valley (average w/ parabola)
        float valleyHeight = valleyAverageIncline * Mathf.Pow((x * scale) - valleyShift, 2);
        curHeight = curHeight > valleyHeight ? (valleyHeight + curHeight) / 2f : curHeight;

        // Fully indent a valley (subtracting parabola)
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
        curLine.transform.localScale = new Vector3(lineThickness, distance / 2f, lineThickness);

        Vector3 middlePoint = (posA + posB) / 2f;
        curLine.transform.position = middlePoint;

        Vector3 rotationDirection = posA - posB;
        curLine.transform.up = rotationDirection;

        // Reference: https://www.youtube.com/watch?v=K-Nckj9tppM
    }

    void CombineInitMeshes(GameObject parent)
    {
        // CREATE LIST OF COMBINE INSTANCES ---
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        combineInstances = new List<CombineInstance[]>();
        CombineInstance[] curCombine = new CombineInstance[combineSize];

        // Create combines of size (combineSize) and add to list
        int meshesLeft = meshFilters.Length;
        int itr = 0;
        while (meshesLeft > combineSize)
        {
            for (int i = 0; i < combineSize; i++)
            {
                curCombine[i].mesh = meshFilters[itr].sharedMesh;
                curCombine[i].transform = meshFilters[itr + i].transform.localToWorldMatrix;
            }

            combineInstances.Add(curCombine);
            curCombine = new CombineInstance[combineSize]; //reset

            meshesLeft -= combineSize;
            itr += combineSize;
        }

        // Create combine of leftover meshes and add to list
        // TODO: This would be cleaner if covered in the prior loop
        CombineInstance[] leftoverCombine = new CombineInstance[meshesLeft];
        for (int i = 0; i < meshesLeft; i++)
        {
            leftoverCombine[i].mesh = meshFilters[i].sharedMesh;
            leftoverCombine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        combineInstances.Add(leftoverCombine);


        // CREATE COMBINED MESHES WITH COMBINE INSTANCES ---
        for (int i = 0; i < combineInstances.Count; i++)
        {
            GameObject newCombine = Instantiate(combinedMeshGO);
            newCombine.name = "CombinedInitMesh";
            newCombine.transform.parent = combinedMeshes.transform;

            newCombine.GetComponent<MeshFilter>().mesh = new Mesh();
            newCombine.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstances[i]);
            newCombine.GetComponent<MeshRenderer>().material = 
                parent.transform.GetChild(0).GetComponent<MeshRenderer>().material; 
            newCombine.SetActive(true);

            // Reset transform
            newCombine.transform.localScale = Vector3.one;
            newCombine.transform.rotation = Quaternion.identity;
            newCombine.transform.position = Vector3.zero;
        }

        // Delete all original mesh gameobjects
        foreach (Transform child in parent.transform)
            Destroy(child.gameObject);
    }

    void UpdateCombineQueue(GameObject parent)
    {
        // If enough child meshes in queue, combine and delete children
        if (parent.GetComponentsInChildren<MeshFilter>().Length >= combineSize)
        {
            CombineQueuedMesh(parent);

            foreach (Transform child in parent.transform)
                Destroy(child.gameObject);
        }
    }

    void CombineQueuedMesh(GameObject parent)
    {
        // CREATE LIST OF COMBINE INSTANCES ---
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] curCombine = new CombineInstance[combineSize];

        for (int i = 0; i < combineSize; i++)
        {
            curCombine[i].mesh = meshFilters[i].sharedMesh;
            curCombine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // CREATE COMBINED MESHES WITH COMBINE INSTANCE ---
        GameObject newCombine = Instantiate(combinedMeshGO);
        newCombine.transform.parent = combinedMeshes.transform;
        newCombine.name = "CombinedRuntimeMesh";

        newCombine.GetComponent<MeshFilter>().mesh = new Mesh();
        newCombine.GetComponent<MeshFilter>().mesh.CombineMeshes(curCombine);
        newCombine.GetComponent<MeshRenderer>().material =
            parent.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        newCombine.SetActive(true);

        // Reset transform
        newCombine.transform.localScale = Vector3.one;
        newCombine.transform.rotation = Quaternion.identity;
        newCombine.transform.position = Vector3.zero;
    }
}