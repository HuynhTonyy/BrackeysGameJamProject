using UnityEngine;
using DG.Tweening;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject portalEffect;
    [SerializeField] private AudioClip teleportClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portalEffect.SetActive(false);
    }
    public void Active(bool isActive)
    {
        portalEffect.SetActive(isActive);
        if (isActive)
        {
            EventManager.Instance.PlaySFX(teleportClip);
        }
    }
}
