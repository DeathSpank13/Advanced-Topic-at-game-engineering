using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Core References")]
    public Transform playerCamera;
    public Collider playerBodyCollider;

    [Header("3D UI Settings")]
    public GameObject indicatorCanvas; // Drag the whole '3D_Indicator' here
    public Text indicatorText;         // Drag the 'IndicatorText' here

    [Header("Raycast Settings")]
    public LayerMask interactableLayer;
    public float interactionRadius = 0.25f;

    [Header("Cart Settings")]
    public Transform cartHoldPoint;
    public float cartRange = 3f;
    public string cartTag = "Cart";
    public Vector3 cartHoldOffset = new Vector3(0f, -0.4f, 0.65f);
    public Vector3 cartRotOffset = new Vector3(-90f, 0f, 0f);

    [Header("Item Settings")]
    public Transform itemHoldPoint;
    public float itemRange = 3f;
    public string itemTag = "Item";

    [Header("Throw Settings")]
    public float maxThrowForce = 15f;
    public float chargeRate = 15f;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float trajectoryTimeStep = 0.05f;

    // State trackers
    private GameObject heldCart = null;
    private Rigidbody cartRb;
    public bool isHoldingCart = false;

    private GameObject heldItem = null;
    private Rigidbody itemRb;
    public bool isHoldingItem = false;

    // Throw state trackers
    private bool isChargingThrow = false;
    private float currentThrowForce = 0f;

    void Start()
    {
        if (trajectoryLine != null) trajectoryLine.enabled = false;
        if (indicatorCanvas != null) indicatorCanvas.SetActive(false); // Hide marker on start
    }

    void Update()
    {
        UpdateUI();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isHoldingCart) ReleaseCart();
            else if (!isHoldingItem) TryGrabCart();
        }

        if (isHoldingItem)
        {
            if (Input.GetMouseButtonDown(1) && !isChargingThrow)
            {
                isChargingThrow = true;
                currentThrowForce = 2f;
                trajectoryLine.enabled = true;
            }

            if (isChargingThrow)
            {
                if (Input.GetMouseButton(1))
                {
                    currentThrowForce += chargeRate * Time.deltaTime;
                    currentThrowForce = Mathf.Clamp(currentThrowForce, 0, maxThrowForce);
                    DrawTrajectory();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    isChargingThrow = false;
                    trajectoryLine.enabled = false;
                    currentThrowForce = 0f;
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    ExecuteThrow();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                DropItem();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !isHoldingCart)
            {
                TryPickupItem();
            }
        }
    }

    // --- UPGRADED 3D UI METHOD ---
    void UpdateUI()
    {
        if (indicatorCanvas == null || indicatorText == null) return;

        // 1. If holding cart, float the marker slightly above the handle
        if (isHoldingCart)
        {
            indicatorCanvas.SetActive(true);
            indicatorCanvas.transform.position = cartHoldPoint.position + (Vector3.up * 0.3f);
            FaceCamera();
            indicatorText.text = "[E]\nRelease Cart";
            return;
        }

        // 2. If holding item, float the marker next to the item in your hand
        if (isHoldingItem)
        {
            indicatorCanvas.SetActive(true);
            indicatorCanvas.transform.position = itemHoldPoint.position + (Vector3.right * 0.3f);
            FaceCamera();

            if (isChargingThrow)
                indicatorText.text = "Release [Right-Click] Throw\n[Left-Click] Cancel";
            else
                indicatorText.text = "[Left-Click] Drop\n[Hold Right-Click] Throw";
            return;
        }

        // 3. Not holding anything? Shoot the SphereCast to find objects in the world!
        RaycastHit hit;
        float maxCastDistance = Mathf.Max(cartRange, itemRange);

        if (Physics.SphereCast(playerCamera.position, interactionRadius, playerCamera.forward, out hit, maxCastDistance, interactableLayer))
        {
            bool validTarget = false;

            if (hit.collider.CompareTag(cartTag) && hit.distance <= cartRange)
            {
                indicatorText.text = "[E]\nGrab Cart";
                validTarget = true;
            }
            else if (hit.collider.CompareTag(itemTag) && hit.distance <= itemRange)
            {
                indicatorText.text = "[Left-Click]\nPick Up";
                validTarget = true;
            }

            if (validTarget)
            {
                indicatorCanvas.SetActive(true);
                // Snap the marker EXACTLY to where the raycast hit the object
                // We pull it backward slightly (0.05f) so it doesn't clip inside the 3D model
                indicatorCanvas.transform.position = hit.point - (playerCamera.forward * 0.05f);
                FaceCamera();
                return;
            }
        }

        // 4. Looking at empty space, hide the marker
        indicatorCanvas.SetActive(false);
    }

    // Helper method to make the 3D Canvas face the player perfectly
    void FaceCamera()
    {
        indicatorCanvas.transform.LookAt(playerCamera);
        // LookAt makes the Z-axis face the camera, which renders Canvas text backwards!
        // We rotate it 180 degrees to fix it.
        indicatorCanvas.transform.Rotate(0, 180, 0);
    }

    // --- EXISTING METHODS (UNCHANGED) ---
    void ExecuteThrow()
    {
        isChargingThrow = false;
        trajectoryLine.enabled = false;
        if (heldItem != null)
        {
            Collider itemCol = heldItem.GetComponent<Collider>();
            if (itemCol != null) Physics.IgnoreCollision(playerBodyCollider, itemCol, false);
            heldItem.transform.SetParent(null);
            itemRb.isKinematic = false;
            itemRb.AddForce(playerCamera.forward * currentThrowForce, ForceMode.Impulse);
            heldItem = null;
            isHoldingItem = false;
            currentThrowForce = 0f;
        }
    }

    void DrawTrajectory()
    {
        Vector3 startPosition = itemHoldPoint.position;
        Vector3 startVelocity = (playerCamera.forward * currentThrowForce) / itemRb.mass;
        trajectoryLine.positionCount = trajectoryPoints;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 point = startPosition + startVelocity * time + Physics.gravity * 0.5f * time * time;
            trajectoryLine.SetPosition(i, point);
        }
    }

    void TryPickupItem()
    {
        RaycastHit hit;
        if (Physics.SphereCast(playerCamera.position, interactionRadius, playerCamera.forward, out hit, itemRange, interactableLayer))
        {
            if (hit.collider.CompareTag(itemTag))
            {
                heldItem = hit.collider.gameObject;
                itemRb = heldItem.GetComponent<Rigidbody>();
                itemRb.isKinematic = true;
                heldItem.transform.SetParent(itemHoldPoint);
                heldItem.transform.localPosition = Vector3.zero;
                Collider itemCol = heldItem.GetComponent<Collider>();
                if (itemCol != null) Physics.IgnoreCollision(playerBodyCollider, itemCol, true);
                isHoldingItem = true;
            }
        }
    }

    void DropItem()
    {
        if (heldItem != null)
        {
            Collider itemCol = heldItem.GetComponent<Collider>();
            if (itemCol != null) Physics.IgnoreCollision(playerBodyCollider, itemCol, false);
            heldItem.transform.SetParent(null);
            itemRb.isKinematic = false;
            heldItem = null;
            isHoldingItem = false;
        }
    }

    void TryGrabCart()
    {
        RaycastHit hit;
        if (Physics.SphereCast(playerCamera.position, interactionRadius, playerCamera.forward, out hit, cartRange, interactableLayer))
        {
            if (hit.collider.CompareTag(cartTag))
            {
                cartRb = hit.collider.GetComponentInParent<Rigidbody>();
                if (cartRb != null)
                {
                    heldCart = cartRb.gameObject;
                    Rigidbody[] allRigidbodies = heldCart.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in allRigidbodies) rb.isKinematic = true;
                    heldCart.transform.SetParent(cartHoldPoint);
                    heldCart.transform.localPosition = cartHoldOffset;
                    heldCart.transform.localEulerAngles = cartRotOffset;
                    Collider[] cartColliders = heldCart.GetComponentsInChildren<Collider>();
                    foreach (Collider cartCol in cartColliders)
                    {
                        Physics.IgnoreCollision(playerBodyCollider, cartCol, true);
                    }
                    isHoldingCart = true;
                }
            }
        }
    }

    void ReleaseCart()
    {
        if (heldCart != null)
        {
            Rigidbody[] allRigidbodies = heldCart.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in allRigidbodies) rb.isKinematic = false;
            Collider[] cartColliders = heldCart.GetComponentsInChildren<Collider>();
            foreach (Collider cartCol in cartColliders)
            {
                Physics.IgnoreCollision(playerBodyCollider, cartCol, false);
            }
            heldCart.transform.SetParent(null);
            heldCart = null;
            isHoldingCart = false;
        }
    }
}