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
    public event Action<int,int> onPlayMoveAnimation;
    public void PlayMoveAnimation(int step, int turnUsed)
    {
        Instance?.onPlayMoveAnimation?.Invoke(step,turnUsed);
    }
    public event Action onPlayerMoveEnd;
    public void PlayerMoveEnd()
    {
        Instance?.onPlayerMoveEnd?.Invoke();
    }
    public event Action OnCompleteAction;
    public void CompleteAction()
    {
        Instance?.OnCompleteAction?.Invoke();
    }
    public event Action onClickDrawCard;
    public void ClickDrawCard()
    {
        Instance?.onClickDrawCard?.Invoke();
    }
    public event Action<AudioClip> onPlaySFX;
    public void PlaySFX(AudioClip audioClip)
    {
        Instance?.onPlaySFX?.Invoke(audioClip);
    }
    #region Grid Events
    public event Action<Vector3> onEnterPortalGrid;
    public void EnterPortalGrid(Vector3 newPosition)
    {
        Instance?.onEnterPortalGrid?.Invoke(newPosition);
    }
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
    public event Action<int, int> onEnterMoveForwardGrid;
    public void EnterMoveForwardGrid(int step, int staminaUsed)
    {
        Instance?.onEnterMoveForwardGrid.Invoke(step, staminaUsed);
    }
    public event Action<int> onEnterMoveBackwardGrid;
    public void EnterMoveBackwardGrid(int step)
    {
        Instance?.onEnterMoveBackwardGrid?.Invoke(step);
    }
    public event Action onExplodeBomb;
    public void ExplodeBomb()
    {
        Instance?.onExplodeBomb?.Invoke();
    }
    public event Action<int> onIslandSinkEnd;
    public void IslandSinkEnd(int sinkToIndex)
    {
        Instance?.onIslandSinkEnd?.Invoke(sinkToIndex);
    }
    #endregion

    #region Nofity Events
    public event Action<int> onStaminaChange;
    public void StaminaChange(int amount)
    {
        Instance?.onStaminaChange?.Invoke(amount);
    }
    public event Action<int> onCardChange;
    public void CardChange(int amount)
    {
        Instance?.onCardChange?.Invoke(amount);
    }
    public event Action<int> onDistanceChange;
    public void DistanceChange(int amount)
    {
        Instance?.onDistanceChange?.Invoke(amount);
    }
    #endregion

}
