using System.Collections;
using UnityEngine;

public class CompassVisual : MonoBehaviour
{
    [SerializeField] private GameObject compas;
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform exitTransform;
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        compas.SetActive(false);
        CompasTrigger.onCompasPicked += ShowCompas;
    }

    public void SetHouse(Transform obj)
    {
        exitTransform = obj;
    }

    private void OnDestroy()
    {
        CompasTrigger.onCompasPicked -= ShowCompas;
    }

    private void ShowCompas()
    {
        compas.SetActive(true);
        StartCoroutine(RotateArrow());
    }

    private IEnumerator RotateArrow()
    {
        while (isActiveAndEnabled)
        {
            Vector2 direction = exitTransform.position - playerTransform.position;
            arrow.transform.rotation =
                Quaternion.AngleAxis(-Vector2.Angle(Vector2.up, direction) * Mathf.Sign(direction.x), Vector3.forward);
            yield return null;
        }
    }
}
