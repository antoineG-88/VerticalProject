using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    #region variables
    [Header("Generation Step debug settings")]
    public bool instantCreation;
    public float timeBetweenRoomCreation;
    public float timeTestingRoom;
    public float timeSearchingOpening;
    public List<Color> gizmoColor;
    [Header("Generation settings")]
    public int towerWidth;
    public int towerHeight;
    public Coord startPositionIndexes;
    public int roomBuildBeforeYokaiRoom;
    public int healthTerminalNumbers;
    [Space]
    public List<Room> roomList;
    public List<Room> deadEndList;
    public List<Room> rightEdgeList;
    public List<Room> leftEdgeList;
    public List<Room> endRoomList;
    public List<Room> yokaiRoomList;
    public List<Room> startRoomList;
    public GameObject fillerPrefab;
    public GameObject leftTowerWall;
    public GameObject rightTowerWall;
    public GameObject verticalDoors;
    public GameObject horizontalDoors;
    public Vector2 verticalDoorsOffset;
    public Vector2 horizontalDoorsOffset;
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
    [HideInInspector] public bool towerCreated;
    public GameObject levelHolderPrefab;
    [Space]
    [Space]
    [Space]
    public bool hideDebugTowerGenerationLog;
    public bool deactivateRoomOnCreation;

    [HideInInspector] public Room.RoomPart[,] towerGrid;
    [HideInInspector] public RoomHandler[,] towerRooms;
    private GameObject levelHolder;
    private bool[,] zoneChecked;
    private bool[] checkedFloors;
    private List<Coord> emptyZones;
    private List<Coord> freeOpeningsUnusable = new List<Coord>();
    private List<Coord> checkedZones = new List<Coord>();
    private List<Coord> accessibleZones = new List<Coord>();
    private List<Coord> freeOpeningZones = new List<Coord>();
    private List<Coord> goodOpeningZones = new List<Coord>();
    private List<int> goodOpeningDirection = new List<int>();
    private Coord nextRoomStart;
    private int nextRoomOpeningDirection;
    private Coord originRoom;
    private int roomBuiltNumber;
    private bool endRoomBuild;
    private bool endRoomChosen;
    private bool yokaiRoomBuild;
    private bool yokaiRoomChosen;
    private bool leftEdgeChosen;
    private bool rightEdgeChosen;
    private int healthTerminalPlaced;

    private bool levelBuilt;
    private bool levelScanned;
    private GridGraph gridGraph;

    private bool nextRoomFinished;
    private bool roomPlacementSuccess;
    private bool openingRemaining;
    private bool nextOpeningResearchFinished;

    private List<ZoneGizmo> currentlyDrawnZoneGismos = new List<ZoneGizmo>();

    public enum Direction { Up, Down, Right, Left };

    #endregion

    private void Awake()
    {
        nextRoomFinished = false;
        levelBuilt = false;
        levelScanned = false;
        ReArrangeRooms(new List<List<Room>> { roomList, deadEndList, leftEdgeList, rightEdgeList, endRoomList, yokaiRoomList, startRoomList});
        StartCoroutine(BuildLevel());
        if (hideDebugTowerGenerationLog)
        {
            Debug.unityLogger.logEnabled = false;
        }
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
    }

    public IEnumerator BuildLevel()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("Beginning Tower creation");
        towerCreated = false;
        int towerCreationAttempt = 0;
        do
        {
            towerCreationAttempt++;
            Debug.Log("Tower Creation attempt number " + towerCreationAttempt);
            if(levelHolder != null)
            {
                Destroy(levelHolder);
            }
            levelHolder = Instantiate(levelHolderPrefab, Vector2.zero, Quaternion.identity);
            levelHolder.name = "LevelHolder";

            roomBuiltNumber = 0;
            endRoomBuild = false;
            yokaiRoomBuild = false;
            healthTerminalPlaced = 0;
            towerGrid = new Room.RoomPart[towerHeight, towerWidth];
            checkedFloors = new bool[towerHeight];
            emptyZones = Coord.CreateFullZoneGrid(towerHeight, towerWidth);
            zoneChecked = new bool[towerHeight, towerWidth];
            towerRooms = new RoomHandler[towerHeight, towerWidth];

            nextRoomStart = startPositionIndexes;
            nextRoomOpeningDirection = 3;
            openingRemaining = true;

            do
            {
                Debug.Log("Start building room number " + (roomBuiltNumber + 1));

                if(roomBuiltNumber == 0)
                {
                    PlaceFirstRoom();
                }
                else
                {
                    StartCoroutine(PlaceNextRoom(nextRoomStart, nextRoomOpeningDirection));
                }

                while (!nextRoomFinished)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (!roomPlacementSuccess)
                {
                    freeOpeningsUnusable.Add(nextRoomStart);
                    zoneChecked[originRoom.x, originRoom.y] = true;
                    Debug.Log("The tile at " + nextRoomStart.x + ", " + nextRoomStart.y + " cannot be used as a connection. Added to unusable list, Filling it with roomFiller");

                    accessibleZones.Remove(Coord.GetZone(accessibleZones, nextRoomStart.x, nextRoomStart.y));
                }
                else
                {
                    roomBuiltNumber++;
                }

                if (accessibleZones.Count > 0)
                {
                    StartCoroutine(FindNextRandomOpening());

                    while (!nextOpeningResearchFinished)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    openingRemaining = false;
                }

                if (!instantCreation && timeBetweenRoomCreation > 0)
                    yield return new WaitForSeconds(timeBetweenRoomCreation);
            }
            while (openingRemaining && roomBuiltNumber <= maxRoom);

            FillEmptyZones();

        } while (!endRoomBuild || !yokaiRoomBuild);

        if(healthTerminalNumbers > 0)
        {
            PLaceHealthTerminal();
        }
        yield return new WaitForFixedUpdate();

        if (!openingRemaining)
        {
            Debug.Log("The Tower couldn't be correctly designed due to no available openings left, Only " + roomBuiltNumber + " rooms has been built");
        }
        else
        {
            Debug.Log("Tower correctly designed with a total of " + roomBuiltNumber + " rooms on " + towerHeight + " floors with " + towerWidth + " zones each");
        }
        towerCreated = true;

        if (hideDebugTowerGenerationLog)
        {
            Debug.unityLogger.logEnabled = true;
        }
    }

    public IEnumerator PlaceNextRoom(Coord nextZone, int openingDirection)
    {
        nextRoomFinished = false;
        bool[] roomsTested = new bool[roomList.Count];
        bool[] deadEndsTested = new bool[deadEndList.Count];
        bool[] rightEdgesTested = new bool[rightEdgeList.Count];
        bool[] leftEdgesTested = new bool[leftEdgeList.Count];
        bool[] endRoomsTested = new bool[endRoomList.Count];
        bool[] yokaiRoomsTested = new bool[yokaiRoomList.Count];
        bool isDeadEnd = false;
        Room selectedRoom = null;
        Room potentialRoom = null;
        bool noMoreRoomAvailable = false;
        Coord connectedPartIndexes = new Coord(0, 0);

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
            bool endRoomLeft = false;
            bool yokaiRoomLeft = false;
            endRoomChosen = false;
            yokaiRoomChosen = false;
            rightEdgeChosen = false;
            leftEdgeChosen = false;

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

                foreach (bool endRoomTested in endRoomsTested)
                {
                    if (!endRoomTested)
                    {
                        endRoomLeft = true;
                    }
                }

                foreach (bool yokaiRoomTested in yokaiRoomsTested)
                {
                    if (!yokaiRoomTested)
                    {
                        yokaiRoomLeft = true;
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
                if(nextZone.x == towerHeight - 1 && endRoomLeft && !endRoomBuild)
                {
                    do
                    {
                        y = Random.Range(0, endRoomList.Count);
                    }
                    while (endRoomsTested[y]);
                    potentialRoom = endRoomList[y];
                    endRoomChosen = true;
                }
                else if (roomBuiltNumber > roomBuildBeforeYokaiRoom && yokaiRoomLeft && !yokaiRoomBuild)
                {
                    do
                    {
                        y = Random.Range(0, yokaiRoomList.Count);
                    }
                    while (yokaiRoomsTested[y]);
                    potentialRoom = yokaiRoomList[y];
                    yokaiRoomChosen = true;
                }
                else if (nextZone.y == 0 && leftEdgeLeft)
                {
                    do
                    {
                        y = Random.Range(0, leftEdgeList.Count);
                    }
                    while (leftEdgesTested[y]);
                    potentialRoom = leftEdgeList[y];
                    leftEdgeChosen = true;
                }
                else if (nextZone.y == (towerWidth - 1) && rightEdgeLeft)
                {
                    do
                    {
                        y = Random.Range(0, rightEdgeList.Count);
                    }
                    while (rightEdgesTested[y]);
                    potentialRoom = rightEdgeList[y];
                    rightEdgeChosen = false;
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
                                Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                if (nextZone.x + relativeIndexes.x < 0 || nextZone.y + relativeIndexes.y < 0 || nextZone.x + relativeIndexes.x >= towerHeight || nextZone.y + relativeIndexes.y >= towerWidth || towerGrid[nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y] != null)
                                {
                                    enoughSpace = false;
                                    Debug.Log("The tile at " + (nextZone.x + relativeIndexes.x) + ", " + (nextZone.y + relativeIndexes.y) + " is already filled");
                                }
                                else
                                {
                                    Debug.Log("The " + potentialRoom + "'s part (" + floorNumber + ", " + zoneNumber + ") can be placed on the tower at " + (nextZone.x + relativeIndexes.x) + ", " + (nextZone.y + relativeIndexes.y));
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
                    }
                    else
                    {
                        Debug.Log(potentialRoom.name + " can fit in the tower. Positioned at Floor " + nextZone.x + " Zone " + nextZone.y);

                        if(mandatoryOpenings && !isDeadEnd)
                        {
                            Debug.Log("Now checking mandatory openings for " + potentialRoom.name + " at " + nextZone.x + ", " + nextZone.y);
                            floorNumber = 0;
                            while (!createMandatoryFreeOpening && floorNumber < potentialRoom.roomParts.GetLength(0))
                            {
                                int zoneNumber = 0;
                                while (!createMandatoryFreeOpening && zoneNumber < potentialRoom.roomParts.GetLength(1))
                                {
                                    Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                    if (potentialRoom.roomParts[floorNumber, zoneNumber] != null
                                        && potentialRoom.roomParts[floorNumber, zoneNumber].openings.Length > 0)
                                    {
                                        Vector2 followingIndexes = Vector2.zero;
                                        int openingDirectionFound = CheckFreeOpening(potentialRoom.roomParts[floorNumber, zoneNumber], nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y);
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
                        }
                        else
                        {
                            Debug.Log("Checking if " + potentialRoom.name + "'s openings are blocked");
                            bool allRoomOpeningsFree = true;
                            if (avoidBlockedOpenings)
                            {
                                freeOpeningZones.Clear();
                                goodOpeningZones.Clear();
                                goodOpeningDirection.Clear();
                                floorNumber = 0;
                                while (allRoomOpeningsFree && floorNumber < potentialRoom.roomParts.GetLength(0))
                                {
                                    int zoneNumber = 0;
                                    while (allRoomOpeningsFree && zoneNumber < potentialRoom.roomParts.GetLength(1))
                                    {
                                        Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                        if (potentialRoom.roomParts[floorNumber, zoneNumber] != null
                                            && potentialRoom.roomParts[floorNumber, zoneNumber].openings.Length > 0)
                                        {
                                            allRoomOpeningsFree = IsBlockingOwnOrOtherOpenings(potentialRoom.roomParts[floorNumber, zoneNumber], nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y);
                                        }
                                        zoneNumber++;
                                    }
                                    floorNumber++;
                                }
                            }

                            if (allRoomOpeningsFree)
                            {
                                accessibleZones.AddRange(freeOpeningZones);

                                selectedRoom = potentialRoom;
                                Debug.Log("Room found ! The room " + selectedRoom.name + " has all needed features. Placed at Floor " + nextZone.x + " Zone " + nextZone.y);

                                RoomHandler newRoomHandler = new RoomHandler(selectedRoom, selectedRoom.roomParts.GetLength(0), selectedRoom.roomParts.GetLength(1));
                                GameData.levelHandler.allTowerRooms.Add(newRoomHandler);

                                // Placement des portes et liaison au roomHandler
                                Vector2 doorPos;
                                for (int i = 0; i < goodOpeningDirection.Count; i++)
                                {
                                    doorPos = Coord.ZoneToTowerPos(goodOpeningZones[i], this);
                                    GameObject newDoor = null;
                                    RoomHandler.RoomDoors newRoomDoors;
                                    switch (goodOpeningDirection[i])
                                    {
                                        case 0:
                                            doorPos.y += tileLength / 2;
                                            doorPos += horizontalDoorsOffset;
                                            newDoor = Instantiate(horizontalDoors, doorPos, horizontalDoors.transform.rotation, levelHolder.transform);
                                            newRoomDoors = new RoomHandler.RoomDoors(newDoor.GetComponent<Doors>(), 0);
                                            newRoomHandler.doors.Add(newRoomDoors);

                                            if (towerRooms[nextZone.x + 1, nextZone.y] != null)
                                            {
                                                towerRooms[nextZone.x + 1, nextZone.y].doors.Add(newRoomDoors);
                                            }
                                            break;

                                        case 1:
                                            doorPos.y -= tileLength / 2;
                                            doorPos += horizontalDoorsOffset;
                                            newDoor = Instantiate(horizontalDoors, doorPos, horizontalDoors.transform.rotation, levelHolder.transform);
                                            newRoomDoors = new RoomHandler.RoomDoors(newDoor.GetComponent<Doors>(), 1);
                                            newRoomHandler.doors.Add(newRoomDoors);

                                            if (towerRooms[nextZone.x - 1, nextZone.y] != null)
                                            {
                                                towerRooms[nextZone.x - 1, nextZone.y].doors.Add(newRoomDoors);
                                            }
                                            break;

                                        case 2:
                                            doorPos.x += tileLength / 2;
                                            doorPos += verticalDoorsOffset;
                                            newDoor = Instantiate(verticalDoors, doorPos, verticalDoors.transform.rotation, levelHolder.transform);
                                            newRoomDoors = new RoomHandler.RoomDoors(newDoor.GetComponent<Doors>(), 2);
                                            newRoomHandler.doors.Add(newRoomDoors);

                                            if (towerRooms[nextZone.x, nextZone.y + 1] != null)
                                            {
                                                towerRooms[nextZone.x, nextZone.y + 1].doors.Add(newRoomDoors);
                                            }
                                            break;

                                        case 3:
                                            doorPos.x -= tileLength / 2;
                                            doorPos += verticalDoorsOffset;
                                            newDoor = Instantiate(verticalDoors, doorPos, verticalDoors.transform.rotation, levelHolder.transform);
                                            newRoomDoors = new RoomHandler.RoomDoors(newDoor.GetComponent<Doors>(), 3);
                                            newRoomHandler.doors.Add(newRoomDoors);

                                            if (towerRooms[nextZone.x, nextZone.y - 1] != null)
                                            {
                                                towerRooms[nextZone.x, nextZone.y - 1].doors.Add(newRoomDoors);
                                            }
                                            break;
                                    }
                                }

                                for (floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
                                {
                                    for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                                    {
                                        Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);

                                        if(floorNumber == 0 && zoneNumber == 0)
                                        {
                                            Vector2 bottomLeftOffset;
                                            bottomLeftOffset.x = (selectedRoom.roomParts.GetLength(1) * tileLength) / 2 - (tileLength / 2);
                                            bottomLeftOffset.y = (selectedRoom.roomParts.GetLength(0) * tileLength) / 2 - (tileLength / 2);
                                            newRoomHandler.SetCenter(Coord.ZoneToTowerPos(nextZone + relativeIndexes, this) + bottomLeftOffset);
                                        }

                                        if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                        {
                                            towerGrid[nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y] = selectedRoom.roomParts[floorNumber, zoneNumber];

                                            towerRooms[nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y] = newRoomHandler;
                                            newRoomHandler.zonesCenterPos.Add(Coord.ZoneToTowerPos(new Coord(nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y), this));

                                            if(leftEdgeChosen && zoneNumber == 0)
                                            {
                                                newRoomHandler.zonesCenterPos.Add(Coord.ZoneToTowerPos(new Coord(nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y - 1), this));
                                            }
                                            else if (rightEdgeChosen && zoneNumber == selectedRoom.roomParts.GetLength(1) - 1)
                                            {
                                                newRoomHandler.zonesCenterPos.Add(Coord.ZoneToTowerPos(new Coord(nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y + 1), this));
                                            }

                                            if(zoneNumber == 0 && nextZone.y == 0 && !leftEdgeChosen)
                                            {
                                                CreateBorder(nextZone + relativeIndexes);
                                            }
                                            else if (zoneNumber == (selectedRoom.roomParts.GetLength(1) - 1) && nextZone.y == (towerWidth - 1) && !rightEdgeChosen)
                                            {
                                                CreateBorder(nextZone + relativeIndexes);
                                                Debug.Log("-y- Border created during : " + selectedRoom.name);
                                            }

                                            Debug.Log(selectedRoom.name + " part placed on " + (nextZone + relativeIndexes));
                                            GameObject newRoomObject = CreateTile(nextZone + relativeIndexes);
                                            newRoomHandler.roomParts.Add(newRoomObject);
                                            newRoomHandler.InitializeRoomObjects();

                                            if( deactivateRoomOnCreation)
                                            {
                                                newRoomHandler.DeActivate();
                                            }
                                        }
                                        else
                                        {
                                            towerGrid[nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y] = null;
                                            Debug.Log(selectedRoom.name + " hole placed on " + nextZone.x + relativeIndexes.x + ", " + nextZone.y + relativeIndexes.y);
                                        }
                                    }
                                }

                                if (fillEmptySpaces)
                                {
                                    Debug.Log("Now ckecking if " + selectedRoom.name + " is creating unaccessible space");

                                    checkedZones.Clear();
                                    bool roomIsBlocking = false;

                                    for (floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
                                    {
                                        for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                                        {
                                            Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                            if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                            {
                                                List<Coord> checkedZones = new List<Coord>();
                                                int direction = 0;
                                                Debug.Log("Checking if zone " + nextZone.x + relativeIndexes.x + ", " + nextZone.y + relativeIndexes.y + " is blocking");
                                                Coord zoneToCheck = new Coord(0, 0);
                                                while (!roomIsBlocking && direction < 4)
                                                {
                                                    direction++;
                                                    switch (direction)
                                                    {
                                                        case 1:
                                                            zoneToCheck = new Coord(nextZone.x + relativeIndexes.x + 1, nextZone.y + relativeIndexes.y);
                                                            break;

                                                        case 2:
                                                            zoneToCheck = new Coord(nextZone.x + relativeIndexes.x - 1, nextZone.y + relativeIndexes.y);
                                                            break;

                                                        case 3:
                                                            zoneToCheck = new Coord(nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y + 1);
                                                            break;

                                                        case 4:
                                                            zoneToCheck = new Coord(nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y - 1);
                                                            break;

                                                        default:
                                                            Debug.LogError("There is no such direction as : " + direction);
                                                            break;
                                                    }

                                                    if (!IsNextZoneBlocking(zoneToCheck, 0, ref checkedZones))
                                                    {
                                                        this.checkedZones.AddRange(checkedZones);
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
                                                Coord relativeIndexes = new Coord(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                                                if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                                                {
                                                    towerGrid[nextZone.x + relativeIndexes.x, nextZone.y + relativeIndexes.y] = null;
                                                    Debug.Log(selectedRoom.name + " part removed on " + nextZone.x + relativeIndexes.x + ", " + nextZone.y + relativeIndexes.y);
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
                                freeOpeningZones.Clear();
                                goodOpeningZones.Clear();
                                goodOpeningDirection.Clear();
                            }
                        }
                    }
                }

                if (selectedRoom == null)
                {
                    if(endRoomChosen)
                    {
                        endRoomsTested[y] = true;
                    }
                    else if(yokaiRoomChosen)
                    {
                        yokaiRoomsTested[y] = true;
                    }
                    else if (!isDeadEnd)
                    {
                        if (nextZone.y == 0 && leftEdgeLeft)
                        {
                            leftEdgesTested[y] = true;
                        }
                        else if (nextZone.y == (towerWidth - 1) && rightEdgeLeft)
                        {
                            rightEdgesTested[y] = true;
                        }
                        else
                        {
                            roomsTested[y] = true;
                        }
                    }
                    else
                    {
                        deadEndsTested[y] = true;
                    }
                }
                else
                {
                    if(endRoomChosen)
                    {
                        Debug.Log("End room successfully placed at " + nextZone);
                        endRoomBuild = true;
                    }
                    else if(yokaiRoomChosen)
                    {
                        Debug.Log("Yokai room successfully placed at " + nextZone);
                        yokaiRoomBuild = true;
                    }
                }
            }
            if (!instantCreation && timeTestingRoom > 0)
                yield return new WaitForSeconds(timeTestingRoom);
        }

        roomPlacementSuccess = !noMoreRoomAvailable;
        nextRoomFinished = true;
        //return !noMoreRoomAvailable;
    }

    [System.ObsoleteAttribute("This method is everlooping and overcomplicated as f*** so don't use it ! (but it makes this script even bigger so ^^)", false)]
    private IEnumerator FindNextOpening()
    {
        nextOpeningResearchFinished = false;
        bool openingAvailable = false;
        Coord openingIndexes = new Coord(0, 0);
        int openingDirection = -1;
        bool floorsRemaining = true;

        int randomFloor = 0;
        int randomZone = 0;
        int randomUncheckedZone;
        while (!openingAvailable && floorsRemaining)
        {

            randomUncheckedZone = Random.Range(0, emptyZones.Count);
            randomFloor = emptyZones[randomUncheckedZone].x;
            randomZone = emptyZones[randomUncheckedZone].y;

            Debug.Log("Checking zone " + randomFloor + ", " + randomZone + " for openings. possibilities : " + emptyZones.Count);

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
                                                openingIndexes = new Coord(randomFloor + 1, randomZone);
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
                                                openingIndexes = new Coord(randomFloor - 1, randomZone);
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
                                                openingIndexes = new Coord(randomFloor, randomZone + 1);
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
                                                openingIndexes = new Coord(randomFloor, randomZone - 1);
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
                            emptyZones.Remove(Coord.GetZone(emptyZones, randomFloor, randomZone));
                            Debug.Log("Zone " + randomFloor + ", " + randomZone + " fully blocked : added to skipList");
                        }
                    }
                    else
                    {
                        zoneChecked[randomFloor, randomZone] = true;
                        emptyZones.Remove(Coord.GetZone(emptyZones, randomFloor, randomZone));
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
            nextRoomStart = new Coord(openingIndexes.x, openingIndexes.y);
            nextRoomOpeningDirection = openingDirection;
            originRoom = new Coord(randomFloor, randomZone);
        }
        else
        {
            originRoom = startPositionIndexes;
            Debug.Log("No free opening Available. Mission failed :,(");
        }

        //openingRemaining = openingAvailable;
        nextOpeningResearchFinished = true;
        //return openingAvailable;
    }

    private IEnumerator FindNextRandomOpening()
    {
        Debug.Log("Searching new free random opening");
        nextOpeningResearchFinished = false;

        //List<Coord> currentlyAccessibleZone = Coord.GetListUnion(accessibleZones, emptyZones);
        List<Coord> currentlyAccessibleZone = accessibleZones;
        int openingDirection = -1;
        int nextOpening = -1;

        bool zoneRemaining = true;
        bool openingFound = false;
        Coord randomZone = null;
        Coord nearbyZone = null;
        while(zoneRemaining && !openingFound)
        {
            randomZone = currentlyAccessibleZone[Random.Range(0, currentlyAccessibleZone.Count)];
            //Debug.Log("Testing empty zone " + randomZone.x + ", " + randomZone.y + "amongst " + currentlyAccessibleZone.Count + "for nearby openings");
            if(towerGrid[randomZone.x, randomZone.y] == null)
            {
                int checkDirection = 0;
                while (checkDirection < 4 && !openingFound)
                {
                    Coord relativeZoneOffset = new Coord(0,0);
                    switch (checkDirection)
                    {
                        case 0:
                            relativeZoneOffset = new Coord(1, 0);
                            openingDirection = 1;
                            break;

                        case 1:
                            relativeZoneOffset = new Coord(-1, 0);
                            openingDirection = 0;
                            break;

                        case 2:
                            relativeZoneOffset = new Coord(0, 1);
                            openingDirection = 3;
                            break;

                        case 3:
                            relativeZoneOffset = new Coord(0, -1);
                            openingDirection = 2;
                            break;
                    }


                    nearbyZone = new Coord(randomZone.x + relativeZoneOffset.x, randomZone.y + relativeZoneOffset.y);

                    if (nearbyZone.x < towerHeight && nearbyZone.x >= 0 && nearbyZone.y < towerWidth && nearbyZone.y >= 0)
                    {
                        Room.RoomPart part = towerGrid[nearbyZone.x, nearbyZone.y];

                        if (part != null)
                        {
                            if (!instantCreation && timeSearchingOpening > 0)
                            {
                                currentlyDrawnZoneGismos.Clear();
                                ZoneGizmo.Draw(nearbyZone, gizmoColor[0], timeSearchingOpening, this);
                                ZoneGizmo.Draw(randomZone, gizmoColor[0], timeSearchingOpening, this);
                                yield return new WaitForSeconds(timeSearchingOpening);
                            }
                            if(part.openings[openingDirection])
                            {
                                openingFound = true;
                                nextOpening = checkDirection;
                                Debug.Log("Opening found for empty zone " + randomZone.x + ", " + randomZone.y + " connected " + openingDirection);
                                ZoneGizmo.Draw(nearbyZone, gizmoColor[2], timeSearchingOpening, this);
                            }
                            if (!instantCreation && timeSearchingOpening > 0)
                                yield return new WaitForSeconds(timeSearchingOpening);
                        }
                    }

                    if(!openingFound)
                    {
                        if (!instantCreation && timeSearchingOpening > 0)
                            ZoneGizmo.Draw(nearbyZone, gizmoColor[1], timeSearchingOpening, this);
                    }

                    checkDirection++;
                    if (!instantCreation && timeSearchingOpening > 0)
                        yield return new WaitForSeconds(timeSearchingOpening);
                }
            }

            if(!openingFound)
            {
                currentlyAccessibleZone.Remove(randomZone);
                //Debug.Log("Zone " + randomZone.x + ", " + randomZone.y + " is not accessible for now, removed from accessible zones");
            }

            if(currentlyAccessibleZone.Count <= 0)
            {
                zoneRemaining = false;
                Debug.Log("No accessible Zone remaining, end of tower creation");
            }
            else
            {
                if (!instantCreation && timeSearchingOpening > 0)
                {
                    currentlyDrawnZoneGismos.Clear();
                    foreach (Coord accessZone in currentlyAccessibleZone)
                    {
                        ZoneGizmo.Draw(accessZone, gizmoColor[2], 1f, this);
                    }
                }
            }
            if (!instantCreation && timeSearchingOpening > 0)
                yield return new WaitForSeconds(timeSearchingOpening);
        }

        if (openingFound)
        {
            nextRoomStart = randomZone;
            nextRoomOpeningDirection = nextOpening;
            originRoom = new Coord(nearbyZone.x, nearbyZone.y);
        }
        else
        {
            originRoom = startPositionIndexes;
            Debug.Log("No free opening Available. Mission failed :,(");
        }
        openingRemaining = openingFound;

        nextOpeningResearchFinished = true;
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
                            && !freeOpeningsUnusable.Contains(new Coord(floorToCheck + 1, zoneToCheck)))
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
                                && !freeOpeningsUnusable.Contains(new Coord(floorToCheck - 1, zoneToCheck)))
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
                                && !freeOpeningsUnusable.Contains(new Coord(floorToCheck, zoneToCheck + 1)))
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
                            && !freeOpeningsUnusable.Contains(new Coord(floorToCheck, zoneToCheck - 1)))
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

    private bool IsBlockingOwnOrOtherOpenings(Room.RoomPart part, int floorToCheck, int zoneToCheck)
    {
        bool isAllFree = true;
        int lookedOpening = 0;
        while (isAllFree && lookedOpening < 4)
        {
            if (part.openings[lookedOpening])
            {
                switch (lookedOpening)
                {
                    case 0:
                        if((floorToCheck + 1) >= towerHeight)
                        {
                            isAllFree = false;
                        }
                        else
                        {
                            if (towerGrid[floorToCheck + 1, zoneToCheck] != null)
                            {
                                if (!towerGrid[floorToCheck + 1, zoneToCheck].openings[1])
                                {
                                    isAllFree = false;
                                }
                                else
                                {
                                    goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    goodOpeningDirection.Add(0);
                                }
                            }
                            else
                            {
                                if (Coord.GetZone(accessibleZones, floorToCheck + 1, zoneToCheck) == null)
                                {
                                    freeOpeningZones.Add(new Coord(floorToCheck + 1, zoneToCheck));
                                    //goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    //goodOpeningDirection.Add(0);
                                }
                            }
                        }
                        break;

                    case 1:
                        if ((floorToCheck - 1) < 0)
                        {
                            isAllFree = false;
                        }
                        else
                        {
                            if (towerGrid[floorToCheck - 1, zoneToCheck] != null)
                            {
                                if (!towerGrid[floorToCheck - 1, zoneToCheck].openings[0])
                                {
                                    isAllFree = false;
                                }
                                else
                                {
                                    goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    goodOpeningDirection.Add(1);
                                }
                            }
                            else
                            {
                                if (Coord.GetZone(accessibleZones, floorToCheck - 1, zoneToCheck) == null)
                                {
                                    freeOpeningZones.Add(new Coord(floorToCheck - 1, zoneToCheck));
                                    //goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    //goodOpeningDirection.Add(1);
                                }
                            }
                        }
                        break;

                    case 2:
                        if ((zoneToCheck + 1) >= towerWidth)
                        {
                            isAllFree = false;
                        }
                        else
                        {
                            if (towerGrid[floorToCheck, zoneToCheck + 1] != null)
                            {
                                if (!towerGrid[floorToCheck, zoneToCheck + 1].openings[3])
                                {
                                    isAllFree = false;
                                }
                                else
                                {
                                    goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    goodOpeningDirection.Add(2);
                                }
                            }
                            else
                            {
                                if (Coord.GetZone(accessibleZones, floorToCheck, zoneToCheck + 1) == null)
                                {
                                    freeOpeningZones.Add(new Coord(floorToCheck, zoneToCheck + 1));
                                    //goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    //goodOpeningDirection.Add(2);
                                }
                            }
                        }
                        break;

                    case 3:
                        if ((zoneToCheck - 1) < 0)
                        {
                            isAllFree = false;
                        }
                        else
                        {
                            if (towerGrid[floorToCheck, zoneToCheck - 1] != null)
                            {
                                if (!towerGrid[floorToCheck, zoneToCheck - 1].openings[2])
                                {
                                    isAllFree = false;
                                }
                                else
                                {
                                    goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    goodOpeningDirection.Add(3);
                                }
                            }
                            else
                            {
                                if (Coord.GetZone(accessibleZones, floorToCheck, zoneToCheck - 1) == null)
                                {
                                    freeOpeningZones.Add(new Coord(floorToCheck, zoneToCheck - 1));
                                    //goodOpeningZones.Add(new Coord(floorToCheck, zoneToCheck));
                                    //goodOpeningDirection.Add(3);
                                }
                            }
                        }
                        break;
                    default:
                        Debug.LogError("The opening direction found does not correspond to any known");
                        break;
                }
            }
            else
            {
                switch(lookedOpening)
                {
                    case 0:
                        if((floorToCheck + 1) < towerHeight && towerGrid[floorToCheck + 1, zoneToCheck] != null && towerGrid[floorToCheck + 1, zoneToCheck].openings[1])
                        {
                            isAllFree = false;
                        }
                        break;

                    case 1:
                        if ((floorToCheck - 1) >= 0 && towerGrid[floorToCheck - 1, zoneToCheck] != null && towerGrid[floorToCheck - 1, zoneToCheck].openings[0])
                        {
                            isAllFree = false;
                        }
                        break;

                    case 2:
                        if ((zoneToCheck + 1) < towerWidth && towerGrid[floorToCheck, zoneToCheck + 1] != null && towerGrid[floorToCheck, zoneToCheck + 1].openings[3])
                        {
                            isAllFree = false;
                        }
                        break;

                    case 3:
                        if ((zoneToCheck - 1) >= 0 && towerGrid[floorToCheck, zoneToCheck - 1] != null && towerGrid[floorToCheck, zoneToCheck - 1].openings[2])
                        {
                            isAllFree = false;
                        }
                        break;
                }
            }
            lookedOpening++;
        }

        return isAllFree;
    }

    private bool IsNextZoneBlocking(Coord zoneGridIndexes, int oppositOriginDirection, ref List<Coord> checkedZones)
    {
        Debug.Log("Checking accessibility for zone " + zoneGridIndexes);
        checkedZones.Add(zoneGridIndexes);
        bool isBlocking = true;
        int directionToCheck = 0;
        while(isBlocking && directionToCheck < 4)
        {
            directionToCheck++;
            int oppositDirection = 0;
            Coord zoneToCheck = new Coord(0, 0);
            if (directionToCheck != oppositOriginDirection)
            {
                switch (directionToCheck)
                {
                    case 1:
                        zoneToCheck = new Coord(zoneGridIndexes.x + 1, zoneGridIndexes.y);
                        oppositDirection = 2;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Up");
                        break;

                    case 2:
                        zoneToCheck = new Coord(zoneGridIndexes.x - 1, zoneGridIndexes.y);
                        oppositDirection = 1;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Down");
                        break;

                    case 3:
                        zoneToCheck = new Coord(zoneGridIndexes.x, zoneGridIndexes.y + 1);
                        oppositDirection = 4;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Right");
                        break;

                    case 4:
                        zoneToCheck = new Coord(zoneGridIndexes.x, zoneGridIndexes.y - 1);
                        oppositDirection = 3;
                        Debug.Log("Checking floor " + zoneToCheck.x + "/ zone " + zoneToCheck.y + ". Direction : Left");
                        break;
                    default:
                        Debug.LogError("There is no such direction as : " + directionToCheck);
                        break;
                }

                if (zoneToCheck.x < towerHeight && zoneToCheck.x >= 0 && zoneToCheck.y < towerWidth && zoneToCheck.y >= 0)
                {
                    if (towerGrid[zoneToCheck.x, zoneToCheck.y] == null)
                    {
                        Debug.Log("Zone " + zoneToCheck + " is empty");
                        if (!checkedZones.Contains(zoneToCheck))
                        {
                            Debug.Log("Zone " + zoneToCheck + " has been already checked");
                            if (this.checkedZones.Contains(zoneToCheck) || !IsNextZoneBlocking(zoneToCheck, oppositDirection, ref checkedZones))
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
                            if(towerGrid[zoneToCheck.x, zoneToCheck.y].openings[i] && i == oppositDirection - 1)
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

    private GameObject CreateTile(Coord zoneToCreate)
    {
        GameObject roomInstantiated = null;
        if(zoneToCreate.x == 0)
        {
            roomInstantiated = Instantiate(fillerPrefab, Coord.ZoneToTowerPos(new Coord(zoneToCreate.x - 1, zoneToCreate.y), this), Quaternion.identity, levelHolder.transform);
            roomInstantiated.name = fillerPrefab.name + " under tower >  " + (zoneToCreate.x - 1) + " / " + zoneToCreate.y;
        }

        if (towerGrid[zoneToCreate.x, zoneToCreate.y] != null)
        {
            roomInstantiated = Instantiate(towerGrid[zoneToCreate.x, zoneToCreate.y].partPrefab, Coord.ZoneToTowerPos(zoneToCreate, this), Quaternion.identity, levelHolder.transform);
            roomInstantiated.name = towerGrid[zoneToCreate.x, zoneToCreate.y].partPrefab.name + "  >  " + zoneToCreate.x + " / " + zoneToCreate.y;
        }
        else
        {
            roomInstantiated = Instantiate(fillerPrefab, Coord.ZoneToTowerPos(zoneToCreate, this), Quaternion.identity, levelHolder.transform);
            roomInstantiated.name = fillerPrefab.name + "  >  " + zoneToCreate.x + " / " + zoneToCreate.y;
        }
        accessibleZones.Remove(Coord.GetZone(accessibleZones, zoneToCreate.x, zoneToCreate.y));
        emptyZones.Remove(Coord.GetZone(emptyZones, zoneToCreate.x, zoneToCreate.y));
        Debug.Log("Creating tile at " + zoneToCreate.x + ", " + zoneToCreate.y + ". Removed from accessibleZones. " + accessibleZones.Count + " accessibleZones remaining");
        return roomInstantiated;
    }

    private void CreateBorder(Coord borderZone)
    {
        Instantiate(borderZone.y == 0 ? leftTowerWall : rightTowerWall, Coord.ZoneToTowerPos(borderZone, this), Quaternion.identity, levelHolder.transform);
    }

    private void FillEmptyZones()
    {
        for (int floorNumber = 0; floorNumber < towerHeight; floorNumber++)
        {
            for (int zoneNumber = 0; zoneNumber < towerWidth; zoneNumber++)
            {
                if (towerGrid[floorNumber, zoneNumber] == null)
                {
                    CreateTile(new Coord(floorNumber, zoneNumber));

                    if (zoneNumber == 0 && !leftEdgeChosen)
                    {
                        CreateBorder(new Coord(floorNumber, zoneNumber));
                    }
                    else if (zoneNumber == towerWidth - 1 && !rightEdgeChosen)
                    {
                        CreateBorder(new Coord(floorNumber, zoneNumber));
                    }
                }
            }
        }
    }

    private void PlaceFirstRoom()
    {
        Room firstRoom = startRoomList[Random.Range(0, startRoomList.Count)];
        RoomHandler startRoomHandler = new RoomHandler(firstRoom, startPositionIndexes.x, startPositionIndexes.y);
        towerRooms[startPositionIndexes.x, startPositionIndexes.y] = startRoomHandler;
        startRoomHandler.zonesCenterPos.Add(Coord.ZoneToTowerPos(startPositionIndexes, this));
        towerGrid[startPositionIndexes.x, startPositionIndexes.y] = firstRoom.roomParts[0, 0];
        CreateTile(startPositionIndexes);
        nextRoomFinished = true;
        roomPlacementSuccess = true;
        accessibleZones = emptyZones;
    }

    private void ReArrangeRooms(List<List<Room>> listOflistOfRoom)
    {
        foreach(List<Room> listOfRoom in listOflistOfRoom)
        {
            foreach(Room room in listOfRoom)
            {
                room.Rearrange();
            }
        }
    }

    private void FillRoomList(List<Room> listOfRoom)
    {
        foreach(Room room in listOfRoom)
        {
            room.allRooms.AddRange(roomList);
            room.allRooms.AddRange(deadEndList);
            room.allRooms.AddRange(leftEdgeList);
            room.allRooms.AddRange(rightEdgeList);
            room.allRooms.AddRange(endRoomList);
            room.allRooms.AddRange(yokaiRoomList);
            room.allRooms.AddRange(startRoomList);
        }
    }

    private void PLaceHealthTerminal()
    {
        bool[] roomFilledWithHT = new bool[GameData.levelHandler.allTowerRooms.Count];
        while (healthTerminalPlaced <= healthTerminalNumbers)
        {
            bool placed = false;
            while(!placed)
            {
                int rand = Random.Range(0, GameData.levelHandler.allTowerRooms.Count);
                if(!roomFilledWithHT[rand] && GameData.levelHandler.allTowerRooms[rand].healTerminal != null)
                {
                    GameData.levelHandler.allTowerRooms[rand].healTerminal.SetActive(true);
                    placed = true;
                    healthTerminalPlaced++;
                    Debug.Log("Health terminal placed at " + GameData.levelHandler.allTowerRooms[rand].center);
                }
            }
        }
    }

    public class ZoneGizmo
    {
        public Coord zone;
        public Color color;
        public float remainingTime;

        public ZoneGizmo(Coord _zone, Color _color, float _time)
        {
            zone = _zone;
            color = _color;
            remainingTime = _time;
        }

        /// <summary>
        /// Draw a gizmo for a zone at a specified color and during a specified time
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="color">0 for blue, 1 for red, 2 for green</param>
        /// <returns></returns>
        public static void Draw(Coord zone, Color color, float appearingTime, LevelBuilder level)
        {
            level.currentlyDrawnZoneGismos.Add(new ZoneGizmo(zone, color, appearingTime));
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = currentlyDrawnZoneGismos.Count - 1; i >= 0; i--)
        {
            if(currentlyDrawnZoneGismos[i].remainingTime > 0)
            {
                Gizmos.color = currentlyDrawnZoneGismos[i].color;
                Gizmos.DrawCube(Coord.ZoneToTowerPos(currentlyDrawnZoneGismos[i].zone, this), new Vector2(tileLength, tileLength));
                currentlyDrawnZoneGismos[i].remainingTime -= Time.deltaTime;
            }
            else
            {
                currentlyDrawnZoneGismos.Remove(currentlyDrawnZoneGismos[i]);
            }
        }
    }
}