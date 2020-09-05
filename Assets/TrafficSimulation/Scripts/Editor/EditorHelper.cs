// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSimulation {
    public static class EditorHelper {
        public static void SetUndoGroup(string label) {
            //Create new Undo Group to collect all changes in one Undo
            Undo.SetCurrentGroupName(label);
        }
        
        public static void BeginUndoGroup(string undoName, TrafficSystem trafficSystem) {
            //Create new Undo Group to collect all changes in one Undo
            Undo.SetCurrentGroupName(undoName);

            //Register all TrafficSystem changes after this (string not relevant here)
            Undo.RegisterFullObjectHierarchyUndo(trafficSystem.gameObject, undoName);
        }

        public static GameObject CreateGameObject(string name, Transform parent = null) {
            GameObject newGameObject = new GameObject(name);

            //Register changes for Undo (string not relevant here)
            Undo.RegisterCreatedObjectUndo(newGameObject, "Spawn new GameObject");
            Undo.SetTransformParent(newGameObject.transform, parent, "Set parent");

            return newGameObject;
        }

        public static T AddComponent<T>(GameObject target) where T : Component {
            return Undo.AddComponent<T>(target);
        }
        
        //Determines if a ray hits a sphere
        public static bool SphereHit(Vector3 center, float radius, Ray r) {
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
