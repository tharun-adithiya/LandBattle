using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameDifficulty gameDifficulty;

    private void Awake()
    {
        #region Singleton 
        if(Instance != null&&Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    #endregion

    }
   

    public void SetDifficulty(Difficulty selectedDifficulty)
    {
        Debug.Log("Difficulty set to "+ selectedDifficulty);
        gameDifficulty.botDifficulty = selectedDifficulty;
        UIManager.Instance.SetWindowState(WindowStates.PlayerShipPlacementUI);
        SceneManager.LoadScene(1);
    }
}
