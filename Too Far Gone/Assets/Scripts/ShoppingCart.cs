using UnityEngine;

public class ShoppingCart : MonoBehaviour
{
    [Header("Rigidbody")]
    [SerializeField] private Rigidbody rb;

    [Header("Front Wheels")]
    [SerializeField] private WheelCollider frontLeftWheel;
    [SerializeField] private WheelCollider frontRightWheel;
    [SerializeField] private Transform frontLeftWheelModel;
    [SerializeField] private Transform frontRightWheelModel;

    [Header("Rear Wheels")]
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;
    [SerializeField] private Transform rearLeftWheelModel;
    [SerializeField] private Transform rearRightWheelModel;

    [Header("Driving")]
    [SerializeField] private float motorTorque = 5000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float rigidbodyMaxSpeed = 100f;

    [Header("Anti Tip")]
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float antiTipStrength = 80f;
    [SerializeField] private float antiTipDamping = 8f;
    [SerializeField] private float centerOfMassYOffset = -0.25f;

    private float driveInput;
    private float steerInput;

    private void Awake()
    {
        if (rb == null)
        {
            return;
        }

        rb.centerOfMass += Vector3.up * centerOfMassYOffset;
        rb.maxLinearVelocity = rigidbodyMaxSpeed;
    }

    private void Update()
    {
        driveInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxisRaw("Horizontal");

        UpdateWheelModels();
    }

    private void FixedUpdate()
    {
        Drive();
        Steer();
        PreventTipping();
    }

    private void Drive()
    {
        if (rb == null)
        {
            return;
        }

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float torque = driveInput * motorTorque;

        if (Mathf.Abs(forwardSpeed) >= maxSpeed &&
            Mathf.Sign(driveInput) == Mathf.Sign(forwardSpeed))
        {
            torque = 0f;
        }

        if (frontLeftWheel != null)
        {
            frontLeftWheel.motorTorque = torque;
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.motorTorque = torque;
        }

        if (rearLeftWheel != null)
        {
            rearLeftWheel.motorTorque = 0f;
        }

        if (rearRightWheel != null)
        {
            rearRightWheel.motorTorque = 0f;
        }
    }

    private void Steer()
    {
        float steerAngle = steerInput * maxSteerAngle;

        if (frontLeftWheel != null)
        {
            frontLeftWheel.steerAngle = steerAngle;
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.steerAngle = steerAngle;
        }

        if (rearLeftWheel != null)
        {
            rearLeftWheel.steerAngle = 0f;
        }

        if (rearRightWheel != null)
        {
            rearRightWheel.steerAngle = 0f;
        }
    }

    private void PreventTipping()
    {
        if (rb == null)
        {
            return;
        }

        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle <= maxTiltAngle)
        {
            return;
        }

        Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);

        if (correctionAxis.sqrMagnitude < 0.001f)
        {
            return;
        }

        correctionAxis.Normalize();

        float excessTilt = tiltAngle - maxTiltAngle;
        float correctionStrength = excessTilt * antiTipStrength;

        rb.AddTorque(
            correctionAxis * correctionStrength,
            ForceMode.Acceleration);

        Vector3 tippingAngularVelocity = new Vector3(
            rb.angularVelocity.x,
            0f,
            rb.angularVelocity.z);

        rb.AddTorque(
            -tippingAngularVelocity * antiTipDamping,
            ForceMode.Acceleration);
    }

    private void UpdateWheelModels()
    {
        UpdateWheelModel(frontLeftWheel, frontLeftWheelModel);
        UpdateWheelModel(frontRightWheel, frontRightWheelModel);
        UpdateWheelModel(rearLeftWheel, rearLeftWheelModel);
        UpdateWheelModel(rearRightWheel, rearRightWheelModel);
    }

    private void UpdateWheelModel(WheelCollider wheelCollider, Transform wheelModel)
    {
        if (wheelCollider == null || wheelModel == null)
        {
            return;
        }

        wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        wheelModel.position = position;
        wheelModel.rotation = rotation;
    }
}