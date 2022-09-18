using System;
using UnityEngine;

public class CompasTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _gameObject;
    public static event Action onCompasPicked = delegate { };
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<PlayerMoveController>(out _))
        {
            GameManager.Instance.hasCompas = true;
            _gameObject.SetActive(false);
            onCompasPicked.Invoke();
        }
    }
}
