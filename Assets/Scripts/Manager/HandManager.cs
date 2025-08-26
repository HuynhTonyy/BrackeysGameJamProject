using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxCardSize;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject hand;
    private List<GameObject> cards = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void UpdateCardPosition()
    {
        if (cards.Count == 0) return;
        float cardSpacing = 1f / maxCardSize;
        float firstPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;
        for (int i = 0; i < cards.Count; i++)
        {
            float currentPos = firstPosition + i * cardSpacing;
            Vector3 worldPos = spline.EvaluatePosition(currentPos);
            Debug.Log(worldPos);    

            Vector3 forward = spline.EvaluateTangent(currentPos);
            // Vector3 up = spline.EvaluateUpVector(currentPos);
            // Quaternion rotation = Quaternion.LookRotation
            // (forward, Vector3.Cross(up, forward).normalized);
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            cards[i].GetComponent<RectTransform>()
    .DOMove(worldPos, 0.25f);
            // cards[i].transform.DOLocalRotateQuaternion(rotation, 0.25f);
            cards[i].GetComponent<RectTransform>()
    .DORotate(new Vector3(0, 0, angle),0.25f);

        }           
    }

    // Update is called once per frame
    void DrawCard()
    {
        if (cards.Count >= maxCardSize) return;
        GameObject cardSpawned = Instantiate(card, spawnPoint.transform.position, Quaternion.identity,hand.transform);
        Debug.Log(cardSpawned.transform.position);
        cards.Add(cardSpawned);
        UpdateCardPosition();
    }
    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            DrawCard();
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            foreach (var card in cards)
            {
                Destroy(card);
            }
            cards.Clear();
        }
    }
}
