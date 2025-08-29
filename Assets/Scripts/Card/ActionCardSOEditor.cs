using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionCardSO))]
public class ActionCardSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the target object
        ActionCardSO actionCard = (ActionCardSO)target;

        // Draw default fields manually
        actionCard.cardName = EditorGUILayout.TextField("Card Name", actionCard.cardName);
        actionCard.cardDescription = EditorGUILayout.TextField("Card Description", actionCard.cardDescription);
        actionCard.cardType = (CardType)EditorGUILayout.EnumPopup("Card Type", actionCard.cardType);
        actionCard.cardObject = (GameObject)EditorGUILayout.ObjectField("Card Object", actionCard.cardObject, typeof(GameObject), false);


        // Show `step` only if CardType is Move
        if (actionCard.cardType == CardType.Move)
        {
            actionCard.step = EditorGUILayout.IntField("Step", actionCard.step);
        }
        else if (actionCard.cardType == CardType.TradeOff)
        {
            actionCard.tradeOffType = (TradeOffType)EditorGUILayout.EnumPopup("TradeOff Type", actionCard.tradeOffType);
            actionCard.isTemporary = EditorGUILayout.Toggle("Is Temporary", actionCard.isTemporary);
            if (actionCard.isTemporary)
            {
                actionCard.duration = EditorGUILayout.IntField("Duration", actionCard.duration);
            }
        }

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(actionCard);
        }
    }
}
