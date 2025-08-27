using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using System.Linq;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxCardSize;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private List<ActionCardSO> cardTypes;
    [SerializeField] private GameObject hand;
    private int cardInHandCapacity = 6;
    private List<GameObject> cards = new();

    private void Start()
    {
        InitCardHand();
    }
    private void InitCardHand()
    {
        for (int i = 0; i < cardInHandCapacity; i++)
        {
            DrawCard();
        }
        SortHandByType();

    }
    private void SortHandByType()
    {
        cards = cards
            .OrderByDescending(c => c.GetComponent<ActionCard>().cardData.cardType)
            .ToList();
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(cards.Count - 1 - i);
        }
        UpdateCardPosition();
    }

    // 🔹 Generic draw (random or specific)
    public void DrawCard(GameObject cardPrefab = null)
    {
        if (cards.Count >= maxCardSize) return;

        int index = Random.Range(0, cardTypes.Count);
        GameObject prefab = cardPrefab ?? cardTypes[index].cardObject;

        GameObject cardSpawned = Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity, hand.transform);

        ActionCard actionCard = cardSpawned.GetComponent<ActionCard>();
        if (actionCard != null)
        {
            actionCard.Init(cardTypes[index], this); // pass self as manager
        }

        cards.Add(cardSpawned);
        SortHandByType();
    }

    public void RemoveCard(GameObject card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            UpdateCardPosition();
        }
    }

    private void UpdateCardPosition()
    {
        if (cards.Count == 0) return;

        float cardSpacing = 0.5f / maxCardSize;
        float firstPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2;

        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cards.Count; i++)
        {
            float currentPos = firstPosition + i * cardSpacing;
            Vector3 worldPos = spline.EvaluatePosition(currentPos);
            Vector3 forward = spline.EvaluateTangent(currentPos);

            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

            RectTransform rect = cards[i].GetComponent<RectTransform>();
            ActionCard card = cards[i].GetComponent<ActionCard>();

            rect.DOMove(worldPos, 1f).OnComplete(() =>
            {
                card.SetBasePosition(rect.localPosition);
            });

            rect.DORotate(new Vector3(0, 0, angle - 180f), 1f);
        }
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            DrawCard(); // random draw
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
