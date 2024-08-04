using UnityEngine;
using Cinemachine;

public class CinemachinePOVExtension : CinemachineExtension
{
    [SerializeField]
    private float horizontalSpeed = 10f;
    [SerializeField]
    private float verticalSpeed = 10f;
    [SerializeField]
    private float clampUpAngle = 80f;
    [SerializeField]
    private float clampDownAngle = 30f;
    private Vector3 startingRotation;
    private CinemachineVirtualCamera vir;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        vir = GetComponent<CinemachineVirtualCamera>();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow)
        {
            if (stage == CinemachineCore.Stage.Aim)
            {
                if (InputManager.instance.IsInputEnabled() && InputManager.instance.isRotateAble)
                {
                    if (startingRotation == Vector3.zero)
                    {
                        startingRotation = transform.localRotation.eulerAngles;
                        if (startingRotation.x > 180f) startingRotation.x -= 360f;
                    }
                    Vector2 deltaInput = InputManager.instance.GetMouseDelta();
                    startingRotation.y += deltaInput.x * horizontalSpeed * Time.deltaTime;
                    startingRotation.x -= deltaInput.y * verticalSpeed * Time.deltaTime;
                    startingRotation.x = Mathf.Clamp(startingRotation.x, -clampUpAngle, clampDownAngle);
                    state.RawOrientation = Quaternion.Euler(startingRotation.x, startingRotation.y, 0f);
                }
                else
                {
                    startingRotation = transform.localRotation.eulerAngles;
                    if (startingRotation.x > 180f) startingRotation.x -= 360f;
                }
            }
        }
    }
}