using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System;
using DG.Tweening;
using System.Linq;
using System.Data.SqlTypes;
using System.Threading;

namespace nostra.booboogames.lawnmowingmayhem
{
    public class GrassGPUInstancing : MonoBehaviour
    {
        //private Matrix4x4[] matrixArray = new Matrix4x4[1000];
        private List<Matrix4x4[]> ListOfMatrixArrays = new List<Matrix4x4[]>();

        public Mesh grassMesh;
        public Material grassMaterial;
        private Matrix4x4 matrix = new Matrix4x4();
        private MaterialPropertyBlock mpb;
        //private Vector4[] collisionBending = new Vector4[1000];

        private int divNo = 300;
        private List<Vector4[]> ListOfCOllisionBendings = new List<Vector4[]>();
        private List<Vector4[]> ListOfNonCOllisionBendings = new List<Vector4[]>();

        public List<Transform> bendTransforms = new List<Transform>();
        private List<GrassCut> bendTransformsScript = new List<GrassCut>();

        public List<Transform> cutTransforms = new List<Transform>();
        private List<GrassCut> cutTransformsScript = new List<GrassCut>();

        private Vector3 overlapBoxPosition;
        private Vector3 overlapBoxExtents;

        private int centerGrass;
        public Transform grassStartPos;
        //public Vector3 startPos;
        public int rows, columns;
        public Vector3[] grassPositions;

        //public LayerMask grassCutterLayer;
        //public LayerMask grassCollisionLayer;

        // Particle System stuff
        public ParticleSystem particleSys = null;
        private Vector3[] cutPositions = new Vector3[10];
        private int cutAmount = 0;
        private ParticleSystem.Particle[] particleArray;

        //public float errorMargin = 0.1f;

        //public float muliplier = 0.6f;

        public float RowValue = 0;
        public float ColValue = 0;
        public float UnitValue = 1.0f;

        public float radius = 2.0f;

        public bool showGrass;
        public bool canCut;
        private Transform player;
        // private GameObject chunk2D;
        private bool inMask;

        public GrassManager grassManager;
        public Transform playerTransform;


        void Start()
        {
            //startPos = transform.position;
            createGrid();

            particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
            //GetComponent<MeshCollider>().enabled = false;
            //GetComponent<MeshRenderer>().enabled = false;

            mpb = new MaterialPropertyBlock();

            ListOfMatrixArrays.Clear();
            ListOfCOllisionBendings.Clear();
            ListOfNonCOllisionBendings.Clear();
            for (int i = 0; i < ((grassPositions.Length) / divNo) + 1; i++)
            {
                ListOfMatrixArrays.Add(new Matrix4x4[divNo]);
                ListOfCOllisionBendings.Add(new Vector4[divNo]);
                ListOfNonCOllisionBendings.Add(new Vector4[divNo]);
            }

            int thousands = 0;
            int subindex = 0;
            for (int i = 0; i < grassPositions.Length; i++)
            {
                subindex = i % divNo;
                if (i != 0 && subindex == 0)
                {
                    thousands += 1;
                }
                matrix.SetTRS(grassPositions[i], Quaternion.identity, Vector3.one);

                ListOfMatrixArrays[thousands][subindex] = matrix;

                float xbend = 0f;
                float zbend = 0f;

                ListOfCOllisionBendings[thousands][subindex].x = xbend;
                ListOfCOllisionBendings[thousands][subindex].z = zbend;
                ListOfCOllisionBendings[thousands][subindex].y = 1;
                ListOfNonCOllisionBendings[thousands][subindex].x = xbend;
                ListOfNonCOllisionBendings[thousands][subindex].z = zbend;
                ListOfNonCOllisionBendings[thousands][subindex].y = 1;
            }

            centerGrass = (int)(grassPositions.Length / 2);
            SetBendTransform();

            player = grassManager.playerTrans;

            // if (grassManager.chunk2D != null)
            // {
            //     chunk2D = Instantiate(grassManager.chunk2D, transform.position, Quaternion.identity, transform);
            //     chunk2D.SetActive(false);
            // }
            cuttedGrassIdx.Clear();
        }

        public void updateHitBox(Vector3 pos, Vector3 size)
        {
            overlapBoxPosition = pos;
            overlapBoxExtents = size;
        }
        List<int> cuttedGrassIdx = new List<int>();
        float LastPerc;
        int totalCut = 0;
        void Update()
        {
            if (inMask)
                return;

            //if (Mathf.Abs(Vector3.Distance(grassPositions[centerGrass], player.position)) > grassManager.maxPlayerDis && !grassManager.onGrass)
            //    return;



            if (Mathf.Abs(player.position.x - grassPositions[centerGrass].x) > grassManager.maxPlayerDisX && !grassManager.onGrass && !showGrass)
            {
                //if (chunk2D != null)
                //    if (!chunk2D.activeInHierarchy && grassManager.onGrass && once)
                //        chunk2D.SetActive(true);
                // if (!chunk2D.activeInHierarchy)
                //     chunk2D.SetActive(true);
                return;
            }
            if (Mathf.Abs(player.position.z - grassPositions[centerGrass].z) > grassManager.maxPlayerDisZ && !grassManager.onGrass && !showGrass)
            {
                //if (chunk2D != null)
                //    if (!chunk2D.activeInHierarchy && grassManager.onGrass && once)
                //        chunk2D.SetActive(true);
                // if (!chunk2D.activeInHierarchy)
                //     chunk2D.SetActive(true);
                return;
            }

            canCut = true;
            if (Mathf.Abs(Vector3.Distance(grassPositions[centerGrass], player.position)) > grassManager.minPlayerDis && !grassManager.onGrass)
                canCut = false;
            if (!canCut && grassManager.upgradeCenter && Mathf.Abs(Vector3.Distance(grassPositions[centerGrass], player.position)) <= (grassManager.minPlayerDis * 3))
                canCut = true;

            // if (chunk2D.activeInHierarchy)
            //     chunk2D.SetActive(false);
            int thousands = 0;
            cutAmount = 0;
            int grassCut = 0;
            int _rangeLevel = PlayerPrefs.GetInt("RangeLevel", 0);

            for (int i = 0; i < grassPositions.Length; i++)
            {
                int subindex = i % divNo;
                if (i != 0 && subindex == 0)
                {
                    thousands += 1;
                }

                // fix initial value (Vector4 starts at 0,0,0,0, we want y to be 1)
                if (ListOfCOllisionBendings[thousands][subindex].y == 0)
                {
                    ListOfCOllisionBendings[thousands][subindex].y = 1f;
                }

                if (canCut)
                {
                    #region MoveGrass

                    //for (int c = 0; c < bendTransforms.Count; c++)
                    //{
                    //    float xdist = ListOfMatrixArrays[thousands][subindex].GetColumn(3).x -
                    //                    bendTransforms[c].position.x;
                    //    float zdist = ListOfMatrixArrays[thousands][subindex].GetColumn(3).z -
                    //                    bendTransforms[c].position.z;
                    //    Vector2 abc;
                    //    abc.x = xdist;
                    //    abc.y = zdist;
                    //    //Vector2 bend = (-abc.normalized) * Mathf.Clamp(1f / abc.sqrMagnitude, 0.5f, 1);
                    //    Vector2 bend = (-abc.normalized) * Mathf.Clamp(1f / abc.sqrMagnitude, 0, 1);

                    //    Vector4 lerpTarget = Vector4.zero;
                    //    Vector4 lerped = Vector4.zero;

                    //    float _dis = grassManager.bendGrassDis;
                    //    //if (bendTransformsScript.Count > 0)
                    //    //    if (c < bendTransformsScript.Count && bendTransformsScript[c] != null)
                    //    //    {
                    //    //        _dis = (grassManager.bendGrassDis * (5 * (bendTransformsScript[c].CutRadius + 0.5f))) + bendTransformsScript[c].CutRadius;
                    //    //        Debug.Log("Dis: " + _dis + "_" + bendTransformsScript[c].name);
                    //    //    }

                    //    if (bendTransforms[c].GetComponent<GrassCut>())
                    //    {
                    //        GrassCut _cutScript = bendTransforms[c].GetComponent<GrassCut>();
                    //        _dis = (grassManager.bendGrassDis * (5 * (_cutScript.CutRadius + 0.5f))) + _cutScript.CutRadius;
                    //        //abc = abc / Mathf.Clamp((_cutScript.CutRadius / 2),1,100);
                    //        if (_rangeLevel >= 2)
                    //        {
                    //            abc = abc / (1 + (_rangeLevel * .2f));
                    //            bend = (-abc.normalized) * Mathf.Clamp(1f / abc.sqrMagnitude, 0, 1);
                    //        }
                    //        //Debug.Log(_dis);
                    //    }

                    //    if ((xdist * xdist + zdist * zdist) < _dis)
                    //    {
                    //        // Lay down
                    //        Vector2 currentDirection;

                    //        currentDirection.x = ListOfCOllisionBendings[thousands][subindex].x;
                    //        currentDirection.y = ListOfCOllisionBendings[thousands][subindex].z;


                    //        if (currentDirection.sqrMagnitude <= 0.4f)
                    //        {
                    //            lerpTarget.x = bend.x;
                    //            lerpTarget.z = bend.y;


                    //            float lerpSpeed = Time.deltaTime * 10f;
                    //            lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                    //            // Apply changes
                    //            ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                    //            ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                    //        }
                    //        else if (currentDirection.sqrMagnitude > 0.4f && bend.sqrMagnitude > currentDirection.sqrMagnitude)
                    //        {
                    //            Vector2 newVec = currentDirection.normalized * bend.magnitude;
                    //            lerpTarget.x = newVec.x;
                    //            lerpTarget.z = newVec.y;

                    //            float lerpSpeed = Time.deltaTime * 10f;
                    //            lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                    //            // Apply changes
                    //            ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                    //            ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Raise
                    //        float lerpSpeed = Time.deltaTime * grassManager.grassRaiseSpeed;
                    //        lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                    //        // Apply changes
                    //        ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                    //        ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                    //    }
                    //}
                    #endregion

                    #region CutGrass
                    // Check for Cutting
                    for (int cut = 0; cut < cutTransforms.Count; cut++)
                    {
                        float xdist2 = ListOfMatrixArrays[thousands][subindex].GetColumn(3).x -
                                        cutTransforms[cut].position.x;
                        float zdist2 = ListOfMatrixArrays[thousands][subindex].GetColumn(3).z -
                                        cutTransforms[cut].position.z;
                        Vector2 abc2;
                        abc2.x = xdist2;
                        abc2.y = zdist2;

                        float _cutRadius = grassManager.range;
                        //if (cutTransformsScript.Count > 0)
                        if (cut < cutTransformsScript.Count && cutTransformsScript[cut] != null)
                        {
                            _cutRadius = cutTransformsScript[cut].CutRadius;
                        }
                        if (abc2.sqrMagnitude < _cutRadius)
                        {
                            // Cut the Grass:
                            //For Center Point
                            if (cut < cutTransformsScript.Count && cutTransformsScript[cut] != null && cutTransformsScript[cut].isMaskCutter)
                            {
                                ListOfCOllisionBendings[thousands][subindex].y = -10000000f;
                                grassCut++;
                            }
                            else if (!grassManager.isinMask && ListOfCOllisionBendings[thousands][subindex].y > 0.3f)
                            {
                                if (count != 0)
                                {
                                    count = 0;
                                    if (!cutTransformsScript[cut].isNotBlade) grassManager.ChangeCarSpeed(1);
                                }

                                ListOfCOllisionBendings[thousands][subindex].y = 0.01f;
                                if (cutAmount + 1 < cutPositions.Length)
                                {

                                    if (!cuttedGrassIdx.Contains(subindex))
                                    {
                                        cuttedGrassIdx.Add(subindex);
                                        grassManager.CutProgression(cuttedGrassIdx.Count / (float)grassPositions.Length);

                                        if (cutTransforms[cut].GetComponent<GrassCut>())
                                        {
                                            cutTransforms[cut].GetComponent<GrassCut>().StartParticle(grassMaterial.GetColor("_TintColor2"));
                                        }



                                    }

                                    cutPositions[cutAmount++] = ListOfMatrixArrays[thousands][subindex].GetColumn(3);

                                }

                            }
                            // else if (!grassManager.isinMask && ListOfCOllisionBendings[thousands][subindex].y <= 0.3f && count < grassManager.gameManager.countThresold)
                            // {
                            //     count++;
                            //     if (count >= grassManager.gameManager.countThresold)
                            //     {
                            //         count = 0;
                            //         if (!cutTransformsScript[cut].isNotBlade) grassManager.ChangeCarSpeed(0);
                            //     }

                            // }
                        }
                    }
                    #endregion
                }
                #region RegrowGrass

                // regrow Grass 
                if (ListOfCOllisionBendings[thousands][subindex].y < 1f && ListOfCOllisionBendings[thousands][subindex].y > -10)
                {
                    if (ListOfCOllisionBendings[thousands][subindex].y <= 0.3f)
                    {
                        ListOfCOllisionBendings[thousands][subindex].y += grassManager.grassRegrowSpeed * Time.deltaTime;
                    }
                    else
                    {
                        ListOfCOllisionBendings[thousands][subindex].y += 5 * grassManager.grassRegrowSpeed * Time.deltaTime;
                    }
                }

                #endregion
            }



            //Set Inactive After in Mask
            if (grassCut >= grassPositions.Length - 1)
            {
                inMask = true;
                gameObject.SetActive(false);
            }



            //// Because we have multiple grass blades per mesh, we increase the single_grass particles per Grass cut:
            //cutAmount *= grassManager.cutPartNo;
            //// Particle Effect:
            //if (cutAmount > 0 && !grassManager.isinMask)
            //{

            //   // handle cutter vfx and speed of car

            //    int old_amount = particleSys.particleCount;

            //    particleSys.Emit(cutAmount);
            //    var main = particleSys.main;
            //    main.startColor = grassMaterial.GetColor("_TintColor2");
            //    int newAmount = particleSys.GetParticles(particleArray);

            //    for (int i = old_amount; i < old_amount + cutAmount; i++)
            //    {
            //        if (i < particleArray.Length - 1)
            //        {
            //            particleArray[i].position = cutPositions[Mathf.Clamp((i - old_amount) / grassManager.cutPartNo, 0, cutPositions.Length - 1)];

            //                //+ grassMaterial.GetColor("_TintColor1"))/2);
            //        }
            //    }
            //    particleSys.SetParticles(particleArray, newAmount);
            //}


            //for (int i = 0; i < ListOfMatrixArrays.Count; i++)
            //{
            //    mpb.SetVectorArray("_CollisionBending", ListOfCOllisionBendings[i]);
            //    Graphics.DrawMeshInstanced(GameManager.instance.meshGrassHigh, 0, grassMaterial, ListOfMatrixArrays[i], ListOfMatrixArrays[i].Length,
            //        mpb, ShadowCastingMode.Off, false, 0, null);
            //}

            mpb.SetVectorArray("_CollisionBending", ListOfCOllisionBendings[0]);
            if (canCut)
                Graphics.DrawMeshInstanced(grassManager.meshGrassHigh, 0, grassMaterial, ListOfMatrixArrays[0], ListOfMatrixArrays[0].Length, mpb, ShadowCastingMode.Off, false, 0, null);
            else
                Graphics.DrawMeshInstanced(grassManager.meshGrassLow, 0, grassMaterial, ListOfMatrixArrays[0], ListOfMatrixArrays[0].Length, mpb, ShadowCastingMode.Off, false, 0, null);
        }

        private void SpawnGress(Vector3 positionToSpawn)
        {
            //var TEMP = InventoryManager.instance.ReturnRefObject(CollectionEnum.Wheat);
            //GameManager.instance.SendingItem(TEMP, positionToSpawn, playerTransform, false);
        }


        int count = 0;
        #region BendTransformThings

        public void AddBendTransform(Transform _trans)
        {
            //if (bendTransforms.Contains(_trans))
            //    return;

            //bendTransforms.Add(_trans);
            //if (_trans.GetComponent<GrassCut>() != null)
            //    bendTransformsScript.Add(_trans.GetComponent<GrassCut>());
        }
        public void RemoveBendTransform(Transform _trans)
        {
            //if (!bendTransforms.Contains(_trans))
            //    return;
            //bendTransforms.Remove(_trans);
            //if (_trans.GetComponent<GrassCut>() != null)
            //    bendTransformsScript.Remove(_trans.GetComponent<GrassCut>());
        }

        private void SetBendTransform()
        {
            //if (bendTransforms.Count > 0)
            //{
            //    bendTransformsScript.Clear();
            //    for (int i = 0; i < bendTransforms.Count; i++)
            //    {
            //        if (bendTransforms[i].GetComponent<GrassCut>() != null)
            //            bendTransformsScript.Remove(bendTransforms[i].GetComponent<GrassCut>());
            //    }
            //}
        }

        #endregion

        #region CutTransformThings
        public void AddCutTransform(Transform _trans)
        {
            if (cutTransforms.Contains(_trans))
                return;

            cutTransforms.Add(_trans);
            if (_trans.GetComponent<GrassCut>() != null)
                cutTransformsScript.Add(_trans.GetComponent<GrassCut>());
        }
        public void RemoveCutTransform(Transform _trans)
        {
            if (!cutTransforms.Contains(_trans))
                return;
            cutTransforms.Remove(_trans);
            if (_trans.GetComponent<GrassCut>() != null)
            {
                cutTransformsScript.Remove(_trans.GetComponent<GrassCut>());
            }
        }

        #endregion

        private void GrassStatic()
        {
            //for (int i = 0; i < 1; i++)
            //{
            mpb.SetVectorArray("_CollisionBending", ListOfNonCOllisionBendings[0]);
            Graphics.DrawMeshInstanced(grassManager.meshGrassLow, 0, grassMaterial, ListOfMatrixArrays[0], ListOfMatrixArrays[0].Length,
                mpb, ShadowCastingMode.Off, false, 0, null);
            //}
        }

        void createGrid()
        {
            divNo = rows * columns;
            RowValue = 0;
            int length = 0;
            grassPositions = new Vector3[rows * columns];
            for (int i = 0; i < rows; i++)
            {
                ColValue = 0;
                for (int j = 0; j < columns; j++)
                {
                    grassPositions[length] = new Vector3(grassStartPos.position.x + RowValue + UnityEngine.Random.Range(-0.3f, 0.3f),
                        grassStartPos.position.y, grassStartPos.position.z + ColValue + UnityEngine.Random.Range(-0.3f, 0.3f));
                    length++;

                    ColValue += UnitValue;
                }
                RowValue += UnitValue;
            }
        }


        public void SetNewGrass(int _row, int _Collumn, bool _isOn)
        {
            rows = _row;
            columns = _Collumn;
            if (_isOn)
            {
                createGrid();
                Start();
            }
        }

        public float cellSize = 1f;
        public Vector3 origin = Vector3.zero;
        public Vector3 right = Vector3.right;
        public Vector3 forward = Vector3.forward;
        public Color gridColor = Color.green;

        private void OnDrawGizmos()
        {
            DrawGrid(grassStartPos.position, columns, rows, cellSize, gridColor);
        }

        void DrawGrid(Vector3 origin, int rows, int columns, float cellSize, Color color)
        {
            Gizmos.color = color;

            // Draw horizontal lines (rows)
            for (int row = 0; row <= rows; row++)
            {
                Vector3 start = origin + Vector3.forward * (row * cellSize);
                Vector3 end = start + Vector3.right * (columns * cellSize);
                Gizmos.DrawLine(start, end);
            }

            // Draw vertical lines (columns)
            for (int col = 0; col <= columns; col++)
            {
                Vector3 start = origin + Vector3.right * (col * cellSize);
                Vector3 end = start + Vector3.forward * (rows * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }

        public void ResetData()
        {
            Start();
        }
    }
}