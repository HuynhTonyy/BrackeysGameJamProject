using UnityEngine;

[CreateAssetMenu(fileName = "GridSO", menuName = "BrackeysGameJamProject/GridSO", order = 0)]
public class GridSO : ScriptableObject
{
    public enum GridType
    {
        Empty,
        MoveForward,
        MoveBackward,
        AddCard,
        DropCard,
        Swamp,
        Teleport,
        Start,
        End,
        CursedFrog,
        Scout,
    }
    public GridType gridType;
    public GameObject gameObject;
}