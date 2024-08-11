using UnityEngine;

public class CoilHeadNeckSpring : MonoBehaviour
{
    public Transform neck;
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform head;
    public Transform headEnd;
    public float springStrength = 10f;
    public float springDamping = 0.5f;
    public float returnSpeed = 1f;
    public float maxStretch = 0.01f;
    public float maxVelocity = 1f;
    public float maxRotationAngle = 30f;
    private Vector3[] initialLocalPositions;
    private Quaternion[] initialLocalRotations;

    void Start()
    {
        initialLocalPositions = new Vector3[6];
        initialLocalRotations = new Quaternion[6];
        Transform[] joints = { neck, joint1, joint2, joint3, head, headEnd };
        for (int i = 0; i < joints.Length; i++)
        {
            initialLocalPositions[i] = joints[i].localPosition;
            initialLocalRotations[i] = joints[i].localRotation;
        }
    }

    void LateUpdate()
    {
        ApplySpringToJoint(neck, null);
        ApplySpringToJoint(joint1, neck);
        ApplySpringToJoint(joint2, joint1);
        ApplySpringToJoint(joint3, joint2);
        ApplySpringToJoint(head, joint3);
        ApplySpringToJoint(headEnd, head);
    }
    void ApplySpringToJoint(Transform joint, Transform parent)
    {
        if (joint == null) return;
        Rigidbody rb = joint.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 initialLocalPos = GetInitialLocalPosition(joint);
        Vector3 currentLocalPos = joint.localPosition;
        Vector3 targetLocalPosition = initialLocalPos;

        // 위치 제한
        Vector3 offset = currentLocalPos - initialLocalPos;
        if (offset.magnitude > maxStretch)
        {
            currentLocalPos = initialLocalPos + offset.normalized * maxStretch;
            joint.localPosition = currentLocalPos;
        }

        Vector3 force = (targetLocalPosition - currentLocalPos) * springStrength;
        rb.AddForce(force, ForceMode.Acceleration);

        // 속도 제한
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }



        // 회전 제한 (기존 코드와 동일)
        Quaternion targetRotation = GetInitialLocalRotation(joint);
        joint.localRotation = Quaternion.RotateTowards(joint.localRotation, targetRotation, returnSpeed * Time.deltaTime);

        Vector3 currentEuler = joint.localRotation.eulerAngles;
        currentEuler.x = Mathf.Clamp(currentEuler.x, -maxRotationAngle, maxRotationAngle);
        currentEuler.y = Mathf.Clamp(currentEuler.y, -maxRotationAngle, maxRotationAngle);
        currentEuler.z = Mathf.Clamp(currentEuler.z, -maxRotationAngle, maxRotationAngle);
        joint.localRotation = Quaternion.Euler(currentEuler);

        if (parent != null)
        {
            float maxDistance = Vector3.Distance(GetInitialLocalPosition(parent), initialLocalPos) + maxStretch;
            Vector3 directionToParent = parent.position - joint.position;
            if (directionToParent.magnitude > maxDistance)
            {
                joint.position = parent.position - directionToParent.normalized * maxDistance;
            }
        }
    }

    Vector3 GetInitialLocalPosition(Transform joint)
    {
        int index = System.Array.IndexOf(new Transform[] { neck, joint1, joint2, joint3, head, headEnd }, joint);
        return initialLocalPositions[index];
    }

    Quaternion GetInitialLocalRotation(Transform joint)
    {
        int index = System.Array.IndexOf(new Transform[] { neck, joint1, joint2, joint3, head, headEnd }, joint);
        return initialLocalRotations[index];
    }
}