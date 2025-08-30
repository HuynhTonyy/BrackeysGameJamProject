using UnityEngine;
using DG.Tweening;
using System;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float gridRadius;
    [SerializeField] private float jumpDistance;
    [SerializeField] private float jumpPower;

    private void OnEnable()
    {
        EventManager.Instance.onPlayMoveCard += PlayMoveAnimation;
        EventManager.Instance.onEnterMoveForwardGrid += PlayMoveAnimation;
    }
    private void OnDisable()
    {
        EventManager.Instance.onPlayMoveCard -= PlayMoveAnimation;
        EventManager.Instance.onEnterMoveForwardGrid -= PlayMoveAnimation;
    }
    private void OnDestroy() {
        EventManager.Instance.onPlayMoveCard -= PlayMoveAnimation;
        EventManager.Instance.onEnterMoveForwardGrid -= PlayMoveAnimation;
        
    }
    void PlayMoveAnimation(int step, int turnUsed)
    {
        if (animator == null) return;
        Sequence mainSequence = DOTween.Sequence();
        Vector3 currentPos = transform.position;
        if (step < 0)
        {
            gridRadius *= -1;
            jumpDistance *= -1;
        }
        float segmentDistance = gridRadius + jumpDistance + gridRadius;
        for (int i = 0; i < Math.Abs(step); i++)
        {
            mainSequence.Append(GetSubSequence(currentPos));
            currentPos += new Vector3(segmentDistance, 0, 0);
        }
        mainSequence.AppendCallback(() =>
        {
            transform.position = currentPos;
            EventManager.Instance.PlayerMoveEnd();
        });

    }
    Sequence GetSubSequence(Vector3 currentPos) {
        Sequence subSequence = DOTween.Sequence();
        float newX = currentPos.x + gridRadius;
        subSequence.Append(transform.DOMoveX(newX, 1f).SetEase(Ease.Linear));
        subSequence.AppendCallback(() =>
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsJumping", true);
        });
        Vector3 jumpTo = new Vector3(newX + jumpDistance, currentPos.y, currentPos.z);
        subSequence.Append(transform.DOJump(jumpTo, jumpPower, 2, 1.5f).SetEase(Ease.Linear));
        subSequence.Append(transform.DOMoveX(jumpTo.x + gridRadius, 1f).SetEase(Ease.Linear));
        subSequence.AppendCallback(() =>
        {
            animator.SetBool("IsJumping", false);
        });
        return subSequence;
    }
}
