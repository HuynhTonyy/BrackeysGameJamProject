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
    [SerializeField] private float scrollZoomSpeed = 100f;
    [SerializeField] private float dragSpeed = 0.05f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 40f;
    [SerializeField] private float followDelay = 2f;    // delay after drag release

    private Vector3 cameraOffset;
    private float fixedY;
    private Vector3 lastMousePos;
    private bool isZoomingToPlayer = true;
    private bool isFollowingPlayer = true;
    private Coroutine followCoroutine;

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
        isZoomingToPlayer = false;
        isFollowingPlayer = true; // enable follow after zoom
    }

    private void Update()
    {
        if (!isZoomingToPlayer)
        {
            // cameraOffset = mainCamera.transform.position - player.transform.position;
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
        if (Mouse.current.rightButton.isPressed)
        {
            // Cancel any pending resume coroutine
            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
                followCoroutine = null;
            }

            isFollowingPlayer = false;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            if (lastMousePos != Vector3.zero)
            {
                Vector3 delta = mousePos - lastMousePos;
                Vector3 move = new Vector3(-delta.x * dragSpeed, 0, 0);
                mainCamera.transform.Translate(move, Space.World);

                // lock Y
                var pos = mainCamera.transform.position;
                pos.y = fixedY;
                pos.x = Mathf.Clamp(pos.x, -10f, 220f);
                mainCamera.transform.position = pos;
            }
            lastMousePos = mousePos;

            // ✅ Update offset while dragging
            if (player != null)
                cameraOffset = mainCamera.transform.position - player.transform.position;
        }
        else
        {
            if (lastMousePos != Vector3.zero)
            {
                if (followCoroutine != null)
                    StopCoroutine(followCoroutine);
                followCoroutine = StartCoroutine(ResumeFollowAfterDelay());
            }
            lastMousePos = Vector3.zero;
        }
    }

    IEnumerator ResumeFollowAfterDelay()
    {
        yield return new WaitForSeconds(followDelay);

        if (player == null) yield break;

        float elapsed = 0f;
        float duration = 1f; // fly-back duration

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // ✅ Player’s position is recalculated every frame
            Vector3 targetPos = new Vector3(player.transform.position.x, fixedY, mainCamera.transform.position.z);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, smoothT);
            yield return null;
        }

        // After flying back, resume hard follow
        isFollowingPlayer = true;
    }
    void HandleSnapToPlayer()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame && player != null)
        {
            Vector3 newPos = player.transform.position + cameraOffset;
            newPos.y = fixedY;
            mainCamera.transform.position = newPos;

            // ✅ Refresh offset immediately
            cameraOffset = mainCamera.transform.position - player.transform.position;

            isFollowingPlayer = true;
        }
    }
    private void LateUpdate()
    {
        if (player != null && (isZoomingToPlayer || isFollowingPlayer))
        {
            Vector3 newPos = mainCamera.transform.position;

            // Center directly on player while following
            newPos.x = player.transform.position.x;
            newPos.y = fixedY;

            mainCamera.transform.position = newPos;
        }
    }
}
