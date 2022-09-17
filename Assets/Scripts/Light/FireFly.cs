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
    private bool isVisibilityChanging;
    public bool IsMooving => isMooving;

    private void Awake()
    {
        isMooving = false;
        isVisibilityChanging = false;
        if (IsPlayerControlled)
        {
            Appear();
        }
    }

    private async UniTask Appear()
    {
        if (!isVisibilityChanging)
        {
            isVisibilityChanging = true;
            lightTransform.localScale = Vector3.zero;
            if (light == null)
            {
                Debug.LogError("No light!");
                await UniTask.Yield();
            }
            else
            {
                for (float time = 0f; time < fadeTime; time += Time.deltaTime)
                {
                    lightTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / fadeTime);
                    light.range = Mathf.Lerp(0f, targetLightSize, time / fadeTime);
                    await UniTask.Yield();
                }

                lightTransform.localScale = Vector3.one;
            }
            isVisibilityChanging = false;
        }
    }
    
    private async UniTask  Disappear()
    {
        if (!isVisibilityChanging)
        {
            isVisibilityChanging = true;
            lightTransform.localScale = Vector3.one;
            if (light == null)
            {
                Debug.LogError("No light!");
                await UniTask.Yield();
            }
            else
            {
                float initialLight = light.range;
                for (float time = 0f; time < fadeTime; time += Time.deltaTime)
                {
                    lightTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / fadeTime);
                    light.range = Mathf.Lerp(initialLight, 0f, time / fadeTime);
                    await UniTask.Yield();
                }

                lightTransform.localScale = Vector3.zero;
            }
            isVisibilityChanging = false;
        }
    }

    public bool MoveToPosition(Vector3 position)
    {
        if(isMooving)
            return false;
        isMooving = true;
        Appear();
        MoveCoroutine(position);
        return true;
    }

    public bool MoveToTransform(Transform transformToMoveTo)
    {
        if(isMooving)
            return false;
        isMooving = true;
        Disappear();
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
    }
    
    private async UniTask MoveTransformCoroutine(Transform destination)
    {
        Vector2 moveDirection = Vector2.zero;
        while (isActiveAndEnabled && Vector2.Distance(transform.position, destination.position) > stoppingDistance)
        {
            moveDirection = (destination.position - transform.position).normalized * speed * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }
        isMooving = false;
    }
}
