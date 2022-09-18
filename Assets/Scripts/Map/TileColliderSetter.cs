using System;
using UnityEngine;

public class TileColliderSetter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private Sides currentSide;
    [SerializeField] private int tileId = -1;
    [SerializeField] private Transform lightTransform;
    public static event Action<Transform> onLightAdded = delegate { };
    public static event Action<Transform> onLightRemoved = delegate { };
    public void SetupColliders(Sides side)
    {
        currentSide = side;
        up.SetActive(!side.HasFlag(Sides.Up));
        down.SetActive(!side.HasFlag(Sides.Down));
        right.SetActive(!side.HasFlag(Sides.Right));
        left.SetActive(!side.HasFlag(Sides.Left));
    }

    public void SetTileInfo(int id)
    {
        tileId = id;
        if (id == MapGeneratorConstants.START_ID || id == MapGeneratorConstants.PLAYER_ID_AND_START)
        {
            lightTransform.gameObject.SetActive(true);
            onLightAdded.Invoke(lightTransform);
        }
        else
        {
            lightTransform.gameObject.SetActive(false);
            onLightRemoved.Invoke(lightTransform);
        }
    }
}
