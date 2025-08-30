using UnityEngine;
using DG.Tweening;
using System;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float gridRadius;
    [SerializeField] private float jumpDistance;
    [SerializeField] private float jumpPower;
    private float newGridRadius;
    private float newJumpDistance;
    private void OnEnable()
    {
        EventManager.Instance.onPlayMoveCard += PlayMoveAnimation;
        EventManager.Instance.onEnterMoveForwardGrid += PlayMoveAnimation;
        EventManager.Instance.onEnterPortalGrid += AppearInLocation;
    }
    private void OnDisable()
    {
        EventManager.Instance.onPlayMoveCard -= PlayMoveAnimation;
        EventManager.Instance.onEnterPortalGrid -= AppearInLocation;
        EventManager.Instance.onEnterMoveForwardGrid -= PlayMoveAnimation;
    }
    private void OnDestroy() {
        EventManager.Instance.onEnterPortalGrid -= AppearInLocation;
        EventManager.Instance.onPlayMoveCard -= PlayMoveAnimation;
        EventManager.Instance.onEnterMoveForwardGrid -= PlayMoveAnimation;
        
    }
    void AppearInLocation(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    void PlayMoveAnimation(int step, int turnUsed)
    {
        if (animator == null) return;
        Sequence mainSequence = DOTween.Sequence();
        Vector3 currentPos = transform.position;
        newGridRadius = gridRadius;
        newJumpDistance = jumpDistance;
        if (step < 0)
        {
            newGridRadius *= -1;
            newJumpDistance *= -1;
        }
        float segmentDistance = newGridRadius*2 + newJumpDistance;
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
        float newX = currentPos.x + newGridRadius;
        subSequence.Append(transform.DOMoveX(newX, 0.5f).SetEase(Ease.Linear));
        subSequence.AppendCallback(() =>
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsJumping", true);
        });
        Vector3 jumpTo = new Vector3(newX + newJumpDistance, currentPos.y, currentPos.z);
        subSequence.Append(transform.DOJump(jumpTo, jumpPower, 2, 0.5f).SetEase(Ease.Linear));
        subSequence.Append(transform.DOMoveX(jumpTo.x + newGridRadius, 0.5f).SetEase(Ease.Linear));
        subSequence.AppendCallback(() =>
        {
            animator.SetBool("IsJumping", false);
        });
        return subSequence;
    }
}
