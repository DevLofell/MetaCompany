using UnityEngine;
using UnityEngine.UI;

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
    public Transform lookAtDir;
    public Sprite icon;

    private ItemComponent item;

    private void Start()
    {
        item = GetComponent<ItemComponent>();
        ItemInitializ();
    }

    private void ItemInitializ()
    {
        if (item != null)
        {
            switch (item.hand)
            {
                case "One":
                    type = ObjectType.ITEM_ONEHAND;
                    break;
                case "Two":
                    if (gameObject.name == "FancyLamp")
                    {
                        type = ObjectType.ITEM_ONEHAND;
                    }
                    else
                    {
                        type = ObjectType.ITEM_TWOHAND;
                    }
                    break;
            }
        }
    }
}