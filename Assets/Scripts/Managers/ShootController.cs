using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShootController : MonoBehaviour
{
    public static bool isPowerShotActivatedForPlayer = false;
    public static bool isPowerShotActivatedForBot = false;

    public static void ResetStaticData()
    {
        isPowerShotActivatedForPlayer = false;
        isPowerShotActivatedForBot = false;
    }

    [Header("VFX")]
    [SerializeField] private ParticleSystem hitVFX;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip missClip;
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
    [SerializeField] private TileBase playerMissTile;
    [SerializeField] private TileBase botMissTile;

    Vector3Int? lastHitCell = null;

    public void PlayerShoot(Vector3 worldPos)
    {
        if (GameManager.Instance.CurrentState != GameState.PlayerShootTurn)
            return;

        if (!botBoardBounds.bounds.Contains(worldPos))
            return;

        Vector3Int cell = enemyTilemap.WorldToCell(worldPos);

        if (!enemyTilemap.HasTile(cell)) return;             // NEW

        if (enemyTilemap.GetTile(cell) == hitTile || enemyTilemap.GetTile(cell) == playerMissTile)
            return;

        if (isPowerShotActivatedForPlayer)
        {
            hitVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));
            AudioManager.Instance.PlaySpecialAbilityShotSFX();
            ExplodeAt(
                cell,
                enemyTilemap,
                botBoardBounds,
                botBoardManager.GetShipAt,
                botBoardManager.RegisterHit,
                playerMissTile
            );

            isPowerShotActivatedForPlayer = false;
            GameManager.Instance.SetGameState(GameState.BotShootTurn);
            StartCoroutine(BotShootRoutine());
            return;
        }

        ShipData ship = botBoardManager.GetShipAt(cell);

        if (ship != null)
        {
            hitVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));
            AudioManager.Instance.PlayShootSFX(hitClip);
            enemyTilemap.SetTile(cell, hitTile);
            botBoardManager.RegisterHit(cell);
            GameManager.Instance.SetGameState(GameState.PlayerShootTurn);
            return;
        }

        botBoardManager.ResetTurnHits();
        playerMissVFX.transform.position = enemyTilemap.GetCellCenterWorld(cell);
        StartCoroutine(VFXCoroutine(playerMissVFX));
        AudioManager.Instance.PlayShootSFX(missClip);
        enemyTilemap.SetTile(cell, playerMissTile);
        GameManager.Instance.SetGameState(GameState.BotShootTurn);
        StartCoroutine(BotShootRoutine());
    }


    void BotShoot()
    {
        if (GameManager.Instance.CurrentState != GameState.BotShootTurn)
            return;

        Vector3Int cell = Vector3Int.zero;
        int safety = 0;                                     // NEW

        do                                                  // NEW loop instead of recursion
        {
            cell = lastHitCell.HasValue ? GetRandomAdjacent(lastHitCell.Value) : GetRandomCell();
            safety++;
        }
        while ((!playerTilemap.HasTile(cell) ||
               playerTilemap.GetTile(cell) == hitTile ||
               playerTilemap.GetTile(cell) == botMissTile) && safety < 50);

        if (safety >= 50) return;                           // NEW

        if (isPowerShotActivatedForBot)
        {
            hitVFX.transform.position = playerTilemap.GetCellCenterWorld(cell);
            StartCoroutine(VFXCoroutine(hitVFX));
            AudioManager.Instance.PlaySpecialAbilityShotSFX();
            ExplodeAt(
                cell,
                playerTilemap,
                playerBoardBounds,
                playerBoardManager.GetShipAt,
                playerBoardManager.RegisterHit,
                botMissTile
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
            AudioManager.Instance.PlayShootSFX(hitClip);
            playerTilemap.SetTile(cell, hitTile);
            playerBoardManager.RegisterHit(cell);
            lastHitCell = cell;
            StartCoroutine(BotShootRoutine());
            return;
        }

        playerBoardManager.ResetTurnHits();
        botMissVFX.transform.position = playerTilemap.GetCellCenterWorld(cell);
        StartCoroutine(VFXCoroutine(botMissVFX));
        AudioManager.Instance.PlayShootSFX(missClip);
        playerTilemap.SetTile(cell, botMissTile);
        lastHitCell = null;                               

        StartCoroutine(PlayerCoroutine());
    }


    void ExplodeAt(
        Vector3Int center,
        Tilemap tilemap,
        BoxCollider2D bounds,
        System.Func<Vector3Int, ShipData> getShip,
        System.Action<Vector3Int> registerHit,
        TileBase missTile)
    {
        Vector3Int[] area =
        {
            center,
            center + Vector3Int.up,
            center + Vector3Int.down,
            center + Vector3Int.left,
            center + Vector3Int.right
        };

        foreach (var cell in area)
        {
            Vector3 world = tilemap.GetCellCenterWorld(cell);

            if (!bounds.bounds.Contains(world))
                continue;

            if (!tilemap.HasTile(cell)) continue;           // NEW

            if (tilemap.GetTile(cell) == hitTile || tilemap.GetTile(cell) == playerMissTile || tilemap.GetTile(cell) == botMissTile)
                continue;

            ShipData ship = getShip(cell);

            if (ship != null)
            {
                tilemap.SetTile(cell, hitTile);
                registerHit(cell);
            }
            else
            {
                tilemap.SetTile(cell, missTile);
            }
        }
    }

    IEnumerator VFXCoroutine(ParticleSystem currentVFX)
    {
        currentVFX.gameObject.SetActive(true);
        yield return new WaitForSeconds(currentVFX.main.duration);
        currentVFX.gameObject.SetActive(false);
    }

    IEnumerator BotShootRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        BotShoot();
    }

    IEnumerator PlayerCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.SetGameState(GameState.PlayerShootTurn);
    }

    Vector3Int GetRandomCell()
    {
        Vector3 p = GetRandomPointInBounds(playerBoardBounds.bounds);
        return playerTilemap.WorldToCell(p);
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );
    }

    Vector3Int GetRandomAdjacent(Vector3Int origin)
    {
        List<Vector3Int> candidates = new();

        Vector3Int[] dirs =
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

        foreach (var dir in dirs)
        {
            Vector3Int c = origin + dir;

            Vector3 worldPos = playerTilemap.GetCellCenterWorld(c);

            if (!playerBoardBounds.bounds.Contains(worldPos))
                continue;

            if (!playerTilemap.HasTile(c))
                continue;

            TileBase t = playerTilemap.GetTile(c);

            if (t == hitTile || t == botMissTile)
                continue;

            candidates.Add(c);
        }

        // If no valid adjacent, fallback to random
        if (candidates.Count == 0)
            return GetRandomCell();

        return candidates[Random.Range(0, candidates.Count)];
    }

}
