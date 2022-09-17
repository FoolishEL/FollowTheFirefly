using System;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMoveController : MonoBehaviour
{
    #region Animations
    [Header("Animation Settings")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [FormerlySerializedAs("up")] [SerializeField]
    private AnimationReferenceAsset upMove;

    [FormerlySerializedAs("down")] [SerializeField]
    private AnimationReferenceAsset downMove;

    [FormerlySerializedAs("right")] [SerializeField]
    private AnimationReferenceAsset rightMove;

    [SerializeField] private AnimationReferenceAsset upIdle;
    [SerializeField] private AnimationReferenceAsset downIdle;
    [SerializeField] private AnimationReferenceAsset rightIdle;
    [FormerlySerializedAs("idleAnimSpeed")] [SerializeField] private float idleAnimStoppingSpeed;
    #endregion
    
    [SerializeField,Space(20f),Header("PLayer Settings")] private float speed = 2f;
    private Vector2Int direction = Vector2Int.zero;
    private Sides _sides;
    private bool isLastStopped = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LightController lightController;

    private void Awake()
    {
        rb.gravityScale = 0f;
        _sides = Sides.Down;
    }

    private void Update()
    {
        direction = Vector2Int.zero;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            direction += Vector2Int.up;
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            direction += Vector2Int.down;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            direction += Vector2Int.left;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            direction += Vector2Int.right;
        }
        
        if (direction != Vector2Int.zero)
        {
            if (CanMoveThisDirection(direction))
            {
                Vector2 directionNormalized = direction;
                directionNormalized.Normalize();
                rb.velocity = (rb.velocity + directionNormalized * speed).normalized * speed;
            }
        }

        if (rb.velocity.magnitude > idleAnimStoppingSpeed)
        {
            isLastStopped = false;
            if (Mathf.Abs(rb.velocity.y) >= Math.Abs(rb.velocity.x))
            {
                if (!((_sides == Sides.Up && rb.velocity.y > 0) || (_sides == Sides.Down && rb.velocity.y <= 0)))
                {
                    skeletonAnimation.state.SetAnimation(0, rb.velocity.y > 0 ? upMove : downMove, true);
                }

                _sides = rb.velocity.y > 0 ? Sides.Up : Sides.Down;
            }
            else
            {
                if (!((_sides == Sides.Right && rb.velocity.x > 0) || (_sides == Sides.Left && rb.velocity.x <= 0)))
                {
                    skeletonAnimation.state.SetAnimation(0, rightMove, true);
                    transform.localScale = new Vector3(rb.velocity.x > 0 ? 1f : -1f, 1f, 1f);
                }

                _sides = rb.velocity.x > 0 ? Sides.Right : Sides.Left;
            }
        }
        else
        {
            if (!isLastStopped)
                if (_sides == Sides.Left || _sides == Sides.Right)
                {
                    skeletonAnimation.state.SetAnimation(0, rightIdle, true);
                }
                else
                {
                    if (_sides == Sides.Up)
                    {
                        skeletonAnimation.state.SetAnimation(0, upIdle, true);
                    }
                    else
                    {
                        skeletonAnimation.state.SetAnimation(0, downIdle, true);
                    }
                }

            isLastStopped = true;
        }
    }

    private bool CanMoveThisDirection(Vector2Int targetDirection)
    {
        var areas = lightController.GetWalkableArea(out var range);
        bool canMove = false;
        canMove = areas.Any(c => Vector2.Distance(c, transform.position) < range);
        if (!canMove)
        {
            canMove = areas.Any(
                c => Vector2.Distance(c, (Vector2)transform.position + targetDirection) < range);
        }
        return canMove;
    }
}