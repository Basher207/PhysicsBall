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

        [SerializeField] private float fireSpeed = 5f;

        private BPhysicsBall InstantiateBall()
        {
            GameObject newBall = Instantiate<GameObject>(ballPrefab);
            BPhysicsBall physicsBall = newBall.GetComponent<BPhysicsBall>();

            return physicsBall;
        }
        
        //Create a ball, and fire it at pointer 
        private void SpawnBall(Vector2 screenPosition)
        {
            BPhysicsBall physicsBall = InstantiateBall();

            Ray WorldPosition = spawnCamera.ScreenPointToRay(screenPosition);

            physicsBall.transform.position = WorldPosition.origin;
            physicsBall.velocity = WorldPosition.direction * fireSpeed;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SpawnBall(eventData.position);
        }
    }
}
