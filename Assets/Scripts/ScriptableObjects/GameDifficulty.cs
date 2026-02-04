using UnityEngine;

[CreateAssetMenu(fileName = "NewScriptableObjectScript", menuName = "Scriptable Objects/NewScriptableObjectScript")]
public class GameDifficulty : ScriptableObject
{
    public Difficulty botDifficulty;
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
