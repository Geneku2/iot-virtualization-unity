﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using DigitalRuby.RainMaker;
using System.Threading;

/// <summary>
/// Client for interacting with the movesim server
/// </summary>
public class AnimalSimulationClient : MonoBehaviour
{
    /// <summary>
    /// Information about animals provided by server
    /// </summary>
    [Serializable]
    public struct AnimalInfo
    {
        public int id;
        public string name;
        public AnimalType type;
        public AnimalState state;
        public Position pos;
    }

    /// <summary>
    /// Location of animal in world
    /// </summary>
    [Serializable]
    public struct Position
    {
        public float x;
        public float z;
    }


    [Serializable]
    public struct DataIn
    {
        public AnimalInfo[] animals;
        public RainLocation[] rainLocations;
    }

    [Serializable]
    public struct DataOut
    {
        // TODO: figure out what data needs to be outputted
    }

    [Serializable]
    public struct Outserver
    {
        public AnimalhugeInfo[] animalhugeinfos;
    }

    [Serializable]
    public struct AnimalhugeInfo
    {
        public int id;
        public string type;
        public string action;
        public float locationx;
        public float locationy;
        public Position velocity;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [SerializeField]
    private AnimalController animalController;
    [SerializeField]
    private AnimalListController animalListController;
    [SerializeField]
    private MapIconController mapIconController;
    [SerializeField]
    private RainController rainController;

    private string serverMessage;
    private bool readserver = false;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;

    private const string ipAddress = "localhost";
    private const int port = 8888;

    void Start()
    {
        if (animalController == null)
        {
            Debug.LogWarning("Animal controller not instantiated, looking...");
            animalController = GameObject.Find("Animals").GetComponent<AnimalController>();
        }

        if (animalListController == null)
        {
            Debug.LogWarning("Animal list controller not instantiated, looking...");
            animalListController = GameObject.Find("AnimalList").GetComponent<AnimalListController>();
        }

        ///if (rainController == null)
        ///{
        ///    Debug.LogWarning("Rain controller not instantiated, looking...");
        ///    rainController = GameObject.Find("Rain").GetComponent<RainController>();
        ///}

        if (mapIconController == null)
        {
            Debug.LogWarning("MapIconController not instantiated, looking...");
            mapIconController = GameObject.Find("Map").GetComponent<MapIconController>();
        }

        DataIn initialData = new DataIn
        {
            animals = new AnimalInfo[4],
            rainLocations = new RainLocation[2]
        };
        Position inipos = new Position
        {
            x = 100,
            z = 100
        };

        AnimalInfo lion0 = new AnimalInfo
        {
            id = 0,
            type = AnimalType.Lion,
            state = 0,
            pos = inipos
        };
        initialData.animals[0] = lion0;

        AnimalInfo lion3 = new AnimalInfo
        {
            id = 3,
            type = AnimalType.Lion,
            state = 0,
            pos = inipos
        };
        initialData.animals[1] = lion3;

        AnimalInfo zebra1 = new AnimalInfo
        {
            id = 1,
            type = AnimalType.Zebra,
            state = 0,
            pos = inipos
        };
        initialData.animals[2] = zebra1;

        AnimalInfo zebra2 = new AnimalInfo
        {
            id = 2,
            type = AnimalType.Zebra,
            state = 0,
            pos = inipos
        };
        initialData.animals[3] = zebra2;

        RainLocation loc1 = new RainLocation
        {
            x = 50,
            z = 50,
            radius = 10,
            intensity = 1.0F
        };
        initialData.rainLocations[0] = loc1;

        RainLocation loc2 = new RainLocation
        {
            x = 20,
            z = 20,
            radius = 5,
            intensity = 1.0F
        };
        initialData.rainLocations[1] = loc2;

        animalController.CreateAnimals(initialData.animals);
        animalListController.InitAnimalList();
        mapIconController.CreateAnimalsIcons();
        rainController.RainLocations = initialData.rainLocations;
        ConnectToTcpServer();
    }

    void Update()
    {
        StartCoroutine(Dosomething());
    }

    IEnumerator Dosomething()
    {
        if (readserver)
        {
            //Debug.Log("readserver is true" );
            ChangeLocation(serverMessage);

            SendMessage();
            readserver = false;
        }

        yield return Wait();

    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(20);
    }

    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData))
            {
                IsBackground = true
            };
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(ipAddress, port);
            Byte[] bytes = new Byte[1500];
            while (true)
            {
                // Get a stream object for reading
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message.
                        serverMessage = Encoding.ASCII.GetString(incommingData);
                        readserver = true;

                        // byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Finished");
                        // stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        //Debug.Log("readserver is true" );
                        //changeLocation(serverMessage);

                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void ChangeLocation(string serverMessage)
    {
        if (serverMessage.Length < 100)
        {
            return;

        }
        //Debug.Log("server message received as: " + serverMessage);
        string checkstr = serverMessage.Substring(0, 18);
        //Debug.Log("check string is " + checkstr);

        if (!checkstr.Equals("{\"animalhugeinfos\""))
        {
            //Debug.Log("check string is " + checkstr);
            return;
        }

        //  string[] modify = serverMessage.Replace("][","!").Split('!');

        DataIn initialData = new DataIn
        {
            animals = new AnimalInfo[4]
        };

        //  string serverMessage2 = "{\"animalhugeinfos\":" +modify[0]+ "}" ;
        //  Debug.Log("change to" + serverMessage2);
        Outserver firstOutInfo = JsonUtility.FromJson<Outserver>(serverMessage);
        Debug.Log(serverMessage);

        //Debug.Log(firstOutInfo.animalhugeinfos.Length);
        for (int i = 0; i < firstOutInfo.animalhugeinfos.Length; i++)
        {
            AnimalhugeInfo animaldata = firstOutInfo.animalhugeinfos[i];
            AnimalInfo newanimal = new AnimalInfo();
            newanimal.id = animaldata.id;


            if (animaldata.action == "eat" || animaldata.action == "drink" || animaldata.action == "stand")
            {
                newanimal.state = AnimalState.Eat;
            }
            else if (animaldata.action == "death")
            {
                newanimal.state = AnimalState.Death;
            }
            else
            {
                newanimal.state = AnimalState.Move;
            }

            if (animaldata.type == "Zebra")
            {
                newanimal.type = AnimalType.Zebra;
            }
            else
            {
                newanimal.type = AnimalType.Lion;
            }

            Position updateposition = new Position
            {
                x = animaldata.locationx / 3,
                z = animaldata.locationy / 3
            };
            newanimal.pos = updateposition;

            initialData.animals[i] = newanimal;
        }

        // debug code
        for (int i = 0; i < firstOutInfo.animalhugeinfos.Length; i++)
        {
            // Debug.Log("id is:");
            //
            // Debug.Log( firstOutInfo.animalhugeinfos[i].id);
            //Debug.Log( initialData.animals[i].id);
            //
            //Debug.Log("type is:");
            // Debug.Log( firstOutInfo.animalhugeinfos[i].type);
            //Debug.Log("type is:" + initialData.animals[i].type);
            //
            // Debug.Log();
            // Debug.Log( firstOutInfo.animalhugeinfos[i].action);
            //Debug.Log("action is:" + initialData.animals[i].state);
            //
            // Debug.Log("position is:");
            // Debug.Log( firstOutInfo.animalhugeinfos[i].locationx);
            //Debug.Log(initialData.animals[i].pos.x);
            //Debug.Log(initialData.animals[i].pos.z);

        }

        animalController.DoAction(initialData.animals);
        mapIconController.UpdateAnimalIcons();
    }

    private void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "Finished";
                // Convert string message to byte array.
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                //Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void OnApplicationQuit()
    {
        socketConnection.GetStream().Close();
        socketConnection.Close();
    }
}