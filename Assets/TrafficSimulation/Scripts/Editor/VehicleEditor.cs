// Traffic Simulation
// https://github.com/mchrbn/unity-traffic-simulation

using UnityEngine;
using UnityEditor;

namespace TrafficSimulation{
    public class VehicleEditor : Editor
    {
        [MenuItem("Component/Traffic Simulation/Setup Vehicle")]
        private static void SetupVehicle(){
            EditorHelper.SetUndoGroup("Setup Vehicle");

            GameObject selected = Selection.activeGameObject;

            //Create raycast anchor
            GameObject anchor = EditorHelper.CreateGameObject("Raycast Anchor", selected.transform);

            //Add AI scripts
            VehicleAI veAi = EditorHelper.AddComponent<VehicleAI>(selected);
            WheelDrive wheelDrive = EditorHelper.AddComponent<WheelDrive>(selected);

            TrafficSystem ts = GameObject.FindObjectOfType<TrafficSystem>();

            //Configure the vehicle AI script with created objects
            anchor.transform.localPosition = Vector3.zero;
            anchor.transform.localRotation = Quaternion.Euler(Vector3.zero);
            veAi.raycastAnchor = anchor.transform;

            if(ts != null) veAi.trafficSystem = ts;

            //Create layer AutonomousVehicle if it doesn't exist
            EditorHelper.CreateLayer("AutonomousVehicle");
            
            //Set the tag and layer name
            selected.tag = "AutonomousVehicle";
            EditorHelper.SetLayer(selected, LayerMask.NameToLayer("AutonomousVehicle"), true);
        }
    }
}