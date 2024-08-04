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
                if (InputManager.instance.IsInputEnabled())
                {
                    if (startingRotation == null)
                    {
                        startingRotation = transform.localRotation.eulerAngles;
                    }
                    Vector2 deltaInput = InputManager.instance.GetMouseDelta();
                    startingRotation.x += deltaInput.x * horizontalSpeed * Time.deltaTime;
                    startingRotation.y += deltaInput.y * verticalSpeed * Time.deltaTime;
                    startingRotation.y = Mathf.Clamp(startingRotation.y, -clampDownAngle, clampUpAngle);
                    state.RawOrientation = Quaternion.Euler(-startingRotation.y, startingRotation.x, 0f);
                }
                else
                {
                    //TODO : 나중에 상호작용이 끝나는 순간의 각도롤 적용
                    startingRotation = new Vector3(90f, 0f, 0f);
                }
            }
        }
    }
}
