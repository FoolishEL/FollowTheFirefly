using UnityEngine;
using UnityEngine.UI;

public class StarButton : MonoBehaviour
{
    [SerializeField] private Button btn;

    private void Awake()
    {
        btn.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveListener(StartGame);
    }
}
