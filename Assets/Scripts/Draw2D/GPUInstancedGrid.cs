using UnityEngine;

public class GPUInstancedGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 0.5f;
    public float viewRange = 10f;
    public Material lineMaterial;
    public Mesh lineMesh;
    
    private Camera cam;
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock propertyBlock;
    
    void Start()
    {
        cam = Camera.main;
        propertyBlock = new MaterialPropertyBlock();
        
        // Create a simple line mesh if none provided
        if (lineMesh == null)
        {
            lineMesh = CreateLineMesh();
        }
    }
    
    void Update()
    {
        RenderGridWithInstancing();
    }
    
    private void RenderGridWithInstancing()
    {
        Vector3 camPos = cam.transform.position;
        int minX = Mathf.FloorToInt((camPos.x - viewRange) / cellSize);
        int maxX = Mathf.CeilToInt((camPos.x + viewRange) / cellSize);
        int minZ = Mathf.FloorToInt((camPos.z - viewRange) / cellSize);
        int maxZ = Mathf.CeilToInt((camPos.z + viewRange) / cellSize);
        
        int lineCount = (maxX - minX + 1) * (maxZ - minZ + 1) * 2;
        
        if (matrices == null || matrices.Length != lineCount)
        {
            matrices = new Matrix4x4[lineCount];
        }
        
        int index = 0;
        
        // Generate matrices for all visible lines
        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Horizontal line
                Vector3 hPos = new Vector3(x * cellSize + cellSize * 0.5f, 0, z * cellSize);
                matrices[index++] = Matrix4x4.TRS(hPos, Quaternion.identity, new Vector3(cellSize, 1, 0.02f));
                
                // Vertical line
                Vector3 vPos = new Vector3(x * cellSize, 0, z * cellSize + cellSize * 0.5f);
                matrices[index++] = Matrix4x4.TRS(vPos, Quaternion.Euler(0, 90, 0), new Vector3(cellSize, 1, 0.02f));
            }
        }
        
        // Render all lines in batches
        int batchSize = 1023; // Unity's limit for Graphics.DrawMeshInstanced
        for (int i = 0; i < matrices.Length; i += batchSize)
        {
            int count = Mathf.Min(batchSize, matrices.Length - i);
            Matrix4x4[] batch = new Matrix4x4[count];
            System.Array.Copy(matrices, i, batch, 0, count);
            
            Graphics.DrawMeshInstanced(lineMesh, 0, lineMaterial, batch, count, propertyBlock);
        }
    }
    
    private Mesh CreateLineMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0, 0),
            new Vector3(0.5f, 0, 0)
        };
        mesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
        return mesh;
    }
}