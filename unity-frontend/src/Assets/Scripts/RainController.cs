using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Defines an area that has rain
/// </summary>
[Serializable]
public struct RainLocation
{
    public float x;
    public float z;
    public float radius;
    public float intensity;
}

namespace DigitalRuby.RainMaker
{

    /// <summary>
    /// Controls the locations that rain should be enabled
    /// </summary>
    public class RainController : MonoBehaviour
    {
        /// <summary>
        /// Locations in world space that should have rain
        /// </summary>
        public RainLocation[] RainLocations
        {
            get;
            set;
        }

        [SerializeField]
        private GameObject player;

        /// <summary>
        /// Script responsible for the rain
        /// </summary>
        private RainScript rainScript;

        void Start()
        {
            if (player == null)
            {
                Debug.LogWarning("Player not instantiated, looking...");
                player = GameObject.FindWithTag("Player");
            }

            rainScript = GetComponent<RainScript>();
        }

        void Update()
        {
            if (RainLocations != null)
            {
                rainScript.RainIntensity = GetRainIntensity();
            }
        }

        /// <summary>
        /// Get what the current rain intensity should be by checking if the player is in any location with rain
        /// </summary>
        /// <returns>Float between 0 and 1</returns>
        private float GetRainIntensity()
        {
            foreach (RainLocation loc in RainLocations)
            {
                if (PlayerInRainLocation(loc))
                {
                    return Mathf.Clamp(loc.intensity, 0f, 1f);
                }
            }
            return 0f;
        }

        /// <summary>
        /// Checks whether the player is inside a given rain location
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        private bool PlayerInRainLocation(RainLocation loc)
        {
            float playerX = player.transform.position.x;
            float playerZ = player.transform.position.z;
            float dist = Vector2.Distance(new Vector2(playerX, playerZ), new Vector2(loc.x, loc.z));
            return dist <= loc.radius;
        }
    }
}


