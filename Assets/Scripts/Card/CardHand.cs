using System.Linq;
using TMPro;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Card Setup")]
    [SerializeField] private ActionCardSO[] defaultCardHand;
    [SerializeField] private GameObject CardPrefab;

    void Start()
    {
        InitCardHand();
    }

    void InitCardHand()
    {
        var sortedCards = defaultCardHand.OrderBy(c => c.cardType).ToArray();
        for (int i = 0; i < sortedCards.Length; i++)
        {
            ActionCardSO data = sortedCards[i];
            Vector3 spawnPos = transform.position + new Vector3(i, 0, 0);
            GameObject cardObj = Instantiate(CardPrefab, spawnPos, Quaternion.identity, transform);
            ActionCard card = cardObj.GetComponent<ActionCard>();
            if (card != null)
            {
                card.Init(data);
            }
        }
    }

    // Update is called once per frame

}
