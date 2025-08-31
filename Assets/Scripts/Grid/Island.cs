using UnityEngine;
using DG.Tweening;

public class Island : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private void Awake()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }
    public void PlayIslandSinkEffect(int sinkToIndex)
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(-10f, 1f).SetEase(Ease.InBack));
        seq.Join(transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            EventManager.Instance.IslandSinkEnd(sinkToIndex);
        });
    }
}
