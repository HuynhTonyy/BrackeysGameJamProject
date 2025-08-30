using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ActionCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ActionCardSO cardData { get; set; }
    [Header("UI References")]
    // [SerializeField] private TMP_Text cardNameText;
    // [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playCardSound, hoverCardSound;

    private HandManager handManager;
    private Image borderCard;
    private bool isHovered;
    [SerializeField] private bool isPlayed = true;
    private bool interactable = false;
    private Button button;
    private RectTransform rectTransform;
    private Vector3 baseLocalPos, originalScale;
    public enum CardLocation { Deck, Hand }
    public CardLocation Location { get; set; }

    void Awake()
    {
        borderCard = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        button = GetComponent<Button>();
        handManager = GetComponentInParent<HandManager>();
        // if (button != null)
        // {
        //     button.onClick.AddListener(OnCardClickToPlay);
        // }
    }
    public void EnableInteraction(bool enable)
    {
        interactable = enable;
    }
    public void EnablePlay(bool enable)
    {

        isPlayed = enable;
    }
    public void Init(ActionCardSO data, HandManager manager)
    {
        cardData = data;
        handManager = manager;
        // cardNameText.SetText(cardData.cardName);
        // cardDescriptionText.SetText(cardData.cardDescription);
        baseLocalPos = transform.localPosition;

        if (borderCard != null)
        {
            switch (cardData.cardType)
            {
                case CardType.Move: borderCard.color = Color.white; break;
                case CardType.TradeOff: borderCard.color = Color.yellow; break;
                default: borderCard.color = Color.white; break;
            }
        }
    }

    // called by HandManager after positioning
    public void SetBasePosition(Vector3 pos)
    {
        baseLocalPos = pos;
        if (!isHovered)
        {
            rectTransform.localPosition = pos;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered || !interactable) return;
        isHovered = true;
        rectTransform.DOLocalMove(baseLocalPos + new Vector3(0, 100f, 0), 0.2f).SetEase(Ease.OutQuad);
        rectTransform.DOScale(originalScale * 1f, 0.2f);
        if (audioSource != null && hoverCardSound != null)
        {
            audioSource.PlayOneShot(hoverCardSound);
            // Debug.Log("Hover sound");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered || !interactable) return;
        isHovered = false;
        rectTransform.DOLocalMove(baseLocalPos, 0.2f).SetEase(Ease.OutQuad);
        rectTransform.DOScale(originalScale, 0.2f);
    }

    public void OnCardClickToPlay()
    {
        if (!interactable || !isPlayed) return;
        
        if (Location == CardLocation.Deck)
        {
            handManager.AddCardToHand(this);
            Location = CardLocation.Hand;
            return;
        }

        isPlayed = false;
        interactable = false;
        isHovered = false;

        PlayCardAnimation();
        EventManager.Instance.PlayCard(cardData);
        if (audioSource != null && playCardSound != null)
        {
            audioSource.PlayOneShot(playCardSound);
            // Debug.Log("Play sound");
        }

    }
    void PlayCardAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rectTransform.DOLocalMove(
            new Vector3(baseLocalPos.x, baseLocalPos.y + 300f, baseLocalPos.z),
            0.2f
        ).SetEase(Ease.InQuad));
        seq.Append(rectTransform.DOScale(1.2f, 0.2f));
        seq.Append(rectTransform.DOScale(0.8f, 0.2f));
        seq.OnComplete(() =>
        {
            EventManager.Instance.CardPlayAnimationEnd(this.gameObject);
        });
        
    }

}
