using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform islandCenter; // The pivot point at the island center
    public float rotationSpeed = 5.0f;
    public float flickDamping = 0.95f;

    public float zoomSpeed = 20f; // Speed of zooming in/out
    public float minZoomDistance = 50f; // Minimum zoom distance
    public float maxZoomDistance = 200f; // Maximum zoom distance

    private bool isDragging = false;
    private float momentum = 0.0f;

    void Update()
    {
        // Begin drag
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            momentum = 0.0f;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Handle rotation
        if (isDragging)
        {
            float mouseDelta = Input.GetAxis("Mouse X");
            float rotationAmount = mouseDelta * rotationSpeed;
            islandCenter.Rotate(0, rotationAmount, 0);
            momentum = rotationAmount;
        }
        else if (Mathf.Abs(momentum) > 0.01f)
        {
            islandCenter.Rotate(0, momentum, 0);
            momentum *= flickDamping;

            if (Mathf.Abs(momentum) < 0.01f)
            {
                momentum = 0;
            }
        }

        // Handle zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            float newZoomDistance = Mathf.Clamp(transform.localPosition.magnitude - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);
            transform.localPosition = transform.localPosition.normalized * newZoomDistance;
        }
    }
}
