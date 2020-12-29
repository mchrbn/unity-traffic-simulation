// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TrafficSimulation {
    [CustomEditor(typeof(TrafficSystem))]
    public class TrafficEditor : Editor {

        private TrafficSystem wps;
        
        //References for moving a waypoint
        private Vector3 startPosition;
        private Vector3 lastPoint;
        private Waypoint lastWaypoint;
        
        [MenuItem("Component/Traffic Simulation/Create Traffic Objects")]
        private static void CreateTraffic(){
            EditorHelper.SetUndoGroup("Create Traffic Objects");
            
            GameObject mainGo = EditorHelper.CreateGameObject("Traffic System");
            mainGo.transform.position = Vector3.zero;
            EditorHelper.AddComponent<TrafficSystem>(mainGo);

            GameObject segmentsGo = EditorHelper.CreateGameObject("Segments", mainGo.transform);
            segmentsGo.transform.position = Vector3.zero;

            GameObject intersectionsGo = EditorHelper.CreateGameObject("Intersections", mainGo.transform);
            intersectionsGo.transform.position = Vector3.zero;
            
            //Close Undo Operation
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        void OnEnable(){
            wps = target as TrafficSystem;
        }

        private void OnSceneGUI() {
            Event e = Event.current;
            if (e == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && e.type == EventType.MouseDown && e.button == 0) {
                //Add a new waypoint on mouseclick + shift
                if (e.shift) {
                    if (wps.curSegment == null) {
                        return;
                    }

                    EditorHelper.BeginUndoGroup("Add Waypoint", wps);
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create a segment + add a new waypoint on mouseclick + ctrl
                else if (e.control) {
                    EditorHelper.BeginUndoGroup("Add Segment", wps);
                    AddSegment(hit.point);
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create an intersection type
                else if (e.alt) {
                    EditorHelper.BeginUndoGroup("Add Intersection", wps);
                    AddIntersection(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }

            //Set waypoint system as the selected gameobject in hierarchy
            Selection.activeGameObject = wps.gameObject;

            //Handle the selected waypoint
            if (lastWaypoint != null) {
                //Uses a endless plain for the ray to hit
                Plane plane = new Plane(Vector3.up.normalized, lastWaypoint.GetVisualPos());
                plane.Raycast(ray, out float dst);
                Vector3 hitPoint = ray.GetPoint(dst);

                //Reset lastPoint if the mouse button is pressed down the first time
                if (e.type == EventType.MouseDown && e.button == 0) {
                    lastPoint = hitPoint;
                    startPosition = lastWaypoint.transform.position;
                }

                //Move the selected waypoint
                if (e.type == EventType.MouseDrag && e.button == 0) {
                    Vector3 realDPos = new Vector3(hitPoint.x - lastPoint.x, 0, hitPoint.z - lastPoint.z);

                    lastWaypoint.transform.position += realDPos;
                    lastPoint = hitPoint;
                }

                //Release the selected waypoint
                if (e.type == EventType.MouseUp && e.button == 0) {
                    Vector3 curPos = lastWaypoint.transform.position;
                    lastWaypoint.transform.position = startPosition;
                    Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move Waypoint");
                    lastWaypoint.transform.position = curPos;
                }

                //Draw a Sphere
                Handles.SphereHandleCap(0, lastWaypoint.GetVisualPos(), Quaternion.identity, wps.waypointSize * 2f, EventType.Repaint);
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                SceneView.RepaintAll();
            }

            //Set the current hovering waypoint
            if (lastWaypoint == null) {
                lastWaypoint = wps.GetAllWaypoints().FirstOrDefault(i => EditorHelper.SphereHit(i.GetVisualPos(), wps.waypointSize, ray));
            }

            //Update the current segment to the currently interacting one
            if (lastWaypoint != null && e.type == EventType.MouseDown) {
                wps.curSegment = lastWaypoint.segment;
            }
            
            //Reset current waypoint
            else if (lastWaypoint != null && e.type == EventType.MouseMove) {
                lastWaypoint = null;
            }
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();

            //Register an Undo if changes are made after this call
            Undo.RecordObject(wps, "Traffic Inspector Edit");

            //Draw the Inspector
            TrafficEditorInspector.DrawInspector(wps, serializedObject, out bool restructureSystem);

            //Rename waypoints if some have been deleted
            if (restructureSystem) {
                RestructureSystem();
            }

            //Repaint the scene if values have been edited
            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddWaypoint(Vector3 position) {
            GameObject go = EditorHelper.CreateGameObject("Waypoint-" + wps.curSegment.waypoints.Count, wps.curSegment.transform);
            go.transform.position = position;

            Waypoint wp = EditorHelper.AddComponent<Waypoint>(go);
            wp.Refresh(wps.curSegment.waypoints.Count, wps.curSegment);

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps.curSegment, "");
            wps.curSegment.waypoints.Add(wp);
        }

        private void AddSegment(Vector3 position) {
            int segId = wps.segments.Count;
            GameObject segGo = EditorHelper.CreateGameObject("Segment-" + segId, wps.transform.GetChild(0).transform);
            segGo.transform.position = position;

            wps.curSegment = EditorHelper.AddComponent<Segment>(segGo);
            wps.curSegment.id = segId;
            wps.curSegment.waypoints = new List<Waypoint>();
            wps.curSegment.nextSegments = new List<Segment>();

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
            wps.segments.Add(wps.curSegment);
        }

        private void AddIntersection(Vector3 position) {
            int intId = wps.intersections.Count;
            GameObject intGo = EditorHelper.CreateGameObject("Intersection-" + intId, wps.transform.GetChild(1).transform);
            intGo.transform.position = position;

            BoxCollider bc = EditorHelper.AddComponent<BoxCollider>(intGo);
            bc.isTrigger = true;
            Intersection intersection = EditorHelper.AddComponent<Intersection>(intGo);
            intersection.id = intId;

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
            wps.intersections.Add(intersection);
        }

        void RestructureSystem(){
            //Rename and restructure segments and waypoints
            List<Segment> nSegments = new List<Segment>();
            int itSeg = 0;
            foreach(Transform tS in wps.transform.GetChild(0).transform){
                Segment segment = tS.GetComponent<Segment>();
                if(segment != null){
                    List<Waypoint> nWaypoints = new List<Waypoint>();
                    segment.id = itSeg;
                    segment.gameObject.name = "Segment-" + itSeg;
                    
                    int itWp = 0;
                    foreach(Transform tW in segment.gameObject.transform){
                        Waypoint waypoint = tW.GetComponent<Waypoint>();
                        if(waypoint != null) {
                            waypoint.Refresh(itWp, segment);
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
            foreach(Transform tI in wps.transform.GetChild(1).transform){
                Intersection intersection = tI.GetComponent<Intersection>();
                if(intersection != null){
                    intersection.id = itInter;
                    intersection.gameObject.name = "Intersection-" + itInter;
                    nIntersections.Add(intersection);
                    itInter++;
                }
            }
            wps.intersections = nIntersections;
            
            //Tell Unity that something changed and the scene has to be saved
            if (!EditorUtility.IsDirty(target)) {
                EditorUtility.SetDirty(target);
            }

            Debug.Log("[Traffic Simulation] Successfully rebuilt the traffic system.");
        }
    }
}
