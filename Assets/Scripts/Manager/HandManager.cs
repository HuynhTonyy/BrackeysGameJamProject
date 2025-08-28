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
    [SerializeField] private List<ActionCardSO> cardTypes;
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

            Vector3 forward = spline.EvaluateTangent(currentPos);
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            cards[i].GetComponent<RectTransform>()
    .DOMove(worldPos, 1f);
            cards[i].GetComponent<RectTransform>()
    .DORotate(new Vector3(0, 0, angle-180f),1f);

        }           
    }

    // Update is called once per frame
    void DrawCard(GameObject card = null)
    {
        if (cards.Count >= maxCardSize) return;
        int index = Random.Range(0, cardTypes.Count);
        GameObject cardSpawned = Instantiate(card==null?cardTypes[index].cardObject:card, spawnPoint.transform.position, Quaternion.identity,hand.transform);
        cards.Add(cardSpawned);
        UpdateCardPosition();
    }
    private void Start() {
        for (int i = 0; i < maxCardSize/2; i++)
        {
            DrawCard(cardTypes[i].cardObject);
            DrawCard(cardTypes[i].cardObject);
        }
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
