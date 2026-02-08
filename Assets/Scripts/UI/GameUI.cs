using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour                             // Behaviour for GameUI Panel. This panel will be active through out the GameScene
{
    [SerializeField] private GameObject saveButton;
    private BotManager botManager;
    private BoardManager playerboardManager;
    private void OnEnable()
    {
        BoardManager.OnPlacedAllShips += ShowSaveButton;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        BoardManager.OnPlacedAllShips -= ShowSaveButton;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        botManager = FindFirstObjectByType<BotManager>();
        playerboardManager = FindFirstObjectByType<BoardManager>();
        if (botManager == null)
        {
            Debug.LogError("BotManager NOT FOUND");
            return;
        }

        saveButton.GetComponent<Button>().onClick.RemoveAllListeners();
        saveButton.GetComponent<Button>().onClick.AddListener(playerboardManager.LockShips);
        saveButton.GetComponent<Button>().onClick.AddListener(botManager.PlaceBotShips);
        
        // NEW - Re-add audio listeners after clearing
        saveButton.GetComponent<Button>().onClick.AddListener(AudioManager.Instance.PlayOnClickButton);
        saveButton.GetComponent<Button>().onClick.AddListener(AudioManager.Instance.PlayOnSave);
    }
    public void ShowSaveButton()=> saveButton.SetActive(true);
    public void HideSaveButton()=>saveButton.SetActive(false);
}
