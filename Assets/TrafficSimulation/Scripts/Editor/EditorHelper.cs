// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

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

        //From S_Darkwell: https://forum.unity.com/threads/adding-layer-by-script.41970/
        public static void CreateLayer(string name){
            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            SerializedProperty firstEmptyProp = null;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);

                var stringValue = layerProp.stringValue;

                if (stringValue == name) return;

                if (i < 8 || stringValue != string.Empty) continue;

                if (firstEmptyProp == null)
                    firstEmptyProp = layerProp;
            }

            if (firstEmptyProp == null)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
                return;
            }

            firstEmptyProp.stringValue = name;
            tagManager.ApplyModifiedProperties();
        }

        //From SkywardRoy: https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/
        public static void SetLayer (this GameObject gameObject, int layer, bool includeChildren = false) {
            if (!includeChildren) {
                gameObject.layer = layer;
                return;
            }
        
            foreach (var child in gameObject.GetComponentsInChildren(typeof(Transform), true)) {
                child.gameObject.layer = layer;
            }
        }
    }
}
