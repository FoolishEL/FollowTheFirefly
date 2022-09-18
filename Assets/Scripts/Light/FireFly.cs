using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireFly : MonoBehaviour
{
    [SerializeField] private Transform lightTransform;

    [SerializeField] private float fadeTime = 1.2f;
    [SerializeField] private float stoppingDistance = .5f;
    [SerializeField] private float speedTo = 2f;
    [SerializeField] private float speedFrom = 2f;
    [HideInInspector] public bool IsPlayerControlled = false;

    private bool isMooving = false;
    private bool isVisibilityChanging;
    private bool isAppearing = false;
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
    private void Start()
    {
        speedTo = GameManager.Instance.FlyToPlayer;
        speedFrom = GameManager.Instance.FlyFromPlayer;
    }

    private async UniTask Appear()
    {
        if(isAppearing)
            return;
        isVisibilityChanging = false;
        lightTransform.localScale = Vector3.zero;
        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            lightTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / fadeTime);
            await UniTask.Yield();
        }

        lightTransform.localScale = Vector3.one;

        isAppearing = false;
    }

    private async UniTask Disappear()
    {
        if (!isVisibilityChanging)
        {
            isVisibilityChanging = true;
            lightTransform.localScale = Vector3.one;
            for (float time = 0f; time < fadeTime; time += Time.deltaTime)
            {
                if(isAppearing)
                    return;
                lightTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / fadeTime);
                await UniTask.Yield();
            }

            lightTransform.localScale = Vector3.zero;
            isVisibilityChanging = false;
        }
    }

    public bool MoveToPosition(Vector3 position)
    {
        if (isMooving)
            return false;
        isMooving = true;
        Appear();
        MoveCoroutine(position);
        return true;
    }

    public bool MoveToTransform(Transform transformToMoveTo)
    {
        if (isMooving)
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
            moveDirection = (destination - transform.position).normalized * speedFrom * Time.deltaTime;
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
            moveDirection = (destination.position - transform.position).normalized * speedTo * Time.deltaTime;
            transform.position += (Vector3)moveDirection;
            await UniTask.Yield();
        }

        isMooving = false;
    }
}