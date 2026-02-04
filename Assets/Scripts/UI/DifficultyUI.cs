using UnityEngine;

public class DifficultyUI : MonoBehaviour
{  
    public void SetDifficultyToEasy() => GameManager.Instance.SetDifficulty(Difficulty.Easy);
    public void SetDifficultyToMedium() => GameManager.Instance.SetDifficulty(Difficulty.Medium);
    public void SetDifficultyToHard() => GameManager.Instance.SetDifficulty(Difficulty.Hard);

}
