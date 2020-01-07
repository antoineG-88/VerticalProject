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
    [Space]
    public float lowPassActivationSpeed;
    public float lowPassFrequency;

    private LevelBuilder levelBuilder;
    private float timeBeforeNextZoneUpdate;
    [HideInInspector] public Coord currentPlayerZone;
    private RoomHandler currentRoom;
    private RoomHandler previousRoom;
    private GridGraph gridGraph;
    [HideInInspector] public List<RoomHandler> allTowerRooms;

    [HideInInspector] public GameObject finalRing;
    private bool towerCreationFlag;
    private AudioSource source;
    private AudioLowPassFilter lowPassFilter;
    private float lowPassState;
    private float originInverseFixedDeltaTime;

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
    }

    private void Update()
    {
        if(GameData.levelBuilder.towerCreated)
        {
            UpdatePlayerProgression();

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
        }

        //source.pitch = Time.fixedDeltaTime * 50;
        if(Time.fixedDeltaTime * (1 / originInverseFixedDeltaTime) < 1 && lowPassState > 0)
        {
            lowPassState -= lowPassState * Time.deltaTime * lowPassActivationSpeed;
        }
        else if (Time.fixedDeltaTime * (1 / originInverseFixedDeltaTime) == 1 && lowPassState < 1)
        {
            lowPassState += (1 - lowPassState) * Time.deltaTime * lowPassActivationSpeed;
        }

        lowPassFilter.cutoffFrequency = lowPassFrequency + ((22000 - lowPassFrequency) * lowPassState);
    }

    private void UpdateZone()
    {
        if(GameData.playerMovement.transform.position.y < levelBuilder.bottomCenterTowerPos.y)
        {
            GameData.playerManager.TakeDamage(200, Vector2.zero, 0);
        }

        if(currentPlayerZone != GetCurrentPlayerZone())
        {
            currentPlayerZone = GetCurrentPlayerZone();
            currentRoom = levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y];

            if(currentRoom != previousRoom)
            {
                currentRoom.Play();
                previousRoom.Pause();
                UpdateNearbyRoomState();
                UpdatePathfindingGraph();
                previousRoom = currentRoom;
            }

            if (levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y] != null)
                UpdateCamera(currentRoom.zonesCenterPos);
        }

        currentRoom.UpdateRoomLockState();
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
        if (finalRing != null && Vector2.Distance(GameData.playerMovement.transform.position, finalRing.transform.position) < minDistanceToFinalRing && GameData.playerGrapplingHandler.attachedObject == finalRing)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        GameData.cameraHandler.CinematicLook(GameData.playerMovement.transform.position, 3.0f, 5.625f, 4.0f);
        Time.timeScale = 0.01f;
        Time.fixedDeltaTime = 0.02f * 0.01f;
        yield return new WaitForSecondsRealtime(3.0f);
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

        if (upRight.x > levelBuilder.towerWidth - 1)
        {
            upRight.x = levelBuilder.towerWidth - 1;
        }

        if (upRight.y > levelBuilder.towerHeight - 1)
        {
            upRight.y = levelBuilder.towerHeight - 1;
        }

        /*foreach(RoomHandler room in allTowerRooms)
        {
            if(!levelBuilder.yokaiRoomList.Contains(room.originRoom))
                room.DeActivate();
        }*/

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