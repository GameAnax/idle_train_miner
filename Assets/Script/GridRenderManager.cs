using System.Collections.Generic;
using UnityEngine;

public class GridRenderManager : MonoBehaviour
{
    public static GridRenderManager instance;

    public Mesh gridMesh;
    public Material gridMaterial;
    public Color grassColor;

    public List<CustomeGrid> allGridCells = new List<CustomeGrid>();
    private List<Matrix4x4[]> matrixBatches = new List<Matrix4x4[]>();
    private List<Vector4[]> ListOfCOllisionBendings = new List<Vector4[]>();
    private List<Vector4[]> listOfColors = new List<Vector4[]>();
    private MaterialPropertyBlock mpb;

    Quaternion correction = Quaternion.Euler(0, 0, 0);


    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        mpb = new MaterialPropertyBlock();
        PrepareBatches();
    }
    void Update()
    {
        // Har frame render karein
        foreach (var batch in matrixBatches)
        {
            mpb.SetVectorArray("_CollisionBending", ListOfCOllisionBendings[0]);
            mpb.SetVectorArray("_TintColor2", listOfColors[0]);
            Graphics.DrawMeshInstanced(gridMesh, 0, gridMaterial, batch, batch.Length, mpb);
        }
    }
    void PrepareBatches()
    {
        matrixBatches.Clear();
        int count = allGridCells.Count;

        // 1023 ke chunks mein divide karne ke liye
        for (int i = 0; i < count; i += 1023)
        {
            int batchSize = Mathf.Min(1023, count - i);
            Matrix4x4[] batch = new Matrix4x4[batchSize];
            ListOfCOllisionBendings.Add(new Vector4[1023]);
            listOfColors.Add(new Vector4[1023]);

            for (int j = 0; j < batchSize; j++)
            {
                // CustomGrid ki position use kar rahe hain
                allGridCells[i + j].gpuMeshIndex = i + j;
                if (allGridCells[i + j].isClear) continue;
                Vector3 pos = allGridCells[i + j].transform.position;
                Quaternion rot = allGridCells[i + j].transform.rotation;
                Vector3 scale = allGridCells[i + j].transform.localScale;

                // batch[j].SetTRS(pos, Quaternion.identity, Vector3.one);
                batch[j] = Matrix4x4.TRS(pos, rot, scale);
                ListOfCOllisionBendings[i][j].x = 0;
                ListOfCOllisionBendings[i][j].z = 0;
                ListOfCOllisionBendings[i][j].y = 1;

                listOfColors[i][j] = grassColor;
            }

            matrixBatches.Add(batch);
        }
    }
    public void HideMesh(int gpuMeshIndex)
    {
        int batchIndex = gpuMeshIndex / 1023;
        int instanceIndex = gpuMeshIndex % 1023;
        matrixBatches[batchIndex][instanceIndex] = Matrix4x4.Scale(Vector3.zero);
    }
    public void BlinkMesh(int gpuMeshIndex, Color color)
    {
        int batchIndex = gpuMeshIndex / 1023;
        int instanceIndex = gpuMeshIndex % 1023;
        listOfColors[batchIndex][instanceIndex] = color;
    }
}
