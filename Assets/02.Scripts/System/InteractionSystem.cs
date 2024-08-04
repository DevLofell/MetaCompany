using System.Collections;
using UnityEngine;
using Cinemachine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private float inputDisableDuration = 0.5f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform targetDir; // 상호작용 시 Follow 타겟

    private int interactableLayerMask;
    private InteractableObject[] interactables;
    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;
    private InputManager inputManager;
    private UIManager uiManager;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool isRotating = false;
    private Coroutine inputDisableCoroutine;
    private Transform originalFollowTarget; // 원래의 Follow 타겟

    private void Awake()
    {
        interactableLayerMask = 1 << LayerMask.NameToLayer("Interactable");
        mainCamera = Camera.main;
        ray = new Ray();
        CacheInteractableObjects();
    }

    private void Start()
    {
        inputManager = InputManager.instance;
        uiManager = UIManager.instance;

        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
        else
        {
            originalFollowTarget = virtualCamera.Follow;
        }

        if (targetDir == null)
        {
            Debug.LogError("Target Dir is not assigned!");
        }
    }

    private void Update()
    {
        RaycastCenter();
    }

    private void CacheInteractableObjects()
    {
        interactables = FindObjectsOfType<InteractableObject>();
    }

    private void RaycastCenter()
    {
        ray.origin = mainCamera.transform.position;
        ray.direction = mainCamera.transform.forward;
        bool hitDetected = Physics.Raycast(ray, out hit, rayDistance, interactableLayerMask);

        if (hitDetected)
        {
            InteractableObject hitObject = hit.collider.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.CompareTag("Interactable"))
            {
                uiManager.UpdateInteractionUI(hitObject.info, 1, false);
                if (inputManager.PlayerInteractionThisFrame() && !isMoving && !isRotating)
                {
                    targetPosition = hitObject.standingTr.position;
                    targetRotation = hitObject.lookAtDir.localRotation;

                    targetDir = hitObject.lookAtDir;

                    // E를 눌렀을 때 상호작용 시퀀스 시작
                    StartCoroutine(InteractionSequence(hitObject));

                    // 입력 비활성화
                    if (inputDisableCoroutine != null)
                        StopCoroutine(inputDisableCoroutine);
                    inputDisableCoroutine = StartCoroutine(DisableInputTemporarily());
                }
                return;
            }
        }
        else
        {
            uiManager.UpdateInteractionUI(0, 0, true);
        }
    }

    private IEnumerator InteractionSequence(InteractableObject hitObject)
    {
        // 먼저 플레이어를 이동 및 회전
        

        // 플레이어 이동이 완료된 후 카메라 Follow 변경
        //virtualCamera.LookAt = targetDir;
        //virtualCamera.LookAt.forward = targetDir.forward - virtualCamera.LookAt.forward;
        targetDir.position = hitObject.lookAtDir.position;
        targetDir.rotation = hitObject.lookAtDir.rotation;

        // 오브젝트 타입에 따른 추가 동작
        switch (hitObject.type)
        {
            case ObjectType.SHIP_LEVER:
                // TODO: 회전, 위치 보간이동 > 회전은 계속, 위치는 일정 다가가면 고정
                break;
            case ObjectType.SHIP_CONSOLE:
                // TODO: 콘솔 전원 끄고 켜기
                inputManager.isRotateAble = false;
                yield return StartCoroutine(MoveAndRotatePlayer());
                break;
            case ObjectType.SHIP_CHARGER:
            case ObjectType.ITEM_ONEHAND:
            case ObjectType.ITEM_TWOHAND:
                // 추가 동작이 필요한 경우 여기에 구현
                break;
        }

        // 일정 시간 후 원래의 Follow 타겟으로 복귀 (필요에 따라 조정 또는 제거)
        //yield return new WaitForSeconds(3f);
        //virtualCamera.LookAt = originalFollowTarget;
    }

    private IEnumerator MoveAndRotatePlayer()
    {
        isMoving = true;
        isRotating = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startTargetDirPosition = targetDir.position;
        Quaternion startTargetDirRotation = targetDir.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < Mathf.Max(moveDuration, rotationDuration))
        {
            float t = elapsedTime / Mathf.Max(moveDuration, rotationDuration);

            // 플레이어 이동 및 회전
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치와 회전 설정
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        while (true)
        {
            if (inputManager.PlayerEndInteraction())
            {
                break;
            }

            float newYRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, 0.1f);

            transform.rotation = Quaternion.Euler(0f, newYRotation, 0f);
            yield return null;
        }

        isMoving = false;
        isRotating = false;
    }

    private IEnumerator DisableInputTemporarily()
    {
        //inputManager.EnableInput(false);
        yield return new WaitForSeconds(inputDisableDuration);
        //inputManager.EnableInput(true);
    }
}