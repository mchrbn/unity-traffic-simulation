using UnityEngine;
using TrafficSimulation;

public class VehicleController : MonoBehaviour
{

    WheelDrive wheelDrive;

    void Start()
    {
        wheelDrive = this.GetComponent<WheelDrive>();
    }

    void Update()
    {
        float acc = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        float brake = Input.GetKey(KeyCode.Space) ? 1 : 0;

        wheelDrive.Move(acc, steering, brake);
    }
}
