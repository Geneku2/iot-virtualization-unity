using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for when the scroll list has buttons for percise selection of items
/// </summary>
/// <typeparam name="T">Assumes button must represent an object like device or animal</typeparam>
public class ScrollListButtonController<T> : ScrollListController
{
    /// <summary>
    /// The underlying object that each button represents
    /// </summary>
    protected List<T> entries;
    public List<T> Entries { get { return entries; } }

    /// <summary>
    /// Currently selected object
    /// </summary>
    public int CurrIndex { get; private set; }

    // Reduces sensitivity of scrolling
    protected const float scrollCooldown = 0.1f;
    protected float currCooldown = 0f;

    new void Awake()
    {
        base.Awake();
        entries = new List<T>();
    }

    /// <summary>
    /// Open/Close the list.
    /// Also select/desleect the current button
    /// </summary>
    /// <param name="show">Whether to open or close the list</param>
    public override void ToggleList(bool show)
    {
        listCanvas.SetActive(show);
        if (show)
        {
            if (items != null && items.Count > 0)
            {
                CurrIndex = GetActiveItem();
                // Makes sure the item is highlighted even if user closes list and opens it again
                items[CurrIndex].GetComponent<Button>().Select();
            }
        }
        else
        {
            // Unselect current button if list closed
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Increment or decrement the index of the currently selected entry subject to a cooldown to decrease scrolling sensitivity
    /// </summary>
    /// <param name="scrollUp">Whether the action is an up or down scroll</param>
    public override void ScrollList(bool scrollUp)
    {
        if (currCooldown < scrollCooldown)
        {
            currCooldown += Time.deltaTime;
        }
        else
        {
            currCooldown = 0f;
            if (scrollUp)
            {
                CurrIndex = GetPrevActiveItem();
            }
            else
            {
                CurrIndex = GetNextActiveItem();
            }
            scrollBarPos = 1f - ((float)CurrIndex / (items.Count - 1));
            scrollRect.verticalNormalizedPosition = scrollBarPos;
        }
        items[CurrIndex].GetComponent<Button>().Select();
    }


    /// <summary>
    /// Tries to find an item in the list that is active
    /// </summary>
    /// <returns>The index of the active item</returns>
    private int GetActiveItem()
    {
        int next = GetNextActiveItem();
        int prev = GetPrevActiveItem();
        if (items[CurrIndex].activeSelf)
        {
            return CurrIndex;
        }
        else if (next != CurrIndex)
        {
            return next;
        }
        else
        {
            return prev;
        }
    }
    /// <summary>
    /// Get the next item in the list that is active
    /// </summary>
    /// <returns>Index of next item that is active or currIndex if none found</returns>
    private int GetNextActiveItem()
    {
        for (int i = CurrIndex + 1; i < items.Count; i++)
        {
            if (items[i].activeSelf)
            {
                return i;
            }
        }
        return CurrIndex;
    }

    /// <summary>
    /// Get the previous item in the list that is active
    /// </summary>
    /// <returns>Index of first previous item that is active or currIndex if none found</returns>
    private int GetPrevActiveItem()
    {
        for (int i = CurrIndex - 1; i >= 0; i--)
        {
            if (items[i].activeSelf)
            {
                return i;
            }
        }
        return CurrIndex;
    }

    /// <summary>
    /// Filter list by deactivating items that are not in the filter
    /// </summary>
    /// <param name="filter">Items to be included</param>
    public void FilterList(List<T> filter)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < filter.Count; j++)
            {
                if (entries[i].Equals(filter[j]))
                {
                    found = true;
                }
            }

            items[i].SetActive(found);
        }
    }

    /// <summary>
    /// Set all items in the list to active
    /// </summary>
    public void ResetList()
    {
        foreach (GameObject g in items)
        {
            g.SetActive(true);
        }
    }

    /// <summary>
    /// Get the entry associated with the currently selected item, but only if the item is active
    /// </summary>
    /// <returns></returns>
    public T GetCurrentEntry()
    {
        if (items[CurrIndex].activeSelf)
        {
            return entries[CurrIndex];
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Add a handler function to each button in the list that will be called when the button is clicked
    /// </summary>
    /// <param name="handler">The function must take in one int which is the index of the button selected and return void</param>
    public void AddButtonHandler(Action<int> handler)
    {
        for (int i = 0; i < items.Count; i++)
        {
            GameObject button = items[i];

            // Can't use i directly because i changes each loop iteration
            int temp = i;
            button.GetComponent<Button>().onClick.AddListener(() => handler(temp));
        }
    }

    protected void SetCurrentIndex(int index)
    {
        CurrIndex = index;
    }
}
