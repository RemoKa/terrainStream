using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class terrainManager : MonoBehaviour

{


    public int LOD = 0;
    private int size = 0;

    public int subdivisions = 3;
    public Mesh[] meshes;

    public int depth = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        size = (int)Mathf.Pow(2, subdivisions);

        meshes = new Mesh[subdivisions];

        for (int i = 0; i < subdivisions; i++)
        {   
            meshes[i] = generateMesh(size, i);
        } 

        GetComponent<MeshFilter>().mesh = meshes[0];
    }

    int getVertIndex(int x, int z, int size)
    {
        return x * size + z;
    }
    int getSquareIndex(int x, int z, int size)
    {
        return x * (size - 1) + z;
    }

    Mesh generateMesh(int targetSize, int depth)
    {

        int localSize = (int)Mathf.Pow(2, depth + 1);
        float localSize2 = Mathf.Pow(2, depth);


        Mesh mesh = new Mesh() { name = "m_01" };

        Vector3[] vertices = new Vector3[localSize * localSize];
        int[] triangles = new int[(localSize - 1) * (localSize - 1) * 2 * 3];

        for (int x = 0; x < localSize; x++)
        {
            for (int z = 0; z < localSize; z++)
            {
                vertices[getVertIndex(x, z, localSize)] = new Vector3(x / localSize2, 0, z / localSize2);
                Debug.Log($"Vertex[{getVertIndex(x, z, localSize)}]: {vertices[getVertIndex(x, z, localSize)]}");
            }

        }

        for (int x = 0; x < localSize - 1; x++)
        {
            for (int z = 0; z < localSize - 1; z++)
            {
                int squareIndex = getSquareIndex(x, z, localSize);
                int localVertIndex = squareIndex * 2 * 3;

                triangles[localVertIndex + 0] = getVertIndex(x + 0, z + 0, localSize);
                triangles[localVertIndex + 1] = getVertIndex(x + 0, z + 1, localSize);
                triangles[localVertIndex + 2] = getVertIndex(x + 1, z + 0, localSize);

                triangles[localVertIndex + 3] = getVertIndex(x + 0, z + 1, localSize);
                triangles[localVertIndex + 4] = getVertIndex(x + 1, z + 1, localSize);
                triangles[localVertIndex + 5] = getVertIndex(x + 1, z + 0, localSize);

            }

        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }


    // Update is called once per frame
    void Update()
    {

        GetComponent<MeshFilter>().mesh = meshes[depth];

    }
}
