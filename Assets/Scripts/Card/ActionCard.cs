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
    private HandManager handManager;
    private Image borderCard;
    private bool isHovered;
    [SerializeField] private bool isPlayed = false;
    private bool interactable = false;
    private Button button;
    private RectTransform rectTransform;
    private Vector3 baseLocalPos, originalScale;

    void Awake()
    {
        borderCard = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        button = GetComponent<Button>();
        handManager = GetComponentInParent<HandManager>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClickToPlay);
        }
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
                case CardType.Move: borderCard.color = Color.green; break;
                case CardType.Buff: borderCard.color = Color.yellow; break;
                case CardType.Debuff: borderCard.color = Color.red; break;
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
        if (!isPlayed || !interactable) return;
        EnablePlay(true);
        EnableInteraction(false);
        isHovered = false;
        Sequence seq = DOTween.Sequence();
        seq.Append(rectTransform.DOLocalMove(
            new Vector3(baseLocalPos.x, baseLocalPos.y + 300f, baseLocalPos.z),
            0.2f
        ).SetEase(Ease.InQuad));
        seq.Append(rectTransform.DOScale(1.2f, 0.2f));
        seq.Append(rectTransform.DOScale(0.8f, 0.2f));
        seq.OnComplete(() =>
        {
            handManager.RemoveCard(this.gameObject);
        });
    }
}
