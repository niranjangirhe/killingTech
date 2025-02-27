﻿using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;

[System.Serializable]
public class ProfileData
{
    public string username;
    public int level;
    public int xp;
    public ProfileData()
    {
        username = "";
        level = 0;
        xp = 0;
    }
    public ProfileData(string un, int lvl, int x)
    {
        username = un;
        level = lvl;
        xp = x;
    }
}
[System.Serializable]
public class MapData
{
    public string name;
    public int scene;
}
public class Launcher : MonoBehaviourPunCallbacks
{
    public InputField usernameField;
    public static ProfileData myProfile = new ProfileData();
    public GameObject tabMain;
    public GameObject tabRooms;
    public GameObject buttonRoom;
    private List<RoomInfo> roomlist;
    public InputField roomnameField;
    public Slider maxPlayerSlider;
    public Text maxPlayersValue;
    public Text mapValue;
    public GameObject tabCreate;

    public MapData[] maps;
    private int currentmap = 0;
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        myProfile = Data.LoadProfile();
        if (!string.IsNullOrEmpty(myProfile.username))
        {
            usernameField.text = myProfile.username.ToString();
        }
         
        Connect();
    }
    public override void OnConnectedToMaster()
    {
        gameObject.GetComponent<MainMenu>().isConnected = true;
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = "0.8.0";
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void Create()
    {
        mapValue.text = "MAP: " + maps[currentmap].name.ToString();
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayerSlider.value;

        options.CustomRoomPropertiesForLobby = new string[] { "map" };

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("map", currentmap);
        options.CustomRoomProperties = properties;
        string roomNameTemp = roomnameField.text;
        if (string.IsNullOrEmpty(roomNameTemp))
            roomNameTemp = myProfile.username;
        PhotonNetwork.CreateRoom(roomNameTemp,options);
    }
    public void ChangeMap()
    {
        currentmap++;
        if (currentmap >= maps.Length) currentmap = 0;
        mapValue.text ="MAP: " + maps[currentmap].name.ToString();
    }
    public void ChangeMaxPlayersSlider(float t_value)
    {
        maxPlayersValue.text = Mathf.RoundToInt(t_value).ToString();
    }
    public void TabCloseAll()
    {
        tabMain.SetActive(false);
        tabRooms.SetActive(false);
        tabCreate.SetActive(false);
    }
    public void TabOpenMain()
    {
        TabCloseAll();
        tabMain.SetActive(true);
    }
    public void TabOpenRooms()
    {
        TabCloseAll();
        tabRooms.SetActive(true);
    }
    public void TabOpenCreate()
    {
        TabCloseAll();
        tabCreate.SetActive(true);

        roomnameField.text = "";

        currentmap = 0;
        mapValue.text = "MAP: " + maps[currentmap].name.ToString();
 

        maxPlayerSlider.value = maxPlayerSlider.maxValue;
        maxPlayersValue.text = Mathf.RoundToInt(maxPlayerSlider.value).ToString();
    }
    private void ClearRoomList()
    {
        Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
        foreach (Transform a in content)
            Destroy(a.gameObject);
    }
    private void VerifyUsername()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            myProfile.username = "Guest_" + Random.Range(100, 1000);
        }
        else
        {
            myProfile.username = usernameField.text;
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> p_list)
    {
        roomlist = p_list;
        ClearRoomList();
        
        Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
        foreach(RoomInfo a in roomlist)
        {
            GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;
            newRoomButton.transform.Find("Name").GetComponent<Text>().text = a.Name;
            newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + "/" +a.MaxPlayers;

            if (a.CustomProperties.ContainsKey("map"))
                newRoomButton.transform.Find("MapBG/MapName").GetComponent<Text>().text = maps[(int)a.CustomProperties["map"]].name;
            else
                newRoomButton.transform.Find("MapBG/MapName").GetComponent<Text>().text = "-----";

            newRoomButton.GetComponent<Button>().onClick.AddListener(delegate {JoinRoom(newRoomButton.transform);});

        }
        base.OnRoomListUpdate(roomlist);
    }
    public void JoinRoom(Transform p_button)
    {
        
        string t_roomName = p_button.transform.Find("Name").GetComponent<Text>().text;
        VerifyUsername();
        PhotonNetwork.JoinRoom(t_roomName);
    }
    public void StartGame()
    {
        VerifyUsername();
        if (PhotonNetwork.CurrentRoom.PlayerCount==1)
        {
            Data.SaveProfile(myProfile);
            PhotonNetwork.LoadLevel(maps[currentmap].scene);
        }
    }
}
