using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject bombObject;
    [SerializeField] private GameObject bombEffect;
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
        bombEffect.SetActive(true);
        bombObject.SetActive(false);
        EventManager.Instance.ExplodeBomb();
    }
}
