using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float rayDistance = 1f;
    private int interactableLayerMask;
    private InteractableObject[] interactables;
    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;
    private InputManager inputManager;
    private UIManager uiManager;

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
            int index = hitObject.info;
            if (hitObject != null && hitObject.CompareTag("Interactable"))
            {
                uiManager.UpdateInteractionUI(hitObject.info, 1, false);
                if (inputManager.PlayerInteractionThisFrame())
                {
                    switch (hitObject.type)
                    {
                        case ObjectType.SHIP_LEVER://TODO : 회전,위치 보간이동 > 회전은 계속, 위치는 일정 다가가면 고정
                            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, hitObject.standingTr.position, Time.deltaTime);
                            break;
                        case ObjectType.SHIP_CONSOLE://TODO : 콘솔 전원켜기
                            gameObject.transform.position = hitObject.standingTr.position;
                            break;
                        case ObjectType.SHIP_CHARGER:
                            break;
                        case ObjectType.ITEM_ONEHAND:
                            break;
                        case ObjectType.ITEM_TWOHAND:
                            break;
                    }
                }
                return;
            }
        }
        else
        {
            uiManager.UpdateInteractionUI(0, 0, true);
        }
    }
}