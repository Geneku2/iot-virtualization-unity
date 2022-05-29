using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimalButton : MonoBehaviour, IPointerClickHandler
{
    public Animal Animal { get; set; }

    public GameObject AnimalNameTextNormal;
    public GameObject AnimalNameTextPressed;

    private Button button;

    public static UIMasterControl UIMasterControl;
    void Start()
    {
        if (UIMasterControl == null)
            UIMasterControl = GameObject.Find("UI Master Control").GetComponent<UIMasterControl>();
        //button = GetComponent<Button>();
        //button.onClick.AddListener(ForwardToMasterControl);
    }

    public void ForwardToMasterControl()
    {
        UIMasterControl.GetComponent<UIMasterControl>().SelectedAnimal = Animal;
        UIMasterControl.GetComponent<UIMasterControl>().SetSelectedAnimalName(Animal.Name);
        UIMasterControl.GetComponent<UIMasterControl>().PopulateAttachedDeviceList();
    }

    public void SetAnimalName(Animal a)
    {
        Animal = a;

        if (gameObject.GetComponent<TextMeshProUGUI>())
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = a.name;
        }

        if (AnimalNameTextNormal != null)
        {
            AnimalNameTextNormal.GetComponent<TextMeshProUGUI>().text = a.name;
        }

        if (AnimalNameTextPressed != null)
        {
            AnimalNameTextPressed.GetComponent<TextMeshProUGUI>().text = a.name;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UIMasterControl.SelectedAnimal = Animal;
            UIMasterControl.SetSelectedAnimalName(Animal.Name);
            UIMasterControl.PopulateAttachedDeviceList();
            UIMasterControl.SetAsideADList();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            UIMasterControl.ClearDevices(Animal);
        }
    }
}
