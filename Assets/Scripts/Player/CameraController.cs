// using Cinemachine; // not Unity.Cinemachine
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private Transform player;

    private void Start()
    {
        // Make sure vcam follows player
        if (vcam != null && player != null)
            vcam.Follow = player;

        // Default zoom level
        vcam.m_Lens.OrthographicSize = 5f;
    }

    // Example: call this to zoom out to world view
    public void ZoomOut(float size, float duration = 1f)
    {
        StartCoroutine(ZoomRoutine(size, duration));
    }

    // Example: call this to zoom back to player
    public void ZoomIn(float size, float duration = 1f)
    {
        StartCoroutine(ZoomRoutine(size, duration));
    }

    private System.Collections.IEnumerator ZoomRoutine(float targetSize, float duration)
    {
        float startSize = vcam.m_Lens.OrthographicSize;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t / duration);
            yield return null;
        }

        vcam.m_Lens.OrthographicSize = targetSize;
    }
}
