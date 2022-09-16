using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private Transform prefab;
    private Queue<Transform> spawnedPrefabs;
    [SerializeField] private float fadeTime = 1.2f;
    [SerializeField] private Camera camera;

    private void Awake()
    {
        spawnedPrefabs = new Queue<Transform>(3);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = prefab.position.z;
            if (spawnedPrefabs.Count == 3)
            {
                var replacedPrefab = spawnedPrefabs.Dequeue();
                StartCoroutine(Disappear(replacedPrefab));
            }
            var newLight = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
            StartCoroutine(Appear(newLight));
            spawnedPrefabs.Enqueue(newLight);
        }
    }

    private IEnumerator Appear(Transform target)
    {
        target.localScale = Vector3.zero;
        float targetLightSize = 1.6f;
        Light light = target.GetComponent<Light>();
        if (light != null)
        {
            targetLightSize = light.range;
        }
        else
        {
            Debug.LogError("No light!");
            yield break;
        }
        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / fadeTime);
            light.range = Mathf.Lerp(0f, targetLightSize, time / fadeTime);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
    
    private IEnumerator Disappear(Transform target)
    {
        target.localScale = Vector3.one;
        Light light = target.GetComponent<Light>();
        if (light == null)
        {
            Debug.LogError("No light!");
            yield break;
        }
        float initialLight = light.range;
        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / fadeTime);
            light.range = Mathf.Lerp(initialLight, 0f, time / fadeTime);
            yield return null;
        }
        target.localScale = Vector3.zero;
        Destroy(target.gameObject);
    }
}
