using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void ChangeUIWindow()
    {
        UIManager.Instance.SetWindowState(WindowStates.DifficultySelection);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
