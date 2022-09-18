using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    private List<Vector2> _lightedPositions;
    private LightController _lightController;

    public void Initialize(LightController lightController)
    {
        _lightedPositions = new List<Vector2>();
        _lightController = lightController;
        _lightController.onActiveLightPositionChanged += UpdateLightPositions;
        UpdateLightPositions();
    }

    private void OnDestroy()
    {
        _lightController.onActiveLightPositionChanged -= UpdateLightPositions;
    }

    private void UpdateLightPositions()
    {
        _lightedPositions.Clear();
        _lightedPositions.AddRange(_lightController.GetWalkableArea(out _));
    }
}
