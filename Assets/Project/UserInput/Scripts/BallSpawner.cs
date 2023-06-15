using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;


namespace PhysicsBall
{
    public class BallSpawner : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Camera spawnCamera;
        [SerializeField] private GameObject ballPrefab;

        private GameObject InstantiateBall()
        {
            GameObject newBall = Instantiate<GameObject>(ballPrefab);
            return newBall;
        }

        private void SpawnBall()
        {
            GameObject ball = InstantiateBall();

            Vector3 WorldPosition = spawnCamera.ScreenToWorldPoint(Input.mousePosition);

            ball.transform.position = WorldPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SpawnBall();
        }
    }
}
