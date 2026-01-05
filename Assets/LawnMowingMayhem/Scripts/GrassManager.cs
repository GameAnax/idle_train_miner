using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace nostra.booboogames.lawnmowingmayhem
{
    public class GrassManager : MonoBehaviour
    {
        [Header("Grass Cutter")]
        public bool onGrass;
        public bool upgradeCenter;

        public GrassGPUInstancing[] allGrassSystems;
        public bool isinMask;
        public int minPlayerDis = 7;
        public int maxPlayerDis = 35;
        public int maxPlayerDisX = 15;
        public int maxPlayerDisZ = 35;
        public float grassRegrowSpeed = 0.05f;
        public float bendGrassDis = 1.8f;
        public float grassRaiseSpeed = 1;

        public int cutPartNo = 1;

        public Material grassGreen;
        public Material grassYellow;
        public ParticleSystem partYellow;

        [Header("Grass Mesh")]
        public Mesh meshGrassLow;
        public Mesh meshGrassHigh;
        public GameObject chunk2D;

        public List<Transform> cutTransform = new List<Transform>();
        public List<Transform> bendTransform = new List<Transform>();
        public Transform playerTrans;
        public float range = 2f;
        public float _pickableSpawnRadius;

        public bool sameGrass = true;
        public float cutPer;

        public MeshRenderer grassArea;
        // Start is called before the first frame update
        public void Start()
        {
            //allGrassSystems = new GrassGPUInstancing[GameManager.instance.allGrassSystems.Length];
            //allGrassSystems = GameManager.instance.allGrassSystems;
            if (sameGrass)
            {
                foreach (GrassGPUInstancing grassSystem in allGrassSystems)
                {
                    grassSystem.grassMaterial = grassYellow;
                    grassSystem.particleSys = partYellow;
                }
            }

            foreach (var i in cutTransform)
            {
                AddCutTransform(i);
            }

            foreach (var i in bendTransform)
            {
                AddBendTransform(i);
            }

            for (int i = 0; i < allGrassSystems.Length; i++)
            {
                allGrassSystems[i].playerTransform = playerTrans;
            }

            grassArea.material.color = ((grassYellow.GetColor("_TintColor2") * 1.1f));// + grassYellow.GetColor("_TintColor1"))/2);
            cutPer = 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddBendTransform(Transform _trans)
        {
            for (int i = 0; i < allGrassSystems.Length; i++)
            {
                allGrassSystems[i].AddBendTransform(_trans);
            }
        }
        public void RemoveBendTransform(Transform _trans)
        {
            for (int i = 0; i < allGrassSystems.Length; i++)
            {
                allGrassSystems[i].RemoveBendTransform(_trans);
            }
        }
        public void AddCutTransform(Transform _trans)
        {

            for (int i = 0; i < allGrassSystems.Length; i++)
            {
                allGrassSystems[i].AddCutTransform(_trans);
            }
        }
        public void RemoveCutTransform(Transform _trans)
        {
            for (int i = 0; i < allGrassSystems.Length; i++)
            {
                allGrassSystems[i].RemoveCutTransform(_trans);
            }
        }



        public int SpawnAfterCutMiniGrass = 10;
        public float waitBeforAdditemInToInventory = 2f;
        int currCutGrass = 0;
        [SerializeField] private ParticleSystem grassParticle;

        public void throwGrass(Vector3 lastHitPoint)
        {

            if (currCutGrass >= SpawnAfterCutMiniGrass)
            {
                Vector3 randomPoint = UnityEngine.Random.insideUnitCircle;
                randomPoint.z = randomPoint.y;
                randomPoint.y = 0f;
                randomPoint = randomPoint.normalized * _pickableSpawnRadius;
                Vector3 position = lastHitPoint;
                //Pickable item = PickablePool.TakeFromPool();
                //item.transform.position = position;
                //_instantiatedPickables.Add(item);
                //TweenHelper.Jump(item.transform, position + randomPoint, 2f, 1, 0.3f);
                EmitParticle(lastHitPoint);
                //StartCoroutine(giveADelayAndsendItemToInventory(waitBeforAdditemInToInventory, item));

                currCutGrass = 0;
            }
            else
            {
                currCutGrass++;
            }
        }

        IEnumerator giveADelayAndsendItemToInventory(float waitTime)
        {
            // AudioManager.Instance.PlayCutSound();

            yield return new WaitForSeconds(waitTime);

        }

        void OnDestroy()
        {

        }

        public void EmitParticle(Vector3 pos)
        {
            //Debug.Log("Emit");
            pos.y = 1f;
            grassParticle.transform.position = pos;
            grassParticle.Play();
        }

        float lastP = 0;
        public float diff;
        public float carCutSpeed = 0.4f;
        public float carNormalSpeed = 0.75f;

        public bool isSecondArea;
        [HideInInspector] public bool cantCut;
        public void CutProgression(float v)
        {
            cutPer = v;

        }

        public void ChangeCarSpeed(int cut)
        {
            float _speed = carNormalSpeed;
            float _carCutSpeed = cantCut ? 0.01f : carCutSpeed;

        }

        public void Regrow()
        {
            Start();
            allGrassSystems[0].ResetData();
            //grassRegrowSpeed = 10f;
        }

        IEnumerator stopGrowing()
        {
            yield return new WaitForSeconds(0.2f);
            grassRegrowSpeed = 0;
        }
    }
}
