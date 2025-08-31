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
    private void OnEnable()
    {
        EventManager.Instance.onEnterMoveBackwardGrid += ActivateBomb;
    }
    private void OnDisable()
    {
        EventManager.Instance.onEnterMoveBackwardGrid -= ActivateBomb;
    }
    void ActivateBomb(int step)
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
