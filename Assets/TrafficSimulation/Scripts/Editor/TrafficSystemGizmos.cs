// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TrafficSimulation {
    public static class TrafficSystemGizmos {
        //Custom Gizmo function
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
        private static void DrawGizmo(TrafficSystem script, GizmoType gizmoType) {
            //Don't go further if we hide gizmos
            if (script.hideGuizmos) {
                return;
            }

            foreach (Segment segment in script.segments) {
                //Draw segment names
                GUIStyle style = new GUIStyle {normal = {textColor = new Color(1, 0, 0)}, fontSize = 15};
                Handles.Label(segment.transform.position, segment.name, style);

                //Draw waypoint
                for (int j = 0; j < segment.waypoints.Count; j++) {
                    //Get current waypoint position
                    Vector3 p = segment.waypoints[j].GetVisualPos();

                    //Draw sphere, increase color to show the direction
                    Gizmos.color = new Color(0f, 0f, 1f, (j + 1) / (float) segment.waypoints.Count);
                    Gizmos.DrawSphere(p, script.waypointSize);
                    
                    //Get next waypoint position
                    Vector3 pNext = Vector3.zero;

                    if (j < segment.waypoints.Count - 1 && segment.waypoints[j + 1] != null) {
                        pNext = segment.waypoints[j + 1].GetVisualPos();
                    }

                    if (pNext != Vector3.zero) {
                        if (segment == script.curSegment) {
                            Gizmos.color = new Color(1f, .3f, .1f);
                        } else {
                            Gizmos.color = new Color(1f, 0f, 0f);
                        }

                        //Draw line
                        Gizmos.DrawLine(p, pNext);

                        int arrows = 0;

                        //Set arrowCount based on arrowDrawType
                        switch (script.arrowDrawType) {
                            case ArrowDraw.FixedCount:
                                arrows = script.arrowCount;
                                break;
                            case ArrowDraw.ByLength:
                                //Minimum of one arrow
                                arrows = Mathf.Max(1, (int) (Vector3.Distance(p, pNext) / script.arrowDistance));
                                break;
                            case ArrowDraw.Off:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        Vector3 forward = (p - pNext).normalized * script.arrowSizeWaypoint;
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
                foreach (Segment nextSegment in segment.nextSegments) {
                    if (nextSegment != null) {
                        Vector3 p1 = segment.waypoints.Last().GetVisualPos();
                        Vector3 p2 = nextSegment.waypoints.First().GetVisualPos();

                        Gizmos.color = new Color(1f, 1f, 0f);
                        Gizmos.DrawLine(p1, p2);

                        if (script.arrowDrawType != ArrowDraw.Off) {
                            //Draw arrow
                            Vector3 center = (p1 + p2) / 2f;
                            Vector3 forward = (p1 - p2).normalized * script.arrowSizeIntersection;
                            Vector3 left = Quaternion.Euler(0, 45, 0) * forward;
                            Vector3 right = Quaternion.Euler(0, -45, 0) * forward;

                            Gizmos.DrawLine(center, center + left);
                            Gizmos.DrawLine(center, center + right);
                        }
                    }
                }
            }
        }
    }
}
