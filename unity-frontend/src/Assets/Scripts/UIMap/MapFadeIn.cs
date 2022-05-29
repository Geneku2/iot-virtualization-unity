using HutongGames.PlayMaker;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class MapFadeIn : MonoBehaviour
{
    // Is the map opened or closed?
    [NonSerialized] public bool mapOpened;
    [SerializeField] private GameObject compass;
    
    // Are we currently fading the map in or out
    bool isAnimating = false;
    
    // The background of the map
    [SerializeField] private GameObject background;
    // The parent object of the animal icons on the map
    [SerializeField] private GameObject icons;
    // The animator controlling our fade effect
    private Animator animator;

    private void Start()
    {
        animator = background.GetComponent<Animator>();
        icons.SetActive(false);
        background.SetActive(false);
        compass.SetActive(false);
        closeMap();
        this.GetComponent<CanvasGroup>().alpha = 1;
    }
    
    private void FixedUpdate()
    {
        // Runs when map is done fading in
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("In Done"))
        {
            icons.SetActive(true);
            isAnimating = false;
            mapOpened = true;
            
        }
        // Runs when map is done fading out
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Out Done"))
        {
            background.SetActive(false);
            isAnimating = false;
            mapOpened = false;
            // Close the map panel object
            closeMapSprite();
            compass.SetActive(false);
        }
        // If we're in either of these two states, we're animating
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("In") || animator.GetCurrentAnimatorStateInfo(0).IsName("Out"))
        {
            isAnimating = true;
        }
    }

    void openMapSprite()
    {
        gameObject.GetComponent<MapClicker>().openMap();
    }

    void closeMapSprite()
    {
        gameObject.GetComponent<MapClicker>().closeMap();
    }
    

    public bool closeMap()
    {
        icons.SetActive(false);
        // If we're already animating, don't do anything
        if (isAnimating) return false;
        // Turn off the icons first
        icons.SetActive(false);
        // Then fade out the map
        animator.Play("Out");
        compass.GetComponent<Animator>().Play("Closing");
        return true;
    }

    public bool openMap()
    {
        // If we're already animating, don't do anything
        if (isAnimating) return false;
        // Workaround to prevent map flashing before fading in
        // The map fade in animation sets alpha to 1 after 0.01 seconds
        background.GetComponent<CanvasGroup>().alpha = 0;
        // Turn on the background
        background.SetActive(true);
        // Turn on the map panel
        openMapSprite();
        // Fade in the map
        animator.Play("In");
        compass.SetActive(true);
        compass.GetComponent<Animator>().Play("Opening");
        return true;
    }
}
