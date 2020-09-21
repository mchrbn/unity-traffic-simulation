using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficSimulation;

public class Deactivate : MonoBehaviour
{
    bool isActive = true;

    GameObject[] cars;
    TrafficSystem ts;

    void Start(){
        cars = GameObject.FindGameObjectsWithTag("AutonomousVehicle");
        ts = GameObject.FindObjectOfType<TrafficSystem>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            if(isActive){
                isActive = false;
                ts.SaveTrafficSystem();
                foreach(GameObject car in cars){
                    car.SetActive(false);
                }
            }
            else{
                isActive = true;

                foreach(GameObject car in cars){
                    car.SetActive(true);
                    
                }
                ts.ResumeTrafficSystem();
            }
        }
    }
}
