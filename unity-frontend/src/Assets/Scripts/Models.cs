// Defines all of the HTTP Request and Response Models
using System;
using UnityEngine;

[Serializable]
public class LoginRequest
{
    public string userName;
    public string password;

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}

[Serializable]
public class LoginResponse
{
    public string token;
    public User currentUser;

    [Serializable]
    public class User
    {
        public Circuit[] circuits;
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}

[Serializable]
public class Circuit
{
    public string circuitId;
    public string circuitName;

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}
