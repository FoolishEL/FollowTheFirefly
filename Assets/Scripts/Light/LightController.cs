using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private void Awake()
    {
        _activeFireFlies = new List<FireFly>();
        _inactiveFireFlies = new List<FireFly>();
        _inRoadFireFlies = new List<FireFly>();
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
                var firstFly = _activeFireFlies.First();
                _inRoadFireFlies.Add(firstFly);
                _activeFireFlies.RemoveAt(0);
                firstFly.MoveToTransform(playerTransform);
                StartCoroutine(ResendFireFly(firstFly, worldPosition));
            }
            else
            {
                FireFly fly;
                if (_inactiveFireFlies.Count != 0)
                {
                    fly = _inactiveFireFlies.First();

                    fly.gameObject.SetActive(true);
                    _inactiveFireFlies.RemoveAt(0);
                }
                else
                {
                    fly = Instantiate(prefab, transform);
                }

                Vector3 startPosition = playerTransform.position;
                startPosition.z = fly.transform.position.z;
                fly.transform.position = startPosition;
                _inRoadFireFlies.Add(fly);
                fly.MoveToPosition(worldPosition);
                fly.onDestibationReached += PlaceIntoActive;
            }

            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (_activeFireFlies.Any(c => Vector2.Distance(worldPosition, c.transform.position) < flyTouchDistance))
            {
                var fly = _activeFireFlies.First(c =>
                    Vector2.Distance(worldPosition, c.transform.position) < flyTouchDistance);
                _activeFireFlies.Remove(fly);
                _inRoadFireFlies.Add(fly);
                fly.MoveToTransform(playerTransform);
                fly.onDestibationReached += PlaceIntoInactive;
            }
        }
    }

    private IEnumerator ResendFireFly(FireFly fireFly, Vector3 destination)
    {
        while (fireFly.IsMooving)
        {
            yield return null;
        }

        fireFly.MoveToPosition(destination);
        fireFly.onDestibationReached += PlaceIntoActive;
    }

    private void PlaceIntoActive(FireFly fireFly)
    {
        _inRoadFireFlies.Remove(fireFly);
        _activeFireFlies.Add(fireFly);
        fireFly.onDestibationReached -= PlaceIntoActive;
    }

    private void PlaceIntoInactive(FireFly fireFly)
    {
        _inRoadFireFlies.Remove(fireFly);
        _inactiveFireFlies.Add(fireFly);
        fireFly.gameObject.SetActive(false);
        fireFly.onDestibationReached -= PlaceIntoInactive;
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
}