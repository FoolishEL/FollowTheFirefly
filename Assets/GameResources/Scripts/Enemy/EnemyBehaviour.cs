using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float stoppingDistance = .2f;
    [SerializeField] private AnimationReferenceAsset becomeAnger;
    [SerializeField] private AnimationReferenceAsset angryWalk;
    [SerializeField] private AnimationReferenceAsset idleAnimation;
    [SerializeField] private float angryDelay = .2f;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float idleRange = 2f;
    [SerializeField, Space(20f)] private float idleMovementRange = 1f;
    [SerializeField] private float idleStoppingDistance = .05f;
    [SerializeField] private float withoutLightFollowAwaitTime = .4f;

    [SerializeField, Space(20f)] private Transform aye;
    [SerializeField] private float ayeRange = .1f;

    private Coroutine _idleCoroutine;
    private Coroutine _killingCoroutine;
    public static event Action beatedByMonster = delegate { };
    
    private List<Vector2> _lightedPositions;
    private LightController _lightController;
    private Transform _playerTransform;
    private Vector2 _idlePosition;
    
    private bool _isKillingMode = false;
    private bool _isNearIdlePosition = true;
    private bool _isMovingToNewPosition = false;
    private bool _isRespawnRequested = false;
    private MonstersAi _monstersAi;

    private void Start()
    {
        withoutLightFollowAwaitTime = GameManager.Instance.MonsterTimeReagre;
    }

    public void Initialize(LightController lightController,Transform playerTransform,MonstersAi monstersAi)
    {
        _isKillingMode = false;
        _isNearIdlePosition = true;
        _isRespawnRequested = false;
        _isMovingToNewPosition = false;
        _monstersAi = monstersAi;
        monstersAi.RegisterEnemy(this);
        _idlePosition = transform.position;
        _playerTransform = playerTransform;
        _lightedPositions = new List<Vector2>();
        _lightController = lightController;
        skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
        _lightController.onActiveLightPositionChanged += UpdateLightPositions;
        UpdateLightPositions();
        _idleCoroutine = StartCoroutine(MonsterIdling());
        StartCoroutine(AyeUpdate());
        StartCoroutine(CheckDistance());
    }

    private IEnumerator CheckDistance()
    {
        WaitForSeconds awaitor = new WaitForSeconds(1f);
        while (isActiveAndEnabled)
        {
            yield return awaitor;
            if(!isActiveAndEnabled)
                yield break;
            if (Vector2.Distance(transform.position, _playerTransform.position) > _monstersAi.MaxAdditionalDistance*1.2f +
                _lightController.CurrentLightSettings.lightRange)
            {
                RequestRespawn();
            }
        }
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
        if (_isMovingToNewPosition || _isKillingMode || _isRespawnRequested)
            return;
        _isMovingToNewPosition = true;
        _idlePosition = position;
        StopCoroutine(_idleCoroutine);
        StartCoroutine(MoveToNewIdlePosition(position));
    }

    private IEnumerator MoveToNewIdlePosition(Vector2 position)
    {
        if (_isKillingMode || _isRespawnRequested)
            yield break;
        Vector2 moveDirection = Vector2.one;
        moveDirection = position - (Vector2)transform.position;
        moveDirection = moveDirection.normalized * movementSpeed * Time.deltaTime;

        while (isActiveAndEnabled && Vector2.Distance(transform.position, position) > stoppingDistance)
        {
            if (_lightedPositions.Any(c =>
                    Vector2.Distance((Vector2)transform.position + moveDirection * 2, c) <
                    _lightController.CurrentLightSettings.lightRange))
            {
                _idlePosition = (Vector2)transform.position - moveDirection * 2f;
                break;
            }
            transform.position += (Vector3)moveDirection;
            yield return null;
        }
        if (!isActiveAndEnabled)
            yield break;
        _isMovingToNewPosition = false;
        _idleCoroutine = StartCoroutine(MonsterIdling());
    }

    private void CheckInRange()
    {
        if (_isKillingMode)
        {
            return;
        }
        foreach (var item in _lightedPositions)
        {
            if (Vector2.Distance(transform.position, item) < _lightController.CurrentLightSettings.lightRange)
            {
                if (_idleCoroutine != null)
                    StopCoroutine(_idleCoroutine);
                _isKillingMode = true;
                StartCoroutine(LightDisapearAwaitor());
                _killingCoroutine = StartCoroutine(KillingMode());
            }
        }
    }

    private IEnumerator LightDisapearAwaitor()
    {
        for (float time = 0; time < withoutLightFollowAwaitTime && isActiveAndEnabled; time += Time.deltaTime)
        {
            if (_lightedPositions.Any(c =>
                    Vector2.Distance(c, transform.position) <=
                    _lightController.CurrentLightSettings.lightRange + .1f))
                time = 0f;
            yield return null;
        }
        _isKillingMode = false;
        StopCoroutine(_killingCoroutine);
        skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
        _isKillingMode = false;
        _isNearIdlePosition = true;
        _isRespawnRequested = false;
        _isMovingToNewPosition = false;
        _idleCoroutine = StartCoroutine(MonsterIdling());
    }

    private IEnumerator KillingMode()
    {
        yield return new WaitForSeconds(angryDelay);
        skeletonAnimation.state.SetAnimation(0, becomeAnger, false);
        yield return new WaitForSeconds(becomeAnger.Animation.Duration * skeletonAnimation.timeScale);
        skeletonAnimation.state.SetAnimation(0, angryWalk, true);
        Vector2 moveDirection = Vector2.one;
        while (Vector2.Distance(transform.position, _playerTransform.position) > stoppingDistance)
        {
            moveDirection = _playerTransform.position - transform.position;
            moveDirection = moveDirection.normalized * movementSpeed * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            yield return null;
        }
        beatedByMonster.Invoke();
    }

    private IEnumerator MonsterIdling()
    {
        while (isActiveAndEnabled)
        {
            int randomCount = Random.Range(3, 7);
            for (int i = 0; i < randomCount; i++)
            {
                yield return null;
                if (_isNearIdlePosition)
                {
                    float time = Random.Range(1f, 4f);
                    yield return new WaitForSeconds(time);
                    Vector2 position2D = (Vector2)transform.position;
                    Vector2 randomPosition = Random.insideUnitCircle * idleRange;
                    randomPosition += _idlePosition;
                    if (_lightedPositions.Any(c =>
                            Vector2.Distance(c, randomPosition) < _lightController.CurrentLightSettings.lightRange))
                    {
                        RequestRespawn();
                        continue;
                    }

                    Vector2 moveStep = randomPosition - position2D;
                    moveStep = moveStep.normalized * movementSpeed * Time.deltaTime;
                    if (Vector2.Distance(transform.position, randomPosition) > idleStoppingDistance)
                    {
                        PLayMoveSound();
                    }
                    while (Vector2.Distance(transform.position, randomPosition) > idleStoppingDistance)
                    {
                        transform.position += (Vector3)moveStep;
                        yield return null;
                    }
                }
            }
            RequestRespawn();
        }
    }
    private void RequestRespawn()
    {
        _isKillingMode = false;
        _isNearIdlePosition = true;
        _isRespawnRequested = true;
        _isMovingToNewPosition = false;
        StopAllCoroutines();
        _monstersAi.RequestRespawn(this);
    }

    public void Respawn(Vector2 position)
    {
        _isKillingMode = false;
        _isNearIdlePosition = true;
        _isRespawnRequested = false;
        _isMovingToNewPosition = false;
        _idlePosition = position;
        skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        UpdateLightPositions();
        _idleCoroutine = StartCoroutine(MonsterIdling());
        StartCoroutine(AyeUpdate());
        StartCoroutine(CheckDistance());
    }

    private void PLayMoveSound()
    {
        AudioManager.instance.Play($"Scratch{Random.Range(1, 2)}", .7f, Random.Range(.4f, .6f));
    }
}
