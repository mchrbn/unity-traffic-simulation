// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TrafficSimulation{
    public class TrafficSystem : MonoBehaviour {

        public bool hideGuizmos = false;
        public float segDetectThresh = 0.1f;
        public ArrowDraw arrowDrawType;
        public int arrowCount = 1;
        public float arrowDistance = 5;

        public List<Segment> segments = new List<Segment>();
        public List<Intersection> intersections = new List<Intersection>();
        public Segment curSegment = null;  

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
                    // Gizmos.color = new Color(0f, 0f, 1f, (j + 1) / (float) segment.waypoints.Count);
                    // Gizmos.DrawSphere(p, .5f);

                    
                    if(pNext != Vector3.zero) {
                        //Draw line
                        Gizmos.color = new Color(1f, 0f, 0f);
                        Gizmos.DrawLine(p, pNext);
                        
                        int arrows = 0;
                        
                        //Set arrowCount based on arrowDrawType
                        switch (arrowDrawType) {
                            case ArrowDraw.FixedCount:
                                arrows = arrowCount;
                                break;
                            case ArrowDraw.ByLength:
                                //Minimum of one arrow
                                arrows = Mathf.Max(1, (int) (Vector3.Distance(p, pNext) / arrowDistance));
                                break;
                            case ArrowDraw.Off:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        Vector3 forward = (p - pNext).normalized;
                        Vector3 left = Quaternion.Euler(0, 45, 0) * forward;
                        Vector3 right = Quaternion.Euler(0, -45, 0) * forward;
                        
                        //Draw arrows
                        for (int i = 1; i < arrows + 1; i++) {
                            Vector3 point = Vector3.Lerp(p, pNext, (float) i / (arrows + 1));
                            Gizmos.DrawLine(point, point + left);
                            Gizmos.DrawLine(point, point + right);
                        }
                    }
                }

                //Draw line linking segments
                foreach(Segment nextSegment in segment.nextSegments){
                    if(nextSegment != null){
                        Vector3 p1 = segment.waypoints.Last().transform.position;
                        Vector3 p2 = nextSegment.waypoints.First().transform.position;

                        Gizmos.color = new Color(1f, 1f, 0f);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }
        }
    }

    public enum ArrowDraw {
        FixedCount, ByLength, Off
    }
}
