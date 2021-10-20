using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public PlayerController[] players;
    public string playerPrefabPath;
    public Transform[] spawnPoints;
    public float respawnTime;

    private int playersInGame;

    public static GameManager instance;

    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
  
    }

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate("Builder", spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        playerObj.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
