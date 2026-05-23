using UnityEngine;

public class CartBasket : MonoBehaviour
{
    // When an item falls into the trigger box...
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            // Make the item a child of the cart! 
            // It still has physics, but now it teleports when the cart teleports.
            other.transform.SetParent(transform.parent);
        }
    }

    // When the player grabs the item and pulls it out of the trigger box...
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            // Unparent it so it becomes an independent object again
            if (other.transform.parent == transform.parent)
            {
                other.transform.SetParent(null);
            }
        }
    }
}