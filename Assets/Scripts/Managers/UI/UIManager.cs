using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }      //Singleton Instance

    [SerializeField] private GameObject currentWindow;
    private GameObject gameUIWindow;

    [Header("Game Windows")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject difficultyUI;
    [SerializeField] private GameObject playerShipPlacementUI;
    [SerializeField] private GameObject botShipPlacementUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject playerShootUI;
    [SerializeField] private GameObject botShootUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject winUI;

    public WindowStates CurrentWindowState { get; private set; } = (WindowStates)(-1);   //Setting the enum variable to -1 to prevent mismatching.

    private void Awake()
    {
        #region Singleton                               
        if (Instance != null && Instance != this)           //Singleton for UIManager
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion                          
    }

    private void Start()
    {
        SetWindowState(WindowStates.Main);                 //Setting up main menu as the staring window
    }

    public void SetWindowState(WindowStates nextWindow)     //Simple FSM to switch between different window states 
    {
        if (CurrentWindowState == nextWindow)
        {
            Debug.Log("Exiting");
            return;
        }
            

        ExitWindow(CurrentWindowState);

        CurrentWindowState = nextWindow;

        EnterWindow(nextWindow);
    }

    void EnterWindow(WindowStates nextWindow)
    {
        if (currentWindow != null)
            Destroy(currentWindow);

        switch (nextWindow)
        {
            case WindowStates.Main:
                currentWindow = Instantiate(mainMenu,transform);
                currentWindow.SetActive(true);
                break;

            case WindowStates.DifficultySelection:
                currentWindow = Instantiate(difficultyUI, transform);
                currentWindow.SetActive(true);
                break;

            case WindowStates.PlayerShipPlacementUI:
                currentWindow = Instantiate(playerShipPlacementUI, transform);
                currentWindow.SetActive(true);
                break;

            case WindowStates.BotShipPlacementUI:
                currentWindow = Instantiate(botShipPlacementUI, transform);
                currentWindow.SetActive(true);
                break;

            case WindowStates.GameUI:
                if (gameUIWindow != null) return;
                gameUIWindow = Instantiate(gameUI, transform);
                gameUIWindow.SetActive(true);
                break;

            case WindowStates.PlayerShootUI:
                currentWindow = playerShootUI;
                currentWindow.SetActive(true);
                break;

            case WindowStates.BotShootUI:
                currentWindow = botShootUI;
                currentWindow.SetActive(true);
                break;

            case WindowStates.WinUI:
                currentWindow = Instantiate(winUI, transform);
                currentWindow.SetActive(true);
                break;

            case WindowStates.GameOverUI:
                currentWindow = Instantiate(gameOverUI, transform);
                currentWindow.SetActive(true);
                break;
        }
    }

    public void PlayClickButton()
    {
        AudioManager.Instance.PlayOnClickButton();
    }
    public void PlaySaved()
    {
        AudioManager.Instance.PlayOnSave();
    }
    void ExitWindow(WindowStates previousWindow)
    {
        Debug.Log("Exiting Window: " + previousWindow);
    }
}
