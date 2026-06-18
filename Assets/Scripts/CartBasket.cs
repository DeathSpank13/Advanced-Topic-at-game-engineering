using UnityEngine;

public class CartBasket : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        // Automatically find the GameManager in the scene so you don't have to link it manually
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            // Keep your existing parenting logic so it doesn't clip through the floor
            other.transform.SetParent(transform.parent);

            // Read the item's name and send it to the GameManager
            ItemInfo info = other.GetComponent<ItemInfo>();
            if (info != null && gameManager != null)
            {
                gameManager.ItemCollected(info.itemName);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            if (other.transform.parent == transform.parent)
            {
                other.transform.SetParent(null);
            }

            // If the item leaves the basket, tell the GameManager to un-check the box
            ItemInfo info = other.GetComponent<ItemInfo>();
            if (info != null && gameManager != null)
            {
                gameManager.ItemRemoved(info.itemName);
            }
        }
    }
}