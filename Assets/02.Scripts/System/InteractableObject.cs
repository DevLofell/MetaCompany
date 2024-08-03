using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public int info;
    [HideInInspector] public Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }
}