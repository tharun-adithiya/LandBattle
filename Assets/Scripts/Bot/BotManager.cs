using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotManager : MonoBehaviour
{
    [SerializeField] private GameDifficulty gameDifficultyData;
    [SerializeField] private BotShipPlacementData[] BotShipPlacementDataHard;
    [SerializeField] private BotShipPlacementData[] BotShipPlacementDataMedium;
    [SerializeField] private BotShipPlacementData[] BotShipPlacementDataEasy;
    [SerializeField] private BotShipController[] botShips;
    [SerializeField] private BotBoardManager botBoardManager;
    [SerializeField] private ParticleSystem[] botDestroyedSmokeVFX;
    [SerializeField] private Tilemap botBoard;
    private int botVFXCounter = 0;
    
    private readonly List<GameObject> spawnedShips = new();

    private void OnEnable()
    {
        BotBoardManager.OnBotShipSunk += PlayDestroyedVFX;    
    }
    private void OnDisable()
    {
        BotBoardManager.OnBotShipSunk -= PlayDestroyedVFX;
    }
    public void PlaceBotShips()
    {
        if (BotShipPlacementDataHard == null || BotShipPlacementDataHard.Length == 0)
        {
            Debug.LogError("No BotShipPlacementData assigned!");
            return;
        }

        if (botBoardManager == null)
        {
            Debug.LogError("BotBoardManager not assigned to BotManager!");
            return;
        }

        // NEW — cleanup previous run
        foreach (var s in spawnedShips)
            if (s) Destroy(s);

        spawnedShips.Clear();
        botBoardManager.ResetBoard();   // NEW

        BotShipPlacementData selectedPlacementData = gameDifficultyData.botDifficulty switch        //Picks up a randomSO for ship placement based on selected difficulty;
        {
            Difficulty.Easy => BotShipPlacementDataEasy[Random.Range(0, BotShipPlacementDataEasy.Length)],

            Difficulty.Medium => BotShipPlacementDataMedium[Random.Range(0, BotShipPlacementDataMedium.Length)],

            Difficulty.Hard => BotShipPlacementDataHard[Random.Range(0, BotShipPlacementDataHard.Length)],
            _ => throw null
        };


        if (selectedPlacementData == null)
        {
            Debug.LogError("Ship placement data is not assigned properly");
            return;
        }

        if (selectedPlacementData == null || selectedPlacementData.ships == null)
        {
            Debug.LogError("Selected BotShipPlacementData invalid!");
            return;
        }

        if (botShips.Length != selectedPlacementData.ships.Length)
        {
            Debug.LogError("BotShips count does not match placement data!");
            return;
        }

        Debug.Log("Selected data " + selectedPlacementData.name);

        for (int i = 0; i < botShips.Length; i++)
        {
            var data = selectedPlacementData.ships[i];

            GameObject spawnedShip = Instantiate(botShips[i].gameObject);
            spawnedShips.Add(spawnedShip);   // NEW

            BotShipController controller = spawnedShip.GetComponent<BotShipController>();

            // Set the board manager reference
            controller.SetBoardManager(botBoardManager);

            // Size
            controller.shipSize = data.size;

            // Controller infers horizontal from provided angle
            controller.isHorizontal = Mathf.Abs(data.rotatedAngle % 180f) < 1f;

            // Makes bot ships invisible
            SpriteRenderer sr = spawnedShip.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            // Places ships at the board. (Registers them on the board)
            // PlaceShip will handle rotation based on isHorizontal
            controller.PlaceAtCell(data.cellPosition);
        }
    }
    public void PlayDestroyedVFX(Vector3Int cell, BotShipController ship)
    {
        AudioManager.Instance.PlayDestroyedSFX();
        ship.GetComponent<SpriteRenderer>().sortingOrder += 1;
        Color shipColor=ship.GetComponent<SpriteRenderer>().color;
        shipColor.a = 0.15f;
        ship.GetComponent<SpriteRenderer>().color=shipColor;
        ParticleSystem vfx = botDestroyedSmokeVFX[botVFXCounter];

        vfx.transform.position = ship.transform.position;
        vfx.transform.rotation = ship.transform.rotation;

        vfx.gameObject.SetActive(true);

        botVFXCounter = (botVFXCounter + 1) % botDestroyedSmokeVFX.Length;
    }

    public void ReleaseVFX()
    {
        foreach (ParticleSystem vfx in botDestroyedSmokeVFX)
        {
            Debug.Log("Releasing smoke vfx");
            vfx.gameObject.SetActive(false);
        }
        botVFXCounter = 0;
    }
}
