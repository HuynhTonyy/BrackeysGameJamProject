using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SimpleCameraZoomNewInput : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private float mapZoomSize = 20f;   // zoomed out start
    [SerializeField] private float playerZoomSize = 3f; // zoomed in target
    [SerializeField] private float zoomDuration = 2f;   // smooth zoom time
    [SerializeField] private float scrollZoomSpeed = 5f;
    [SerializeField] private float dragSpeed = 0.05f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 40f;

    private Vector3 cameraOffset;
    private float fixedY;
    private Vector3 lastMousePos;
    private bool isZoomingToPlayer = true;
    private Vector2 worldBounds = new Vector2(50f, 0);

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        fixedY = mainCamera.transform.position.y;

        if (player != null)
            cameraOffset = mainCamera.transform.position - player.transform.position;

        // Start zoomed out
        mainCamera.orthographicSize = mapZoomSize;

        // Smooth zoom into player
        StartCoroutine(ZoomToPlayer());
    }

    private IEnumerator ZoomToPlayer()
    {
        float startSize = mainCamera.orthographicSize;
        float t = 0f;

        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, playerZoomSize, t / zoomDuration);
            yield return null;
        }

        mainCamera.orthographicSize = playerZoomSize;
        isZoomingToPlayer = false; // done zooming, allow free control
    }

    private void Update()
    {
        if (!isZoomingToPlayer) // don’t allow manual input while auto-zooming
        {
            HandleScrollZoom();
            HandleDrag();
            HandleSnapToPlayer();
        }
    }

    void HandleScrollZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            mainCamera.orthographicSize -= scroll * scrollZoomSpeed * Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandleDrag()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            if (lastMousePos != Vector3.zero)
            {
                Vector3 delta = mousePos - lastMousePos;
                Vector3 move = new Vector3(-delta.x * dragSpeed, 0, 0);
                mainCamera.transform.Translate(move, Space.World);

                // keep Y locked
                var pos = mainCamera.transform.position;
                pos.y = fixedY;
                pos.x = Mathf.Clamp(pos.x, -10f, 200f);
                mainCamera.transform.position = pos;
            }
            lastMousePos = mousePos;
        }
        else
        {
            lastMousePos = Vector3.zero;
        }
    }
    void HandleSnapToPlayer()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame && player != null)
        {
            Vector3 newPos = player.transform.position + cameraOffset;
            newPos.y = fixedY;
            mainCamera.transform.position = newPos;
        }
    }

    private void LateUpdate()
    {
        if (player != null && isZoomingToPlayer) // only follow player during auto-zoom
        {
            Vector3 newPos = player.transform.position + cameraOffset;
            newPos.y = fixedY;
            mainCamera.transform.position = newPos;
        }
    }
}
