using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PhysicsBall
{
    public interface BPhysicsSimulatable
    {
        void Init(BPhysicsSimulator bPhysicsSimulator);
        /// <summary>
        /// Compute the forces needed, but don't apply them
        /// </summary>
        void BPhysicsComputeForces();
        /// <summary>
        /// Apply calculated forces
        /// </summary>
        void BPhysicsApplyForces();

        //These are already satisfied on MonoBehaviour. Both MonoBehaviour.transform & MonoBehaviour.gameObject 
        Transform transform { get; }
        GameObject gameObject { get; }
    }

    /// <summary>
    /// Didn't have time to use, but easily calculable, for events, spawning arrows where bounces happen and stuff
    /// </summary>
    public struct BCollision
    {
        public Vector3 CollisionPoint { get; private set; }
        public BPhysicsSimulatable CollisionObject { get; private set; }
        public double CollisionTime { get; private set; }

        public BCollision(Vector3 collisionPosition, BPhysicsSimulatable collisionObject, double collisionTime)
        {
            this.CollisionPoint = collisionPosition;
            this.CollisionObject = collisionObject;
            this.CollisionTime = collisionTime;
        }
    }

    public class BPhysicsSimulator : MonoBehaviour
    {
        public static BPhysicsSimulator instance;

        [SerializeField] private BPhysicsSurface bPhysicsSurface;
        [SerializeField] private BPhysicsRunner physicsRunner;

        [SerializeField] private Vector3 gravity = new Vector3(0f, -9.806f, 0f);

        [Range(0f, 0.9f)] [SerializeField] private float airDrag;

        [Range(0f, 1000f)] [SerializeField] private float frictionDrag;

        public float AirDrag
        {
            get => airDrag;
            set => airDrag = value;
        }

        public float FrictionDrag
        {
            get => frictionDrag;
            set => frictionDrag = value;
        }

        private List<BPhysicsSimulatable> currentlySimulatingObjects;


        public BPhysicsSurface BPhysicsSurface => bPhysicsSurface;

        //Converting to float to remove the need for casting down the line
        public float DeltaTime => (float)physicsRunner.DeltaTime;
        public double SimulationTime => physicsRunner.SimulationTime;
        public Vector3 Gravity => gravity;

        private void Awake()
        {
            instance = this;
            currentlySimulatingObjects = new List<BPhysicsSimulatable>();
        }

        public void AddToSimulation(BPhysicsSimulatable bPhysicsSimulatable)
        {
            //Check if gameobject is alive
            if (!bPhysicsSimulatable.gameObject)
            {
                return;
            }

            bPhysicsSimulatable.Init(this);

            if (!currentlySimulatingObjects.Contains(bPhysicsSimulatable))
                currentlySimulatingObjects.Add(bPhysicsSimulatable);
        }

        public void RemoveFromSimulation(BPhysicsSimulatable bPhysicsSimulatable)
        {
            currentlySimulatingObjects.Remove(bPhysicsSimulatable);
        }



        
        
        /// <summary>
        /// Physics sim is broken into two stages
        /// RequestCalculateForces where forces are calculated
        /// RequestApplyForces where forces are applied
        /// </summary>

        public void SimulateTick()
        {
            RequestCalculateForces();
            RequestApplyForces();
        }
        
        private void RequestCalculateForces()
        {
            for (int i = currentlySimulatingObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    //Check if object is not destroyed
                    if (currentlySimulatingObjects[i].gameObject)
                        currentlySimulatingObjects[i].BPhysicsComputeForces();
                    else
                        throw new Exception("simulated object is destroyed");
                }
                catch (Exception e)
                {
                    //If an error occurs, remove object from simulation 
                    RemoveFromSimulation(currentlySimulatingObjects[i]);
                    Debug.LogError(e);
                }
            }
        }

        private void RequestApplyForces()
        {
            for (int i = currentlySimulatingObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    //Check if object is not destroyed
                    if (currentlySimulatingObjects[i].gameObject)
                        currentlySimulatingObjects[i].BPhysicsApplyForces();
                }
                catch (Exception e)
                {
                    //If an error occurs, remove object from simulation 
                    RemoveFromSimulation(currentlySimulatingObjects[i]);
                    Debug.LogError(e);
                }
            }
        }
    }
}
