using System;

/// <summary>
/// Possible animal states. Taken from [AnimalType]Character.cs
/// TODO: Figure out how to sync this with the backend for consistency
/// </summary>
[Serializable]
public enum AnimalState
{
    Move,
    Eat,
    Death,
    Attack,
    Hit,
    Jump,
    Sit,
}

