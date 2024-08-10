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
    [SerializeField] private GameObject consoleObj;

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
    private Transform originalLookAtTarget;
    public GameObject grabObj;
    private InteractableObject hitObject;
    private Rigidbody rb;
    private PlayerAnimation anim;
    private InventorySystem inven;

    private void Awake()
    {
        interactableLayerMask = 1 << LayerMask.NameToLayer("Interactable");
        mainCamera = Camera.main;
        ray = new Ray();
        CacheInteractableObjects();
    }

    private void Start()
    {
        inven = GetComponent<InventorySystem>();
        anim = GetComponent<PlayerAnimation>();
        inputManager = InputManager.instance;
        uiManager = UIManager.instance;

        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
        else
        {
            originalLookAtTarget = virtualCamera.LookAt;
        }

        if (targetDir == null)
        {
            Debug.LogError("Target Dir is not assigned!");
        }
    }
    
    private void Update()
    {
        if (inputManager.IsInputEnabled() && inputManager.raycastAble)
        {
            RaycastCenter();
        }
        //if (inputManager.PlayerEndInteraction())
        {
            inputManager.raycastAble = true;
        }
        if (inputManager.PlayerDropItem())
        {
            inven.PullOutItem();
            anim.IsOneHand(false);
        }
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
            hitObject = hit.collider.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.CompareTag("Interactable"))
            {
                uiManager.UpdateInteractionUI(hitObject.info, 1, false);
                if (inputManager.PlayerInteractionThisFrame() && !isMoving && !isRotating)
                {
                    if (hitObject == null)
                    {
                        print("hit");
                    }
                    else
                    {
                        // E를 눌렀을 때 상호작용 시퀀스 시작
                        StartCoroutine(InteractionSequence(hitObject));
                    }


                    // 입력 비활성화
                    if (inputDisableCoroutine != null)
                        StopCoroutine(inputDisableCoroutine);
                    inputManager.raycastAble = false;
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
        if (hitObject.standingTr != null && hitObject.lookAtDir != null)
        {
            targetPosition = hitObject.standingTr.position;
            targetRotation = hitObject.lookAtDir.localRotation;

            targetDir = hitObject.lookAtDir;
            // 플레이어 이동이 완료된 후 카메라 Follow 변경
            virtualCamera.LookAt = targetDir;
            virtualCamera.LookAt.position = targetDir.position;
            virtualCamera.LookAt.rotation = targetDir.rotation;
            virtualCamera.LookAt = originalLookAtTarget;
        }

        // 오브젝트 타입에 따른 추가 동작
        switch (hitObject.type)
        {
            case ObjectType.SHIP_LEVER:
                // TODO: 회전, 위치 보간이동 > 회전은 계속, 위치는 일정 다가가면 고정
                // 플레이어 상하회전은 고개를 직접 회전
                // 일단 E 누르자마자 씬이동
                inputDisableCoroutine = StartCoroutine(DisableInputTemporarily());
                inputManager.isRotateAble = false;
                virtualCamera.LookAt = targetDir;
                yield return StartCoroutine(MoveAndRotatePlayer());
                virtualCamera.LookAt = originalLookAtTarget;
                break;
            case ObjectType.SHIP_CONSOLE:
                inputDisableCoroutine = StartCoroutine(DisableInputTemporarily());
                uiManager.UpdateInteractionUI(1, 0, false);
                consoleObj.SetActive(true);
                inputManager.isRotateAble = false;
                
                yield return StartCoroutine(MoveAndRotatePlayer());
                
                break;
            case ObjectType.SHIP_CHARGER:
            case ObjectType.ITEM_ONEHAND:
                inputManager.isAttackAble = true;
                rb = hitObject.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                uiManager.UpdateInteractionUI(0, 0, false);
                // E 누르면 인벤토리 Image 저장
                inven.PutIndexInventory(hitObject.gameObject, hitObject.icon);
                // 손의 좌표에 순간이동
                hitObject.transform.position = grabObj.transform.position;
                hitObject.transform.rotation = grabObj.transform.rotation;
                hitObject.transform.SetParent(grabObj.transform);
                hitObject.GetComponent<BoxCollider>().enabled = false;
                anim.IsOneHand(true);

                break;
            case ObjectType.ITEM_TWOHAND:
                rb = hitObject.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                uiManager.UpdateInteractionUI(0, 0, false);
                // E 누르면 인벤토리 Image 저장
                inven.PutIndexInventory(hitObject.gameObject, hitObject.icon);
                // 손의 좌표에 순간이동
                hitObject.transform.position = grabObj.transform.position;
                hitObject.transform.rotation = grabObj.transform.rotation;

                hitObject.transform.SetParent(grabObj.transform);
                //공격시에 타이밍에 맞춰 true
                hitObject.GetComponent<BoxCollider>().enabled = false;
                anim.IsTwoHand(true);
                //TODO : 내려놓으면 애니메이션 해제
                //공격시 콜라이더 on
                //OntriggerEnter
                //스크립터블오브젝트 클래스 넣기
                break;
        }
    }

    private IEnumerator MoveAndRotatePlayer()
    {
        isMoving = true;
        isRotating = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < Mathf.Max(moveDuration, rotationDuration))
        {
            float t = elapsedTime / Mathf.Max(moveDuration, rotationDuration);
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            gameObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        while (true)
        {
            if (inputManager.PlayerEndInteraction())
            {
                consoleObj.SetActive(false);
                inputManager.EnableInput(true);
                inputManager.isRotateAble = true;
                break;
            }

            // 콘솔 화면만 보게 돌려주는 기능 추가
            if (hitObject.type == ObjectType.SHIP_CONSOLE)
            {
                float newYRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, newYRotation, 0f);
            }

            yield return null;
        }

        isMoving = false;
        isRotating = false;
    }

    private IEnumerator DisableInputTemporarily()
    {
        inputManager.EnableInput(false);
        
        yield return new WaitForSeconds(inputDisableDuration);
    }
}