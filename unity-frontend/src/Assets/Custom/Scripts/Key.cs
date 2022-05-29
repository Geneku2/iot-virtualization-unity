using System;
using UnityEngine;
using TMPro;
using VRTK;

public class UserInfo
{
    public string username;
    public string password;
}

public class Key : MonoBehaviour
{
    [Header("Needs to be Assigned From Every Key!")]
    [SerializeField] TextMeshProUGUI inputTextField;

    [Header("Needs to be Assigned ONLY From the Done Key!")]
    [SerializeField] GameObject keyboardParent;

    private bool usernameTyped;
    private bool passwordTyped;


    private void Start()
    {
        inputTextField.text = "Please type your username!";
        usernameTyped = false;
        passwordTyped = false;
    }

    public void onClickKey()
    {
        if (inputTextField.text == "Please type your username!" || inputTextField.text == "Please type your password!") {
            inputTextField.text = "";
        }

        inputTextField.text += (gameObject.name).ToLower();
    }

    public void onClickDelete()
    {
        try
        {
            inputTextField.text = inputTextField.text.Substring(0, inputTextField.text.Length - 1);
        } 
        catch (Exception e)
        {
            print("String length is 0");
            inputTextField.text = "";
        }

        if (inputTextField.text == "")
        {
            if (usernameTyped)
            {
                inputTextField.text = "Please type your password!";
            } else
            {
                inputTextField.text = "Please type your username!";
            }
        }
    }

    public void onClickClear()
    {
        if (!usernameTyped)
        {
            inputTextField.text = "Please type your username!";
        } else
        {
            inputTextField.text = "Please type your password!";
        }
    }

    public void onClickDone()
    {
        if (!usernameTyped)
        {
            if (inputTextField.text == "Please type your username!")
            {
                PlayerPrefs.SetString("username", "");
            } else
            {
                PlayerPrefs.SetString("username", inputTextField.text);
            }
            
            inputTextField.text = "Please type your password!";
            usernameTyped = true;
        }
        else
        {
            if (inputTextField.text == "Please type your password!")
            {
                PlayerPrefs.SetString("password", "");
            } else
            {
                PlayerPrefs.SetString("password", inputTextField.text);
            }
            
            inputTextField.text = "";
            passwordTyped = true;

            if (usernameTyped && passwordTyped)
            {
                UserInfo userInfo = new UserInfo();
                userInfo.username = PlayerPrefs.GetString("username");
                userInfo.password = PlayerPrefs.GetString("password");

                string jsonOutput = JsonUtility.ToJson(userInfo);
                print(jsonOutput);
            }

            keyboardParent.SetActive(false);
        }
    }
}
