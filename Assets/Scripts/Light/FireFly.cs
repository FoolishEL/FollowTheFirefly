using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireFly : MonoBehaviour
{
    [SerializeField] private Light light;

    [SerializeField] private Transform lightTransform;

    [SerializeField] private float fadeTime = 1.2f;
    [SerializeField] private float targetLightSize = 1.6f;
    [SerializeField] private float stoppingDistance = .5f;
    [SerializeField] private float speed = 2f;
    [HideInInspector]
    public bool IsPlayerControlled = false;

    private bool isMooving = false;
    public bool IsMooving => isMooving;
    public event Action<FireFly> onDestibationReached = delegate { };

    private void Awake()
    {
        isMooving = false;
        if (IsPlayerControlled)
        {
            StartCoroutine(Appear());
        }
    }

    private IEnumerator Appear()
    {
        lightTransform.localScale = Vector3.zero;
        if (light == null)
        {
            Debug.LogError("No light!");
            yield break;
        }
        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            lightTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / fadeTime);
            light.range = Mathf.Lerp(0f, targetLightSize, time / fadeTime);
            yield return null;
        }
        lightTransform.localScale = Vector3.one;
    }
    
    private IEnumerator Disappear()
    {
        lightTransform.localScale = Vector3.one;
        if (light == null)
        {
            Debug.LogError("No light!");
            yield break;
        }
        float initialLight = light.range;
        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            lightTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / fadeTime);
            light.range = Mathf.Lerp(initialLight, 0f, time / fadeTime);
            yield return null;
        }
        lightTransform.localScale = Vector3.zero;
    }

    public bool MoveToPosition(Vector3 position)
    {
        if(isMooving)
            return false;
        isMooving = true;
        StartCoroutine(Appear());
        MoveCoroutine(position);
        return true;
    }

    public bool MoveToTransform(Transform transformToMoveTo)
    {
        if(isMooving)
            return false;
        isMooving = true;
        StartCoroutine(Disappear());
        MoveTransformCoroutine(transformToMoveTo);
        return true;
    }

    private async UniTask MoveCoroutine(Vector3 destination)
    {
        Vector2 moveDirection = Vector2.zero;
        while (Vector2.Distance(transform.position, destination) > stoppingDistance)
        {
            moveDirection = (destination - transform.position).normalized * speed * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }
        isMooving = false;
        onDestibationReached.Invoke(this);
    }
    
    private async UniTask MoveTransformCoroutine(Transform destination)
    {
        Vector2 moveDirection = Vector2.zero;
        while (Vector2.Distance(transform.position, destination.position) > stoppingDistance)
        {
            moveDirection = (destination.position - transform.position).normalized * speed * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }
        isMooving = false;
        onDestibationReached.Invoke(this);
    }
}
