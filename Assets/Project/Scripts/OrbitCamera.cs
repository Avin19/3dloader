using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform target; // The model to look at
    public float rotationSpeed = 5f; // Speed of rotation
    public float zoomSpeed = 5f; // Speed of zooming
    public float minZoom = 2f, maxZoom = 20f; // Zoom limits

    private float distance = 10f;
    private float currentX = 0f, currentY = 0f;
    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            distance = Vector3.Distance(transform.position, target.position);
            offset = transform.position - target.position;
        }
    }

    void Update()
    {
        if (target == null) return;

        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // Right Mouse Button to rotate
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentX += mouseX;
            currentY -= mouseY;
            currentY = Mathf.Clamp(currentY, -80f, 80f); // Prevent flipping
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minZoom, maxZoom);
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 newPosition = target.position - (rotation * Vector3.forward * distance);
        transform.position = newPosition;
        transform.LookAt(target);
    }
}
