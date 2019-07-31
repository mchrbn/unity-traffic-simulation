// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation{
    public enum IntersectionType{
        STOP,
        TRAFFIC_LIGHT
    }

    public class Intersection : MonoBehaviour
    {
        public IntersectionType intersectionType;
        public int id;

        //For traffic lights only
        public float lightsDuration = 8;
        public float orangeLightDuration = 2;
        public List<Segment> lightsNbr1;
        public List<Segment> lightsNbr2;

   
        List<GameObject> vehiclesQueue;
        [HideInInspector]
        public int curLightRed = 1;
        bool block = false;

        void Start(){
            vehiclesQueue = new List<GameObject>();
            if(intersectionType == IntersectionType.TRAFFIC_LIGHT)
                InvokeRepeating("SwitchLights", lightsDuration, lightsDuration);
        }

        void SwitchLights(){

            if(curLightRed == 1) curLightRed = 2;
            else if(curLightRed == 2) curLightRed = 1;            
            
            //Wait few seconds after light transition before making the other car move (= orange light)
            Invoke("MoveVehiclesQueue", orangeLightDuration);
        }

        void OnTriggerEnter(Collider other) {
            if(other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.STOP)
                TriggerStop(other.gameObject);
            else if(other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.TRAFFIC_LIGHT)
                TriggerLight(other.gameObject);
        }

        void OnTriggerExit(Collider other) {
            if(other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.STOP)
                ExitStop(other.gameObject);
            else if(other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.TRAFFIC_LIGHT)
                ExitLight(other.gameObject);
        }

        void TriggerStop(GameObject vehicle){
            //Check if there are other cars in the queue
            //if that's the case, stop the vehicle
            if(vehiclesQueue.Count > 0){
                vehicle.GetComponent<CarAI>().hasToStop = true;
            }
            vehiclesQueue.Add(vehicle);
        }

        void ExitStop(GameObject vehicle){
            if(vehiclesQueue.Count == 0)
                return;

            //Remove from queue move the next vehicle
            vehicle.GetComponent<CarAI>().hasToGo = false;
            vehiclesQueue.RemoveAt(0);

            //Get next car in queue and make it move
            if(vehiclesQueue.Count > 0){
                vehiclesQueue[0].GetComponent<CarAI>().hasToStop = false;
                vehiclesQueue[0].GetComponent<CarAI>().hasToGo = true;
            }
        }

        void TriggerLight(GameObject vehicle){
            int vehicleSegment = vehicle.GetComponent<CarAI>().curSeg;
            if(IsRedLightSegment(vehicleSegment)){
                vehicle.GetComponent<CarAI>().hasToStop = true;
                vehiclesQueue.Add(vehicle);
            }
            else{
                vehicle.GetComponent<CarAI>().hasToGo = true;
            }
        }

        void ExitLight(GameObject vehicle){
            vehicle.GetComponent<CarAI>().hasToStop = false;
            vehicle.GetComponent<CarAI>().hasToGo = false;
        }

        bool IsRedLightSegment(int vehicleSegment){
            if(curLightRed == 1){
                foreach(Segment segment in lightsNbr1){
                    if(segment.id == vehicleSegment)
                        return true;
                }
            }
            else{
                foreach(Segment segment in lightsNbr2){
                    if(segment.id == vehicleSegment)
                        return true;
                }
            }
            return false;
        }

        void MoveVehiclesQueue(){
            //Move all vehicles in queue
            List<GameObject> nVehiclesQueue = new List<GameObject>(vehiclesQueue);
            foreach(GameObject vehicle in vehiclesQueue){
                if(!IsRedLightSegment(vehicle.GetComponent<CarAI>().curSeg)){
                    vehicle.GetComponent<CarAI>().hasToStop = false;
                    vehicle.GetComponent<CarAI>().hasToGo = true;
                    nVehiclesQueue.Remove(vehicle);
                }
            }
            vehiclesQueue = nVehiclesQueue;
        }
    }
}
