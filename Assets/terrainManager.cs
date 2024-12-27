using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class terrainManager : MonoBehaviour

{
    private Mesh mesh;

    public int size = 10;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh() { name = "m_01" };


        

        Vector3[] vertices = new Vector3[size * size];
        int [] triangles = new int[(size - 1) * (size - 1) * 2 * 3];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                vertices[getVertIndex(x, z)] = new Vector3(x, 0, z);
                Debug.Log($"Vertex[{getVertIndex(x, z)}]: {vertices[getVertIndex(x, z)]}");
            }

        }

        for (int x = 0; x < size - 1; x++)
        {
            for (int z = 0; z < size - 1; z++)
            {
                int squareIndex  = getSquareIndex(x, z);
                int localVertIndex = squareIndex * 2 * 3;

                triangles[localVertIndex + 0] = getVertIndex(x + 0, z + 0);
                triangles[localVertIndex + 1] = getVertIndex(x + 0, z + 1);
                triangles[localVertIndex + 2] = getVertIndex(x + 1, z + 0);

                triangles[localVertIndex + 3] = getVertIndex(x + 0, z + 1);
                triangles[localVertIndex + 4] = getVertIndex(x + 1, z + 1);
                triangles[localVertIndex + 5] = getVertIndex(x + 1, z + 0);

            }

        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    int getVertIndex(int x, int z)
    {
        return x * size + z;
    }

    int getSquareIndex(int x, int z)
    {
        return x * (size - 1) + z;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
