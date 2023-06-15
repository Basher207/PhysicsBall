using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PhysicsBall
{
    public class BPhysicsRunner : MonoBehaviour
    {
        public event Action onPreBPhysicsUpdate = delegate {};
        public event Action onPostBPhysicsUpdate = delegate {};
    
        
        
        [Header("Target physics ticks per second")]
        [SerializeField] private double targetUpdatesPerSecond = 120f;
        
        [Header("Prevents hang ups, when updates take too long")]
        [SerializeField] private double maxComputeSecondsPerFrame = 0.02f;
        
        [SerializeField] private BPhysicsSimulator bPhysicsSimulator;
        
        
        
        //Real reffers to unscaled time 
        private DateTime simulationRealStartTime;
        private DateTime previousUpdateStartTime;
        
        //Offset of the simulation off of the simulationRealStartTime
        private TimeSpan simRealTimeOffset;
        private TimeSpan simulatedTime;
        
        //Total physics ticks
        private long physicsTickCount;
        
        private DateTime CurrentRealTime => DateTime.Now;
    
        //Time between ticks
        public double DeltaTime => 1f / targetUpdatesPerSecond;
        public double SimulationTime => simulatedTime.TotalSeconds;
    
    
        private void Start()
        {
            simulationRealStartTime = CurrentRealTime;
            previousUpdateStartTime = simulationRealStartTime;
            simRealTimeOffset = new TimeSpan();
            physicsTickCount = 0;
        }
    
        private void Update()
        {
            UpdatePhysics();
        }
    
        /// <summary>
        /// Updates the physics ticks as to catch up with the elapsed time
        /// </summary>
        private void UpdatePhysics()
        {
            //CurrentSimulationTime based on currentSimulationTime
            //During a physics update run, only this time is used for frame counting
            DateTime updateRealTimeStart = CurrentRealTime;
            
            //Compensate for Time.timeScale by offsetting simRealTimeOffset
            double secondsPassedSincePreviousUpdate = (updateRealTimeStart - previousUpdateStartTime).TotalSeconds;
            double timeScaleOffsetDefault = 1 - Time.timeScale;
            simRealTimeOffset -= TimeSpan.FromSeconds(secondsPassedSincePreviousUpdate * timeScaleOffsetDefault);
            
            //Based on updateRealTimeStart
            TimeSpan currentSimulationTime = (updateRealTimeStart - simulationRealStartTime) + simRealTimeOffset;
            DateTime physicsUpdateTimeLimit = updateRealTimeStart + TimeSpan.FromSeconds(maxComputeSecondsPerFrame);
    
            long targetTotalPhysicsTicks = (long)(currentSimulationTime.TotalSeconds / DeltaTime);
            long neededTicks = targetTotalPhysicsTicks - physicsTickCount;
            
            TimeSpan targetSimulationTime = currentSimulationTime + TimeSpan.FromSeconds(neededTicks * DeltaTime);
    
    
            
            //At the start of a physics update, we set the simRealTimeOffset back
            //After which, we compensate for this with every new tick
            
            //If we have to half due to maxComputeSecondsPerFrame, this compensation
            //will keep us from forcing the next update to deal with ticks we didn't have time for
            //possibly preventing crashes, as updates take longer then frames.
            simRealTimeOffset -= targetSimulationTime - currentSimulationTime;
            
            //Update as many ticks towards the targetTotalPhysicsTicks within the allotted update time
            for (; physicsTickCount < targetTotalPhysicsTicks; physicsTickCount++)
            {
                TimeSpan timeTillLimit = physicsUpdateTimeLimit - CurrentRealTime;
                
                //Reached physicsUpdateTimeLimit
                 if (timeTillLimit.TotalMilliseconds < 0)
                 {
                     break;
                 }
                
                DoPhysicsTick();
                
                currentSimulationTime += TimeSpan.FromSeconds(DeltaTime);
                simRealTimeOffset += TimeSpan.FromSeconds(DeltaTime);
                simulatedTime = currentSimulationTime;
            }
    
            previousUpdateStartTime = updateRealTimeStart;
        }
    
        private void DoPhysicsTick()
        {
            try
            {
                onPreBPhysicsUpdate.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
    
    
            bPhysicsSimulator.SimulateTick();
            
    
            try
            {
                onPostBPhysicsUpdate.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}