using UnityEngine;

public enum GameState               //States in the game. Game Manager handles them.
{
    Start,
    PlayerPlacementTurn,
    BotPlacementTurn,
    PlayerShootTurn,
    BotShootTurn,
    Win,
    Lose,
    Restart,
}
