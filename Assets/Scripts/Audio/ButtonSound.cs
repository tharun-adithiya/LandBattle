using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    void Start()
    {
        // Automatically add the listener when the button starts up
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOnClickButton();
        }
    }
}