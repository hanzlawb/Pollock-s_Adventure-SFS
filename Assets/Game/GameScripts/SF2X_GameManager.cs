using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Threading.Tasks;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using TMPro;

/**
 * The Game Manager controls all functions of this example. 
 * By adding this script to your scene and configuring it correctly,
 * all other necessary components will be added automatically.
 */
public class SF2X_GameManager : MonoBehaviour
{
    /**
    * Global Variables
    */
    #region  Global Definitions
    private GlobalManager globalManager;
    private static SF2X_GameManager instance;
    private SmartFox smartFox;
    public static bool invertMouseY = false;
    private GameObject playerObj;
    private Camera cam;
    private SF2X_CharacterController localPlayerController;
    private SF2X_CharacterController remotePlayerController;
    private SF2X_AimIK localPlayerAimIK;
    private SF2X_AimIK remotePlayerAimIK;
    private CharacterController characterController;
    private PlayerInput localPlayerInput;
    private CapsuleCollider playerCollider;

    public int clientServerLag;
    public double lastServerTime = 0;
    public double lastLocalTime = 0;
    public bool resurrect;
    private Dictionary<int, SF2X_CharacterController> recipients = new Dictionary<int, SF2X_CharacterController>();
    private Dictionary<int, GameObject> items = new Dictionary<int, GameObject>();
    public static SF2X_GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    /**
    * Player Character Variables
    */
    #region  Player Character Definitions

    [System.Serializable]
    public class playerprefab
    {
        public GameObject playerPrefab;
        public float cameraTargetHeight = 1.4f;
        public float colliderHeight = 1.8f;
        public float colliderCenter = 0.9f;
    }
    [SerializeField] public playerprefab[] playerPrefab;
    [SerializeField]
    public Color[] colorarray = {   new Color32(239, 190, 125, 255),
                                    new Color32(255, 109, 106, 255),
                                    new Color32(139, 211, 230, 255),
                                    new Color32(177, 162, 202, 255),
                                    new Color32(220, 220, 220, 255),
                                    new Color32(57, 57, 57, 255) };

    public Vector3 collidercenter;
    public float colliderRadius = 0.3f;
    public float colliderheight;
    public Vector3 cameratarget = new Vector3(0, 1.4f, 0);
    [SerializeField] public int waitToRespawn = 0;
    public GameObject GetPlayerObject()
    {
        return playerObj;
    }
    #endregion
    /**
    * User Interface Variables
    */
    #region  User Interface Definitions
    [SerializeField] public GameObject[] healthStars;
    [SerializeField] public GameObject[] loadedBullets;
    [SerializeField] public GameObject[] unloadedBullets;
    [SerializeField] public GameObject[] kills;
    [SerializeField] public GameObject[] crossHairs;
    [SerializeField] public GameObject ammoPrefab;
    [SerializeField] public GameObject healthPrefab;
    [SerializeField] public GameObject healthBar;
    private GameObject HealthIndicator;
    [SerializeField] public CanvasGroup helpCanvas;
    [SerializeField] public CanvasGroup infoCanvas1;
    [SerializeField] public CanvasGroup infoCanvas2;
    [SerializeField] public CanvasGroup killCanvas;
    private int playerHealth;
    private float duration = 0.5f;
    #endregion

    /**
    * CineMachine Variables
    */
    #region  CineMachine Definitions
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    public Cinemachine3rdPersonFollow cinemachine3rdPersonFollow;
    private CinemachineCollider cinemachineCollider;
    public Vector3 damping;
    [SerializeField] public Vector3 firstShoulderOffset;
    [SerializeField] public Vector3 secondShoulderOffset;
    [SerializeField] public Vector3 thirdShoulderOffset;
    public float cameraDistance;
    #endregion
    /**
    * Network and Input Variables
    */
    #region  Network and Input Definitions
    [SerializeField]
    public InputActionAsset inputActionAsset;
    public enum InterpolationMode
    {
        Simple,
        Complex
    }
    [SerializeField]
    public InterpolationMode NetworkSyncMode = InterpolationMode.Complex;
    #endregion
    /**
    * Audio Clips Variables
    */
    #region  Audio Definitions
    [SerializeField] public AudioClip gunshot;
    [SerializeField] public AudioClip reload;
    [SerializeField] public AudioClip footstep;
    [SerializeField] public AudioClip landing;
    [SerializeField] public AudioClip wounded;
    private AudioSource audioSource;
    private AudioSource remoteAudioSource;
    #endregion
  
  
    /**
       * ------------------------------------------------------
       * Unity Methods Awake and Start
       * ------------------------------------------------------
       * 
       * Awake is execited before Start
       * 
       */

    #region  Unity Methods
    void Awake()
    {
        globalManager = GlobalManager.Instance;
        instance = this;
        cam = Camera.main;
        cam.gameObject.TryGetComponent<CinemachineBrain>(out var cineBrain);
        this.gameObject.TryGetComponent<CinemachineVirtualCamera>(out var cineVirtCAM);
        if (cineBrain == null)
        {
            cineBrain = cam.gameObject.AddComponent<CinemachineBrain>();
        }
        if (cineVirtCAM == null)
        {
            cinemachineVirtualCamera = this.gameObject.AddComponent<CinemachineVirtualCamera>();
        }
    }

    void Start()
    {
        smartFox = globalManager.GetSfsClient();
        if (smartFox == null)
        {
            SceneManager.LoadScene("lobby");
            return;
        }
        SubscribeDelegates();
        SendSpawnRequest();
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        if (NetworkSyncMode == InterpolationMode.Complex)
            smartFox.EnableLagMonitor(true, 1, 10);


    }

    #endregion

    /**
      * ------------------------------------------------------
      * SmartFoxServer event listeners
      * ------------------------------------------------------
      * 
      * See the Smartfox Documentation to understand each method
      * 
      */

    #region  SmartFoxServer Event Listeners

    private void SubscribeDelegates()
    {
        smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserJoinRoom);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
    }

    private void UnsubscribeDelegates()
    {
        smartFox.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        smartFox.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserJoinRoom);
        smartFox.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
    }
    private void OnConnectionLost(BaseEvent evt)
    {
        UnsubscribeDelegates();
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("lobby");
    }
    public void OnExitGame()
    {
        UnsubscribeDelegates();
        smartFox.Send(new LeaveRoomRequest());
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("lobby");
    }

    private void OnUserJoinRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];
        string info = (user.Name + " has joined the game");
        infoCanvas1.GetComponentInChildren<TMP_Text>().text = info;
        StartCoroutine(FadeInfoText1());
    }
    private void OnUserLeaveRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];
        string info = (user.Name + " has left the game");
        DestroyEnemy(user.Id);
        infoCanvas1.GetComponentInChildren<TMP_Text>().text = info;
        StartCoroutine(FadeInfoText1());


    }

    void OnApplicationQuit()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnsubscribeDelegates();
    }

    public void OnPingPong(BaseEvent evt)
    {
        clientServerLag = (int)evt.Params["lagValue"] / 2;
    }

    #endregion
   
    /**
      * ------------------------------------------------------
      * Network Send Methods
      * ------------------------------------------------------
      * 
      * This section contains the creation of SFS Objects that 
      * are transmitted to the Server Scripts
      * 
      */

    #region Network Send Methods

    public void SendSpawnRequest()
    {
        int colors1 = Random.Range(0, colorarray.Length);
        int prefab1 = Random.Range(0, playerPrefab.Length);
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        data.PutInt("prefab", prefab1);
        data.PutInt("color", colors1);
        ExtensionRequest request = new ExtensionRequest("spawnMe", data, room);
        smartFox.Send(request);
    }

    public void SendShot(int target)
    {
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        data.PutInt("target", target);
        ExtensionRequest request = new ExtensionRequest("shot", data, room);
        smartFox.Send(request);
    }

    public void SendReload()
    {
        Room room = smartFox.LastJoinedRoom;
        ExtensionRequest request = new ExtensionRequest("reload", new SFSObject(), room);
        smartFox.Send(request);
    }

    public void SendTransform(SF2X_CharacterTransform chtransform)
    {
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        chtransform.ToSFSObject(data);
        ExtensionRequest request = new ExtensionRequest("sendTransform", data, room, true); // True flag = UDP
        smartFox.Send(request);
    }

    public void SendAnimationState(string message)
    {
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        data.PutUtfString("msg", message);
        ExtensionRequest request = new ExtensionRequest("sendAnim", data, room);
        smartFox.Send(request);
    }

    public void TimeSyncRequest()
    {
        Room room = smartFox.LastJoinedRoom;
        ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), room);
        smartFox.Send(request);
    }
    public async void ResurrectPlayer()
    {

        if (killCanvas)
            StartCoroutine(FadeCGAlpha(1f, 0f, waitToRespawn, killCanvas));
        await Task.Delay(waitToRespawn * 1000);
 
        localPlayerController.dead = false;
        localPlayerController.GetComponent<CharacterController>().enabled = false;
        SendSpawnRequest();

    }

    public async void DestroyPlayer(int id)
    {
        await Task.Delay((waitToRespawn - 1) * 1000);
        if (recipients.ContainsKey(id))
        {
            SF2X_CharacterController remotePlayerController = recipients[id];
            if (remotePlayerController)
                Destroy(remotePlayerController.gameObject);
        }
    }

    public void DestroyEnemy(int id)
    {
        if (recipients.ContainsKey(id))
        {
            SF2X_CharacterController remotePlayerController = recipients[id];
            if (remotePlayerController)
                Destroy(remotePlayerController.gameObject);
        }
    }
    #endregion
  
    /**
      * ------------------------------------------------------
      *    Network Receive Methods
      * ------------------------------------------------------
      * 
      * This section contains Methods that are executed when 
      * they receive of SFS Objects from the Server Scripts
      * 
      */

    #region Network Receive Methods

    private void OnExtensionResponse(BaseEvent evt)
    {
        string cmd = (string)evt.Params["cmd"];
        ISFSObject sfsobject = (SFSObject)evt.Params["params"];
        switch (cmd)
        {
            case "spawnPlayer":
                {
                    HandleInstantiatePlayer(sfsobject);
                }
                break;
            case "transform":
                {
                    HandleTransform(sfsobject);
                }
                break;
            case "notransform":
                {
                    HandleNoTransform(sfsobject);
                }
                break;
            case "killed":
                {
                    HandleKill(sfsobject);
                }
                break;

            case "health":
                {
                    HandleHealthChange(sfsobject);
                }
                break;
            case "anim":
                {
                    HandleAnimation(sfsobject);
                }
                break;
            case "score":
                {
                    HandleScoreChange(sfsobject);
                }
                break;
            case "ammo":
                {
                    HandleAmmoCountChange(sfsobject);
                }
                break;
            case "spawnItem":
                {
                    HandleItem(sfsobject);
                }
                break;
            case "removeItem":
                {
                    HandleRemoveItem(sfsobject);
                }
                break;
            case "enemyShotFired":
                {
                    HandleShotFired(sfsobject);
                }
                break;
            case "time":
                {
                    HandleServerTime(sfsobject);
                }
                break;
            case "reloaded":
                {
                    HandleReload(sfsobject);
                }
                break;
        }
    }
    private void HandleInstantiatePlayer(ISFSObject sfsobject)
    {
        ISFSObject playerData = sfsobject.GetSFSObject("player");
        int userId = playerData.GetInt("id");
        int score = playerData.GetInt("score");
        int prefab = playerData.GetInt("prefab");
        int colors = playerData.GetInt("color");

        SF2X_CharacterTransform chtransform = SF2X_CharacterTransform.FromSFSObject(playerData);
        User user = smartFox.UserManager.GetUserById(userId);
        string name = user.Name;
        if (userId == smartFox.MySelf.Id)
        {
            if (playerObj == null)
            {
                playerObj = GameObject.Instantiate(playerPrefab[prefab].playerPrefab) as GameObject;
                collidercenter = new Vector3(0, playerPrefab[prefab].colliderCenter, 0);
                colliderheight = playerPrefab[prefab].colliderHeight;
                playerObj.transform.position = chtransform.Position;
                playerObj.transform.localEulerAngles = chtransform.AngleRotationFPS;
                playerObj.name = user.Name;
                playerCollider = playerObj.AddComponent<CapsuleCollider>();
                playerCollider.center = collidercenter;
                playerCollider.radius = colliderRadius;
                playerCollider.height = colliderheight;
                GameObject cameraTarget = new GameObject("CameraTarget");
                cameraTarget.transform.parent = playerObj.transform;
                cameraTarget.transform.localPosition = cameratarget;
                Material[] materials = playerObj.GetComponentInChildren<SkinnedMeshRenderer>().materials;
                materials[0].SetColor("_Color", colorarray[colors]);
                localPlayerInput = playerObj.AddComponent<PlayerInput>();
                localPlayerInput.actions = inputActionAsset;
                inputActionAsset.Enable();
                characterController = playerObj.AddComponent<CharacterController>();
                characterController.center = collidercenter;
                characterController.height = colliderheight;
                characterController.radius = colliderRadius;
                cinemachine3rdPersonFollow = cinemachineVirtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
                cinemachineVirtualCamera.Follow = cameraTarget.transform;
                cinemachineVirtualCamera.LookAt = cameraTarget.transform;
                cinemachineCollider = cinemachineVirtualCamera.AddComponent<CinemachineCollider>();
                cinemachineCollider.m_IgnoreTag = "Player";
                cinemachineCollider.m_SmoothingTime = 1.0f;
                cinemachineCollider.m_Damping = 1.0f;
                cinemachineCollider.m_MinimumDistanceFromTarget = 0.4f;
                damping = new Vector3(0.1f, 0.1f, 0.1f);
                cameraDistance = 0.0f;
                cinemachine3rdPersonFollow.Damping = damping;
                cinemachine3rdPersonFollow.ShoulderOffset = secondShoulderOffset;
                cinemachine3rdPersonFollow.CameraDistance = cameraDistance;
                localPlayerController = playerObj.AddComponent<SF2X_CharacterController>();
                localPlayerController.isPlayer = true;
                localPlayerController.CinemachineCameraTarget = cameraTarget;
                localPlayerAimIK = localPlayerController.GetComponentInChildren<SF2X_AimIK>();
                audioSource = playerObj.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.maxDistance = 30f;
                playerHealth = healthStars.Length;

            }
            else
            {
                localPlayerController.transform.position = chtransform.Position;
                localPlayerController.AnimationSync("respawn");           
                localPlayerController.GetComponent<CharacterController>().enabled = true;
                localPlayerController.aim = false;
                localPlayerController.isAiming = false;
                localPlayerAimIK.enabled = true;
                for (int i = 0; i < healthStars.Length; i++)
                {
                    healthStars[i].SetActive(true);
                }
                localPlayerController.dead = false;
                SendTransform(localPlayerController.lastState);
                playerHealth = healthStars.Length;
            }
        }
        else
        {
            GameObject playerObj = GameObject.Instantiate(playerPrefab[prefab].playerPrefab) as GameObject;
            playerObj.transform.position = chtransform.Position;
            playerObj.transform.localEulerAngles = chtransform.AngleRotationFPS;
            Material[] materials = playerObj.GetComponentInChildren<SkinnedMeshRenderer>().materials;
            materials[0].SetColor("_Color", colorarray[colors]);
            characterController = playerObj.AddComponent<CharacterController>();
            collidercenter = new Vector3(0, playerPrefab[prefab].colliderCenter, 0);
            colliderheight = playerPrefab[prefab].colliderHeight;
            characterController.center = collidercenter;
            characterController.height = colliderheight;
            characterController.radius = colliderRadius;
            characterController.enabled = false;
            remotePlayerController = playerObj.AddComponent<SF2X_CharacterController>();
            remotePlayerController.isPlayer = false;
            remoteAudioSource = playerObj.AddComponent<AudioSource>();
            remoteAudioSource.spatialBlend = 1f;
            remoteAudioSource.maxDistance = 30f;
            playerObj.name = user.Name;
            recipients[userId] = playerObj.GetComponent<SF2X_CharacterController>();
            playerObj.GetComponent<SF2X_CharacterController>().userid = userId;
            playerCollider = playerObj.AddComponent<CapsuleCollider>();
            playerCollider.center = collidercenter;
            playerCollider.radius = colliderRadius;
            playerCollider.height = colliderheight;
            remotePlayerAimIK = remotePlayerController.GetComponentInChildren<SF2X_AimIK>();
            HealthIndicator = Instantiate(healthBar) as GameObject;
            HealthIndicator.name = "HealthIndicator";
            HealthIndicator.transform.parent = playerObj.GetComponent<Transform>();
            HealthIndicator.transform.localPosition = new Vector3(0, 0.8f, 0);
            HealthIndicator.transform.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = user.Name;
            if (NetworkSyncMode == InterpolationMode.Complex)
                playerObj.AddComponent<SF2X_SyncManager>();
        }
    }

    private void HandleTransform(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        SF2X_CharacterTransform chtransform = SF2X_CharacterTransform.FromSFSObject(sfsobject);
        if (userId != smartFox.MySelf.Id)
        {
            if (recipients.ContainsKey(userId))
            {
                SF2X_CharacterController remotePlayerController = recipients[userId];
                remotePlayerController.ReceiveTransform(chtransform);
            }
        }
    }

    private void HandleAnimation(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        string msg = sfsobject.GetUtfString("msg");
        if (userId != smartFox.MySelf.Id)
        {
            if (recipients.ContainsKey(userId))
            {
                SF2X_CharacterController remotePlayerController = recipients[userId];
                remotePlayerController.AnimationSync(msg);
            }
        }
    }

    private void HandleNoTransform(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        SF2X_CharacterTransform chtransform = SF2X_CharacterTransform.FromSFSObject(sfsobject);
        if (userId == smartFox.MySelf.Id)
        {
            chtransform.ResetTransform(this.GetPlayerObject().transform);
        }
    }

    private void HandleServerTime(ISFSObject sfsobject)
    {
        long time = sfsobject.GetLong("t");
        double timePassed = SF2X_GameManager.Instance.clientServerLag / 2.0f;
        lastServerTime = Convert.ToDouble(time) + timePassed;
        lastLocalTime = Time.time;
    }
    private void HandleKill(ISFSObject sfsobject)
    {

        Debug.Log("Handle Kill:" );
        int userId = sfsobject.GetInt("id");
        int killerId = sfsobject.GetInt("killerId");
        User user = smartFox.UserManager.GetUserById(userId);
        string name1 = user.Name;
        User killer = smartFox.UserManager.GetUserById(killerId);
        string name2 = killer.Name;
        if (userId != smartFox.MySelf.Id)
        {
            if (recipients.ContainsKey(userId))
            {
                 SF2X_CharacterController remotePlayerController = recipients[userId];
                remotePlayerController.GetComponentInChildren<SF2X_AimIK>().enabled = false;
                   if (remotePlayerController.dead == false) 
                    remotePlayerController.AnimationSync("die");
                SF2X_HealthBar remoteHealthBar = remotePlayerController.GetComponentInChildren<SF2X_HealthBar>();
                for (int i = 0; i < remoteHealthBar.remoteHealthStars.Length; i++)
                {
                    remoteHealthBar.remoteHealthStars[i].SetActive(false);
                }
                remotePlayerController.dead = true;
                DestroyPlayer(userId);
            }
        }
        else
        {
            StartCoroutine(FadeCGAlpha(0f, 1f, 0.1f, killCanvas));
            localPlayerAimIK.enabled = false;
            if (localPlayerController.dead == false)
                localPlayerController.AnimationSync("die");
            for (int i = 0; i < healthStars.Length; i++)
            {
                healthStars[i].SetActive(false);
            }
            localPlayerController.dead = true;
            localPlayerController.died = true;


        }

        string info = (name2 + " has shot " +name1 + " dead");
        infoCanvas1.GetComponentInChildren<TMP_Text>().text = info;
        StartCoroutine(FadeInfoText1());
    }

    private void HandleHealthChange(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        int health = sfsobject.GetInt("health");
        Debug.Log("health:" + health);
        if (userId == smartFox.MySelf.Id)
        {
            if (health < playerHealth)
            {
                if(localPlayerController.dead == false)
                {
                    StartCoroutine(FadeKillScreen());
                    localPlayerController.AnimationSync("wounded");
                }
            }
 
            for (int i = 0; i < healthStars.Length; i++)
            {
                healthStars[i].SetActive(false);
            }
            for (int i = 0; i < health; i++)
            {
                healthStars[i].SetActive(true);
            }
            playerHealth = health;
        }
        else
        {
            if (recipients.ContainsKey(userId))
            {
                SF2X_CharacterController remotePlayerController = recipients[userId];
                if (localPlayerController.dead == false)
                    remotePlayerController.AnimationSync("wounded");
                SF2X_HealthBar remoteHealthBar = remotePlayerController.GetComponentInChildren<SF2X_HealthBar>();
                for (int i = 0; i < remoteHealthBar.remoteHealthStars.Length; i++)
                {
                    remoteHealthBar.remoteHealthStars[i].SetActive(false);
                }
                for (int i = 0; i < health; i++)
                {
                    remoteHealthBar.remoteHealthStars[i].SetActive(true);
                }
            }
        }
    }

    private void HandleAmmoCountChange(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        if (userId != smartFox.MySelf.Id) return;
        int loadedAmmo = sfsobject.GetInt("ammo");
        int maxAmmo = sfsobject.GetInt("maxAmmo");
        int ammo = sfsobject.GetInt("unloadedAmmo");
        for (int i = 0; i < loadedBullets.Length; i++)
        {
            loadedBullets[i].SetActive(false);
        }
        for (int i = 0; i < loadedAmmo; i++)
        {
            loadedBullets[i].SetActive(true);
        }
        for (int i = 0; i < unloadedBullets.Length; i++)
        {
            unloadedBullets[i].SetActive(false);
        }
        for (int i = 0; i < ammo; i++)
        {
            unloadedBullets[i].SetActive(true);
        }
    }

    private void HandleScoreChange(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        int c = sfsobject.GetInt("score");
        if (userId != smartFox.MySelf.Id) return;

        if (c <= kills.Length)
        {
            for (int i = 0; i < kills.Length; i++)
            {
                kills[i].SetActive(false);
            }
            for (int i = 0; i < c; i++)
            {
                kills[i].SetActive(true);
            }
        }

     
    }

    private void HandleItem(ISFSObject sfsobject)
    {


        ISFSObject item = sfsobject.GetSFSObject("item");
        int id = item.GetInt("id");
        string itemType = item.GetUtfString("type");
        SF2X_CharacterTransform chtransform = SF2X_CharacterTransform.FromSFSObject(item);
        GameObject itemPrefab = null;
        if (itemType == "Ammo")
        {
            itemPrefab = ammoPrefab;
        }
        else
        {
            itemPrefab = healthPrefab;
        }
        GameObject itemObj = GameObject.Instantiate(itemPrefab) as GameObject;
        itemObj.transform.position = chtransform.Position;
        items[id] = itemObj;
        string info = ("Health and Ammo spawned in Buildings");
        infoCanvas2.GetComponentInChildren<TMP_Text>().text = info;
        StartCoroutine(FadeInfoText2());

    }

    private void HandleRemoveItem(ISFSObject sfsobject)
    {
        int playerId = sfsobject.GetInt("playerId");
        ISFSObject item = sfsobject.GetSFSObject("item");
        int id = item.GetInt("id");
        string type = item.GetUtfString("type");
        if (items.ContainsKey(id))
        {
            Destroy(items[id]);
            items.Remove(id);
        }
    }
    private void HandleShotFired(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        if (userId != smartFox.MySelf.Id)
        {
            SF2X_CharacterController remotePlayerController = recipients[userId];
            remotePlayerController.GetComponentInChildren<SF2X_AimIK>().shoot = true;
            remotePlayerController.AnimationSync("shoot");
            remotePlayerController.GetComponent<AudioSource>().PlayOneShot(gunshot);
        }
        else
        {
            localPlayerAimIK.shoot = true;
            localPlayerController.AnimationSync("shoot");
            audioSource.PlayOneShot(gunshot);
        }
    }

    private void HandleReload(ISFSObject sfsobject)
    {
        int userId = sfsobject.GetInt("id");
        if (userId != smartFox.MySelf.Id)
        {
            SF2X_CharacterController remotePlayerController = recipients[userId];
            remotePlayerController.AnimationSync("reload");
        }
        else
        {
            localPlayerController.AnimationSync("reload");
            audioSource.PlayOneShot(reload);
        }
    }

    #endregion

    /**
    * ------------------------------------------------------
    *    Audio and UI Methods
    * ------------------------------------------------------
    * 
    * This section include Methods for Audio and UI 
    * 
    */

    #region Audio and UI Methods

    public void playMusic()
    {
        this.GetComponent<AudioSource>().Play();
    }
    public void stopMusic()
    {
        this.GetComponent<AudioSource>().Stop();
    }

    public void fadeHelpIn(CanvasGroup canvas)
    {
        StartCoroutine(FadeCGAlpha(0f, 1f, duration, canvas));
    }

    public void fadeHelpOut(CanvasGroup canvas)
    {
        StartCoroutine(FadeCGAlpha(1f, 0f, duration, canvas));
    }
    private IEnumerator FadeCGAlpha(float from, float to, float duration, CanvasGroup canvas)
    {
        float elaspedTime = 0f;
        while (elaspedTime <= duration)
        {
            elaspedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(from, to, elaspedTime / duration);
            yield return null;
        }
        canvas.alpha = to;
    }
    private IEnumerator FadeInfoText1()
    {
        fadeHelpIn(infoCanvas1);
        yield return new WaitForSeconds(5);
        fadeHelpOut(infoCanvas1);

    }

    private IEnumerator FadeInfoText2()
    {
        fadeHelpIn(infoCanvas2);
        yield return new WaitForSeconds(5);
        fadeHelpOut(infoCanvas2);

    }
    private IEnumerator FadeKillScreen()
    {
        StartCoroutine(FadeCGAlpha(0f, 1f, 0.1f, killCanvas));
         yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeCGAlpha(1f, 0f, 0f, killCanvas));

    }
}
#endregion
