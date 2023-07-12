using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private FireFly prefab;
    [SerializeField]
    private List<FireFly> _activeFireFlies;
    [SerializeField]
    private List<FireFly> _inRoadFireFlies;
    [SerializeField]
    private List<FireFly> _inactiveFireFlies;
    private List<FireFly> _allFlies;
    [SerializeField] private Camera camera;
    [SerializeField] private int startMaxPLayerLights = 4;
    [SerializeField] private List<Transform> staticBonusLights;
    [SerializeField] private LightSettings lightSettings;
    [SerializeField] private float flyTouchDistance = 1f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform lampTransform;
    [SerializeField] private AnimationForLantern animController;
    [SerializeField] private float lightsOffAnimation = 2f;
    public LightSettings CurrentLightSettings => lightSettings;
    public event Action onActiveLightPositionChanged = delegate { };

    private void Awake()
    {
        _activeFireFlies = new List<FireFly>();
        _inactiveFireFlies = new List<FireFly>();
        _inRoadFireFlies = new List<FireFly>();
        _allFlies = new List<FireFly>();
        for (int i = 0; i < startMaxPLayerLights; i++)
        {
            var fly = Instantiate(prefab, transform);
            Vector3 startPosition = lampTransform.position;
            startPosition.z = fly.transform.position.z;
            fly.gameObject.SetActive(false);
            fly.transform.position = startPosition;
            _allFlies.Add(fly);
            _inactiveFireFlies.Add(fly);
        }
        TileColliderSetter.onLightAdded += AddStaticLight;
        TileColliderSetter.onLightRemoved += RemoveStaticLight;
        EnemyBehaviour.beatedByMonster += TurnOffLights;
    }

    private void TurnOffLights()
    {
        if(!GameManager.Instance.isPlaying)
            return;
        GameManager.Instance.isPlaying = false;
        List<Transform> areasCenters = new List<Transform>();
        areasCenters.AddRange(_activeFireFlies.Select(c => c.transform));
        areasCenters.AddRange(staticBonusLights);
        DelayLightsOff(areasCenters);
    }

    private async UniTask DelayLightsOff(List<Transform> lights)
    {
        for (float time = 0; time < lightsOffAnimation; time += Time.deltaTime)
        {
            await UniTask.Yield();
            foreach (var light in lights)
            {
                light.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / lightsOffAnimation);
            }
        }
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        GameManager.Instance.ToMainMenu();
    }

    private void OnDestroy()
    {
        TileColliderSetter.onLightAdded -= AddStaticLight;
        TileColliderSetter.onLightRemoved -= RemoveStaticLight;
        EnemyBehaviour.beatedByMonster -= TurnOffLights;
    }

    private void Start()
    {
        animController.SetNumber(4);
        StartCoroutine(FlyFixer());
    }

    private void Update()
    {
        if(!GameManager.Instance.isPlaying)
            return;
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
                    Debug.LogError("No flies!!");
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
                    onActiveLightPositionChanged.Invoke();
                    StartCoroutine(PlaceIntoInactiveAwaitor(fly));
                }
            }
        }
    }

    private IEnumerator FlyFixer()
    {
        WaitForSeconds awaiter = new WaitForSeconds(1f);
        while (isActiveAndEnabled)
        {
            yield return awaiter;
            if ((_activeFireFlies.Count + _inactiveFireFlies.Count + _inRoadFireFlies.Count != startMaxPLayerLights) ||
                (_activeFireFlies.Distinct().Count() != _activeFireFlies.Count) ||
                (_inactiveFireFlies.Distinct().Count() != _inactiveFireFlies.Count) ||
                (_inRoadFireFlies.Distinct().Count() != _inRoadFireFlies.Count))
                FixFlies();
        }
    }

    private void FixFlies()
    {
        _activeFireFlies.Clear();
        _inactiveFireFlies.Clear();
        _inRoadFireFlies.Clear();
        for (int i = 0; i < _allFlies.Count; i++)
        {
            if (_allFlies[i].IsMooving)
                _inRoadFireFlies.Add(_allFlies[i]);
            else
            {
                if (!_allFlies[i].gameObject.activeSelf)
                {
                    _inactiveFireFlies.Add(_allFlies[i]);
                }
                else
                {
                    _activeFireFlies.Add(_allFlies[i]);
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
        onActiveLightPositionChanged.Invoke();
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

    public void AddStaticLight(Transform transform)
    {
        staticBonusLights.Add(transform);
    }

    public void RemoveStaticLight(Transform transform)
    {
        staticBonusLights.Remove(transform);
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