// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace TrafficSimulation{

    [CustomEditor(typeof(TrafficSystem))]
    public class TrafficEditor : Editor {

        TrafficSystem wps;

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

        void OnEnable(){
            wps = target as TrafficSystem;
        }

        void OnSceneGUI(){
            Event e = Event.current;
            if(e == null) return;

            //Add a new waypoint on mouseclick + shift
            if(e.type == EventType.MouseDown && e.shift){

                if(wps.curSegment == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit)){
                    AddWaypoint(hit.point);
                }
            }
            //Create a segment + add a new waypoint on mousclick + ctrl
            else if(e.type == EventType.MouseDown && e.control){
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit)){
                    AddSegment(hit.point);
                    AddWaypoint(hit.point);
                }
            }
            //Create an intersection type
            else if(e.type == EventType.MouseDown && e.alt){
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit)){
                    AddIntersection(hit.point);
                }
            }

            //Set waypoint system as the selected gameobject in hierarchy
            Selection.activeGameObject = wps.gameObject;
        }

        public override void OnInspectorGUI(){

            //Editor properties
            EditorGUILayout.LabelField("Guizmo Config", EditorStyles.boldLabel);
            wps.hideGuizmos = EditorGUILayout.Toggle("Hide Guizmos", wps.hideGuizmos);
            EditorGUILayout.LabelField("System Config", EditorStyles.boldLabel);
            wps.segDetectThresh = EditorGUILayout.FloatField("Segment Detection Threshold", wps.segDetectThresh);
            EditorGUILayout.HelpBox("Ctrl + Left Click to create a new segment\nShift + Left Click to create a new waypoint.\nAlt + Left Click to create a new intersection", MessageType.Info);
            EditorGUILayout.HelpBox("Reminder: The cars will follow the point depending on the sequence you added them. (go to the 1st waypoint added, then to the second, etc.)", MessageType.Info);


            //Rename waypoints if some have been deleted
            if(GUILayout.Button("Re-Structure Traffic System")){
                RestructureSystem();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void AddWaypoint(Vector3 position){
            GameObject go = new GameObject("Waypoint-" + wps.curSegment.waypoints.Count);
            go.transform.position = position;
            go.transform.SetParent(wps.curSegment.transform);

            Waypoint wp = go.AddComponent<Waypoint>();
            wp.id = wps.curSegment.waypoints.Count;
            wp.segment = wps.curSegment;

            wps.curSegment.waypoints.Add(wp);
        }

        void AddSegment(Vector3 position){
            int segId = wps.segments.Count;
            GameObject segGo = new GameObject("Segment-" + segId);
            segGo.transform.position = position;
            segGo.transform.SetParent(wps.transform.GetChild(0).transform);
            wps.curSegment = segGo.AddComponent<Segment>();
            wps.curSegment.id = segId;
            wps.curSegment.waypoints = new List<Waypoint>();
            wps.curSegment.nextSegments = new List<Segment>();
            wps.segments.Add(wps.curSegment);
        }

        void AddIntersection(Vector3 position){
            int intId = wps.intersections.Count;
            GameObject intGo = new GameObject("Intersection-" + intId);
            intGo.transform.position = position;
            intGo.transform.SetParent(wps.transform.GetChild(1).transform);
            BoxCollider bc = intGo.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            Intersection intersection = intGo.AddComponent<Intersection>();
            intersection.id = intId;
            wps.intersections.Add(intersection);
        }

        void RestructureSystem(){

            //Rename and restructure segments and waypoitns
            List<Segment> nSegments = new List<Segment>();
            int itSeg = 0;
            foreach(Segment segment in wps.segments){
                if(segment != null){
                    List<Waypoint> nWaypoints = new List<Waypoint>();
                    segment.id = itSeg;
                    segment.gameObject.name = "Segment-" + itSeg;
                    
                    int itWp = 0;
                    foreach(Waypoint waypoint in segment.waypoints){
                        if(waypoint != null){
                            waypoint.id = itWp;
                            waypoint.segment = segment;
                            waypoint.gameObject.name = "Waypoint-" + itWp;
                            nWaypoints.Add(waypoint);
                            itWp++;
                        }
                    }

                    segment.waypoints = nWaypoints;
                    nSegments.Add(segment);
                    itSeg++;
                }
            }

            //Check if next segments still exist
            foreach(Segment segment in nSegments){
                List<Segment> nNextSegments = new List<Segment>();
                foreach(Segment nextSeg in segment.nextSegments){
                    if(nextSeg != null){
                        nNextSegments.Add(nextSeg);
                    }
                }
                segment.nextSegments = nNextSegments;
            }
            wps.segments = nSegments;

            //Check intersections
            List<Intersection> nIntersections = new List<Intersection>();
            int itInter = 0;
            foreach(Intersection intersection in wps.intersections){
                if(intersection != null){
                    intersection.id = itInter;
                    intersection.gameObject.name = "Intersection-" + itInter;
                    nIntersections.Add(intersection);
                    itInter++;
                }
            }
            wps.intersections = nIntersections;

            Debug.Log("[Traffic Simulation] Successfully rebuilt the traffic system.");
        }
    }
}
