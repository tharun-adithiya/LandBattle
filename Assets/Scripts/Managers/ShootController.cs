using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShootController : MonoBehaviour
{
    public static bool isPowerShotActivatedForPlayer = false;
    public static bool isPowerShotActivatedForBot = false;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hitVFX;
    [SerializeField] private ParticleSystem playerMissVFX;
    [SerializeField] private ParticleSystem botMissVFX;

    [Header("Player Shooting")]
    [SerializeField] private Tilemap enemyTilemap;
    [SerializeField] private BoxCollider2D botBoardBounds;
    [SerializeField] private BotBoardManager botBoardManager;

    [Header("Bot Shooting")]
    [SerializeField] private Tilemap playerTilemap;
    [SerializeField] private BoxCollider2D playerBoardBounds;
    [SerializeField] private BoardManager playerBoardManager;

    [Header("Tiles")]
    [SerializeField] private TileBase hitTile;
    [SerializeField] private TileBase missTile;

    Vector3Int? lastHitCell = null;

    public void PlayerShoot(Vector3 worldPos)           //This method handles player shoot logic based on PlayerShootController
    {
        if (GameManager.Instance.CurrentState != GameState.PlayerShootTurn)
            return;

        if (!botBoardBounds.bounds.Contains(worldPos))
            return;

        Vector3Int cell = enemyTilemap.WorldToCell(worldPos);

        if (enemyTilemap.GetTile(cell) == hitTile || enemyTilemap.GetTile(cell) == missTile)
            return;

        if (isPowerShotActivatedForPlayer)                                                   //Handling powered up shots separately
        {
            hitVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));

            ExplodeAt(
                cell,
                enemyTilemap,
                botBoardBounds,
                botBoardManager.GetShipAt,
                botBoardManager.RegisterHit
            );

            isPowerShotActivatedForPlayer = false;
            GameManager.Instance.SetGameState(GameState.BotShootTurn);
            StartCoroutine(BotShootRoutine());
            return;
        }

        ShipData ship = botBoardManager.GetShipAt(cell);

        if (ship != null)                                                                   //If ship foound, Registers ship damage via BotBoardManager
        {
            hitVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));
            enemyTilemap.SetTile(cell, hitTile);
            botBoardManager.RegisterHit(cell);

            GameManager.Instance.SetGameState(GameState.PlayerShootTurn);                   //Changes game state again to player to let the player shoot until the player misses
            return;
        }

        botBoardManager.ResetTurnHits();                                                    //Resets combo
        playerMissVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
        StartCoroutine(VFXCoroutine(playerMissVFX));
        enemyTilemap.SetTile(cell, missTile);

        GameManager.Instance.SetGameState(GameState.BotShootTurn);
        StartCoroutine(BotShootRoutine());
    }


    void BotShoot()                                                                 //No inputs, so this method alone handles bot shooting logic. Holds same logic as PlayerShoot()
    {
        if (GameManager.Instance.CurrentState != GameState.BotShootTurn)
            return;

        Vector3Int cell = lastHitCell.HasValue ? GetRandomAdjacent(lastHitCell.Value) : GetRandomCell();     //If hit, tries to find the possible ship placement. Else, picks up a random tile   

        if (playerTilemap.GetTile(cell) == hitTile || playerTilemap.GetTile(cell) == missTile)         //Prevents shooting on tiles,which were already destroyed
        {
            BotShoot();
            return;
        }

        if (isPowerShotActivatedForBot)
        {
            hitVFX.transform.position = playerTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));

            ExplodeAt(
                cell,
                playerTilemap,
                playerBoardBounds,
                playerBoardManager.GetShipAt,
                playerBoardManager.RegisterHit
            );

            isPowerShotActivatedForBot = false;
            lastHitCell = null;
            StartCoroutine(PlayerCoroutine());
            return;
        }

        ShipData ship = playerBoardManager.GetShipAt(cell);

        if (ship != null)
        {
            hitVFX.transform.position = playerTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));
            playerTilemap.SetTile(cell, hitTile);
            playerBoardManager.RegisterHit(cell);

            lastHitCell = cell;
            StartCoroutine(BotShootRoutine());
            return;
        }

        playerBoardManager.ResetTurnHits();
        botMissVFX.transform.position = playerTilemap.GetCellCenterWorld(cell);
        StartCoroutine(VFXCoroutine(botMissVFX));
        playerTilemap.SetTile(cell, missTile);
        lastHitCell = null;

        StartCoroutine(PlayerCoroutine());
    }


    void ExplodeAt(                                             //This method handles powered up shot. Calculates the center and affects the provided radius of tiles
        Vector3Int center,
        Tilemap tilemap,
        BoxCollider2D bounds,
        System.Func<Vector3Int, ShipData> getShip,
        System.Action<Vector3Int> registerHit)
    {
        Vector3Int[] area =
        {
            center,
            center + Vector3Int.up,
            center + Vector3Int.down,
            center + Vector3Int.left,
            center + Vector3Int.right
        };

        foreach (var c in area)
        {
            Vector3 world = tilemap.GetCellCenterWorld(c);

            if (!bounds.bounds.Contains(world))
                continue;

            if (tilemap.GetTile(c) == hitTile || tilemap.GetTile(c) == missTile)
                continue;

            ShipData ship = getShip(c);

            if (ship != null)
            {
                tilemap.SetTile(c, hitTile);
                registerHit(c);
            }
            else
            {
                tilemap.SetTile(c, missTile);
            }
        }
    }


    IEnumerator VFXCoroutine(ParticleSystem currentVFX)                     //Coroutine to allow the VFX to play entirely before being SetActive(false)
    {
        currentVFX.gameObject.SetActive(true);
        yield return new WaitForSeconds(currentVFX.main.duration);
        currentVFX.gameObject.SetActive(false);
    }

    IEnumerator BotShootRoutine()                                           //Coroutine to imitate bot thinking. Delays bot shot by provided seconds
    {
        yield return new WaitForSeconds(1.5f);
        BotShoot();
    }

    IEnumerator PlayerCoroutine()                                           //Coroutine to have a cooldown for player shots. Prevents glitchy turns
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.SetGameState(GameState.PlayerShootTurn);
    }

    // Helper Methods

    Vector3Int GetRandomCell()                                          //Get's  random cell position for the bot to shoot at.
    {
        Vector3 p = GetRandomPointInBounds(playerBoardBounds.bounds);
        return playerTilemap.WorldToCell(p);
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)                       //Get random points from the provided bounds
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );
    }

    Vector3Int GetRandomAdjacent(Vector3Int origin)                   //If the bot hits the ship, this method will be called. This method narrows down the random tile picking logic by checking out only 4 directions
    {
        Vector3Int[] dirs =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        return origin + dirs[Random.Range(0, dirs.Length)];
    }
}
