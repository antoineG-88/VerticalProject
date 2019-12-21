﻿using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LevelBuilder : MonoBehaviour
{
    [Header("Generation Step timing settings")]
    public float loopTime;
    [Header("Generation settings")]
    public int towerWidth;
    public int towerHeight;
    public Vector2 startPositionIndexes;
    public List<Room> roomList;
    public List<Room> deadEndList;
    public List<Room> rightEdgeList;
    public List<Room> leftEdgeList;
    public GameObject fillerPrefab;
    [Space]
    [Space]
    public bool fillEmptySpaces;
    [Space]
    public bool avoidBlockedOpenings;
    [Space]
    public bool mandatoryOpenings;
    public List<Direction> mandatoryPlacementDirections;
    public bool createIntentionalDeadEnd;
    [Range(0,100)] public float intentionalDeadEndChance;
    [Space]
    [Space]
    public float creatingTime;
    public Vector2 bottomCenterTowerPos;
    public float tileLength;
    public int maxRoom;

    private Room.RoomPart[,] towerGrid;
    private bool[,] zoneChecked;
    private bool[] checkedFloors;
    private List<UnCheckedZone> uncheckedZones;
    private List<Vector2> freeOpeningsUnusable = new List<Vector2>();
    private Vector2 nextRoomStart;
    private int nextRoomOpeningDirection;
    private Vector2 originRoom;
    private int roomBuiltNumber;

    private bool levelBuilt;
    private bool levelScanned;
    private GridGraph gridGraph;
    private List<Vector2> accessibleZones = new List<Vector2>();

    private bool nextRoomFinished;
    private bool roomPlacementSuccess;
    private bool openingRemaining;
    private bool nextOpeningResearchFinished;

    public enum Direction { Up, Down, Right, Left };

    private void Awake()
    {
        nextRoomFinished = false;
        levelBuilt = false;
        levelScanned = false;
        ReArrangeRooms();
        StartCoroutine(BuildLevel());
    }

    private void Update()
    {
        if(levelBuilt && !levelScanned)
        {
            levelScanned = true;
            gridGraph = (GridGraph)AstarPath.active.graphs[0];
            gridGraph.center = new Vector3(0.0f, 0.0f, 0.0f);
            gridGraph.SetDimensions(48, 48, 0.5f);
            AstarPath.active.Scan();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Listing of unCheckedZones :");
            foreach(UnCheckedZone unCheckedZone in uncheckedZones)
            {
                Debug.Log("Unchecked zone at " + unCheckedZone.gridX + ", " + unCheckedZone.gridY);
            }
        }
    }

    public IEnumerator BuildLevel()
    {
        bool towerCorrectlyBuild = false;
        roomBuiltNumber = 0;
        towerGrid = new Room.RoomPart[towerHeight, towerWidth];
        checkedFloors = new bool[towerHeight];
        uncheckedZones = UnCheckedZone.CreateUncheckGrid(towerHeight, towerWidth);
        zoneChecked = new bool[towerHeight, towerWidth];


        Debug.Log("Beginning Tower creation");
        nextRoomStart = startPositionIndexes;
        nextRoomOpeningDirection = 3;
        bool openingRemaining = true;

        do
        {
            Debug.Log("Start building room number " + (roomBuiltNumber + 1));
            StartCoroutine(PlaceNextRoom(nextRoomStart, nextRoomOpeningDirection));

            while (!nextRoomFinished)
            {
                yield return new WaitForEndOfFrame();
            }

            if (!roomPlacementSuccess)
            {
                freeOpeningsUnusable.Add(nextRoomStart);
                uncheckedZones.Remove(UnCheckedZone.GetUncheckZone(uncheckedZones, (int)originRoom.x, (int)originRoom.y));
                zoneChecked[(int)originRoom.x, (int)originRoom.y] = true;
                Debug.Log((int)originRoom.x + ", " + (int)originRoom.y + " removed from uncheckedList");
                Debug.Log("The tile at " + nextRoomStart.x + ", " + nextRoomStart.y + " cannot be used as a connection. Added to unusable list");
                CreateTile((int)nextRoomStart.x, (int)nextRoomStart.y);
            }
            else
            {
                roomBuiltNumber++;
            }

            yield return new WaitForSeconds(loopTime);

            StartCoroutine(FindNextOpening());

            while (!nextOpeningResearchFinished)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        while (openingRemaining && nextRoomStart.x <= towerHeight && roomBuiltNumber <= maxRoom);

        if (!openingRemaining)
        {
            Debug.Log("The Tower couldn't be correctly designed due to no available openings left, Only " + roomBuiltNumber + " rooms has been built");
        }
        else
        {
            Debug.Log("Tower correctly designed with a total of " + roomBuiltNumber + " rooms on " + towerHeight + " floors with " + towerWidth + " zones each");
        }

        //StartCoroutine(CreateTower());
    }

    public IEnumerator PlaceNextRoom(Vector2 gridIndexes, int openingDirection)
    {
        nextRoomFinished = false;
        bool[] roomsTested = new bool[roomList.Count];
        bool[] deadEndsTested = new bool[deadEndList.Count];
        bool[] rightEdgesTested = new bool[rightEdgeList.Count];
        bool[] leftEdgesTested = new bool[leftEdgeList.Count];
        bool isDeadEnd = false;
        Room selectedRoom = null;
        Room potentialRoom = null;
        bool noMoreRoomAvailable = false;
        Vector2 connectedPartIndexes = Vector2.zero;
        for (int i = 0; i < roomsTested.Length; i++)
        {
            roomsTested[i] = false;
        }
        for (int i = 0; i < deadEndsTested.Length; i++)
        {
            deadEndsTested[i] = false;
        }
        for (int i = 0; i < rightEdgesTested.Length; i++)
        {
            rightEdgesTested[i] = false;
        }
        for (int i = 0; i < leftEdgesTested.Length; i++)
        {
            leftEdgesTested[i] = false;
        }

        if (createIntentionalDeadEnd)
        {
            if (Random.Range(0, 100) < intentionalDeadEndChance)
            {
                isDeadEnd = true;
                Debug.Log("Intentional dead end chosen");
            }
        }

        while (selectedRoom == null && !noMoreRoomAvailable)
        {
            bool roomLeft = false;
            bool deadEndLeft = false;
            bool rightEdgeLeft = false;
            bool leftEdgeLeft = false;

            if (!isDeadEnd)
            {
                Debug.Log("Checking if there is room not tested");
                foreach (bool roomTested in roomsTested)
                {
                    if (!roomTested)
                    {
                        roomLeft = true;
                    }
                }

                foreach (bool leftEdgeTested in leftEdgesTested)
                {
                    if (!leftEdgeTested)
                    {
                        leftEdgeLeft = true;
                    }
                }

                foreach (bool rightEdgeTested in rightEdgesTested)
                {
                    if (!rightEdgeTested)
                    {
                        rightEdgeLeft = true;
                    }
                }
            }
            else
            {
                Debug.Log("Checking if there is dead end not tested");
                foreach (bool deadEndTested in deadEndsTested)
                {
                    if (!deadEndTested)
                    {
                        deadEndLeft = true;
                    }
                }
            }

            int y = -1;

            if (!roomLeft && isDeadEnd && !deadEndLeft)
            {
                noMoreRoomAvailable = true;
                Debug.Log("No dead-end can fit the actual case.");
            }
            else if(!roomLeft && !isDeadEnd)
            {
                isDeadEnd = true;
                Debug.Log("No room can fit the actual case.");
            }
            else
            {
                if (gridIndexes.y == 0 && leftEdgeLeft)
                {
                    do
                    {
                        y = Random.Range(0, leftEdgeList.Count);
                    }
                    while (leftEdgesTested[y]);
                    potentialRoom = leftEdgeList[y];
                }
                else if (gridIndexes.y == (towerWidth - 1) && rightEdgeLeft)
                {
                    do
                    {
                        y = Random.Range(0, rightEdgeList.Count);
                    }
                    while (rightEdgesTested[y]);
                    potentialRoom = rightEdgeList[y];
                }
                else if(!isDeadEnd)
                {
                    do
                    {
                        y = Random.Range(0, roomList.Count);
                    }
                    while (roomsTested[y]);
                    potentialRoom = roomList[y];
                }
                else
                {
                    do
                    {
                        y = Random.Range(0, deadEndList.Count);
                    }
                    while (deadEndsTested[y]);
                    potentialRoom = deadEndList[y];
                }

                Debug.Log("Searching corresponding opening on " + potentialRoom.name);
                int floorNumber = 0;

                bool hasOpposedOpening = false;
                while (!hasOpposedOpening && floorNumber < potentialRoom.roomParts.GetLength(0))
                {
                    int zoneNumber = 0;
                    while (!hasOpposedOpening && zoneNumber < potentialRoom.roomParts.GetLength(1))
                    {
                        //Debug.Log("... part at position " + floorNumber + ", " + zoneNumber);
                        if (potentialRoom.roomParts[floorNumber, zoneNumber] != null && potentialRoom.roomParts[floorNumber,zoneNumber].openings.Length > 0 && potentialRoom.roomParts[floorNumber, zoneNumber].openings[openingDirection])
                        {
                            connectedPartIndexes.Set(floorNumber, zoneNumber);
                            hasOpposedOpening = true;
                            Debug.Log("Corresponding opening found on position " + floorNumber + ", " + zoneNumber);
                        }
                        zoneNumber++;
                    }
                    floorNumber++;
                }

                bool enoughSpace = true;
                if (!hasOpposedOpening)
                {
                    Debug.Log("The room called : " + potentialRoom.name + " do not have any corresponding opening");
                    if (!isDeadEnd)
                    {
                        if (gridIndexes.y == 0 && leftEdgeLeft)
                        {
                            leftEdgesTested[y] = true;
                        }
                        else if (gridIndexes.y == (towerWidth - 1) && rightEdgeLeft)
                        {
                            leftEdgesTested[y] = false;
                        }

                        roomsTested[y] = true;
                    }
                    else
                    {
                        deadEndsTested[y] = true;
                    }
                }
                else
                {
                    Debug.Log("Checking space disponibilities with " + potentialRoom.name);
                    floorNumber = 0;
                    while (enoughSpace && floorNumber < potentialRoom.roomParts.GetLength(0))
                    {
                        int zoneNumber = 0;
                        while (enoughSpace && zoneNumber < potentialRoom.roomParts.GetLength(1))
                        {
                            if (potentialRoom.roomParts[floorNumber, zoneNumber] != null)
                            {
                                Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                if (gridIndexes.x + relativeIndexes.x < 0 || gridIndexes.y + relativeIndexes.y < 0 || gridIndexes.x + relativeIndexes.x >= towerHeight || gridIndexes.y + relativeIndexes.y >= towerWidth || towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] != null)
                                {
                                    enoughSpace = false;
                                    Debug.Log("The tile at " + ((int)gridIndexes.x + (int)relativeIndexes.x) + ", " + ((int)gridIndexes.y + (int)relativeIndexes.y) + " is already filled");
                                }
                                else
                                {
                                    Debug.Log("The " + potentialRoom + "'s part (" + floorNumber + ", " + zoneNumber + ") can be placed on the tower at " + ((int)gridIndexes.x + (int)relativeIndexes.x) + ", " + ((int)gridIndexes.y + (int)relativeIndexes.y));
                                }
                            }
                            zoneNumber++;
                        }
                        floorNumber++;
                    }

                    bool createMandatoryFreeOpening = false;
                    if (!enoughSpace)
                    {
                        Debug.Log(potentialRoom.name + " do not fit in the tower");
                        if(!isDeadEnd)
                        {
                            roomsTested[y] = true;
                        }
                        else
                        {
                            deadEndsTested[y] = true;
                        }
                    }
                    else
                    {
                        Debug.Log(potentialRoom.name + " can fit in the tower. Positioned at Floor " + (int)gridIndexes.x + " Zone " + (int)gridIndexes.y);

                        if(mandatoryOpenings && !isDeadEnd)
                        {
                            Debug.Log("Now checking mandatory openings for " + potentialRoom.name + " at " + (int)gridIndexes.x + ", " + (int)gridIndexes.y);
                            floorNumber = 0;
                            while (!createMandatoryFreeOpening && floorNumber < potentialRoom.roomParts.GetLength(0))
                            {
                                int zoneNumber = 0;
                                while (!createMandatoryFreeOpening && zoneNumber < potentialRoom.roomParts.GetLength(1))
                                {
                                    Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                    if (potentialRoom.roomParts[floorNumber, zoneNumber] != null
                                        && potentialRoom.roomParts[floorNumber, zoneNumber].openings.Length > 0)
                                    {
                                        Vector2 followingIndexes = Vector2.zero;
                                        int opposedOpening = 0;
                                        int openingDirectionFound = CheckFreeOpening(potentialRoom.roomParts[floorNumber, zoneNumber], (int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y);
                                        if (openingDirectionFound != -1)
                                        {
                                            if ((openingDirectionFound == 0 && mandatoryPlacementDirections.Contains(Direction.Up))
                                                || (openingDirectionFound == 1 && mandatoryPlacementDirections.Contains(Direction.Down))
                                                || (openingDirectionFound == 2 && mandatoryPlacementDirections.Contains(Direction.Right))
                                                || (openingDirectionFound == 3 && mandatoryPlacementDirections.Contains(Direction.Left)))
                                            {
                                                createMandatoryFreeOpening = true;
                                                Debug.Log("Mandatory opening found. Direction : " + openingDirectionFound);
                                            }
                                        }
                                    }
                                    zoneNumber++;
                                }
                                floorNumber++;
                            }
                        }
                        else
                        {
                            createMandatoryFreeOpening = true;
                        }
                        

                        if (!createMandatoryFreeOpening)
                        {
                            Debug.Log("No free mandatory opening can be created with " + potentialRoom.name + ". Room not placed");
                            roomsTested[y] = true;
                        }
                        else
                        {
                            Debug.Log("Checking if " + potentialRoom.name + "'s openings are blocked");
                            bool allRoomOpeningsFree = true;
                            if (avoidBlockedOpenings)
                            {
                                floorNumber = 0;
                                while (allRoomOpeningsFree && floorNumber < potentialRoom.roomParts.GetLength(0))
                                {
                                    int zoneNumber = 0;
                                    while (allRoomOpeningsFree && zoneNumber < potentialRoom.roomParts.GetLength(1))
                                    {
                                        yield return new WaitForEndOfFrame();
                                        Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                        if (potentialRoom.roomParts[floorNumber, zoneNumber] != null
                                            && potentialRoom.roomParts[floorNumber, zoneNumber].openings.Length > 0)
                                        {
                                            allRoomOpeningsFree = CheckIfAllOpeningsAreFree(potentialRoom.roomParts[floorNumber, zoneNumber], (int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y);
                                        }
                                        zoneNumber++;
                                    }
                                    floorNumber++;
                                }
                            }

                            if (allRoomOpeningsFree)
                            {
                                selectedRoom = potentialRoom;
                                Debug.Log("Room found ! The room " + selectedRoom.name + " has all needed features. Placed at Floor " + (int)gridIndexes.x + " Zone " + (int)gridIndexes.y);

                                for (floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
                                {
                                    for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                                    {
                                        Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                        if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                        {
                                            towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] = selectedRoom.roomParts[floorNumber, zoneNumber];
                                            Debug.Log(selectedRoom.name + " part placed on " + (int)gridIndexes.x + (int)relativeIndexes.x + ", " + (int)gridIndexes.y + (int)relativeIndexes.y);
                                            CreateTile((int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y);
                                        }
                                        else
                                        {
                                            towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] = null;
                                            Debug.Log(selectedRoom.name + " hole placed on " + (int)gridIndexes.x + (int)relativeIndexes.x + ", " + (int)gridIndexes.y + (int)relativeIndexes.y);
                                        }
                                    }
                                }

                                if (fillEmptySpaces)
                                {
                                    Debug.Log("Now ckecking if " + selectedRoom.name + " is creating unaccessible space");

                                    accessibleZones.Clear();
                                    bool roomIsBlocking = false;

                                    for (floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
                                    {
                                        for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                                        {
                                            Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                            if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                            {
                                                List<Vector2> checkedZones = new List<Vector2>();
                                                int direction = 0;
                                                Debug.Log("Checking if zone " + gridIndexes.x + relativeIndexes.x + ", " + gridIndexes.y + relativeIndexes.y + " is blocking");
                                                Vector2 zoneToCheck = Vector2.zero;
                                                while (!roomIsBlocking && direction < 4)
                                                {
                                                    direction++;
                                                    switch (direction)
                                                    {
                                                        case 1:
                                                            zoneToCheck = new Vector2(gridIndexes.x + relativeIndexes.x + 1, gridIndexes.y + relativeIndexes.y);
                                                            break;

                                                        case 2:
                                                            zoneToCheck = new Vector2(gridIndexes.x + relativeIndexes.x - 1, gridIndexes.y + relativeIndexes.y);
                                                            break;

                                                        case 3:
                                                            zoneToCheck = new Vector2(gridIndexes.x + relativeIndexes.x, gridIndexes.y + relativeIndexes.y + 1);
                                                            break;

                                                        case 4:
                                                            zoneToCheck = new Vector2(gridIndexes.x + relativeIndexes.x, gridIndexes.y + relativeIndexes.y - 1);
                                                            break;

                                                        default:
                                                            Debug.LogError("There is no such direction as : " + direction);
                                                            break;
                                                    }

                                                    if (!IsNextZoneBlocking(zoneToCheck, 0, ref checkedZones))
                                                    {
                                                        accessibleZones.AddRange(checkedZones);
                                                    }
                                                    else
                                                    {
                                                        roomIsBlocking = true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (roomIsBlocking)
                                    {
                                        Debug.Log(selectedRoom.name + " is creating un-accessible space. Removing the room from the tower");

                                        for (floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
                                        {
                                            for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                                            {
                                                Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                                if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                                {
                                                    towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] = null;
                                                    Debug.Log(selectedRoom.name + " part removed on " + (int)gridIndexes.x + (int)relativeIndexes.x + ", " + (int)gridIndexes.y + (int)relativeIndexes.y);
                                                }
                                            }
                                        }

                                        selectedRoom = null;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Some openings on " + potentialRoom.name + " are blocked");
                            }
                        }
                    }
                }

                if (selectedRoom == null)
                {
                    if (!isDeadEnd)
                    {
                        if (gridIndexes.y == 0 && leftEdgeLeft)
                        {
                            leftEdgesTested[y] = true;
                        }
                        else if (gridIndexes.y == (towerWidth - 1) && rightEdgeLeft)
                        {
                            leftEdgesTested[y] = false;
                        }

                        roomsTested[y] = true;
                    }
                    else
                    {
                        deadEndsTested[y] = true;
                    }
                }
            }
        }

        roomPlacementSuccess = !noMoreRoomAvailable;
        nextRoomFinished = true;
        //return !noMoreRoomAvailable;
    }

    private IEnumerator FindNextOpening()
    {
        nextOpeningResearchFinished = false;
        bool openingAvailable = false;
        Vector2 openingIndexes = Vector2.zero;
        int openingDirection = -1;
        bool floorsRemaining = true;

        int randomFloor = 0;
        int randomZone = 0;
        int randomEmptyZone;
        while (!openingAvailable && floorsRemaining)
        {

            randomEmptyZone = Random.Range(0, uncheckedZones.Count);
            randomFloor = uncheckedZones[randomEmptyZone].gridX;
            randomZone = uncheckedZones[randomEmptyZone].gridY;

            Debug.Log("Checking zone " + randomFloor + ", " + randomZone + " for openings. possibilities : " + uncheckedZones.Count);

            if (!checkedFloors[randomFloor])
            {
                if (towerGrid[randomFloor, randomZone] != null && !zoneChecked[randomFloor, randomZone])
                {
                    Room.RoomPart lookedPart = towerGrid[randomFloor, randomZone];
                    if (lookedPart.openings.Length > 0)
                    {
                        Debug.Log("Openings found on Floor " + randomFloor + " at Zone " + randomZone);
                        bool freeOpeningsOnZone = false;
                        int lookedOpening = 0;
                        while (!openingAvailable && lookedOpening < 4)
                        {
                            if (lookedPart.openings[lookedOpening])
                            {
                                switch (lookedOpening)
                                {
                                    case 0:
                                        if ((randomFloor + 1) < towerHeight)
                                        {
                                            if (towerGrid[randomFloor + 1, randomZone] == null)
                                            {
                                                openingAvailable = true;
                                                openingDirection = 1;
                                                openingIndexes = new Vector2(randomFloor + 1, randomZone);
                                                Debug.Log("Free opening found on Zone " + randomZone + ", floor " + randomFloor + ". Direction : Up");
                                            }
                                        }
                                        break;

                                    case 1:
                                        if ((randomFloor - 1) >= 0)
                                        {
                                            if (towerGrid[randomFloor - 1, randomZone] == null)
                                            {
                                                openingAvailable = true;
                                                openingDirection = 0;
                                                openingIndexes = new Vector2(randomFloor - 1, randomZone);
                                                Debug.Log("Free opening found on Zone " + randomZone + ", floor " + randomFloor + ". Direction : Down");
                                            }
                                        }
                                        break;

                                    case 2:
                                        if ((randomZone + 1) < towerWidth)
                                        {
                                            if (towerGrid[randomFloor, randomZone + 1] == null)
                                            {
                                                openingAvailable = true;
                                                openingDirection = 3;
                                                openingIndexes = new Vector2(randomFloor, randomZone + 1);
                                                Debug.Log("Free opening found on Zone " + randomZone + ", floor " + randomFloor + ". Direction : Right");
                                            }
                                        }
                                        break;

                                    case 3:
                                        if ((randomZone - 1) >= 0)
                                        {
                                            if (towerGrid[randomFloor, randomZone - 1] == null)
                                            {
                                                openingAvailable = true;
                                                openingDirection = 2;
                                                openingIndexes = new Vector2(randomFloor, randomZone - 1);
                                                Debug.Log("Free opening found on Zone " + randomZone + ", floor " + randomFloor + ". Direction : Left");
                                            }
                                        }
                                        break;
                                    default:
                                        Debug.Log("The opening direction found does correspond to any known");
                                        break;
                                }

                                if (!openingAvailable)
                                {
                                    Debug.Log("The opening found at " + randomFloor + ", " + randomZone + " is blocked");
                                }
                                else
                                {
                                    freeOpeningsOnZone = true;
                                }
                            }
                            lookedOpening++;
                        }

                        if (!freeOpeningsOnZone)
                        {
                            zoneChecked[randomFloor, randomZone] = true;
                            uncheckedZones.Remove(UnCheckedZone.GetUncheckZone(uncheckedZones, randomFloor, randomZone));
                            Debug.Log("Zone " + randomFloor + ", " + randomZone + " fully blocked : added to skipList");
                        }
                    }
                    else
                    {
                        zoneChecked[randomFloor, randomZone] = true;
                        uncheckedZones.Remove(UnCheckedZone.GetUncheckZone(uncheckedZones, randomFloor, randomZone));
                        Debug.Log("No openings on Zone " + randomFloor + ", " + randomZone + " : added to skipList");
                    }
                }
                else
                {
                    if(towerGrid[randomFloor, randomZone] == null)
                    {
                        Debug.Log("Zone " + randomFloor + ", " + randomZone + " is null");
                    }
                }

                checkedFloors[randomFloor] = true;
                for (int i = 0; i < towerWidth; i++)
                {
                    if (!zoneChecked[randomFloor, i])
                    {
                        checkedFloors[randomFloor] = false;
                    }
                }

                if(checkedFloors[randomFloor] == true)
                {
                    Debug.Log("---Floor " + randomFloor + " isFullyBlocked ");
                }
            }
            else
            {
                Debug.Log("Floor " + randomFloor + "checked");
            }


            floorsRemaining = false;
            for (int i = 0; i < towerHeight - 1; i++)
            {
                if (!checkedFloors[i])
                {
                    floorsRemaining = true;
                }
            }
            Debug.Log("Floors remaining");
            yield return new WaitForEndOfFrame();
        }

        if(openingAvailable)
        {
            nextRoomStart = openingIndexes;
            nextRoomOpeningDirection = openingDirection;
            originRoom = new Vector2(randomFloor, randomZone);
        }
        else
        {
            originRoom = startPositionIndexes;
            Debug.Log("No free opening Available. Mission failed :,(");
        }

        openingRemaining = openingAvailable;
        nextOpeningResearchFinished = true;
        //return openingAvailable;
    }

    /// <summary>
    /// Return the direction of a free opening find with a roompart at a specified tile. Return -1 if no free opening is found
    /// </summary>
    /// <param name="part">The roompart which will be checked</param>
    /// <param name="floorToCheck">The floor number where the part will be checked</param>
    /// <param name="zoneToCheck">The zone number where the part will be checked</param>
    /// <returns></returns>
    private int CheckFreeOpening(Room.RoomPart part ,int floorToCheck, int zoneToCheck)
    {
        bool isFree = false;
        int lookedOpening = 0;
        int openingFound = -1;
        while (!isFree && lookedOpening < 4)
        {
            if (part.openings[lookedOpening])
            {
                switch (lookedOpening)
                {
                    case 0:
                        if ((floorToCheck + 1) < towerHeight)
                        {
                            if (towerGrid[floorToCheck + 1, zoneToCheck] == null
                            && !freeOpeningsUnusable.Contains(new Vector2(floorToCheck + 1, zoneToCheck)))
                            {
                                isFree = true;
                                openingFound = 0;
                                Debug.Log("Free opening found on Zone " + zoneToCheck + ", Floor " + floorToCheck + ". Direction : Up");
                            }
                        }
                        break;

                    case 1:
                        if ((floorToCheck - 1) >= 0)
                        {
                            if (towerGrid[floorToCheck - 1, zoneToCheck] == null
                                && !freeOpeningsUnusable.Contains(new Vector2(floorToCheck - 1, zoneToCheck)))
                            {
                                isFree = true;
                                openingFound = 1;
                                Debug.Log("Free opening found on Zone " + zoneToCheck + ", Floor " + floorToCheck + ". Direction : Down");
                            }
                        }
                        break;

                    case 2:
                        if ((zoneToCheck + 1) < towerWidth)
                        {
                            if (towerGrid[floorToCheck, zoneToCheck + 1] == null
                                && !freeOpeningsUnusable.Contains(new Vector2(floorToCheck, zoneToCheck + 1)))
                            {
                                isFree = true;
                                openingFound = 2;
                                Debug.Log("Free opening found on Zone " + zoneToCheck + ", Floor " + floorToCheck + ". Direction : Right");
                            }
                        }
                        break;

                    case 3:
                        if ((zoneToCheck - 1) >= 0)
                        {
                            if (towerGrid[floorToCheck, zoneToCheck - 1] == null
                            && !freeOpeningsUnusable.Contains(new Vector2(floorToCheck, zoneToCheck - 1)))
                            {
                                isFree = true;
                                openingFound = 3;
                                Debug.Log("Free opening found on Zone " + zoneToCheck + ", floor " + floorToCheck + ". Direction : Left");
                            }
                        }
                        break;
                    default:
                        Debug.Log("The opening direction found does not correspond to any known");
                        break;
                }

                if (!isFree)
                {
                    Debug.Log("The opening at " + floorToCheck + ", " + zoneToCheck + " is blocked");
                }
            }
            lookedOpening++;
        }

        return openingFound;
    }

    private bool CheckIfAllOpeningsAreFree(Room.RoomPart part, int floorToCheck, int zoneToCheck)
    {
        bool isAllFree = true;
        int lookedOpening = 0;
        int openingFound = -1;
        while (isAllFree && lookedOpening < 4)
        {
            if (part.openings[lookedOpening])
            {
                switch (lookedOpening)
                {
                    case 0:
                        if ((floorToCheck + 1) >= towerHeight
                            || (towerGrid[floorToCheck + 1, zoneToCheck] != null && !towerGrid[floorToCheck + 1, zoneToCheck].openings[1]))
                        {
                            isAllFree = false;
                        }
                        break;

                    case 1:
                        if ((floorToCheck - 1) < 0
                            || (towerGrid[floorToCheck - 1, zoneToCheck] != null && !towerGrid[floorToCheck - 1, zoneToCheck].openings[0]))
                        {
                            isAllFree = false;
                        }
                        break;

                    case 2:
                        if ((zoneToCheck + 1) >= towerWidth
                            || (towerGrid[floorToCheck, zoneToCheck + 1] != null && !towerGrid[floorToCheck, zoneToCheck + 1].openings[3]))
                        {
                            isAllFree = false;
                        }
                        break;

                    case 3:
                        if ((zoneToCheck - 1) < 0
                            || (towerGrid[floorToCheck, zoneToCheck - 1] != null && !towerGrid[floorToCheck, zoneToCheck - 1].openings[2]))
                        {
                            isAllFree = false;
                        }
                        break;
                    default:
                        Debug.LogError("The opening direction found does not correspond to any known");
                        break;
                }
            }
            lookedOpening++;
        }

        return isAllFree;
    }

    private bool IsNextZoneBlocking(Vector2 zoneGridIndexes, int oppositOriginDirection, ref List<Vector2> checkedZones)
    {
        Debug.Log("Checking accessibility for zone " + zoneGridIndexes);
        checkedZones.Add(zoneGridIndexes);
        bool isBlocking = true;
        int directionToCheck = 0;
        while(isBlocking && directionToCheck < 4)
        {
            directionToCheck++;
            int oppositDirection = 0;
            Vector2 zoneToCheck = Vector2.zero;
            if (directionToCheck != oppositOriginDirection)
            {
                switch (directionToCheck)
                {
                    case 1:
                        zoneToCheck = new Vector2(zoneGridIndexes.x + 1, zoneGridIndexes.y);
                        oppositDirection = 2;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Up");
                        break;

                    case 2:
                        zoneToCheck = new Vector2(zoneGridIndexes.x - 1, zoneGridIndexes.y);
                        oppositDirection = 1;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Down");
                        break;

                    case 3:
                        zoneToCheck = new Vector2(zoneGridIndexes.x, zoneGridIndexes.y + 1);
                        oppositDirection = 4;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Right");
                        break;

                    case 4:
                        zoneToCheck = new Vector2(zoneGridIndexes.x, zoneGridIndexes.y - 1);
                        oppositDirection = 3;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Left");
                        break;
                    default:
                        Debug.LogError("There is no such direction as : " + directionToCheck);
                        break;
                }

                if (zoneToCheck.x < towerHeight && zoneToCheck.x >= 0 && zoneToCheck.y < towerWidth && zoneToCheck.y >= 0)
                {
                    if (towerGrid[(int)zoneToCheck.x, (int)zoneToCheck.y] == null)
                    {
                        Debug.Log("Zone " + zoneToCheck + " is empty");
                        if (!checkedZones.Contains(zoneToCheck))
                        {
                            Debug.Log("Zone " + zoneToCheck + " has been already checked");
                            if (accessibleZones.Contains(zoneToCheck) || !IsNextZoneBlocking(zoneToCheck, oppositDirection, ref checkedZones))
                            {
                                Debug.Log("Zone " + zoneGridIndexes + " is accessible because it is adjacent to the accessible zone : " + zoneToCheck);
                                isBlocking = false;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Zone " + zoneToCheck + " is already filled, checking if there is corresponding opening");
                        for (int i = 0; i < 4; i++)
                        {
                            if(towerGrid[(int)zoneToCheck.x, (int)zoneToCheck.y].openings[i] && i == oppositDirection - 1)
                            {
                                isBlocking = false;
                                Debug.Log("Zone " + zoneToCheck + " has a corresponding opening !!, making it accessible");
                            }
                            else
                            {
                                Debug.Log("Zone " + zoneToCheck + " has no corresponding opening");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Zone " + zoneToCheck + " is out of the tower");
                }
            }
        }

        if(isBlocking)
        {
            Debug.Log("Zone " + zoneGridIndexes + " is un-accessible");
        }

        return isBlocking;
    }

    private IEnumerator CreateTower()
    {
        Vector2 originGridPos = new Vector2(bottomCenterTowerPos.x - towerWidth * tileLength / 2 + tileLength / 2, bottomCenterTowerPos.y + tileLength / 2);
        for(int floorNumber = 0; floorNumber < towerHeight; floorNumber++)
        {
            for(int zoneNumber = 0; zoneNumber < towerWidth; zoneNumber++)
            {
                if(towerGrid[floorNumber,zoneNumber] != null)
                {
                    Instantiate(towerGrid[floorNumber, zoneNumber].partPrefab, new Vector2(originGridPos.x + zoneNumber * tileLength, originGridPos.y + floorNumber * tileLength), Quaternion.identity);
                }
                else
                {
                    Instantiate(fillerPrefab, new Vector2(originGridPos.x + zoneNumber * tileLength, originGridPos.y + floorNumber * tileLength), Quaternion.identity);
                }

                if(creatingTime / (towerHeight * towerWidth) > 0.001f)
                {
                    yield return new WaitForSeconds(creatingTime / (towerHeight * towerWidth));
                }
            }
        }
        levelBuilt = true;
        yield return new WaitForSeconds(creatingTime);
        Debug.Log("Tower created.");
    }

    private void CreateTile(int gridFloor, int gridZone)
    {
        Vector2 originGridPos = new Vector2(bottomCenterTowerPos.x - towerWidth * tileLength / 2 + tileLength / 2, bottomCenterTowerPos.y + tileLength / 2);
        if (towerGrid[gridFloor, gridZone] != null)
        {
            Instantiate(towerGrid[gridFloor, gridZone].partPrefab, new Vector2(originGridPos.x + gridZone * tileLength, originGridPos.y + gridFloor * tileLength), Quaternion.identity);
        }
        else
        {
            Instantiate(fillerPrefab, new Vector2(originGridPos.x + gridZone * tileLength, originGridPos.y + gridFloor * tileLength), Quaternion.identity);
        }
    }

    public void ReArrangeRooms()
    {
        foreach(Room room in roomList)
        {
            room.Rearrange();
        }

        foreach (Room deadEnd in deadEndList)
        {
            deadEnd.Rearrange();
        }

        foreach (Room leftEdge in leftEdgeList)
        {
            leftEdge.Rearrange();
        }

        foreach (Room rightEdge in rightEdgeList)
        {
            rightEdge.Rearrange();
        }
    }

    private class UnCheckedZone
    {
        public int gridX;
        public int gridY;

        public UnCheckedZone(int xGridPos, int yGridPos)
        {
            gridX = xGridPos;
            gridY = yGridPos;
        }

        public static List<UnCheckedZone> CreateUncheckGrid(int towerH, int towerW)
        {
            List<UnCheckedZone> uncheckZone = new List<UnCheckedZone>();

            for(int i = 0; i < towerH; i++)
            {
                for(int t = 0; t < towerW; t++)
                {
                    uncheckZone.Add(new UnCheckedZone(i, t));
                }
            }

            return uncheckZone;
        }

        public static UnCheckedZone GetUncheckZone(List<UnCheckedZone> zones, int floor, int zone)
        {

            UnCheckedZone unCheckedZone = null;
            int i = 0;
            while (i < zones.Count && unCheckedZone == null)
            {
                if (zones[i].gridX == floor && zones[i].gridY == zone)
                {
                    zones.RemoveAt(i);
                    unCheckedZone = zones[i];
                    Debug.Log("Uncheck Zone found at " + i);
                    break;
                }
                i++;
            }

            if(unCheckedZone != null)
            {
                Debug.Log("Unchecked zone " + floor + ", " + zone + "coudn't be removed from the list");
            }

            return unCheckedZone;
        }
    }
}