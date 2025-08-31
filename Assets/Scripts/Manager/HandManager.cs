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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drawCardSound;
    [SerializeField] private List<ActionCardSO> cardTypes;
    [SerializeField] private GameObject hand, deck;
    private List<GameObject> handCards = new();
    private List<GameObject> selectionCards = new();
    private bool isRepeat { get; set; } = false;

    private void OnEnable()
    {
        EventManager.Instance.onCardPlayAnimationEnd += RemoveCard;
        EventManager.Instance.onPlayCard += PlayCardAbility;
        EventManager.Instance.onEnterAddCardGrid += SelectCardToHand;
        EventManager.Instance.onEnterDropCardGrid += DropRandomCard;
        EventManager.Instance.OnCompleteAction += ShowHand;
        EventManager.Instance.onPlayerMoveEnd += CheckEmptyHand;

    }
    private void OnDisable()
    {
        EventManager.Instance.onCardPlayAnimationEnd -= RemoveCard;
        EventManager.Instance.onPlayCard -= PlayCardAbility;
        EventManager.Instance.onEnterAddCardGrid -= SelectCardToHand;
        EventManager.Instance.onEnterDropCardGrid -= DropRandomCard;
        EventManager.Instance.onPlayerMoveEnd -= CheckEmptyHand;
        EventManager.Instance.OnCompleteAction -= ShowHand;

    }
    private void OnDestroy()
    {
        EventManager.Instance.onCardPlayAnimationEnd -= RemoveCard;
        EventManager.Instance.onPlayCard -= PlayCardAbility;
        EventManager.Instance.onEnterAddCardGrid -= SelectCardToHand;
        EventManager.Instance.onPlayerMoveEnd -= CheckEmptyHand;
        EventManager.Instance.onEnterDropCardGrid -= DropRandomCard;
        EventManager.Instance.OnCompleteAction -= ShowHand;

    }
    public void OnClickDrawCard() {
        EventManager.Instance.CardChange(1);
        DrawCard();
    }
    private void ShowHand()
    {
        ChangeHandState(HandState.selecting);
    }
    void PlayCardAbility(ActionCardSO cardData)
    {
        if (cardData.cardType == CardType.Move)
        {
            ChangeHandState(HandState.hiding);
            if (isRepeat)
            {
                isRepeat = false;
                EventManager.Instance.PlayMoveCard(cardData.step * 2, 1);
            }
            else
            {
                EventManager.Instance.PlayMoveCard(cardData.step, 1);
            }
        }
        else if (cardData.cardType == CardType.TradeOff)
        {
            if (cardData.tradeOffType == TradeOffType.Repeat)
            {
                isRepeat = true;
                DropRandomCard();
            }
        }
        UpdateCardPosition(handCards, splineContainer.Spline);
    }
    void CheckEmptyHand()
    {
        if (handCards.Count <= 0)
        {
            DrawCard();
            DrawCard();
        }
    }
    public void DropRandomCard()
    {
        if (handCards.Count == 0)
        {
            CheckEmptyHand();
            return;
        }
        // Pick a random card
        GameObject randomCard = handCards[Random.Range(0, handCards.Count)];
        ActionCard card = randomCard.GetComponent<ActionCard>();
        // Disable its interaction
        card.EnablePlay(false);
        // Animate it "dropping" or fading out (optional)
        // Remove from hand
        handCards.Remove(randomCard);
        EventManager.Instance.CardChange(-1);
        randomCard.transform.DOMoveY(-Screen.height, 0.5f) // fly downwards
            .OnComplete(() =>
            {
                Destroy(randomCard);
            }); // then destroy it
        CheckEmptyHand();
        UpdateCardPosition(handCards, splineContainer.Spline);

    }
    [SerializeField] public HandState currentHandState;
    [SerializeField] public DeckState currentDeckState;
    private Vector3 baseHandPos;
    private float hideOffsetY = 350f;
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
        ShowSelectedDeck();
        // 🔹 Handle Deck State
        switch (currentDeckState)
        {
            case DeckState.selecting:
                SetDeckPlayble(true);
                break;
            // case DeckState.waiting:
            //     // maybe show empty deck or idle
            //     SetDeckPlayble(false);
            //     break;
            case DeckState.hiding:
                HideSelectedDeck();
                SetDeckPlayble(false);
                ChangeHandState(HandState.selecting);
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
            // case HandState.discarding:
            //     SetHandPlayble(true);
            //     HideHand(false);
            //     break;
            default:
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
        RectTransform rect = hand.GetComponent<RectTransform>();
        if (rect == null) return;

        rect.DOKill();

        if (hide && currentHandState == HandState.hiding)
        {
            // Move down off screen
            rect.DOLocalMoveY(baseHandPos.y - hideOffsetY, 0.5f);
        }
        else
        {
            // Always reset back to spawn pos
            rect.DOLocalMoveY(baseHandPos.y, 0.5f);
        }
    }

    private void Start()
    {
        InitCardHand();
        RectTransform rect = hand.GetComponent<RectTransform>();
        if (rect != null)
            baseHandPos = rect.localPosition;
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
        .OrderByDescending(c => cardTypes.IndexOf(c.GetComponent<ActionCard>().cardData))
        .ToList();

        for (int i = 0; i < sortedList.Count; i++)
        {
            sortedList[i].transform.SetSiblingIndex(i);
        }

        return sortedList;
    }
    // 🔹 Generic draw (random or specific)
    private void DrawCard(GameObject cardPrefab = null)
    {
        if (handCards.Count >= maxCardSize)
        {   // Shake the hand if full
            hand.GetComponent<RectTransform>().DOShakePosition(0.2f, new Vector3(15f, 0f, 0f), 15, 90, false, true);
            return;
        }
        int index = Random.Range(0, cardTypes.Count);
        GameObject prefab = cardPrefab ?? cardTypes[index].cardObject;
        GameObject cardSpawned = Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity, hand.transform);
        ActionCard actionCard = cardSpawned.GetComponent<ActionCard>();
        if (actionCard != null)
        {
            actionCard.Init(cardTypes[index], this, CardLocation.Hand); // pass self as manager
        }
        handCards.Add(cardSpawned);
        handCards = SortCardByType(handCards);
        UpdateCardPosition(handCards, splineContainer.Spline);
        if (audioSource != null && drawCardSound != null)
        {
            audioSource.PlayOneShot(drawCardSound);
        }
    }
    private void UpdateCardPosition(List<GameObject> list = null, Spline spline = null)
    {
        if (list == null || list.Count == 0) return;
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
                actionCard.Init(cardTypes[index], this, CardLocation.Deck);
                if (currentDeckState == DeckState.waiting)
                    actionCard.EnablePlay(false);
                else
                    actionCard.EnablePlay(true);
            }
            selectionCards.Add(cardSpawned);
        }
        selectionCards = SortCardByType(selectionCards);
        UpdateCardPosition(selectionCards, chooseCardSplineContainer.Spline);
        if (audioSource != null && drawCardSound != null)
        {
            audioSource.PlayOneShot(drawCardSound);
        }
    }
    public void AddCardToHand(ActionCard card)
    {
        // Remove from deck
        if (selectionCards.Contains(card.gameObject))
        {
            selectionCards.Remove(card.gameObject);
        }
        if (!handCards.Contains(card.gameObject))
        {
            handCards.Add(card.gameObject);
        }
        card.transform.SetParent(hand.transform, false);
        handCards = SortCardByType(handCards);
        UpdateCardPosition(handCards, splineContainer.Spline);
        ChangeDeckState(DeckState.hiding);
    }

    private void SelectCardToHand(GameObject cardPrefab = null)
    {
        if (currentDeckState != DeckState.hiding && handCards.Count < maxCardSize)
        {
            ChangeDeckState(DeckState.hiding);
            ChangeHandState(HandState.selecting);
        }
        else
        {
            ChangeDeckState(DeckState.selecting);
            ChangeHandState(HandState.waiting);
        }
    }
    public void RemoveCard(GameObject card)
    {
        if (card == null) return;
        // Remove from any list it belongs to

        // Kill any running tweens and destroy
        card.GetComponent<RectTransform>()?.DOKill();
        Destroy(card);
        if (handCards.Contains(card))
            handCards.Remove(card);
        if (selectionCards.Contains(card))
            selectionCards.Remove(card);
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

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeDeckState(DeckState.hiding);
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
