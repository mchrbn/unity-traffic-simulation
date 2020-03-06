// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace TrafficSimulation{

    [CustomEditor(typeof(TrafficSystem))]
    public class TrafficEditor : Editor {

        private TrafficSystem wps;
        
        //References for moving a waypoint
        private Vector3 startPosition;
        private Vector3 lastPoint;
        private Waypoint lastWaypoint;
        
        [MenuItem("Component/Traffic Simulation/Create Traffic Objects")]
        static void CreateTraffic(){
            //Create new Undo Group to collect all changes in one Undo
            Undo.SetCurrentGroupName("Create Traffic Objects");
            
            GameObject mainGo = CreateGameObjectWithUndo("Traffic System");
            mainGo.transform.position = Vector3.zero;
            AddComponentWithUndo<TrafficSystem>(mainGo);

            GameObject segmentsGo = CreateGameObjectWithUndo("Segments", mainGo.transform);
            segmentsGo.transform.position = Vector3.zero;

            GameObject intersectionsGo = CreateGameObjectWithUndo("Intersections", mainGo.transform);
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

                    BeginUndoGroup("Add Waypoint");
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create a segment + add a new waypoint on mouseclick + ctrl
                else if (e.control) {
                    BeginUndoGroup("Add Segment");
                    AddSegment(hit.point);
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create an intersection type
                else if (e.alt) {
                    BeginUndoGroup("Add Intersection");
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
                Plane plane = new Plane(Vector3.up.normalized, lastWaypoint.transform.position);
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
                Handles.SphereHandleCap(0, lastWaypoint.transform.position, Quaternion.identity, wps.waypointSize * 2f, EventType.Repaint);
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                SceneView.RepaintAll();
            }

            //Set the current hovering waypoint
            if (lastWaypoint == null) {
                lastWaypoint = wps.GetAllWaypoints().FirstOrDefault(i => SphereHit(i.transform.position, wps.waypointSize, ray));
            }
            
            //Reset current waypoint
            else if (lastWaypoint != null && e.type == EventType.MouseMove) {
                lastWaypoint = null;
            }
        }

        public override void OnInspectorGUI(){
            EditorGUI.BeginChangeCheck();
            
            //Editor properties
            EditorGUILayout.LabelField("Guizmo Config", EditorStyles.boldLabel);
            wps.hideGuizmos = EditorGUILayout.Toggle("Hide Guizmos", wps.hideGuizmos);
            
            //ArrowDrawType selection
            wps.arrowDrawType = (ArrowDraw) EditorGUILayout.EnumPopup("Arrow Draw Type", wps.arrowDrawType);
            EditorGUI.indentLevel++;

            switch (wps.arrowDrawType) {
                case ArrowDraw.FixedCount:
                    wps.arrowCount = Mathf.Max(1, EditorGUILayout.IntField("Count", wps.arrowCount));
                    break;
                case ArrowDraw.ByLength:
                    wps.arrowDistance = EditorGUILayout.FloatField("Distance Between Arrows", wps.arrowDistance);
                    break;
                case ArrowDraw.Off:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (wps.arrowDrawType != ArrowDraw.Off) {
                wps.arrowSizeWaypoint = EditorGUILayout.FloatField("Arrow Size Waypoint", wps.arrowSizeWaypoint);
                wps.arrowSizeIntersection = EditorGUILayout.FloatField("Arrow Size Intersection", wps.arrowSizeIntersection);
            }
            
            EditorGUI.indentLevel--;

            wps.waypointSize = EditorGUILayout.FloatField("Waypoint Size", wps.waypointSize);
            
            //System Config
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("System Config", EditorStyles.boldLabel);
            wps.segDetectThresh = EditorGUILayout.FloatField("Segment Detection Threshold", wps.segDetectThresh);
            
            //Helper
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Ctrl + Left Click to create a new segment\nShift + Left Click to create a new waypoint.\nAlt + Left Click to create a new intersection", MessageType.Info);
            EditorGUILayout.HelpBox("Reminder: The cars will follow the point depending on the sequence you added them. (go to the 1st waypoint added, then to the second, etc.)", MessageType.Info);


            //Rename waypoints if some have been deleted
            if(GUILayout.Button("Re-Structure Traffic System")){
                RestructureSystem();
            }

            //Repaint the scene if values have been edited
            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddWaypoint(Vector3 position) {
            GameObject go = CreateGameObjectWithUndo("Waypoint-" + wps.curSegment.waypoints.Count, wps.curSegment.transform);
            go.transform.position = position;

            Waypoint wp = AddComponentWithUndo<Waypoint>(go);
            wp.Refresh(wps.curSegment.waypoints.Count, wps.curSegment);

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps.curSegment, "");
            wps.curSegment.waypoints.Add(wp);
        }

        private void AddSegment(Vector3 position) {
            int segId = wps.segments.Count;
            GameObject segGo = CreateGameObjectWithUndo("Segment-" + segId, wps.transform.GetChild(0).transform);
            segGo.transform.position = position;

            wps.curSegment = AddComponentWithUndo<Segment>(segGo);
            wps.curSegment.id = segId;
            wps.curSegment.waypoints = new List<Waypoint>();
            wps.curSegment.nextSegments = new List<Segment>();

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
            wps.segments.Add(wps.curSegment);
        }

        private void AddIntersection(Vector3 position) {
            int intId = wps.intersections.Count;
            GameObject intGo = CreateGameObjectWithUndo("Intersection-" + intId, wps.transform.GetChild(1).transform);
            intGo.transform.position = position;

            BoxCollider bc = AddComponentWithUndo<BoxCollider>(intGo);
            bc.isTrigger = true;
            Intersection intersection = AddComponentWithUndo<Intersection>(intGo);
            intersection.id = intId;

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
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
            foreach(Intersection intersection in wps.intersections){
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

        private void BeginUndoGroup(string undoName) {
            //Create new Undo Group to collect all changes in one Undo
            Undo.SetCurrentGroupName(undoName);

            //Register all TrafficSystem changes after this (string not relevant here)
            Undo.RegisterFullObjectHierarchyUndo(wps.gameObject, undoName);
        }

        private static GameObject CreateGameObjectWithUndo(string name, Transform parent = null) {
            GameObject newGameObject = new GameObject(name);

            //Register changes (string not relevant here)
            Undo.RegisterCreatedObjectUndo(newGameObject, "Spawn new GameObject");
            Undo.SetTransformParent(newGameObject.transform, parent, "Set parent");

            return newGameObject;
        }

        private static T AddComponentWithUndo<T>(GameObject target) where T : Component {
            return Undo.AddComponent<T>(target);
        }
        
        //Determines if a ray hits a sphere
        private static bool SphereHit(Vector3 center, float radius, Ray r) {
            Vector3 oc = r.origin - center;
            float a = Vector3.Dot(r.direction, r.direction);
            float b = 2f * Vector3.Dot(oc, r.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4f * a * c;

            if (discriminant < 0f) {
                return false;
            }

            float sqrt = Mathf.Sqrt(discriminant);

            return -b - sqrt > 0f || -b + sqrt > 0f;
        }
    }
}
