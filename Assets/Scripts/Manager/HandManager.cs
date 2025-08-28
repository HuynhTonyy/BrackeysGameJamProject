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
    [SerializeField] private SplineContainer splineContainer, chooseCardSplineContainer;
    [SerializeField] private List<ActionCardSO> cardTypes;
    [SerializeField] private GameObject hand, deck;
    private List<GameObject> handCards = new();
    private List<GameObject> selectionCards = new();
    [SerializeField] public HandState currentHandState;
    [SerializeField] public DeckState currentDeckState;
    private Vector3 baseHandPos;
    private float hideOffsetY = -300f;
    public enum DeckState
    {
        waiting,
        selecting,
        hiding
    }
    public enum HandState
    {
        waiting,
        selecting,
        discarding,
        hiding
    }

    public void ChangeDeckState(DeckState newDeckState)
    {
        currentDeckState = newDeckState;
        // 🔹 Handle Deck State
        switch (currentDeckState)
        {
            case DeckState.selecting:
                SetDeckPlayble(true);
                break;

            case DeckState.waiting:
                // maybe show empty deck or idle
                ShowSelectedDeck();
                SetDeckPlayble(false);
                ChangeHandState(HandState.discarding);
                break;

            case DeckState.hiding:
                HideSelectedDeck();
                break;
        }
    }
    public void ChangeHandState(HandState newHandState)
    {
        currentHandState = newHandState;
        switch (currentHandState)
        {
            case HandState.waiting:
                SetHandPlayble(false);
                HideHand(false);
                break;

            case HandState.selecting:
                SetHandPlayble(true);
                HideHand(false);
                break;

            case HandState.hiding:
                SetHandPlayble(false);
                HideHand(true);
                break;

            case HandState.discarding:
                SetHandPlayble(true);
                HideHand(false);
                break;
        }
    }

    private void HideSelectedDeck()
    {
        foreach (var card in selectionCards)
        {
            Destroy(card);
        }
        selectionCards.Clear();
    }

    private void SetHandPlayble(bool enable)
    {
        foreach (var card in handCards)
        {
            var actionCard = card.GetComponent<ActionCard>();
            if (actionCard != null)
            {
                if (enable) actionCard.EnablePlay(true);
                else actionCard.EnablePlay(false);
            }
        }
    }
    private void SetDeckPlayble(bool enable)
    {
        foreach (var card in selectionCards)
        {
            var actionCard = card.GetComponent<ActionCard>();
            if (actionCard != null)
            {
                if (enable) actionCard.EnablePlay(true);
                else actionCard.EnablePlay(false);
            }
        }
    }
    private void HideHand(bool hide)
    {
        if (currentHandState == HandState.hiding && hide == false)
        {
            currentHandState = HandState.selecting;
        }
        RectTransform rect = hand.GetComponent<RectTransform>();
        baseHandPos = hand.GetComponent<RectTransform>().localPosition;
        if (rect == null) return;
        rect.DOKill(); // stop old tween
        if (hide)
        {
            rect.DOLocalMoveY(baseHandPos.y + hideOffsetY, 0.5f).SetEase(Ease.InCubic);
        }
        else
        {
            rect.DOLocalMoveY(baseHandPos.y - hideOffsetY, 0.5f).SetEase(Ease.OutCubic);
        }
    }

    private void Start()
    {
        InitCardHand();
        ChangeDeckState(DeckState.hiding);
        ChangeHandState(HandState.selecting);

    }
    private void InitCardHand()
    {
        for (int i = 0; i < maxCardSize; i++)
        {
            DrawCard();
        }
    }
    private List<GameObject> SortCardByType(List<GameObject> listCards)
    {
        if (listCards == null || listCards.Count == 0)
            return new List<GameObject>();

        List<GameObject> sortedList = listCards
            .OrderByDescending(c => c.GetComponent<ActionCard>().cardData.cardType)
            .ToList();

        for (int i = 0; i < sortedList.Count; i++)
        {
            sortedList[i].transform.SetSiblingIndex(i);
        }

        return sortedList;
    }
    // 🔹 Generic draw (random or specific)
    public void DrawCard(GameObject cardPrefab = null)
    {
        if (handCards.Count >= maxCardSize)
        {
            // Shake the hand if full
            hand.GetComponent<RectTransform>().DOShakePosition(0.2f, new Vector3(15f, 0f, 0f), 15, 90, false, true);
            return;
        }
        int index = Random.Range(0, cardTypes.Count);
        GameObject prefab = cardPrefab ?? cardTypes[index].cardObject;
        GameObject cardSpawned = Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity, hand.transform);
        ActionCard actionCard = cardSpawned.GetComponent<ActionCard>();
        if (actionCard != null)
        {
            actionCard.Init(cardTypes[index], this); // pass self as manager
        }
        handCards.Add(cardSpawned);
        handCards = SortCardByType(handCards);
        UpdateCardPosition(handCards, splineContainer.Spline);
    }
    private void UpdateCardPosition(List<GameObject> list = null, Spline spline = null)
    {
        if (list.Count == 0) return;
        float cardSpacing;
        if (spline == splineContainer.Spline)
            cardSpacing = 1f / maxCardSize;
        else if (spline == chooseCardSplineContainer.Spline)
            cardSpacing = 0.4f;
        else
            cardSpacing = 0.5f;

        float firstPosition = 0.5f - (list.Count - 1) * cardSpacing / 2;
        for (int i = 0; i < list.Count; i++)
        {
            RectTransform rect = list[i].GetComponent<RectTransform>();
            ActionCard card = list[i].GetComponent<ActionCard>();
            if (rect == null || card == null) continue;

            float currentPos = firstPosition + i * cardSpacing;
            Vector3 worldPos = spline.EvaluatePosition(currentPos);
            Vector3 forward = spline.EvaluateTangent(currentPos);
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            // Make a separate sequence per card
            Sequence seq = DOTween.Sequence();
            seq.Join(rect.DOLocalMove(worldPos, 0.3f));
            seq.Join(rect.DORotate(new Vector3(0, 0, angle - 180f), 0.3f));
            seq.OnComplete(() =>
            {
                if (card != null && rect != null)
                {
                    card.SetBasePosition(rect.localPosition);
                    card.EnableInteraction(true);
                }
            });
        }
    }
    private void ShowSelectedDeck(GameObject cardPrefab = null)
    {
        // Destroy previous selection cards
        if (currentDeckState == DeckState.hiding) return;

        foreach (var card in selectionCards)
        {
            card.GetComponent<RectTransform>()?.DOKill();
            Destroy(card);
        }
        selectionCards.Clear();

        // Spawn new selection cards
        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, cardTypes.Count);
            GameObject prefab = cardPrefab ?? cardTypes[index].cardObject;
            GameObject cardSpawned = Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity, deck.transform);
            ActionCard actionCard = cardSpawned.GetComponent<ActionCard>();
            if (actionCard != null)
            {
                actionCard.Init(cardTypes[index], this);
                if (currentDeckState == DeckState.waiting)
                    actionCard.EnablePlay(false);
                else
                    actionCard.EnablePlay(true);
            }
            selectionCards.Add(cardSpawned);
        }

        selectionCards = SortCardByType(selectionCards);
        UpdateCardPosition(selectionCards, chooseCardSplineContainer.Spline);
    }
    public void RemoveCard(GameObject card)
    {
        if (card == null) return;
        // Remove from any list it belongs to
        if (handCards.Contains(card))
            handCards.Remove(card);
        if (selectionCards.Contains(card))
            selectionCards.Remove(card);

        // Kill any running tweens and destroy
        card.GetComponent<RectTransform>()?.DOKill();
        Destroy(card);
        // Recalculate positions
        UpdateCardPosition(handCards, splineContainer.Spline);
        UpdateCardPosition(selectionCards, chooseCardSplineContainer.Spline);

    }
    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            DrawCard();
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentDeckState != DeckState.hiding)
            {
                ChangeDeckState(DeckState.hiding);
                ChangeHandState(HandState.selecting);
            }
            else
            {
                ChangeDeckState(DeckState.waiting);
                ChangeHandState(HandState.selecting);
            }
        }
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (currentHandState != HandState.hiding)
            {
                ChangeHandState(HandState.hiding);
            }
            else
            {
                ChangeHandState(HandState.selecting);
            }

        }
    }
}
