using UnityEngine;

public enum ObjectType
{
    SHIP_LEVER,
    SHIP_CONSOLE,
    SHIP_CHARGER,
    ITEM_ONEHAND,
    ITEM_TWOHAND
};
public class InteractableObject : MonoBehaviour
{
    public ObjectType type;
    public int info;
    public Transform standingTr;
}