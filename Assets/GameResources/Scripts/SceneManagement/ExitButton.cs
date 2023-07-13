using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private Button btn;

    private void Awake()
    {
        btn.onClick.AddListener(Exit);
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveListener(Exit);
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}