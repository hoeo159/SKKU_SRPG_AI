using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Trackball : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform focusTarget;

    [Header("Orbit (RMB Drag)")]
    [SerializeField] private bool orbitEnabled = true;
    [SerializeField] private float orbitSpeed = 0.20f; // 마우스 감도
    [SerializeField] private float minPitch = 25f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private bool invertY = true;

    [Header("Pan (MMB Drag)")]
    [SerializeField] private bool panEnabled = true;
    [SerializeField] private float panSpeed = 0.02f;

    [Header("Zoom (Wheel)")]
    [SerializeField] private bool zoomEnabled = true;
    [SerializeField] private float zoomSpeed = 0.40f;
    [SerializeField] private float minOrthoSize = 4f;
    [SerializeField] private float maxOrthoSize = 25f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 60f;

    [Header("Smoothing")]
    [SerializeField] private float positionLerp = 15f;
    [SerializeField] private float rotationLerp = 15f;

    private float yaw, pitch, distance;
    private float desiredYaw, desiredPitch, desiredDistance;
    private Vector3 desiredPivotPos;

    private void Awake()
    {
        if (pivot == null) pivot = transform;

        if (cam == null)
            cam = GetComponentInChildren<Camera>();

        if (cam == null)
            cam = Camera.main;
    }

    private void Start()
    {
        InitializeFromCurrentSetup();
    }

    private void InitializeFromCurrentSetup()
    {
        if (cam == null) return;

        Vector3 e = pivot.rotation.eulerAngles;
        desiredYaw = yaw = e.y;

        pitch = NormalizePitch(e.x);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        desiredPitch = pitch;

        distance = Mathf.Abs(cam.transform.localPosition.z);
        if (distance <= 0.01f) distance = 20f;
        desiredDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        desiredPivotPos = pivot.position;

        if (cam.orthographic)
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
    }

    private float NormalizePitch(float x)
    {
        if (x > 180f) x -= 360f;
        return x;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if(!overUI)
        {
            HandleFocus();
            HandleOrbit();
            HandlePan();
            HandleZoom();
        }

        Apply();
    }

    public void FocusTo(Transform target, bool snap = false)
    {
        if (target == null) return;
        FocusTo(target.position, snap);
    }

    public void FocusTo(Vector3 worldPos, bool snap = false)
    {
        desiredPivotPos = worldPos;

        if (snap)
        {
            pivot.position = worldPos;
            desiredPivotPos = worldPos;
        }
    }

    private void HandleFocus()
    {
        if (focusTarget == null) return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            desiredPivotPos = focusTarget.position;
    }

    private void HandleOrbit()
    {
        if (!orbitEnabled || Mouse.current == null) return;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            desiredYaw += delta.x * orbitSpeed;

            float dy = delta.y * orbitSpeed;
            desiredPitch += invertY ? dy : -dy;

            desiredPitch = Mathf.Clamp(desiredPitch, minPitch, maxPitch);
        }
    }

    private void HandlePan()
    {
        if (!panEnabled || Mouse.current == null) return;

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            Vector3 right = pivot.right;
            Vector3 forward = Vector3.ProjectOnPlane(pivot.forward, Vector3.up).normalized;

            float scale = cam.orthographic ? cam.orthographicSize : desiredDistance;
            float k = panSpeed * Mathf.Max(0.01f, scale) * 0.01f;

            desiredPivotPos += (-right * delta.x - forward * delta.y) * k;
        }
    }

    private void HandleZoom()
    {
        if (!zoomEnabled || Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float step = scroll * zoomSpeed * 0.01f;

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - step, minOrthoSize, maxOrthoSize);
        }
        else
        {
            desiredDistance = Mathf.Clamp(desiredDistance - step * 20f, minDistance, maxDistance);
        }
    }

    private void Apply()
    {
        float posT = 1f - Mathf.Exp(-positionLerp * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationLerp * Time.deltaTime);

        pivot.position = Vector3.Lerp(pivot.position, desiredPivotPos, posT);

        yaw = Mathf.LerpAngle(yaw, desiredYaw, rotT);
        pitch = Mathf.Lerp(pitch, desiredPitch, rotT);
        pivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

        distance = Mathf.Lerp(distance, desiredDistance, rotT);

        Vector3 lp = cam.transform.localPosition;
        lp.x = 0f;
        lp.y = 0f;
        lp.z = -distance;
        cam.transform.localPosition = lp;

        cam.transform.localRotation = Quaternion.identity;
    }
}