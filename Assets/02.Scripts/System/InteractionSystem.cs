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

    private void FixedUpdate()
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
            int index = hit.collider.GetComponent<InteractableObject>().info;
            if (interactables[index].CompareTag("Interactable"))
            {
                uiManager.UpdateInteractionUI(interactables[index].info, 1, false);
                if (inputManager.PlayerInteractionThisFrame())
                {
                    if (interactables[index].info == 1)
                    {

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