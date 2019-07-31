// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TrafficSimulation{
    public class TrafficSystem : MonoBehaviour {

        public bool hideGuizmos = false;
        public float segDetectThresh = 0.1f;
        public List<Segment> segments = new List<Segment>();
        public List<Intersection> intersections = new List<Intersection>();
        public Segment curSegment = null;  

        [MenuItem("Component/Traffic Simulation/Create Traffic Objects")]
        static void CreateTraffic(){
            GameObject mainGo = new GameObject("Traffic System");
            mainGo.transform.position = Vector3.zero;
            mainGo.AddComponent<TrafficSystem>();

            GameObject segmentsGo = new GameObject("Segments");
            segmentsGo.transform.position = Vector3.zero;
            segmentsGo.transform.SetParent(mainGo.transform);

            GameObject intersectionsGo = new GameObject("Intersections");
            intersectionsGo.transform.position = Vector3.zero;
            intersectionsGo.transform.SetParent(mainGo.transform);
        }

        //Draw guizmos on editor window
        void OnDrawGizmos() {
            
            //Don't go further if we hide guizmos
            if(hideGuizmos) return;

            foreach(Segment segment in segments){
                //Draw waypoint
                for(int j=0; j<segment.waypoints.Count; j++){

                    //Get current waypoint position
                    Vector3 p = segment.waypoints[j].transform.position;
                    p = new Vector3(p.x, p.y + 0.5f, p.z);
                    
                    //Get next waypoint position
                    Vector3 pNext = Vector3.zero;
                    if(j < segment.waypoints.Count-1 && segment.waypoints[j+1] != null){
                        pNext = segment.waypoints[j+1].transform.position;
                        pNext = new Vector3(pNext.x, pNext.y + 0.5f, pNext.z);
                    }
                    // else if(j == segments[i].waypoints.Count-1){
                    //     pNext = segments[i].waypoints[0].transform.position;
                    //     pNext = new Vector3(pNext.x, pNext.y + 0.5f, pNext.z);
                    // }

                    //Draw sphere, increase color to show the direction
                    Gizmos.color = new Color(0f, 0f, 1f, (j + 1) / (float) segment.waypoints.Count);
                    Gizmos.DrawSphere(p, 0.4f);

                    //Draw line
                    Gizmos.color = new Color(1f, 0f, 0f);
                    if(pNext != Vector3.zero)
                        Gizmos.DrawLine(p, pNext);
                }

                //Draw line linking segments
                foreach(Segment nextSegment in segment.nextSegments){
                    if(nextSegment != null){
                        Vector3 p1 = segment.waypoints[segment.waypoints.Count-1].transform.position;
                        Vector3 p2 = nextSegment.transform.position;

                        Gizmos.color = new Color(1f, 1f, 0f);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }
        }
    }
}
