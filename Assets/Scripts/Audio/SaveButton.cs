using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    void Start()
    {
        // Automatically add the listener when the button starts up
        GetComponent<Button>().onClick.AddListener(PlaySound);
        GetComponent<Button>().onClick.AddListener(PlayNotification);
    }
    void PlaySound()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("Playing click");
            AudioManager.Instance.PlayOnClickButton();
        }
    }
    void PlayNotification()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("Notifying");
            AudioManager.Instance.PlayOnSave();
        }
    }
}
