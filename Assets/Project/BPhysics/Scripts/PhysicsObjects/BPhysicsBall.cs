using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBall
{
    public class BPhysicsBall : MonoBehaviour, BPhysicsSimulatable
    {
        [SerializeField] private float Radius => transform.localScale.x / 2f;

        [SerializeField] private float maxPushBackForce = 10f;
        [SerializeField] private AnimationCurve pushCurve;
        
        [Header("Compensating for fast balls glitching underground")]
        [SerializeField] private float belowSurfacePushForce = 30f;



        public event Action<BCollision> OnCollisionDetected;
        
        private Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
        }

        private Vector3 velocity;
        private Vector3 velocityToAddNextFrame;
        private BPhysicsSimulator bPhysicsSimulator;




        private void Start()
        {
            BPhysicsSimulator.instance.AddToSimulation(this);
        }

        public void Init(BPhysicsSimulator physicsSimulator)
        {
            bPhysicsSimulator = physicsSimulator;
        }

        private void OnDisable()
        {
            if (bPhysicsSimulator)
            {
                bPhysicsSimulator.RemoveFromSimulation(this);
            }
        }

        private void OnEnable()
        {
            if (bPhysicsSimulator)
            {
                bPhysicsSimulator.AddToSimulation(this);
            }
        }


        
        
        
        
        public void BPhysicsComputeForces()
        {
            velocityToAddNextFrame += bPhysicsSimulator.Gravity * bPhysicsSimulator.DeltaTime;
        }

        public void BPhysicsApplyForces()
        {
            ComputeCollisions();

            velocity += velocityToAddNextFrame;

            float airDragDivider = 1f + bPhysicsSimulator.AirDrag * bPhysicsSimulator.DeltaTime;
            velocity /= airDragDivider;

            position += velocity * bPhysicsSimulator.DeltaTime;

            transform.position = position;

            velocityToAddNextFrame = Vector3.zero;
        }
        
        
        
        private void ComputeCollisions()
        {
            //Find the point, and normal of the point under the ball
            float yAtPoint = bPhysicsSimulator.BPhysicsSurface.GetYAtWorldPosition(position);
            Vector3 surfaceNormal = bPhysicsSimulator.BPhysicsSurface.GetNormalAtWorldPosition(position);
            Vector3 surfacePoint = position;
            surfacePoint.y = yAtPoint;
            
            //Use that to create a plane that's used to detect and handle collisions 
            Plane collisionPlane = new Plane(surfaceNormal, surfacePoint);
            float distanceToPlaneSurface = collisionPlane.GetDistanceToPoint(position);

            //Is the ball moving away, or towards the collisionPlane
            bool movingTowardsPlane = Vector3.Dot(collisionPlane.normal, velocity) < 0;
            
            float penetration = Radius - Mathf.Abs(distanceToPlaneSurface);

            float normalPenetration = penetration / Radius;

            if (penetration > 0f && movingTowardsPlane)
            {
                float pushBackNormalised = pushCurve.Evaluate(normalPenetration);
                float pushBackForce = pushBackNormalised * maxPushBackForce;

                velocityToAddNextFrame += collisionPlane.normal * (pushBackForce * bPhysicsSimulator.DeltaTime);

                float frictionDivider =
                    1f + bPhysicsSimulator.FrictionDrag * pushBackNormalised * bPhysicsSimulator.DeltaTime;

                Vector3 portionOfVelocityTowardsNormal = surfaceNormal * Vector3.Dot(surfaceNormal, velocity);
                Vector3 portionOfVelocityNotTowardsNormal = velocity - portionOfVelocityTowardsNormal;

                portionOfVelocityNotTowardsNormal /= frictionDivider;
                velocity = portionOfVelocityTowardsNormal + portionOfVelocityNotTowardsNormal;
            }

            
            //Check if the ball is under the surface (Likely due to velocity too high making it bug through the surface)
            //If so, push the ball up
            float heightFromSurfacePoint = position.y - surfacePoint.y;
            if (heightFromSurfacePoint < 0f)
            {
                velocityToAddNextFrame += new Vector3(0f, belowSurfacePushForce, 0f) * bPhysicsSimulator.DeltaTime;
            }
            
            
            // Visualises the ball penetration by squishing it on the surface
            // float ballNormalHeight = 1f - Mathf.Min(Mathf.Abs(normalPenetration), 1f);
            // transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x * (ballNormalHeight), transform.localScale.z);
            // transform.up = surfaceNormal;
        }
    }
}