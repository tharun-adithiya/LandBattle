using UnityEngine;

public class PlayerShipManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] DestroyedSmokeVFX;
    private int VFXCounter = 0;
    private void OnEnable()
    {
        BoardManager.OnPlayerShipSunk +=PlayDestroyedVFX;
    }
    private void OnDisable()
    {
        BoardManager.OnPlayerShipSunk -= PlayDestroyedVFX;
    }
    public void PlayDestroyedVFX(Vector3Int cell, ShipController ship)
    {
        AudioManager.Instance.PlayDestroyedSFX();
        ship.GetComponent<SpriteRenderer>().sortingOrder += 1;
        Color shipColor = ship.GetComponent<SpriteRenderer>().color;
        shipColor.a = 0.15f;
        ship.GetComponent<SpriteRenderer>().color = shipColor;
        ParticleSystem vfx = DestroyedSmokeVFX[VFXCounter];

        vfx.transform.position = ship.transform.position;
        vfx.transform.rotation = ship.transform.rotation;

        vfx.gameObject.SetActive(true);

        VFXCounter = (VFXCounter + 1) % DestroyedSmokeVFX.Length;
    }
    public void ReleaseVFX()
    {
        foreach (ParticleSystem vfx in DestroyedSmokeVFX)
        {
            Debug.Log("Releasing smoke vfx");
            vfx.gameObject.SetActive(false);
        }
        VFXCounter = 0;
    }
}
