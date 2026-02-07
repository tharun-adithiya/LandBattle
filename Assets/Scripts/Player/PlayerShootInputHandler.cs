using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShootInputHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ShootController shootController;                      //Input handler for player shoot

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;

        shootController.PlayerShoot(worldPos);
    }
}
