using UnityEngine;
using UnityEngine.InputSystem.iOS;

[CreateAssetMenu(fileName = "ActionCardSO", menuName = "Scriptable Objects/ActionCardSO")]
public class ActionCardSO : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public CardType cardType;
    // public Sprite cardImage;
}

public enum CardType
{
    Move,
    Buff,
    Debuff
}
