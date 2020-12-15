// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation {

    /*
        [x] Check if speed assign is right (topspeed?)
        [x] Option for not animating wheels
        [x] Remove navmesh agent
        [-] Check prefab #6 issue
        [x] Replace all car with vehicle
        [-] Vehicle editor
        [-] Better README, set instructions in wiki?
        [x] Reorganize vehicle properties into one class
        [x] HasToStop + HasToGo is probably not right way to do
        [x] Tooltip on public fields
        [x] Change VehiclePhysics
        [-] Deaccelerate when see stop in front
        [x] Bug on red light, all cars start together
        [x] Slow start of acceleration (on red light and stop)
        [x] If car stop because of too close, make it go back a bit (?)
        [x] Car stop if starts inside collider

    */

    public class VehicleAI : MonoBehaviour
    {
        [Header("Traffic System")]
        [Tooltip("Current active traffic system")]
        public TrafficSystem trafficSystem;

        [Tooltip("Determine when the vehicle has reached its target. Can be used to \"anticipate\" earlier the next waypoint (the higher this number his, the earlier it will anticipate the next waypoint)")]
        public float waypointThresh = 6;


        [Header("Radar")]
        [Tooltip("Empty gameobject from where the rays will be casted")]
        public Transform raycastAnchor;

        [Tooltip("Length of the casted rays")]
        public float raycastLength = 5;

        [Tooltip("Spacing between each rays")]
        public int raySpacing = 2;

        [Tooltip("Number of rays to be casted")]
        public int raysNumber = 6;

        [Tooltip("If detected vehicle is below this distance, ego vehicle will stop")]
        public float emergencyBrakeThresh = 2f;

        [Tooltip("If detected vehicle is below this distance (and above, above distance), ego vehicle will slow down")]
        public float slowDownThresh = 4f;

        [HideInInspector] public bool hasToStop =  false;

        private WheelDrive wheelDrive;
        private int curWp = 0;
        private float initMaxSpeed = 0;
        private int targetSegment = 0;
        private int pastTargetSegment = -1;

        void Start()
        {
            wheelDrive = this.GetComponent<WheelDrive>();

            if(trafficSystem == null)
                return;

            initMaxSpeed = wheelDrive.maxSpeed;
            SetWaypointVehicleIsOn();
        }

        void Update(){
            if(trafficSystem == null)
                return;

            WaypointChecker();
            MoveVehicle();
        }


        void WaypointChecker(){
            GameObject waypoint = trafficSystem.segments[targetSegment].waypoints[curWp].gameObject;

            //Position of next waypoint relative to the car
            Vector3 wpDist = this.transform.InverseTransformPoint(new Vector3(waypoint.transform.position.x, this.transform.position.y, waypoint.transform.position.z));

            //Go to next waypoint if arrived to current
            if(wpDist.magnitude < waypointThresh){
                curWp++;
                if(curWp >= trafficSystem.segments[targetSegment].waypoints.Count){
                    pastTargetSegment = targetSegment;
                    targetSegment = GetNextSegmentId();
                    curWp = 0;
                }
            }
        }

        void MoveVehicle(){

            //Default, full acceleration, no break and no steering
            float acc = 1;
            float brake = 0;
            float steering = 0;
            wheelDrive.maxSpeed = initMaxSpeed;

            //1. Check if the car has to stop
            if(hasToStop){
                acc = 0;
                brake = 1;
                wheelDrive.maxSpeed /= 2f;
            }
            else{
                //2. Check if there are vehicles which are detected by the radar
                float hitDist;
                VehicleAI otherVehicle = GetDetectedVehicle(out hitDist);

                if(otherVehicle != null){

                    //Check if it's front vehicle
                    float dot = Vector3.Dot(this.transform.forward, otherVehicle.transform.forward);

                    //If detected front vehicle max speed is lower than ego vehicle, then decrease ego vehicle max speed
                    if(otherVehicle.wheelDrive.maxSpeed < wheelDrive.maxSpeed && dot > .8f){
                        float ms = Mathf.Max(wheelDrive.GetSpeedMS(otherVehicle.wheelDrive.maxSpeed) - .5f, .1f);
                        wheelDrive.maxSpeed = wheelDrive.GetSpeedUnit(ms);
                    }
                    
                    //If the two vehicles are too close, and facing the same direction, brake the ego vehicle
                    if(hitDist < emergencyBrakeThresh && dot > .8f){
                        acc = 0;
                        brake = 1;
                        wheelDrive.maxSpeed = Mathf.Max(wheelDrive.maxSpeed / 2f, wheelDrive.minSpeed);
                    }

                    //If the two vehicles are too close, and not facing same direction, slight make the ego vehicle go backward
                    else if(hitDist < emergencyBrakeThresh && dot <= .8f){
                        acc = -.3f;
                        brake = 0f;
                        wheelDrive.maxSpeed = Mathf.Max(wheelDrive.maxSpeed / 2f, wheelDrive.minSpeed);
                    }

                    //If the two vehicles are getting close, slow down their speed
                    else if(hitDist < slowDownThresh){
                        acc = .5f;
                        brake = 0f;
                        wheelDrive.maxSpeed = Mathf.Max(wheelDrive.maxSpeed / 2f, wheelDrive.minSpeed);
                    }
                }

                //Check if we need to steer to follow path
                //if we are going backward, do not steer
                if(acc > 0f){
                    Vector3 desiredVel = trafficSystem.segments[targetSegment].waypoints[curWp].transform.position - this.transform.position;
                    steering = Mathf.Clamp(this.transform.InverseTransformDirection(desiredVel.normalized).x, -1f, 1f);
                }
                else if(acc < 0){
                    float randomSteering = Random.Range(-.3f, .3f);
                    steering = randomSteering;
                }

            }

            //Move the car
            wheelDrive.Move(acc, steering, brake);
        }


        VehicleAI GetDetectedVehicle(out float _hitDist){
            VehicleAI detectedVehicle = null;
            float minDist = 1000f;

            float initRay = (raysNumber / 2f) * raySpacing;
            float hitDist =  0f;
            for(float a=-initRay; a<=initRay; a+=raySpacing){
                VehicleAI frontVehicleAI = null;
                CastRay(raycastAnchor.transform.position, a, this.transform.forward, raycastLength, out frontVehicleAI, out hitDist);

                if(frontVehicleAI == null) continue;

                float dist = Vector3.Distance(this.transform.position, frontVehicleAI.transform.position);
                if(frontVehicleAI != null && dist < minDist) {
                    detectedVehicle = frontVehicleAI;
                    minDist = dist;
                    break;
                }
            }

            _hitDist = hitDist;
            return detectedVehicle;
        }

        
        void CastRay(Vector3 _anchor, float _angle, Vector3 _dir, float _length, out VehicleAI _outVehicleAI, out float _outHitDistance){
            _outVehicleAI = null;
            _outHitDistance = -1f;

            //Draw raycast
            Debug.DrawRay(_anchor, Quaternion.Euler(0, _angle, 0) * _dir * _length, new Color(1, 0, 0, 0.5f));

            //Detect hit only on the autonomous vehicle layer
            int layer = 1 << LayerMask.NameToLayer("AutonomousVehicle");
            RaycastHit hit;
            if(Physics.Raycast(_anchor, Quaternion.Euler(0, _angle, 0) * _dir, out hit, _length, layer)){
                _outVehicleAI = hit.collider.GetComponentInParent<VehicleAI>();
                _outHitDistance = hit.distance;
            }
        }

        int GetNextSegmentId(){
            if(trafficSystem.segments[targetSegment].nextSegments.Count == 0)
                return 0;
            int c = Random.Range(0, trafficSystem.segments[targetSegment].nextSegments.Count);
            return trafficSystem.segments[targetSegment].nextSegments[c].id;
        }

        void SetWaypointVehicleIsOn(){
            //Find segment
            foreach(Segment segment in trafficSystem.segments){
                if(segment.IsOnSegment(this.transform.position)){
                    targetSegment = segment.id;

                    //Find nearest waypoint to start within the segment
                    float minDist = float.MaxValue;
                    for(int j=0; j<trafficSystem.segments[targetSegment].waypoints.Count; j++){
                        float d = Vector3.Distance(this.transform.position, trafficSystem.segments[targetSegment].waypoints[j].transform.position);

                        //Only take in front points
                        Vector3 lSpace = this.transform.InverseTransformPoint(trafficSystem.segments[targetSegment].waypoints[j].transform.position);
                        if(d < minDist && lSpace.z > 0){
                            minDist = d;
                            curWp = j;
                        }
                    }
                    break;
                }
            }
        }

        public int GetSegmentVehicleIsIn(){
            int vehicleSegment = targetSegment;
            bool isOnSegment = trafficSystem.segments[vehicleSegment].IsOnSegment(this.transform.position);
            if(!isOnSegment){
                bool isOnPSegement = trafficSystem.segments[pastTargetSegment].IsOnSegment(this.transform.position);
                if(isOnPSegement)
                    vehicleSegment = pastTargetSegment;
            }
            return vehicleSegment;
        }
    }
}