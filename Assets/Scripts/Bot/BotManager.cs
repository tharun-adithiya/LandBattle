using UnityEngine;

public class BotManager : MonoBehaviour
{
    [SerializeField] private BotShipPlacementData[] botShipPlacementData;
    [SerializeField] private BotShipController[] botShips;

    public void PlaceBotShips()
    {
        if (botShipPlacementData == null || botShipPlacementData.Length == 0)
        {
            Debug.LogError("No BotShipPlacementData assigned!");
            return;
        }

        BotShipPlacementData selectedPlacementData = botShipPlacementData[Random.Range(0, botShipPlacementData.Length)]; //Picks up a randomSO for ship placement

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
            BotShipController controller = spawnedShip.GetComponent<BotShipController>();

            // Size
            controller.shipSize = data.size;

            // Applies rotation directly from SO
            spawnedShip.transform.rotation = Quaternion.Euler(0, 0, data.rotatedAngle);

            //  Controller infers horizontal from provided angle
            controller.isHorizontal = Mathf.Abs(data.rotatedAngle % 180f) < 1f;

            // Makes bot ships invisible
            SpriteRenderer sr = spawnedShip.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = 0.1f;
            sr.color = c;

            // Places ships at the board. (Registers them on the board)
            controller.PlaceAtCell(data.cellPosition);
        }
    }
}
