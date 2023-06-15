using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace PhysicsBall
{
    public class BPhysicsSurface : MonoBehaviour
    {

        [SerializeField] private MeshRenderer surface;
        [SerializeField] private MeshMaker meshMaker;


        private Material surfaceMaterial;
        private float propagationSpeed = 0f;

        public float WavePropagationOffset => _wavePropagationOffset;

        private void Awake()
        {
            surfaceMaterial = surface.material;
        }

        private void Update()
        {
            _wavePropagationOffset += propagationSpeed * Time.deltaTime;
            surfaceMaterial.SetFloat("_wavePropagationOffset", _wavePropagationOffset);
        }

        public float GetYAtWorldPosition(Vector3 worldPosition)
        {
            Vector2 uvPos = new Vector2(worldPosition.x, worldPosition.z);
            return GetYPosAtUvPoint(uvPos);
        }

        public Vector3 GetNormalAtWorldPosition(Vector3 worldPosition)
        {
            Vector2 uvPos = new Vector2(worldPosition.x, worldPosition.z);
            return GetNormalAtUvPoint(uvPos);
        }

        public void SetWavePropagationSpeed(float speed)
        {
            propagationSpeed = speed;
        }

        //This region has code copied from CurvedSurface shader, for ease of iteration

        #region Copied from shader

        float _wavePropagationOffset;
        float GetYPosAtUvPoint(float2 pos)
        {
            float firstTerm = 0.3f * sin(_wavePropagationOffset + 3 * sqrt(pow(pos.x - 5, 2) + pow(pos.y - 5, 2)));
            float secondTerm = 0.5f * cos(pos.x + pos.y);

            float yPos = firstTerm + secondTerm;
            return yPos;
        }

        float3 GetNormalAtUvPoint(float2 pos)
        {
            float delta = 0.001f;

            float fx = GetYPosAtUvPoint(float2(pos.x + delta, pos.y)) - GetYPosAtUvPoint(float2(pos.x - delta, pos.y));
            float fz = GetYPosAtUvPoint(float2(pos.x, pos.y + delta)) - GetYPosAtUvPoint(float2(pos.x, pos.y - delta));
            float3 normal = normalize(float3(-fx, 2 * delta, -fz));

            return normal;
        }
        
        #endregion
    }
}