using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PCSettingsQuit : MonoBehaviour
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

    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        buttonImage = this.gameObject.GetComponent<Image>();
        originalColor = buttonImage.color;
    }
}
