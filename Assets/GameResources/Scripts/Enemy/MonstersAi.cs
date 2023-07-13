using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonstersAi : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [FormerlySerializedAs("_lightController")] [SerializeField]
    private LightController lightController;

    private readonly List<EnemyBehaviour> _enemies = new List<EnemyBehaviour>();
    private List<Vector2> _lightPositions;
    [SerializeField] private float additionalLightAvoidance = 2f;
    [SerializeField] private float maxAdditionalDistance = 6f;
    public float MaxAdditionalDistance => maxAdditionalDistance;
    [SerializeField] private float timePositionChange = 3f;

    [Header("Avoidance settings")] [SerializeField]
    private int playerSurroundingMapSize = 11;

    [SerializeField, Range(.1f, 1f)] private float cellsSize = .4f;

    private (Vector2, bool)[,] _map;
    private float _range = 1.2f;
    private List<Vector2> suitablePoses;

    private void Start()
    {
        suitablePoses = new List<Vector2>();
        _lightPositions = new List<Vector2>();
        lightController.onActiveLightPositionChanged += UpdateLightPositions;
        if (playerSurroundingMapSize % 2 == 0)
            playerSurroundingMapSize += 1;
        _map = new (Vector2, bool)[playerSurroundingMapSize, playerSurroundingMapSize];
        for (int i = 0; i < playerSurroundingMapSize; i++)
        {
            for (int j = 0; j < playerSurroundingMapSize; j++)
            {
                _map[i, j] = (
                    new Vector2(i - (playerSurroundingMapSize - 1) / 2, j - (playerSurroundingMapSize - 1) / 2) *
                    cellsSize,
                    false);
            }
        }

        UpdateLightPositions();
        //StartCoroutine(SwitchMonstersPositions());
    }

    private IEnumerator SwitchMonstersPositions()
    {
        WaitForSeconds awaitor = new WaitForSeconds(timePositionChange);
        while (isActiveAndEnabled)
        {
            CalculateSutiblePoses();
            foreach (var enemy in _enemies)
            {
                Vector2 currentPosition = Vector2.zero;
                float minDistance = 1000f;
                var positionsCanReach =
                    suitablePoses.Where(c => !_lightPositions.Any(k =>
                        IsRouteIntersect(enemy.transform.position, c + (Vector2)playerTransform.position, k,
                            _range + additionalLightAvoidance))
                    ).ToList();
                if (positionsCanReach.Any())
                {
                    currentPosition = positionsCanReach[Random.Range(0, positionsCanReach.Count)];
                }
                else
                {
                    continue;
                }
                enemy.SetIdlePosition(currentPosition + (Vector2)playerTransform.position);
            }

            yield return awaitor;
        }
    }

    private void CalculateSutiblePoses()
    {
        suitablePoses.Clear();
        for (int i = 0; i < playerSurroundingMapSize; i++)
        {
            for (int j = 0; j < playerSurroundingMapSize; j++)
            {
                bool isSutible = !_lightPositions.Any(c =>
                    Vector2.Distance(c, _map[i, j].Item1 + (Vector2)playerTransform.position) <
                    _range + additionalLightAvoidance);

                isSutible &= _lightPositions.Any(c =>
                    Vector2.Distance(c, _map[i, j].Item1 + (Vector2)playerTransform.position) <
                    _range + maxAdditionalDistance);

                _map[i, j].Item2 = isSutible;
                if (isSutible)
                    suitablePoses.Add(_map[i, j].Item1);
            }
        }
    }

    public static bool IsRouteIntersect(Vector2 firstPoint, Vector2 secondPoint, Vector2 circleCenter,float _range)
    {
        float currRange = _range;
        currRange *= currRange;
        float x, A, B, C, D;
        float slope = (secondPoint.y - firstPoint.y) / (secondPoint.x - firstPoint.x);
        float YIntercept = secondPoint.y - slope * secondPoint.x;
        bool IsVertical = firstPoint.x == secondPoint.x;

        if (IsVertical)
        {
            x = firstPoint.x;
            B = -2 * circleCenter.y;
            C = circleCenter.x * circleCenter.x + circleCenter.y * circleCenter.y - currRange + x * x - 2 * circleCenter.x * x;
            D = B * B - 4 * C;
            if (D >= 0)
                return true;
        }
        else
        {
            A = slope * slope + 1;
            B = 2 * (slope * YIntercept - slope * circleCenter.y - circleCenter.x);
            C = circleCenter.x * circleCenter.x + circleCenter.y * circleCenter.y - currRange + YIntercept * YIntercept -
                2 * YIntercept * circleCenter.y;
            D = B * B - 4 * A * C;
            if(D>=0)
                return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        lightController.onActiveLightPositionChanged -= UpdateLightPositions;
    }

    private void UpdateLightPositions()
    {
        _lightPositions.Clear();
        _lightPositions.AddRange(lightController.GetWalkableArea(out var range));
        _range = range;
    }

    public void RegisterEnemy(EnemyBehaviour behaviour)
    {
        _enemies.Add(behaviour);
    }

    public void RequestRespawn(EnemyBehaviour behaviour)
    {
        CalculateSutiblePoses();
        Vector2 position =
            GetCenterOfMass(_enemies.Where(c => c != behaviour).Select(c => (Vector2)transform.position).ToList());
        int pos = Random.Range(0, suitablePoses.Count);
        behaviour.Respawn(suitablePoses[pos] + (Vector2)playerTransform.position);
    }

    Vector2 GetCenterOfMass(List<Vector2> positions)
    {
        Vector2 position = Vector2.zero;
        foreach (var item in positions)
        {
            position += item;
        }
        return position / positions.Count;
    }
    
#if UNITY_EDITOR && false
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
            return;
        for (int i = 0; i < playerSurroundingMapSize; i++)
        {
            for (int j = 0; j < playerSurroundingMapSize; j++)
            {
                Handles.color = _map[i, j].Item2 ? Color.green : Color.red;
                Handles.DrawSolidDisc(playerTransform.position + (Vector3)(Vector2)_map[i, j].Item1, Vector3.forward,
                    .05f);
                Handles.Label(playerTransform.position + (Vector3)(Vector2)_map[i, j].Item1, $"({i},{j})");
            }
        }
    }
#endif
}