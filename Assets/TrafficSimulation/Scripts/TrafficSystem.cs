// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrafficSimulation {
    public class TrafficSystem : MonoBehaviour {
        public bool hideGuizmos = false;
        public float segDetectThresh = 0.1f;
        public ArrowDraw arrowDrawType = ArrowDraw.ByLength;
        public int arrowCount = 1;
        public float arrowDistance = 5;
        public float arrowSizeWaypoint = 1;
        public float arrowSizeIntersection = 0.5f;
        public float waypointSize = 0.5f;
        
        public List<Segment> segments = new List<Segment>();
        public List<Intersection> intersections = new List<Intersection>();
        public Segment curSegment = null;  
        
        public List<Waypoint> GetAllWaypoints() {
            List<Waypoint> points = new List<Waypoint>();

            foreach (Segment segment in segments) {
                points.AddRange(segment.waypoints);
            }

            return points;
        }
    }

    public enum ArrowDraw {
        FixedCount, ByLength, Off
    }
}
