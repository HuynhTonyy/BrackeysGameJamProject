using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    private bool isCleared = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Disappear()
    {
        if(isCleared) return;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform cloudPart = gameObject.transform.GetChild(i);
            //get the direction 
            Vector3 centerPoint = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
            Vector3 direction = (cloudPart.position - centerPoint).normalized;
            // Debug.Log(direction);
            cloudPart.DOMove(direction, 0.25f).SetRelative().SetEase(Ease.OutQuad);
            Renderer renderer = cloudPart.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                material.DOFade(0f, 0.25f).OnComplete(() => { });
            }
            cloudPart.DOScale(0f, 0.25f).SetEase(Ease.OutQuad);
        }
        isCleared = true;
    }
}
