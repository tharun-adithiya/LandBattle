using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameDifficulty gameDifficulty;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        SetGameState(GameState.Start);
    }


    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        ExitState(CurrentState);

        CurrentState = newState;

        EnterState(newState);
    }

    private void EnterState(GameState state)
    {
        Debug.Log("Entering State: " + state);

        switch (state)
        {
            case GameState.Start:
                SceneManager.LoadScene(0);
                UIManager.Instance.SetWindowState(WindowStates.Main);
                ShootController.ResetStaticData();
                break;

            case GameState.PlayerPlacementTurn:
                AudioManager.Instance.StopBGSFX();
                StartCoroutine(PlayerPlacementUIWaitRoutine());
                break;

            case GameState.BotPlacementTurn:
                StartCoroutine(BotThinkingRoutine());
                break;

            case GameState.PlayerShootTurn:
                break;

            case GameState.BotShootTurn:
                break;

            case GameState.Win:
                UIManager.Instance.SetWindowState(WindowStates.WinUI);
                break;

            case GameState.Lose:
                UIManager.Instance.SetWindowState(WindowStates.GameOverUI);
                break;

            case GameState.Restart:
                SceneManager.LoadScene(0);      //Start from Main scene but load Difficulty Selection
                UIManager.Instance.SetWindowState(WindowStates.DifficultySelection);
                ShootController.ResetStaticData();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        Debug.Log("Exiting State: " + state);
    }

    public void SetDifficulty(Difficulty selectedDifficulty)
    {
        Debug.Log("Difficulty set to " + selectedDifficulty);

        gameDifficulty.botDifficulty = selectedDifficulty;

        UIManager.Instance.SetWindowState(WindowStates.GameUI);

        SceneManager.LoadScene(1);

        SetGameState(GameState.PlayerPlacementTurn);
    }

    private IEnumerator PlayerPlacementUIWaitRoutine()
    {
        UIManager.Instance.SetWindowState(WindowStates.PlayerShipPlacementUI);
        yield return new WaitForSeconds(2.5f);
        UIManager.Instance.SetWindowState(WindowStates.GameUI);
    }
    private IEnumerator BotThinkingRoutine()
    {
        UIManager.Instance.SetWindowState(WindowStates.BotShipPlacementUI);

        yield return new WaitForSeconds(2.5f);

        UIManager.Instance.SetWindowState(WindowStates.GameUI);

        SetGameState(GameState.PlayerShootTurn);
    }
}
