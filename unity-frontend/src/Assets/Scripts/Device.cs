using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using OVRSimpleJSON;
using System.Runtime.InteropServices;

/// <summary>
/// Represents the devices/circuits that users have built. Devices are attached to animals and each sensor on the device provides information about that animals and its current surroundings.
/// </summary>
public class Device : IEquatable<Device>
{
	// Prevents memory hogging
	private const int MAX_CONSOLE_LOGS = 100;

	// Sensor types
	public enum Sensor
    {
		gps,
		ultrasonic,
		temperature,
		humidity,
		airQuality,
		pulseOxi,
		underfined
    }

	public const float ULTRASONIC_MAX = 200f;

	bool IEquatable<Device>.Equals(Device other)
	{
		return ID == other.ID;
	}

	public string ID { get; }

	public string Name { get; }

	/// <summary>
	/// All of the sensors on this device
	/// </summary>
	public List<int> SensorTypes { get; set; }

	/// <summary>
	/// List of animals with this device attached
	/// </summary>
	public List<Animal> AttachedAnimals { get; private set; }

	public Dictionary<Animal, List<float>> SensorData;

	/// <summary>
	/// Key is animal ID
	/// Value is list of console logs about sensor info for that animal
	/// </summary>
	public Dictionary<int, List<string>> AnimalToConsoleLogs { get; private set; }

	public Device(string name, string id, List<int> sensorTypes)
	{
		Name = name;
		ID = id;
		SensorTypes = sensorTypes;
		AttachedAnimals = new List<Animal>();
		SensorData = new Dictionary<Animal, List<float>>();
		AnimalToConsoleLogs = new Dictionary<int, List<string>>();
	}

	public static Sensor SensorFromID(int sensor_id)
    {
		switch (sensor_id)
        {
			case 27:	return Sensor.gps;
			case 3: return Sensor.ultrasonic;
			case 9: return Sensor.airQuality;
			case 24:	return Sensor.temperature;
			case 25:	return Sensor.humidity;
			case 31:	return Sensor.pulseOxi;
        }
		return Sensor.underfined;
    }

	public static string SensorPrefix(Sensor s)
	{
		switch (s)
		{
			case Sensor.gps: return "\"location\": ";
			case Sensor.ultrasonic: return "\"distance\": ";
			case Sensor.airQuality: return "\"air quality\": ";
			case Sensor.temperature: return "\"temperature\": ";
			case Sensor.humidity: return "\"humidity\": ";
			case Sensor.pulseOxi: return "\"pulseOxygen\": ";
			//not here but instead in log files
		}
		return "\"undefined:\": ";
	}
    /// <summary>
    /// Adds the animal this device was attached to to the list
    /// </summary>
    /// <param name="a">The animal this device was attached to</param>
    public void AttachAnimalToDevice(Animal a)
	{
		AttachedAnimals.Add(a);
	}

	/// <summary>
	/// Adds the animal this device was attached to to the list
	/// </summary>
	/// <param name="a">The animal this device was attached to</param>
	public void DetachAnimalFromDevice(Animal a)
	{
		AttachedAnimals.Remove(a);
	}

	/// <summary>
	/// Assumes JSON.Parse was already called on the server message and the sensor info being passed in is of the form:
	/// {
	///    "sensorName": sensor data,
	///    "sensorName 2": sensor data,
	/// }
	/// </summary>
	/// <param name="animalId"></param>
	/// <param name="sensorInfo"></param>
	/// 
	public void AddSensorInfo(Animal animal)
    {
		//Debug.Log("HERE TEST IF CALLED");
		if (!AnimalToConsoleLogs.ContainsKey(animal.ID))
		{
			AnimalToConsoleLogs.Add(animal.ID, new List<string>());
		}
		List<string> logs = AnimalToConsoleLogs[animal.ID];

		// Add Sensor Readings to Console Output
		foreach (int s_id in SensorTypes)
        {
			Sensor s = SensorFromID(s_id);
			if (s != Sensor.underfined)
            {
				string consoleLog = s + ": " + animal.SensorData[s_id];
				if (logs.Count > MAX_CONSOLE_LOGS)
				{
					logs.RemoveAt(0);
				}
				logs.Add(consoleLog);
			}
		}
	}

	public void AddConsoleLogs(int animalID, string consoleLog)
	{
		if (!AnimalToConsoleLogs.ContainsKey(animalID))
		{
			AnimalToConsoleLogs.Add(animalID, new List<string>());
		}
		List<string> logs = AnimalToConsoleLogs[animalID];
		//Debug.Log(logs[-1]);
		if (logs.Count > MAX_CONSOLE_LOGS)
		{
			logs.RemoveAt(0);
		}
		logs.Add(consoleLog);
		
	}

	public void AddConsoleLogs(int animalId, JSONNode sensorInfo)
    {
		if (!AnimalToConsoleLogs.ContainsKey(animalId))
        {
			AnimalToConsoleLogs.Add(animalId, new List<string>());
        }

		foreach (string sensor in sensorInfo.Keys)
        {
			string consoleLog = sensor + ": " + sensorInfo[sensor];
			List<string> logs = AnimalToConsoleLogs[animalId];
			if (logs.Count > MAX_CONSOLE_LOGS)
            {
				logs.RemoveAt(0);
            }
			logs.Add(consoleLog);
        }
    }
}
