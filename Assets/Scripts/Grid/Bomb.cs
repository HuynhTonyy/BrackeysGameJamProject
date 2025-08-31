using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject bombObject;
    [SerializeField] private GameObject bombEffect;
    [SerializeField] private AudioClip explosionClip;
    void Start()
    {
        bombObject.SetActive(false);
        bombEffect.SetActive(false);
    }
    public void ActivateBomb()
    {
        StartCoroutine(ActivateBombCoroutine());
    }
    IEnumerator ActivateBombCoroutine()
    {
        bombObject.SetActive(true);
        yield return new WaitForSeconds(0.75f);
        EventManager.Instance.PlaySFX(explosionClip);
        bombEffect.SetActive(true);
        bombObject.SetActive(false);
        EventManager.Instance.ExplodeBomb();
    }
}
