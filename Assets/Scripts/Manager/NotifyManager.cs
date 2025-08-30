using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class NotifyManager : MonoBehaviour
{
    public static NotifyManager Instance;
    [SerializeField] private RectTransform notifySpawnPoint;
    [SerializeField] private GameObject notifyPrefab;
    [SerializeField] private Sprite staminaSprite, cardSprite, distanceSprite;
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
    private void OnEnable()
    {
        EventManager.Instance.onStaminaChange += CallStaminaNotify;
        EventManager.Instance.onCardChange += CallCardNotify;
        EventManager.Instance.onDistanceChange += CallDistanceNotify;
    }
    private void OnDisable()
    {
        EventManager.Instance.onStaminaChange -= CallStaminaNotify;
        EventManager.Instance.onCardChange -= CallCardNotify;
        EventManager.Instance.onDistanceChange -= CallDistanceNotify;
    }
    private void CallStaminaNotify(int amount)
    {
        CallNotify(staminaSprite, amount);
    }
    private void CallCardNotify(int amount)
    {
        CallNotify(cardSprite, amount);
    }
    private void CallDistanceNotify(int amount)
    {
        CallNotify(distanceSprite, amount);
    }
    private void CallNotify(Sprite sprite, int amount)
    {
        GameObject notify = Instantiate(notifyPrefab, notifySpawnPoint);
        notify.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        notify.transform.GetChild(1).GetComponent<TMP_Text>().text = amount > 0?"+"+amount:amount.ToString();
        Sequence seq = DOTween.Sequence();
        seq.Join(notify.transform.DOLocalMoveY(200, 3f).SetRelative().SetEase(Ease.OutCubic));
        seq.Join(notify.transform.DOScale(0, 3f)).OnComplete(() => Destroy(notify));
    }
}
