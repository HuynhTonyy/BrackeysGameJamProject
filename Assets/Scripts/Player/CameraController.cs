using UnityEngine;
using System.Collections;

public class SimpleCameraZoom : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;  // your player camera
    [SerializeField] private GameObject player;    // reference to player object
    [SerializeField] private float mapZoomSize = 20f;   // zoomed out (see map)
    [SerializeField] private float playerZoomSize = 3f; // zoomed in (follow player)
    [SerializeField] private float zoomDuration = 2f;   // how long to zoom
    private Vector3 cameraOffset;
    private float fixedY;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        fixedY = mainCamera.transform.position.y;
        if (player != null)
            cameraOffset = mainCamera.transform.position - player.transform.position;

        // Start zoomed out
        mainCamera.orthographicSize = mapZoomSize;

        // Start coroutine to zoom into player
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
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPos = player.transform.position + cameraOffset;
            newPos.y = fixedY; // lock Y
            mainCamera.transform.position = newPos;
        }
    }
}
