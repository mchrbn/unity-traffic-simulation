// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficSimulation;

public class RedLightStatus : MonoBehaviour
{

    public int lightGroupId;  // Belong to traffic light 1 or 2?
    public Intersection intersection;
    
    Light pointLight;

    void Start(){
        //Get point light
        pointLight = this.transform.GetChild(0).GetComponent<Light>();

        //Check if the status of this light is green or red
        if(lightGroupId == intersection.curLightRed)
            pointLight.color = new Color(1, 0, 0);
        else
            pointLight.color = new Color(0, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(lightGroupId == intersection.curLightRed)
            pointLight.color = new Color(1, 0, 0);
        else
            pointLight.color = new Color(0, 1, 0);
    }
}
