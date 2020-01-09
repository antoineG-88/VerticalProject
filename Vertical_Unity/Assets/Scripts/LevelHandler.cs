using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class LevelHandler : MonoBehaviour
{
    public float minDistanceToFinalRing;

    public CameraHandler cameraHandler;
    public ParallaxHandler backGroundParallaxHandler;
    public float currentZoneUpdateTime;
    public float fallDeathOffset;
    [Space]
    public float lowPassActivationSpeed;
    public float lowPassFrequency;
    [Space]
    public float liftCameraSize;
    public Vector2 cameraOffset;
    public float liftSpeed;
    public float timeBeforeLift;
    public bool isFinalLevel;
    public bool optimize;

    private LevelBuilder levelBuilder;
    private float timeBeforeNextZoneUpdate;
    [HideInInspector] public Coord currentPlayerZone;
    private RoomHandler currentRoom;
    private RoomHandler previousRoom;
    private GridGraph gridGraph;
    [HideInInspector] public List<RoomHandler> allTowerRooms;


    private bool towerCreationFlag;
    private AudioSource source;
    private AudioLowPassFilter lowPassFilter;
    private float lowPassState;
    private float originInverseFixedDeltaTime;
    private float startLevelTime;
    private bool isLifting;
    private Vector2 liftPos;

    void Start()
    {
        gridGraph = (GridGraph)AstarData.active.graphs[0];
        levelBuilder = GetComponent<LevelBuilder>();
        timeBeforeNextZoneUpdate = 0;
        currentPlayerZone = new Coord(0, 2);
        previousRoom = new RoomHandler(null, 0, 0);
        allTowerRooms = new List<RoomHandler>();
        towerCreationFlag = true;
        source = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        lowPassState = 1;
        originInverseFixedDeltaTime = Time.fixedDeltaTime;
        GameData.gameController.OpenLoading();
        startLevelTime = Time.realtimeSinceStartup;
        isLifting = false;
    }

    private void Update()
    {
        if(GameData.levelBuilder.towerCreated)
        {
            if(!isFinalLevel)
                liftPos = GameObject.Find("EndLiftPos").transform.position;

            if (timeBeforeNextZoneUpdate > 0)
            {
                timeBeforeNextZoneUpdate -= Time.deltaTime;
            }
            else
            {
                timeBeforeNextZoneUpdate = currentZoneUpdateTime;
                UpdateZone();
            }


            if (towerCreationFlag)
            {
                towerCreationFlag = false;
                foreach (RoomHandler room in allTowerRooms)
                {
                    room.Pause();
                }

                StartCoroutine(GameData.gameController.CloseLoading());
            }


            UpdatePlayerProgression();
        }


        if(Time.fixedDeltaTime * (1 / originInverseFixedDeltaTime) < 1 && lowPassState > 0)
        {
            lowPassState -= lowPassState * Time.deltaTime * lowPassActivationSpeed;
        }
        else if (Time.fixedDeltaTime * (1 / originInverseFixedDeltaTime) == 1 && lowPassState < 1)
        {
            lowPassState += (1 - lowPassState) * Time.deltaTime * lowPassActivationSpeed;
        }

        lowPassFilter.cutoffFrequency = lowPassFrequency + ((22000 - lowPassFrequency) * lowPassState);


        if(Input.GetKeyDown(KeyCode.M))
        {
            if(isFinalLevel)
            {
                GameData.playerMovement.transform.position = GameObject.Find("EndPortal").transform.position;
            }
            else
            {
                GameData.playerMovement.transform.position = liftPos + new Vector2(4, 2);
            }
        }
    }

    private void UpdateZone()
    {
        if(GameData.playerMovement.transform.position.y < levelBuilder.bottomCenterTowerPos.y + fallDeathOffset)
        {
            GameData.playerManager.TakeDamage(200, Vector2.zero, 0);
        }

        if(currentPlayerZone != GetCurrentPlayerZone())
        {
            currentPlayerZone = GetCurrentPlayerZone();
            currentRoom = levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y];

            if(currentRoom != previousRoom && currentRoom != null)
            {
                UpdateNearbyRoomState();
                previousRoom.Pause();
                if(optimize)
                UpdatePathfindingGraph();
                previousRoom = currentRoom;
                currentRoom.Play();
            }

            if (levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y] != null)
                UpdateCamera(currentRoom.zonesCenterPos);
        }

        Debug.Log(currentRoom.currentEnemies.Count);
        if(currentRoom.currentEnemies.Count > 0)
        {
            Debug.Log(currentRoom.currentEnemies[0].gameObject.name);
        }
        Debug.Log("Before : " + currentRoom.islocked);
        currentRoom.UpdateRoomLockState();
        Debug.Log("After : " + currentRoom.islocked);
    }

    private Coord GetCurrentPlayerZone()
    {
        Coord currentZone = new Coord(0, 0);
        float tileLength = levelBuilder.tileLength;
        Vector2 originPos = levelBuilder.bottomCenterTowerPos - new Vector2((levelBuilder.towerWidth * tileLength) / 2, 0);
        Vector2 pos = GameData.playerMovement.transform.position;

        while(pos.y > (originPos.y + tileLength))
        {
            currentZone.x++;
            pos.y -= tileLength;
        }

        while (pos.x > (originPos.x + tileLength))
        {
            currentZone.y++;
            pos.x -= tileLength;
        }

        if(currentZone.x > levelBuilder.towerHeight - 1)
        {
            currentZone.x = levelBuilder.towerHeight - 1;
        }

        if (currentZone.y > levelBuilder.towerWidth - 1)
        {
            currentZone.y = levelBuilder.towerWidth - 1;
        }

        return currentZone;
    }

    private void UpdateCamera(List<Vector2> zonesCenter)
    {
        cameraHandler.UpdateRoomPos(zonesCenter);
        cameraHandler.roomWidth = levelBuilder.tileLength;
    }

    private void UpdatePathfindingGraph()
    {
        gridGraph.SetDimensions((int)((currentRoom.originRoom.roomParts.GetLength(1) / gridGraph.nodeSize) * levelBuilder.tileLength), (int)((currentRoom.originRoom.roomParts.GetLength(0) / gridGraph.nodeSize) * levelBuilder.tileLength), gridGraph.nodeSize);
        gridGraph.center = currentRoom.center;
        gridGraph.Scan();
    }

    private void UpdatePlayerProgression()
    {
        if(isFinalLevel)
        {
            EndPortalHandler endPortalHandler = GameObject.Find("EndPortal").GetComponent<EndPortalHandler>();
            if ((currentRoom.originRoom.name == "PortalRoom1" || currentRoom.originRoom.name == "PortalRoom2") && currentRoom.currentEnemies.Count == 0)
            {
                StartCoroutine(endPortalHandler.End());
            }
        }
        else
        {
            if (Vector2.Distance(GameData.playerMovement.transform.position, liftPos) < minDistanceToFinalRing && !isLifting)
            {
                StartCoroutine(TransitionToNextLevel());
            }
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        isLifting = true;
        Animator tubeAnimator = GameObject.Find("Tube").GetComponent<Animator>();
        GameData.gameController.takePlayerInput = false;
        Vector2 endLiftPos = GameObject.Find("EndLiftPos").transform.position;
        GameData.playerMovement.transform.position = endLiftPos;
        GameData.playerMovement.Propel(Vector2.zero, true, true);
        StartCoroutine(GameData.cameraHandler.CinematicLook(endLiftPos + cameraOffset, 100, liftCameraSize, 4.0f));
        PlayerData.timeScore += Time.realtimeSinceStartup - startLevelTime;
        tubeAnimator.SetBool("IsDown", true);
        yield return new WaitForSecondsRealtime(timeBeforeLift);
        float timer = 2;
        Animator playerAnimator = GameData.playerVisuals.animator;
        while (timer > 0)
        {
            playerAnimator.SetBool("IsInTheAir", true);
            GameData.playerMovement.Propel(Vector2.up * liftSpeed, true, true);
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        GameData.gameController.LoadNextLevel();
    }

    private void UpdateNearbyRoomState()
    {
        Coord bottomLeft = new Coord(currentPlayerZone.x -1, currentPlayerZone.y - 1);
        Coord upRight = new Coord(currentPlayerZone.x + 1, currentPlayerZone.y + 1);


        if (bottomLeft.x < 0)
        {
            bottomLeft.x = 0;
        }

        if (bottomLeft.y < 0)
        {
            bottomLeft.y = 0;
        }

        if (upRight.y > (levelBuilder.towerWidth - 1))
        {
            upRight.y = levelBuilder.towerWidth - 1;
        }

        if (upRight.x > (levelBuilder.towerHeight - 1))
        {
            upRight.x = levelBuilder.towerHeight - 1;
        }

        foreach(RoomHandler room in allTowerRooms)
        {
            if(!levelBuilder.yokaiRoomList.Contains(room.originRoom) && !levelBuilder.endRoomList.Contains(room.originRoom))
                room.DeActivate();
        }

        for (int i = bottomLeft.x; i <= upRight.x; i++)
        {
            for (int y = bottomLeft.y; y <= upRight.y; y++)
            {
                if(levelBuilder.towerRooms[i, y] != null)
                {
                    levelBuilder.towerRooms[i, y].Activate();
                }
            }
        }
    }
}