using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class LevelHandler : MonoBehaviour
{
    public float minDistanceToFinalRing;

    public CameraHandler cameraHandler;
    public float currentZoneUpdateTime;

    private LevelBuilder levelBuilder;
    private Room.RoomPart[,] towerGrid;
    private float timeBeforeNextZoneUpdate;
    private Coord currentPlayerZone;
    private RoomHandler currentRoom;
    private RoomHandler previousRoom;
    private GridGraph gridGraph;

    [HideInInspector] public GameObject finalRing;

    void Start()
    {
        gridGraph = (GridGraph)AstarData.active.graphs[0];
        levelBuilder = GetComponent<LevelBuilder>();
        towerGrid = levelBuilder.towerGrid;
        timeBeforeNextZoneUpdate = 0;
        currentPlayerZone = new Coord(0, 2);
        previousRoom = new RoomHandler(null, 0, 0);
    }

    private void Update()
    {
        UpdatePlayerProgression();

        if(timeBeforeNextZoneUpdate > 0)
        {
            timeBeforeNextZoneUpdate -= Time.deltaTime;
        }
        else
        {
            timeBeforeNextZoneUpdate = currentZoneUpdateTime;
            UpdateZone();
        }
    }

    private void UpdateZone()
    {
        if(currentPlayerZone != GetCurrentPlayerZone())
        {
            currentPlayerZone = GetCurrentPlayerZone();
            currentRoom = levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y];

            if(currentRoom != previousRoom)
            {
                currentRoom.Play();
                previousRoom.Pause();
                UpdatePathfindingGraph();
                previousRoom = currentRoom;
            }

            if (levelBuilder.towerRooms[currentPlayerZone.x, currentPlayerZone.y] != null)
                UpdateCamera(currentRoom.zonesCenterPos);
        }
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
            TransitionToNextLevel();
        }
    }

    private void TransitionToNextLevel()
    {
        GameData.gameController.LoadNextLevel();
    }
}