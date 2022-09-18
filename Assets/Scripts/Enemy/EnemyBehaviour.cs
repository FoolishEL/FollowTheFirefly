using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float stoppingDistance = .2f;
    [SerializeField] private AnimationReferenceAsset becomeAnger;
    [SerializeField] private AnimationReferenceAsset angryWalk;
    [SerializeField] private float angryDelay = .2f;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField, Space(20f)] private float idleMovementRange = 1f;
    [SerializeField] private float idleStoppingDistance = .05f;

    [SerializeField, Space(20f)] private Transform aye;
    [SerializeField] private float ayeRange = .1f;

    private Coroutine _idleCoroutine;
    public static event Action beatedByMonster = delegate { };
    
    private List<Vector2> _lightedPositions;
    private LightController _lightController;
    private Transform _playerTransform;
    private bool isKillingMode = false;
    private Vector2 _idlePosition;
    private bool _isNearIdlePosition = true;
    private bool isMoveingToNewPosition = false;

    public void Initialize(LightController lightController,Transform playerTransform,MonstersAi monstersAi)
    {
        monstersAi.RegisterEnemy(this);
        _isNearIdlePosition = true;
        _idlePosition = transform.position;
        _playerTransform = playerTransform;
        _lightedPositions = new List<Vector2>();
        _lightController = lightController;
        _lightController.onActiveLightPositionChanged += UpdateLightPositions;
        UpdateLightPositions();
        _idleCoroutine = StartCoroutine(MonsterIdling());
        StartCoroutine(AyeUpdate());

    }

    private IEnumerator AyeUpdate()
    {
        while (isActiveAndEnabled)
        {
            aye.localPosition = (Vector2)(_playerTransform.position - aye.transform.position).normalized * ayeRange;
            yield return null;
        }
    }

    private void OnDestroy()
    {
        _lightController.onActiveLightPositionChanged -= UpdateLightPositions;
        if (_idleCoroutine != null)
            StopCoroutine(_idleCoroutine);
    }

    private void UpdateLightPositions()
    {
        _lightedPositions.Clear();
        _lightedPositions.AddRange(_lightController.GetWalkableArea(out _));
        CheckInRange();
    }

    public void SetIdlePosition(Vector2 position)
    {
        if(isMoveingToNewPosition)
            return;
        isMoveingToNewPosition = true;
        _idlePosition = position;
        StopCoroutine(_idleCoroutine);
        MoveToNewIdlePosition(position);
    }

    private async UniTask MoveToNewIdlePosition(Vector2 position)
    {
        Vector2 moveDirection = Vector2.one;
        while (Vector2.Distance(transform.position, position) > stoppingDistance)
        {
            moveDirection = position - (Vector2)transform.position;
            moveDirection = moveDirection.normalized * movementSpeed * Time.deltaTime;
            if (_lightedPositions.Any(c =>
                    Vector2.Distance((Vector2)transform.position + moveDirection, c) <
                    _lightController.CurrentLightSettings.lightRange + 2f))
            {
                await UniTask.Yield();
                break;
            }
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }
        isMoveingToNewPosition = false;
        _idleCoroutine = StartCoroutine(MonsterIdling());
    }

    private void CheckInRange()
    {
        if(isKillingMode)
            return;
        foreach (var item in _lightedPositions)
        {
            if (Vector2.Distance(transform.position, item) < _lightController.CurrentLightSettings.lightRange)
            {
                if (_idleCoroutine != null)
                    StopCoroutine(_idleCoroutine);
                isKillingMode = true;
                _lightController.onActiveLightPositionChanged -= UpdateLightPositions;
                KillingMode();
            }
        }
    }

    private async UniTask KillingMode()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(angryDelay));
        skeletonAnimation.state.SetAnimation(0, becomeAnger, false);
        await UniTask.Delay(TimeSpan.FromSeconds(becomeAnger.Animation.Duration * skeletonAnimation.timeScale));
        skeletonAnimation.state.SetAnimation(0, angryWalk, true);
        Vector2 moveDirection = Vector2.one;
        while (Vector2.Distance(transform.position, _playerTransform.position) > stoppingDistance)
        {
            moveDirection = _playerTransform.position - transform.position;
            moveDirection = moveDirection.normalized * movementSpeed * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }
        beatedByMonster.Invoke();
    }

    private IEnumerator MonsterIdling()
    {
        while (isActiveAndEnabled)
        {
            yield return null;
            if (_isNearIdlePosition)
            {
                float time = Random.Range(2f, 4f);
                yield return new WaitForSeconds(time);
                Vector2 position2D = (Vector2)transform.position;
                Vector2 randomPosition = Random.insideUnitCircle * idleMovementRange;
                randomPosition += _idlePosition;
                if (_lightedPositions.Any(c =>
                        Vector2.Distance(c, randomPosition) < _lightController.CurrentLightSettings.lightRange + 2f))
                {
                    continue;
                }
                Vector2 moveStep = randomPosition - position2D;
                moveStep = moveStep.normalized * movementSpeed * Time.deltaTime;
                while (Vector2.Distance(transform.position, randomPosition) > idleStoppingDistance)
                {
                    transform.position += (Vector3)moveStep;
                    yield return null;
                }
            }
        }
    }
}
