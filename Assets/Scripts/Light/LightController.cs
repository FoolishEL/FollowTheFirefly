using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private FireFly prefab;
    private List<FireFly> _activeFireFlies;
    private List<FireFly> _inRoadFireFlies;
    private List<FireFly> _inactiveFireFlies;
    [SerializeField] private Camera camera;
    [SerializeField] private int startMaxPLayerLights = 4;
    [SerializeField] private List<Transform> staticBonusLights;
    [SerializeField] private LightSettings lightSettings;
    [SerializeField] private float flyTouchDistance = 1f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform lampTransform;
    [SerializeField] private AnimationForLantern animController;

    private void Awake()
    {
        _activeFireFlies = new List<FireFly>();
        _inactiveFireFlies = new List<FireFly>();
        _inRoadFireFlies = new List<FireFly>();
        animController.SetNumber(4);
    }

    private void Update()
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = prefab.transform.position.z;
        if (Input.GetMouseButtonDown(0))
        {
            if (_activeFireFlies.Count + _inRoadFireFlies.Count == startMaxPLayerLights)
            {
                if (_activeFireFlies.Count == 0)
                    return;
                if (TryGetSafeFlyToReplace(out var fly))
                {
                    if (fly.MoveToTransform(lampTransform))
                    {
                        _inRoadFireFlies.Add(fly);
                        _activeFireFlies.RemoveAt(0);
                        StartCoroutine(ResendFireFly(fly, worldPosition));
                    }
                }
            }
            else
            {
                FireFly fly;
                if (_inactiveFireFlies.Count != 0)
                {
                    fly = _inactiveFireFlies.First();

                    fly.gameObject.SetActive(true);
                    if (fly.MoveToPosition(worldPosition))
                    {
                        _inactiveFireFlies.RemoveAt(0);
                        animController.SetNumber(animController.CurrentCount - 1);
                        Vector3 startPosition = lampTransform.position;
                        startPosition.z = fly.transform.position.z;
                        fly.transform.position = startPosition;
                        _inRoadFireFlies.Add(fly);
                        StartCoroutine(PlaceIntoActiveAwaitor(fly));
                    }
                    else
                    {
                        fly.gameObject.SetActive(false);
                    }
                }
                else
                {
                    fly = Instantiate(prefab, transform);
                    Vector3 startPosition = lampTransform.position;
                    startPosition.z = fly.transform.position.z;
                    fly.transform.position = startPosition;
                    _inRoadFireFlies.Add(fly);
                    fly.MoveToPosition(worldPosition);
                    animController.SetNumber(animController.CurrentCount - 1);
                    StartCoroutine(PlaceIntoActiveAwaitor(fly));
                }

            }
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetSafeFlyInRange(worldPosition,out var fly))
            {
                if (fly.MoveToTransform(lampTransform))
                {
                    _activeFireFlies.Remove(fly);
                    _inRoadFireFlies.Add(fly);
                    StartCoroutine(PlaceIntoInactiveAwaitor(fly));
                }
            }
        }
    }

    private bool TryGetSafeFlyInRange(Vector3 worldPosition,  out FireFly fly)
    {
        var flies = _activeFireFlies.Where(c =>
            Vector2.Distance(worldPosition, c.transform.position) < flyTouchDistance).ToList();
        fly = null;
        if (flies.Any())
        {
            List<Vector2> areasCenters = new List<Vector2>();
            foreach (var suitableFly in flies)
            {
                areasCenters.Clear();
                areasCenters.AddRange(_activeFireFlies.Where(c => c != suitableFly)
                    .Select(c => (Vector2)c.transform.position));
                areasCenters.AddRange(staticBonusLights.Select(c => (Vector2)c.position));
                if (areasCenters.Any(c => Vector2.Distance(c, playerTransform.position) < lightSettings.walkAbleRange))
                {
                    fly = suitableFly;
                    return true;
                }
            }
        }
        return fly != null;
    }

    private bool TryGetSafeFlyToReplace(out FireFly fly)
    {
        fly = null;
        List<(FireFly, bool)> isFlyInPlayerRange = new List<(FireFly, bool)>();
        foreach (var f in _activeFireFlies)
        {
            isFlyInPlayerRange.Add((f,
                Vector2.Distance(playerTransform.position, f.transform.position) <= lightSettings.walkAbleRange));
        }

        List<Vector2> areasCenters = new List<Vector2>();
        for (int i = 0; i < isFlyInPlayerRange.Count; i++)
        {
            if (isFlyInPlayerRange[i].Item2)
            {
                areasCenters.Clear();
                areasCenters.AddRange(_activeFireFlies.Where(c => c != isFlyInPlayerRange[i].Item2)
                    .Select(c => (Vector2)c.transform.position));
                areasCenters.AddRange(staticBonusLights.Select(c => (Vector2)c.position));
                if (areasCenters.Any(c => Vector2.Distance(c, playerTransform.position) < lightSettings.walkAbleRange))
                {
                    fly = isFlyInPlayerRange[i].Item1;
                    return true;
                }
            }
            else
            {
                fly = isFlyInPlayerRange[i].Item1;
                break;
            }
        }
        return fly != null;
    }

    private IEnumerator ResendFireFly(FireFly fireFly, Vector3 destination)
    {
        while (fireFly.IsMooving)
        {
            yield return null;
        }
        yield return null;
        yield return null;
        if (fireFly.MoveToPosition(destination))
            StartCoroutine(PlaceIntoActiveAwaitor(fireFly));
    }

    private IEnumerator PlaceIntoActiveAwaitor(FireFly fireFly)
    {
        while (isActiveAndEnabled && fireFly.IsMooving)
        {
            yield return null;
        }
        _inRoadFireFlies.Remove(fireFly);
        _activeFireFlies.Add(fireFly);
    }

    private IEnumerator PlaceIntoInactiveAwaitor(FireFly fireFly)
    {
        while (isActiveAndEnabled&& fireFly.IsMooving)
        {
            yield return null;
        }
        animController.SetNumber(animController.CurrentCount + 1);
        _inRoadFireFlies.Remove(fireFly);
        _inactiveFireFlies.Add(fireFly);
        fireFly.gameObject.SetActive(false);
    }

    public IReadOnlyList<Vector2> GetWalkableArea(out float range)
    {
        range = lightSettings.walkAbleRange;
        List<Vector2> areasCenters = new List<Vector2>();
        areasCenters.AddRange(_activeFireFlies.Select(c => (Vector2)c.transform.position));
        areasCenters.AddRange(staticBonusLights.Select(c => (Vector2)c.position));
        return areasCenters;
    }

    [Serializable]
    public struct LightSettings
    {
        public float lightRange;
        public float walkAbleRange;
    }
    
    [Serializable]
    public class AnimationForLantern
    {
        public int CurrentCount { get; protected set; } = 4;
        [SerializeField] private AnimationReferenceAsset zero;
        [SerializeField] private AnimationReferenceAsset one;
        [SerializeField] private AnimationReferenceAsset two;
        [SerializeField] private AnimationReferenceAsset three;
        [SerializeField] private AnimationReferenceAsset four;
        [SerializeField] private SkeletonAnimation animation;

        public void SetNumber(int number)
        {
            CurrentCount = number;
            AnimationReferenceAsset current = zero;
            if (number == 1)
                current = one;
            if (number == 2)
                current = two;
            if (number == 3)
                current = three;
            if (number == 4)
                current = four;
            animation.state.SetAnimation(0, current, true);
        }
    }
}