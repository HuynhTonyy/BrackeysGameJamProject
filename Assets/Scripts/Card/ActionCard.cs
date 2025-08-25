using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionCard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    ActionCardSO cardData; // this will hold reference to the ScriptableObject

    [Header("UI References")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    private Image borderCard;

    void Awake()
    {
        borderCard = GetComponent<Image>();
    }
    public void Init(ActionCardSO data)
    {
        cardData = data;
        cardNameText.SetText(cardData.cardName);
        cardDescriptionText.SetText(cardData.cardDescription);

        if (borderCard != null)
        {
            switch (cardData.cardType)
            {
                case CardType.Move: borderCard.color = Color.green; break;
                case CardType.Buff: borderCard.color = Color.yellow; break;
                case CardType.Debuff: borderCard.color = Color.magenta; break;
                default: borderCard.color = Color.white; break;
            }
        }
    }
}
