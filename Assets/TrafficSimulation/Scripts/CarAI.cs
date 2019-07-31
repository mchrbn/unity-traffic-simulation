// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TrafficSimulation {
    public class CarAI : MonoBehaviour
    {
        [Header("Traffic System")]
        public TrafficSystem trafficSystem;
        public int waypointThresh = 6;

        [Header("Raycast")]
        public Transform raycastAnchor;
        public float raycastLength = 5;
        public int raySpacing = 2;
        public int raysNumber = 6;

        [Header("Vehicle")]
        public float minTopSpeed;
        public float maxTopSpeed;

        public bool hasToStop = false;
        public bool hasToGo = false;

        VehiclePhysics carController;
        NavMeshAgent agent;
        int curWp = 0;
        [HideInInspector]
        public int curSeg = 0;
        float initialTopSpeed;


        void Start()
        {
            carController = this.GetComponent<VehiclePhysics>();

            initialTopSpeed = Random.Range(minTopSpeed, maxTopSpeed);
            carController.Topspeed = initialTopSpeed;

            //Create navmesh agent children
            GameObject aiGo = new GameObject("NavmeshAgent");
            aiGo.transform.SetParent(this.transform, false);
            agent = aiGo.AddComponent<NavMeshAgent>();
            agent.radius = 0.7f;
            agent.height = 1;
            agent.speed = 1;

            if(trafficSystem == null)
                return;

            //Find segment
            foreach(Segment segment in trafficSystem.segments){
                if(segment.IsOnSegment(this.transform.position)){
                    curSeg = segment.id;
                    //Find nearest waypoint to start within the segment
                    float minDist = float.MaxValue;
                    for(int j=0; j<trafficSystem.segments[curSeg].waypoints.Count; j++){
                        float d = Vector3.Distance(this.transform.position, trafficSystem.segments[curSeg].waypoints[j].transform.position);
                        //Only take in front points
                        Vector3 lSpace = this.transform.InverseTransformPoint(trafficSystem.segments[curSeg].waypoints[j].transform.position);
                        if(d < minDist && lSpace.z > 0){
                            minDist = d;
                            curWp = j;
                        }
                    }
                    break;
                }
            }
        }

        void Update(){
            if(trafficSystem == null)
                return;

            //Set navmesh agent in front of the car
            agent.transform.position = this.transform.position + (this.transform.forward * carController.MotorWheels[0].transform.localPosition.z);

            WaypointChecker();
            float topSpeed = GetCarSpeed();
            MoveVehicle(topSpeed);
        }

        //
        void WaypointChecker(){
            GameObject waypoint = trafficSystem.segments[curSeg].waypoints[curWp].gameObject;
            //Position of next waypoint relative to the car
            Vector3 nextWp = this.transform.InverseTransformPoint(new Vector3(waypoint.transform.position.x, this.transform.position.y, waypoint.transform.position.z));

            //Set agent destination depending on waypoint
            agent.SetDestination(waypoint.transform.position);

            //Go to next waypoint if arrived to current
            if(nextWp.magnitude < waypointThresh){
                curWp++;
                //If looping
                //if (curWp >= trafficSystem.segments[curSeg].waypoints.Count) curWp = 0;
                //If end then find the next segment
                if(curWp >= trafficSystem.segments[curSeg].waypoints.Count){
                    curSeg = GetNextSegmentId();
                    curWp = 0;
                }
            }
        }

        /// If vehicle within raycast hit length, return its speed. Otherwise return the original initial top speed of the vehicle.
        /// In order to avoid that the current car is going fast than the front one and collide.
        /// If the car has to stop, return 0
        float GetCarSpeed(){
            //If car has to stop, set speed to 0
            if(hasToStop)
                return 0;

            Vector3 anchor = new Vector3(this.transform.position.x, this.transform.position.y + 1f, this.transform.position.z);
            if(raycastAnchor != null)
                anchor = raycastAnchor.position;
            
            //Check if we are going to collide with a car in front
            CarAI otherCarAI = null;
            float topSpeed = initialTopSpeed;
            float initRay = (raysNumber / 2f) * raySpacing;

            for(float a=-initRay; a<=initRay; a+=raySpacing){
                otherCarAI = CastRay(anchor, a, this.transform.forward, raycastLength);

                //If rays collide with a car, adapt the top speed to be the same as the one of the front vehicle
                if(otherCarAI != null && otherCarAI.carController != null && carController.Topspeed > otherCarAI.carController.Topspeed){
                    //Check if the car is on the same lane or not. If not the same lane, then we do not adapt the vehicle speed to the one in front
                    //(it just means that the rays are hitting a car on the opposite lane...which shouldn't influence the car's speed)

                    if(hasToGo && !IsOnSameLane(otherCarAI.transform))
                        return topSpeed;
                        
                    topSpeed = otherCarAI.carController.Topspeed - 0.1f;
                    break;
                }
            }
            return topSpeed;
        }   

        /// 
        void MoveVehicle(float _topSpeed){
            //Wheel steering value
            float steering = Mathf.Clamp(this.transform.InverseTransformDirection(agent.desiredVelocity).x, -1f, 1f);
            
            //If car is turning then decrease it's maximum
            float topSpeed = _topSpeed;
            if(steering > 0.2f || steering < -0.2f && carController.Topspeed > 15) topSpeed = initialTopSpeed / 2f;
            carController.Topspeed = topSpeed;

            //Move the car
            carController.Move(steering, 1f, 0f);
        }

        //
        CarAI CastRay(Vector3 anchor, float angle, Vector3 dir, float length){
            //Draw raycast
            Debug.DrawRay(anchor, Quaternion.Euler(0, angle, 0) * dir * length, new Color(1, 0, 0, 0.5f));

            //Detect hit only on the autonomous vehicle layer
            int layer = 1 << LayerMask.NameToLayer("AutonomousVehicle");
            RaycastHit hit;
            if(Physics.Raycast(anchor, Quaternion.Euler(0, angle, 0) * dir, out hit, length, layer))
                return hit.collider.GetComponentInParent<CarAI>();

            return null;
        }

        int GetNextSegmentId(){
            if(trafficSystem.segments[curSeg].nextSegments.Count == 0)
                return 0;
            int c = Random.Range(0, trafficSystem.segments[curSeg].nextSegments.Count);
            return trafficSystem.segments[curSeg].nextSegments[c].id;
        }

        bool IsOnSameLane(Transform otherCar){
            Vector3 diff = this.transform.forward - otherCar.transform.forward;
            if(Mathf.Abs(diff.x) < 0.3f && Mathf.Abs(diff.z) < 0.3f) return true;
            else return false;
        }
    }
}