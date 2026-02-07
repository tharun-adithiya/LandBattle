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

    // =====================
    // STATE MACHINE ENTRY
    // =====================

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
                break;

            case GameState.PlayerPlacementTurn:
                break;

            case GameState.BotPlacementTurn:
                StartCoroutine(BotThinkingRoutine());
                break;

            case GameState.PlayerShootTurn:
                break;

            case GameState.BotShootTurn:
                break;

            case GameState.Win:
                //UIManager.Instance.SetWindowState(WindowStates.WinUI);
                break;

            case GameState.Lose:
                //UIManager.Instance.SetWindowState(WindowStates.LoseUI);
                break;

            case GameState.Restart:
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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


    private IEnumerator BotThinkingRoutine()
    {
        UIManager.Instance.SetWindowState(WindowStates.BotShipPlacementUI);

        yield return new WaitForSeconds(5f);

        UIManager.Instance.SetWindowState(WindowStates.GameUI);

        SetGameState(GameState.PlayerShootTurn);
    }
}
