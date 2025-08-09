using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform islandCenter; // The pivot point at the island center
    public float rotationSpeed = 5.0f;
    public float flickDamping = 0.95f;

    public float zoomSpeed = 20f; // Speed of zooming in/out
    public float minZoomDistance = 50f; // Minimum zoom distance
    public float maxZoomDistance = 200f; // Maximum zoom distance

    public float autoRotateSpeed = 0.5f; // Speed of auto-rotation
    private bool isDragging = false;
    private float momentum = 0.0f;

    private float timer = 0.0f; // Timer for auto-rotation

    void Update()
    {
        // Begin drag && Makes sure user isn't clicking on UI elements
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = true;
            momentum = 0.0f;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            timer = 0.0f;
        }

        // Handle rotation
        if (isDragging)
        {
            float mouseDelta = Input.GetAxis("Mouse X");
            float rotationAmount = mouseDelta * rotationSpeed;
            islandCenter.Rotate(0, rotationAmount, 0);
            momentum = rotationAmount;
        }
        else
        {
            if (Mathf.Abs(momentum) > 0.01f)
            {
                islandCenter.Rotate(0, momentum, 0);
                momentum *= flickDamping;

                if (Mathf.Abs(momentum) < 0.01f)
                {
                    momentum = 0;
                }
            }

            // Start counting up when not dragging
            timer += Time.deltaTime;
            if (timer > 3.0f) // If no interaction for 3 seconds, start auto-rotating
            {
                islandCenter.Rotate(0, autoRotateSpeed * Time.deltaTime, 0);
            }
        }

        // Handle zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            float newZoomDistance = Mathf.Clamp(transform.localPosition.magnitude - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);
            transform.localPosition = transform.localPosition.normalized * newZoomDistance;
            timer = 0.0f; // Reset timer when zooming
        }
    }
}
