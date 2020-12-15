// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation {
    public class Segment : MonoBehaviour {
        public List<Segment> nextSegments;
        
        [HideInInspector] public int id;
        [HideInInspector] public List<Waypoint> waypoints;

        public bool IsOnSegment(Vector3 _p){
            TrafficSystem ts = GetComponentInParent<TrafficSystem>();

            for(int i=0; i < waypoints.Count - 1; i++){
                float d1 = Vector3.Distance(waypoints[i].transform.position, _p);
                float d2 = Vector3.Distance(waypoints[i+1].transform.position, _p);
                float d3 = Vector3.Distance(waypoints[i].transform.position, waypoints[i+1].transform.position);
                float a = (d1 + d2) - d3;
                if(a < ts.segDetectThresh && a > -ts.segDetectThresh)
                    return true;

            }
            return false;
        }
    }
}
