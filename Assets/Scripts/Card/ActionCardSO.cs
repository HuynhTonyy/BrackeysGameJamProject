using UnityEngine;
using UnityEngine.InputSystem.iOS;

[CreateAssetMenu(fileName = "ActionCardSO", menuName = "Scriptable Objects/ActionCardSO")]
public class ActionCardSO : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public CardType cardType;
    public GameObject cardObject;
    public int step;
}

public enum CardType
{
    Move,
    Buff,
    Debuff
}
