using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridRenderManager : MonoBehaviour
{
    public static GridRenderManager instance;

    [Serializable]
    public class MeshData
    {
        public MeshType meshType;
        public Mesh mesh;
        public Material material;
        public Color color;
        public List<CustomeGrid> allGridCells = new List<CustomeGrid>();
        public List<Matrix4x4[]> matrixBatches = new();
        public List<Vector4[]> ListOfCOllisionBendings = new List<Vector4[]>();
        public List<Vector4[]> listOfColors = new List<Vector4[]>();
        public List<float[]> speed = new List<float[]>();
    }


    public List<MeshData> meshDatas;
    Dictionary<MeshType, MeshData> keyValuePairs = new();


    private int batchCount = 500;
    // public Mesh gridMesh;
    // public Material gridMaterial;
    // public Color grassColor;

    // public List<CustomeGrid> allGridCells = new List<CustomeGrid>();
    // private List<Matrix4x4[]> matrixBatches = new List<Matrix4x4[]>();
    // private List<Vector4[]> ListOfCOllisionBendings = new List<Vector4[]>();
    // private List<Vector4[]> listOfColors = new List<Vector4[]>();
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
        foreach (var item in keyValuePairs)
        {
            foreach (var innerItem in item.Value.matrixBatches)
            {
                mpb.SetVectorArray("_CollisionBending", item.Value.ListOfCOllisionBendings[0]);
                mpb.SetVectorArray("_TintColor2", item.Value.listOfColors[0]);
                mpb.SetFloatArray("_Speed", item.Value.speed[0]);
                Graphics.DrawMeshInstanced(item.Value.mesh, 0, item.Value.material, innerItem, innerItem.Length, mpb);
            }
        }

        // Har frame render karein
        // foreach (var batch in matrixBatches)
        // {
        //     mpb.SetVectorArray("_CollisionBending", ListOfCOllisionBendings[0]);
        //     mpb.SetVectorArray("_TintColor2", listOfColors[0]);
        //     Graphics.DrawMeshInstanced(gridMesh, 0, gridMaterial, batch, batch.Length, mpb);
        // }
    }
    void PrepareBatches()
    {
        int totalMeshCount = meshDatas.Count;
        for (int k = 0; k < totalMeshCount; k++)
        {
            MeshData meshData = meshDatas[k];
            meshData.matrixBatches.Clear();
            int count = meshData.allGridCells.Count;
            for (int i = 0; i < count; i += batchCount)
            {
                int batchSize = Mathf.Min(batchCount, count - i);
                Matrix4x4[] batch = new Matrix4x4[batchSize];
                meshData.ListOfCOllisionBendings.Add(new Vector4[batchCount]);
                meshData.listOfColors.Add(new Vector4[batchCount]);
                meshData.speed.Add(new float[batchCount]);
                for (int j = 0; j < batchSize; j++)
                {
                    meshData.allGridCells[i + j].gpuMeshIndex = i + j;
                    if (meshData.allGridCells[i + j].isClear) continue;
                    Vector3 pos = meshData.allGridCells[i + j].transform.position;
                    Quaternion rot = meshData.allGridCells[i + j].transform.rotation;
                    Vector3 scale = meshData.allGridCells[i + j].transform.localScale;

                    pos.x += UnityEngine.Random.Range(-0.3f, 0.3f);
                    pos.z += UnityEngine.Random.Range(-0.3f, 0.3f);

                    // batch[j].SetTRS(pos, Quaternion.identity, Vector3.one);
                    batch[j] = Matrix4x4.TRS(pos, rot, scale);
                    meshData.ListOfCOllisionBendings[i][j].x = 0;
                    meshData.ListOfCOllisionBendings[i][j].z = 0;
                    meshData.ListOfCOllisionBendings[i][j].y = 1;

                    meshData.listOfColors[i][j] = meshData.color;
                    meshData.speed[i][j] = 0.2f;
                }
                meshData.matrixBatches.Add(batch);
            }
            keyValuePairs.Add(meshData.meshType, meshData);
        }
    }
    // void PrepareBatches()
    // {
    //     matrixBatches.Clear();
    //     int count = allGridCells.Count;

    //     // 1023 ke chunks mein divide karne ke liye
    //     for (int i = 0; i < count; i += 1023)
    //     {
    //         int batchSize = Mathf.Min(1023, count - i);
    //         Matrix4x4[] batch = new Matrix4x4[batchSize];
    //         ListOfCOllisionBendings.Add(new Vector4[1023]);
    //         listOfColors.Add(new Vector4[1023]);

    //         for (int j = 0; j < batchSize; j++)
    //         {
    //             // CustomGrid ki position use kar rahe hain
    //             allGridCells[i + j].gpuMeshIndex = i + j;
    //             if (allGridCells[i + j].isClear) continue;
    //             Vector3 pos = allGridCells[i + j].transform.position;
    //             Quaternion rot = allGridCells[i + j].transform.rotation;
    //             Vector3 scale = allGridCells[i + j].transform.localScale * 0.7f;

    //             // batch[j].SetTRS(pos, Quaternion.identity, Vector3.one);
    //             batch[j] = Matrix4x4.TRS(pos, rot, scale);
    //             ListOfCOllisionBendings[i][j].x = 0;
    //             ListOfCOllisionBendings[i][j].z = 0;
    //             ListOfCOllisionBendings[i][j].y = 1;

    //             listOfColors[i][j] = grassColor;
    //         }

    //         matrixBatches.Add(batch);
    //     }
    // }

    public void HideMesh(MeshType meshType, int gpuMeshIndex)
    {
        MeshData meshData = keyValuePairs[meshType];
        int batchIndex = gpuMeshIndex / batchCount;
        int instanceIndex = gpuMeshIndex % batchCount;
        meshData.matrixBatches[batchIndex][instanceIndex] = Matrix4x4.Scale(Vector3.zero);
    }
    public void BlinkMesh(MeshType meshType, int gpuMeshIndex, Color color)
    {
        MeshData meshData = keyValuePairs[meshType];
        int batchIndex = gpuMeshIndex / batchCount;
        int instanceIndex = gpuMeshIndex % batchCount;
        meshData.listOfColors[batchIndex][instanceIndex] = color;
    }
    public void TouchEffect(MeshType meshType, int gpuMeshIndex, float scaleMultiplier)
    {
        MeshData meshData = keyValuePairs[meshType];
        int batchIndex = gpuMeshIndex / batchCount;
        int instanceIndex = gpuMeshIndex % batchCount;
        meshData.matrixBatches[batchIndex][instanceIndex] = Matrix4x4.TRS(meshData.matrixBatches[batchIndex][instanceIndex].GetPosition(), meshData.matrixBatches[batchIndex][instanceIndex].rotation, Vector3.one * scaleMultiplier);
        // meshData.matrixBatches[batchIndex][instanceIndex] = Matrix4x4.Scale(Vector3.one * scaleMultiplier);
    }
    public void UpdateSpeed(MeshType meshType, int gpuMeshIndex, float _speed)
    {
        MeshData meshData = keyValuePairs[meshType];
        int batchIndex = gpuMeshIndex / batchCount;
        int instanceIndex = gpuMeshIndex % batchCount;
        meshData.speed[batchIndex][instanceIndex] = _speed;
    }
}




