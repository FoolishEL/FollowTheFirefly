using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HouseVisual : MonoBehaviour
{
    [SerializeField] private GameObject winText;

    private void Awake()
    {
        HouseTrigger.onWinGame += OnWinGame;
    }

    private void OnDestroy()
    {
        HouseTrigger.onWinGame -= OnWinGame;
    }

    private void OnWinGame()
    {
        winText.SetActive(false);
        EndAwaiter();
    }

    private async UniTask EndAwaiter()
    {
        GameManager.Instance.isPlaying = false;
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        GameManager.Instance.ToMainMenu();
    }
}
