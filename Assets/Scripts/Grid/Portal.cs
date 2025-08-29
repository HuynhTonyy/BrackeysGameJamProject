using UnityEngine;
using DG.Tweening;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject portal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portal.transform.DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
                 .SetLoops(-1, LoopType.Restart)
                 .SetEase(Ease.Linear);
        ScaleRandomly();
    }
    void ScaleRandomly()
    {
        float randomScale = Random.Range(0.8f, 1.25f);
        portal.transform.DOScale(Vector3.one * randomScale, 0.5f)
                .SetEase(Ease.InOutSine)
                .OnComplete(ScaleRandomly);
    }
}
