using System;
using UnityEngine;

public class HouseTrigger : MonoBehaviour
{
    public static event Action onWinGame = delegate { };
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<PlayerMoveController>(out _))
        {
            onWinGame.Invoke();
        }
    }
}
