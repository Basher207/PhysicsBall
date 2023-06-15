using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBall
{
    public class BPhysicsBall : MonoBehaviour, BPhysicsSimulatable
    {

        [SerializeField] private float maxPushBackForce = 10f;
        
        [Header("Will scale the ball pushback force, depending on how penetrated it is")]
        [SerializeField] private AnimationCurve pushCurve;
        
        [Header("Compensating for fast balls glitching underground")]
        [SerializeField] private float belowSurfacePushForce = 30f;



        /// <summary>
        /// Supposed to be called on collision, altho that's not currently being handled
        /// </summary>
        public event Action<BCollision> OnCollisionDetected;
        
        /// <summary>
        /// For now position is used from the transform, altho can be a swapped if needed.
        /// </summary>
        public Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
        }
        private float Radius => transform.localScale.x / 2f;
        
        
        public Vector3 velocity { get; set; }
        
        private Vector3 velocityToNextFrame;
        private BPhysicsSimulator bPhysicsSimulator;



        //PlaceHolder singleton to add ball into the simulator 
        private void Start()
        {
            BPhysicsSimulator.instance.AddToSimulation(this);
        }

        /// <summary>
        /// The simulator simulating this ball.
        /// </summary>
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


        
        
        
        /// <summary>
        /// Compute forces that would need to be added
        /// Note the staged calculation is used for more advanced collision detection
        /// that isn't hear yet.
        /// </summary>
        public void BPhysicsComputeForces()
        {
            velocityToNextFrame += bPhysicsSimulator.Gravity * bPhysicsSimulator.DeltaTime;
        }

        public void BPhysicsApplyForces()
        {
            ComputeCollisions();

            velocity += velocityToNextFrame;

            float airDragDivider = 1f + bPhysicsSimulator.AirDrag * bPhysicsSimulator.DeltaTime;
            velocity /= airDragDivider;

            position += velocity * bPhysicsSimulator.DeltaTime;

            transform.position = position;

            velocityToNextFrame = Vector3.zero;
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

            //Is the ball moving away, or towards the collisionPlane?
            bool movingTowardsPlane = Vector3.Dot(collisionPlane.normal, velocity) < 0;
            
            //Is the plane penetrating the surface of the ball? If so by how much 
            float penetration = Radius - Mathf.Abs(distanceToPlaneSurface);

            //Penetration normalised between 0 and 1
            float normalPenetration = penetration / Radius;
            
            //If the ball is being penetrated, and its moving towards the plane:
            //Handle collision! by making it only collide when movingTowardsPlane. It 
            //Allows for better rolling, altho better simulation can be done by not doing this.
            if (penetration > 0f && movingTowardsPlane)
            {
                //How much should the push back force be?
                float pushBackNormalised = pushCurve.Evaluate(normalPenetration);
                float pushBackForce = pushBackNormalised * maxPushBackForce;

                //Add velocity pushing away from the plane, using the calculated pushBackForce
                velocityToNextFrame += collisionPlane.normal * (pushBackForce * bPhysicsSimulator.DeltaTime);

                //The division that would represent the velocity loss due to friction
                float frictionDivider =
                    1f + bPhysicsSimulator.FrictionDrag * pushBackNormalised * bPhysicsSimulator.DeltaTime;

                //Breaking down the velocity into two parts: Pointing towards the normal, and not.
                //friction should only apply to velocity along the friction surface, thereby not to any velocity 
                //pointing away from the normal. 
                Vector3 portionOfVelocityTowardsNormal = surfaceNormal * Vector3.Dot(surfaceNormal, velocity);
                Vector3 portionOfVelocityNotTowardsNormal = velocity - portionOfVelocityTowardsNormal;
                
                //Apply the frictionDivider to the portionOfVelocityNotTowardsNormal
                portionOfVelocityNotTowardsNormal /= frictionDivider;
                
                //Recombine the two parts into a post friction velocity.
                velocity = portionOfVelocityTowardsNormal + portionOfVelocityNotTowardsNormal;
            }

            
            //Check if the ball is under the surface (Likely due to velocity too high making it bug through the surface)
            //If so, push the ball up
            float heightFromSurfacePoint = position.y - surfacePoint.y;
            if (heightFromSurfacePoint < 0f)
            {
                velocityToNextFrame += new Vector3(0f, belowSurfacePushForce, 0f) * bPhysicsSimulator.DeltaTime;
            }
            
            
            // Visualises the ball penetration by squishing it on the surface
            // float ballNormalHeight = 1f - Mathf.Min(Mathf.Abs(normalPenetration), 1f);
            // transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x * (ballNormalHeight), transform.localScale.z);
            // transform.up = surfaceNormal;
        }
    }
}