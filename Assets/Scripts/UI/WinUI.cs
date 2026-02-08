using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
    public void MoveToMainMenu()
    {
        GameManager.Instance.SetGameState(GameState.Start);
    }

    public void RestartFromDifficultySelection()
    {
        GameManager.Instance.SetGameState(GameState.Restart);
    }
}
