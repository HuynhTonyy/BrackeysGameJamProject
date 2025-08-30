using UnityEngine;
using UnityEngine.Events;
using System;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public event Action onTurnChange;
    public void TurnChange()
    {
        Instance?.onTurnChange?.Invoke();
    }
    public event Action onGameStart;
    public void GameStart()
    {
        Instance?.onGameStart?.Invoke();
    }
    public event Action onGameEnd;
    public void GameEnd()
    {
        Instance?.onGameEnd?.Invoke();
    }
    public event Action<ActionCardSO> onPlayCard;
    public void PlayCard(ActionCardSO cardData)
    {
        Instance?.onPlayCard?.Invoke(cardData);
    }
    public event Action<GameObject> onCardPlayAnimationEnd;
    public void CardPlayAnimationEnd(GameObject gameObject)
    {
        Instance?.onCardPlayAnimationEnd?.Invoke(gameObject);
    }
    public event Action onRepeatCardPlayed;
    public void RepeatCardPlayed()
    {
        Instance?.onRepeatCardPlayed?.Invoke();
    }
    public event Action<int, int> onPlayMoveCard;
    public void PlayMoveCard(int step, int turnUsed)
    {
        Instance?.onPlayMoveCard?.Invoke(step, turnUsed);
    }
    public event Action onPlayerMoveEnd;
    public void PlayerMoveEnd()
    {
        Instance?.onPlayerMoveEnd?.Invoke();
    }
    #region Grid Events
    public event Action<GameObject> onEnterAddCardGrid;
    public void EnterAddCardGrid(GameObject cardPrefab = null)
    {
        Instance?.onEnterAddCardGrid?.Invoke(cardPrefab);
    }
    public event Action onEnterDropCardGrid;
    public void EnterDropCardGrid()
    {
        Instance?.onEnterDropCardGrid?.Invoke();
    }
    #endregion

}
