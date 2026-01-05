
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;
using DG.Tweening;
using System;

namespace nostra.booboogames.lawnmowingmayhem
{

    public class GrassCut : MonoBehaviour
    {
        public float CutRadius;
        public bool isMaskCutter;
        public float normalSpeed;

        public ParticleSystem grassPart;

        public ParticleSystem circleVFX;

        MeshRenderer mesh;
        ParticleSystemRenderer partRend;
        MaterialPropertyBlock matBlock;
        public bool isNotBlade = false;
        private void Awake()
        {
            mesh = GetComponent<MeshRenderer>();
        }
        // Start is called before the first frame update
        void Start()
        {
            //if (!isMaskCutter)
            //    CutRadius = 10;


            if (!isNotBlade)
            {
                CutRadius = (0.5f * transform.localScale.x) * 10;
                partRend = grassPart.GetComponent<ParticleSystemRenderer>();
                matBlock = new MaterialPropertyBlock();
            }


            //var vfx = circleVFX.main;
            //vfx.startSize = CutRadius/28;
        }

        // Update is called once per frame
        void Update()
        {
            //if(gameObject.layer != layermask)
            //    gameObject.layer = layermask;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, CutRadius);

        }

        public void changeVFXSpeed(float val)
        {
            if (!isNotBlade)
            {
                var vfx = circleVFX.main;
                vfx.simulationSpeed = 0.1f * val;
            }

        }

        public void CantCutEffect(bool val)
        {
            if (mesh == null) return;
            if (val)
            {
                mesh.material.DOColor(Color.red, 0.2f).OnComplete(() =>
                {
                    mesh.material.DOColor(Color.white, 0.2f).OnComplete(() =>
                    {
                        if (val) CantCutEffect(val);
                    });
                });
            }

            else
            {
                DOTween.Kill(mesh.material);
                mesh.material.color = Color.white;
            }
        }



        public void StartParticle(Color c)
        {
            if (partRend && !isNotBlade)
            {
                partRend.GetPropertyBlock(matBlock);
                matBlock.SetColor("_EmissionColor", c);
                partRend.SetPropertyBlock(matBlock);
                grassPart.Play();
            }

        }
    }
}