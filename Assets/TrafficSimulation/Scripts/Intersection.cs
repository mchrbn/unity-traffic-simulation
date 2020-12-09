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

        //For stop only
        public List<Segment> prioritySegments;
        List<GameObject> vehiclesInIntersection;

        //For traffic lights only
        public float lightsDuration = 8;
        public float orangeLightDuration = 2;
        public List<Segment> lightsNbr1;
        public List<Segment> lightsNbr2;

        List<GameObject> vehiclesQueue;
        [HideInInspector]
        public int curLightRed = 1;

        void Start(){
            vehiclesQueue = new List<GameObject>();
            vehiclesInIntersection = new List<GameObject>();
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
            //Check if vehicle is already in the list if yes abort
            if(IsAlreadyInIntersection(other.gameObject)) return;

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

            VehicleAI vehicleAI = vehicle.GetComponent<VehicleAI>();
            if(!IsOnPrioritySegment(vehicleAI)){
                if(vehiclesQueue.Count > 0 || vehiclesInIntersection.Count > 0){
                    vehicleAI.hasToStop = true;
                    vehicleAI.hasToGo = false;
                    vehiclesQueue.Add(vehicle);
                }
                else{
                    vehiclesInIntersection.Add(vehicle);
                    vehicleAI.hasToGo = true;
                    vehicleAI.hasToStop = false;
                }
            }
            else{
                vehiclesInIntersection.Add(vehicle);
            }
        }

        void ExitStop(GameObject vehicle){

            vehicle.GetComponent<VehicleAI>().hasToGo = false;
            vehicle.GetComponent<VehicleAI>().hasToStop = false;
            vehiclesInIntersection.Remove(vehicle);
            vehiclesQueue.Remove(vehicle);

            if(vehiclesQueue.Count > 0 && vehiclesInIntersection.Count == 0){
                vehiclesQueue[0].GetComponent<VehicleAI>().hasToStop = false;
                vehiclesQueue[0].GetComponent<VehicleAI>().hasToGo = true;
            }
        }

        void TriggerLight(GameObject vehicle){
            int vehicleSegment = vehicle.GetComponent<VehicleAI>().curSeg;
            if(IsRedLightSegment(vehicleSegment)){
                vehicle.GetComponent<VehicleAI>().hasToStop = true;
                vehicle.GetComponent<VehicleAI>().hasToGo = false;
                vehiclesQueue.Add(vehicle);
            }
            else{
                vehicle.GetComponent<VehicleAI>().hasToGo = true;
                vehicle.GetComponent<VehicleAI>().hasToStop = false;
            }
        }

        void ExitLight(GameObject vehicle){
            vehicle.GetComponent<VehicleAI>().hasToStop = false;
            vehicle.GetComponent<VehicleAI>().hasToGo = false;
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
                if(!IsRedLightSegment(vehicle.GetComponent<VehicleAI>().curSeg)){
                    vehicle.GetComponent<VehicleAI>().hasToStop = false;
                    vehicle.GetComponent<VehicleAI>().hasToGo = true;
                    nVehiclesQueue.Remove(vehicle);
                }
            }
            vehiclesQueue = nVehiclesQueue;
        }

        bool IsOnPrioritySegment(VehicleAI vehicleAI){
            foreach(Segment nsSeg in prioritySegments){
                if(vehicleAI.curSeg == nsSeg.id)
                    return true;
            }
            return false;
        }

        bool IsAlreadyInIntersection(GameObject target){
            foreach(GameObject vehicle in vehiclesInIntersection){
                if(vehicle.GetInstanceID() == target.GetInstanceID()) return true;
            }
            foreach(GameObject vehicle in vehiclesQueue){
                if(vehicle.GetInstanceID() == target.GetInstanceID()) return true;
            }

            return false;
        } 

        List<GameObject> memVehiclesQueue = new List<GameObject>();
        List<GameObject> memVehiclesInIntersection = new List<GameObject>();

        public void SaveIntersectionStatus(){
            memVehiclesQueue = vehiclesQueue;
            memVehiclesInIntersection = vehiclesInIntersection;
        }

        public void ResumeIntersectionStatus(){
            foreach(GameObject v in vehiclesInIntersection){
                foreach(GameObject v2 in memVehiclesInIntersection){
                    if(v.GetInstanceID() == v2.GetInstanceID()){
                        v.GetComponent<VehicleAI>().hasToStop = v2.GetComponent<VehicleAI>().hasToStop;
                        v.GetComponent<VehicleAI>().hasToGo = v2.GetComponent<VehicleAI>().hasToGo;
                        break;
                    }
                }
            }
            foreach(GameObject v in vehiclesQueue){
                foreach(GameObject v2 in memVehiclesQueue){
                    if(v.GetInstanceID() == v2.GetInstanceID()){
                        v.GetComponent<VehicleAI>().hasToStop = v2.GetComponent<VehicleAI>().hasToStop;
                        v.GetComponent<VehicleAI>().hasToGo = v2.GetComponent<VehicleAI>().hasToGo;
                        break;
                    }
                }
            }
        }
    }
}
