using UnityEngine;
using DG.Tweening;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void OnEnable() {
        EventManager.Instance.onPlayMoveCard += PlayMoveAnimation;
    }
    void PlayMoveAnimation(int step, int turnUsed)
    {
        if (animator != null)
        {
            animator.SetTrigger("Move");
        }
        // transform.DOMoveX(transform.position.x + step * 2.0f, step * 0.5f).SetEase(Ease.Linear).OnComplete(()=>
        // {
        //     EventManager.Instance.PlayerMoveEnd();
        // });
    }
}
