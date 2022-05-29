using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic class for manipulating a scroll list
/// </summary>
public class ScrollListController : MonoBehaviour
{
    /// <summary>
    /// The canvas/window associated with this list that is turned on/off
    /// </summary>
    public GameObject listCanvas;

    /// <summary>
    /// Copies of this are instantiated into the list
    /// </summary>
    [SerializeField]
    protected GameObject itemTemplate;

    /// <summary>
    /// The instantiated items are held here
    /// </summary>
    protected List<GameObject> items;

    /// <summary>
    /// The component responsible for scrolling through the list
    /// </summary>
    protected ScrollRect scrollRect;

    /// <summary>
    /// Current position of the scroll bar, 1 = top, 0 = bottom
    /// </summary>
    protected float scrollBarPos = 1f;

    /// <summary>
    /// Controls sensitivity of scrolling
    /// </summary>
    protected float scrollDelta = 0.05f;

    // If overriding Awake, make sure to either call it in child method or initialize the objects below
    protected void Awake()
    {
        items = new List<GameObject>();
        scrollRect = GetComponent<ScrollRect>();
    }

    // If overriding Start, make sure to either call it in child method
    protected void Start()
    {
        // Assume list is active initially for start to be called and turns itself off
        if (listCanvas)
        {
            listCanvas.SetActive(false);
        }
    }

    public bool IsActive()
    {
        return listCanvas.activeSelf;
    }

    /// <summary>
    /// Open/Close the list
    /// </summary>
    /// <param name="show"></param>
    public virtual void ToggleList(bool show)
    {
        listCanvas.SetActive(show);
    }

    /// <summary>
    /// Invert the current state of the list
    /// </summary>
    public virtual void ToggleList()
    {
        ToggleList(!listCanvas.activeSelf);
    }

    /// <summary>
    /// Adjusts the position of the scroll bar to simulate scrolling
    /// </summary>
    /// <param name="scrollUp">Whether the action is an up or down scroll</param>
    public virtual void ScrollList(bool scrollUp)
    {
        if (scrollUp)
        {
            scrollBarPos = Mathf.Clamp(scrollBarPos + scrollDelta, 0, 1);
        }
        else
        {
            scrollBarPos = Mathf.Clamp(scrollBarPos - scrollDelta, 0, 1);
        }
        scrollRect.verticalNormalizedPosition = scrollBarPos;
    }

    /// <summary>
    /// Add a new item to the list
    /// </summary>
    /// <param name="name">Optional param. If newitem has a text component, the text is set to name</param>
    public virtual void AddItem(string name = null)
    {
        GameObject newItem = Instantiate(itemTemplate);
        newItem.SetActive(true);

        // Sets new entry's parent as the template's parent
        newItem.transform.SetParent(itemTemplate.transform.parent, false);

        if (name != null)
        {
            newItem.GetComponentInChildren<Text>().text = name;
        }

        items.Add(newItem);
    }
}
