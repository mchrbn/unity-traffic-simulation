/*

   Class from Polarith AI Free | Movement, Steering
   http://polarith.com/ai/ 

 */

using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation
{
    public class VehiclePhysics : MonoBehaviour
    {

        /// <summary>
        /// All wheelCollider of the vehicle.
        /// </summary>
        [Tooltip("All wheelCollider of the vehicle.")]
        public List<WheelCollider> WheelColliders = new List<WheelCollider>();

        /// <summary>
        /// A subset of <see cref="WheelColliders"/> containing all instances to which motor torque is applied.
        /// </summary>
        [Tooltip("A subset of 'WheelColliders' containing all instances to which motor torque is applied.")]
        public List<WheelCollider> MotorWheels = new List<WheelCollider>();

        /// <summary>
        /// A subset of <see cref="WheelColliders"/> containing all instances to which brake torque is applied.
        /// </summary>
        [Tooltip("A subset of 'WheelColliders' containing all instances to which brake torque is applied.")]
        public List<WheelCollider> BrakeWheels = new List<WheelCollider>();

        /// <summary>
        /// A subset of <see cref="WheelColliders"/> containing all instances that are able to steer.
        /// </summary>
        [Tooltip("A subset of 'WheelColliders' containing all instances that are able to steer.")]
        public List<WheelCollider> SteeringWheels = new List<WheelCollider>();

        /// <summary>
        /// The mesh representation of the wheels. They are rotated according to the corresponding <see
        /// cref="WheelColliders"/>.
        /// </summary>
        [Tooltip("The mesh representation of the wheels. They are rotated according to the corresponding " +
            "'WheelColliders'")]
        public List<GameObject> WheelMeshes = new List<GameObject>();

        /// <summary>
        /// Maximum possible angle for steering in degrees.
        /// </summary>
        [Tooltip("Maximum possible angle for steering in degrees..")]
        public float MaximumSteerAngle = 25.0f;

        /// <summary>
        /// Determines the magnitude of the applied steering helper. From 0 = raw physics to 1 the car will grip in the
        /// direction it is facing.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("Determines the magnitude of the applied steering helper. From 0 = raw physics to 1 the car will " +
            "grip in the direction it is facing.")]
        public float SteerHelper;

        /// <summary>
        /// The magnitude of the applied traction control. From 0 = no traction control to 1 = full interference.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("The magnitude of the applied traction control. From 0 = no traction control to 1 = full " +
            "interference.")]
        public float TractionControl;

        /// <summary>
        /// The torque that is applied to all together over the motor wheels. Hence, the torque per wheel is
        /// FullTorqueOverAllWheels / <see cref="MotorWheels"/>.Count.
        /// </summary>
        [Tooltip("The torque that is applied to all together over the motor wheels. Hence, the torque per wheel " +
            "is FullTorqueOverAllWheels / MotorWheels.Count.")]
        public float FullTorqueOverAllWheels;

        /// <summary>
        /// The torque for driving backwards.
        /// </summary>
        [Tooltip("The torque for driving backwards.")]
        public float ReverseTorque;

        /// <summary>
        /// Force to create more grip.
        /// </summary>
        [Tooltip("Force to create more grip.")]
        public float Downforce = 100f;

        /// <summary>
        /// The maximum speed of the vehicle in km/h.
        /// </summary>
        [HideInInspector]
        public float Topspeed = 200;

        /// <summary>
        /// A forward slip bigger than this will activate TractionControl.
        /// </summary>
        [Tooltip("A forward slip bigger than this will activate TractionControl.")]
        public float SlipLimit;

        /// <summary>
        /// Brake torque for every BrakeWheel.
        /// </summary>
        [Tooltip("Brake torque for every BrakeWheel.")]
        public float BrakeTorque;

        //--------------------------------------------------------------------------------------------------------------

        private Rigidbody body;
        private float oldRotation;
        private float currentTorque;

        /// <summary>
        /// Brings the input acceleration and steer angle to the streets by using the <see cref="WheelColliders"/>.
        /// </summary>
        /// <param name="steering">
        /// Expects a value in the range of [-1, 1] which is then mapped to [- <see cref="MaximumSteerAngle"/>, <see
        /// cref="MaximumSteerAngle"/>]. Thus, -1 means turn left and 1 turn right, from the cars point of view.
        /// </param>
        /// <param name="acceleration">
        /// Expects a value in the range [0, 1]. It translates into the current torque that is applied to the
        /// <see cref="MotorWheels"/> where 0 is don't move and 1 is full possible torque.
        /// </param>
        /// <param name="brake">
        /// Expects a value in the range [-1, 0]. Translates into a torque that is applied to the <see
        /// cref="BrakeWheels"/> in the opposite direction. To achieve a braking effect.
        /// </param>
        public void Move(float steering, float acceleration, float brake)
        {
            if (!enabled)
                return;

            // Clamp the input values
            steering = Mathf.Clamp(steering, -1.0f, 1.0f);
            acceleration = Mathf.Clamp(acceleration, 0.0f, 1.0f);
            brake = -1.0f * Mathf.Clamp(brake, -1.0f, 0.0f);

            ApplySteeringAngle(steering);

            ApplySteerHelper();

            ApplyDrive(acceleration, brake);

            CapSpeed();

            ApplyDownforce();

            ApplyTractionControl();
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            currentTorque = FullTorqueOverAllWheels - (TractionControl * FullTorqueOverAllWheels);
            WheelColliders[0].ConfigureVehicleSubsteps(1.0f, 12, 15);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Update()
        {
            // Update wheel representation
            Quaternion quat;
            Vector3 position;
            for (int i = 0; i < WheelColliders.Count; i++)
            {
                WheelColliders[i].GetWorldPose(out position, out quat);
                WheelMeshes[i].transform.position = position;
                WheelMeshes[i].transform.rotation = quat;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ApplyDrive(float acceleration, float brake)
        {
            float thrustTorque = 0.0f;

            if (MotorWheels.Count > 0)
                thrustTorque = acceleration * (currentTorque / MotorWheels.Count);

            foreach (WheelCollider motorWheel in MotorWheels)
                motorWheel.motorTorque = thrustTorque;

            // Reset brake torque, otherwise this could lead to problems and the car won´t start again
            foreach (WheelCollider brakeWheel in BrakeWheels)
                brakeWheel.brakeTorque = 0f;

            // Use brake torque if the vehicle is moving fast. Else, use the motor for braking.
            if (body.velocity.magnitude > 1 && Vector3.Angle(transform.forward, body.velocity) < 50f)
            {
                for (int i = 0; i < BrakeWheels.Count; i++)
                    BrakeWheels[i].brakeTorque = BrakeTorque * brake;
            }
            else if (brake > 0)
            {
                for (int i = 0; i < MotorWheels.Count; i++)
                    MotorWheels[i].motorTorque = -ReverseTorque * brake;
            }

        }

        //--------------------------------------------------------------------------------------------------------------

        private void ApplySteeringAngle(float steeringAngle)
        {
            steeringAngle = steeringAngle * MaximumSteerAngle;
            foreach (WheelCollider steeringWheel in SteeringWheels)
                steeringWheel.steerAngle = steeringAngle;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ApplyDownforce()
        {
            body.AddForce(-transform.up * Downforce * body.velocity.magnitude);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ApplyTractionControl()
        {
            WheelHit wheelHit;
            for (int i = 0; i < MotorWheels.Count; i++)
            {
                MotorWheels[0].GetGroundHit(out wheelHit);
                if (wheelHit.forwardSlip >= SlipLimit && currentTorque >= 0)
                {
                    currentTorque -= 10.0f * TractionControl;
                }
                else
                {
                    currentTorque += 10.0f * TractionControl;
                    if (currentTorque > FullTorqueOverAllWheels)
                        currentTorque = FullTorqueOverAllWheels;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ApplySteerHelper()
        {
            WheelHit wheelhit;

            // Check if all wheels are actually on the ground and return if not
            foreach (WheelCollider wheel in WheelColliders)
            {
                wheel.GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return;
            }

            // Avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10.0f)
            {
                float turnadjust = (transform.eulerAngles.y - oldRotation) * SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                body.velocity = velRotation * body.velocity;
            }
            oldRotation = transform.eulerAngles.y;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void CapSpeed()
        {
            float speed = body.velocity.magnitude;

            speed *= 3.6f;
            if (speed > Topspeed)
                body.velocity = (Topspeed / 3.6f) * body.velocity.normalized;

        }
    }
}
