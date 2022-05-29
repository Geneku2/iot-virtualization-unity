using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PCSettingsReturnToMenu : MonoBehaviour
{
    private Image buttonImage;
    private Color originalColor;

    private void OnMouseExit()
    {
        buttonImage.color = originalColor;
    }

    private void OnMouseDown()
    {
        buttonImage.color = Color.gray;
    }

    public void returnToMenu()
    {
        buttonImage.color = originalColor;

        // Return to the menu room, and make sure to restore timescale
        Time.timeScale = 1.0f; 
        SceneManager.LoadScene("MenuRoomPC");
    }

    void Start()
    {
        buttonImage = this.gameObject.GetComponent<Image>();
        originalColor = buttonImage.color;
    }
}
