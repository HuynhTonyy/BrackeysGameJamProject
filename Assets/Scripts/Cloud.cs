using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cloud : MonoBehaviour
{
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
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform cloudPart = gameObject.transform.GetChild(i);
            //get the direction 
            Vector3 centerPoint = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y + 0.5f, gameObject.transform.position.z);
            Vector3 direction = (cloudPart.position - centerPoint).normalized;
            Debug.Log(direction);
            cloudPart.DOMove(direction * 1f, 1f).SetEase(Ease.OutCubic);
            Renderer renderer = cloudPart.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                material.DOFade(0f, 1f).OnComplete(()=>{ });
            }
        }
        
    }
}
