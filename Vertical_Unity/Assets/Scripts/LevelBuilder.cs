using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [Header("Generation settings")]
    public int towerWidth;
    public int towerHeight;
    public Vector2 startPositionIndexes;
    public List<Room> roomList;
    public List<Room> deadEndList;
    public GameObject fillerPrefab;
    [Space]
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
    private List<Vector2> freeOpeningsUnusable = new List<Vector2>();
    private Vector2 nextRoomStart;
    private int nextRoomOpeningDirection;
    private int roomBuiltNumber;

    private bool levelBuilt;
    private bool levelScanned;
    private AstarPath astarPath;

    public enum Direction { Up, Down, Right, Left };

    private void Awake()
    {
        levelBuilt = false;
        levelScanned = false;
        ReArrangeRooms();
        BuildLevel();
    }

    private void Update()
    {
        if(levelBuilt && !levelScanned)
        {
            levelScanned = true;
            AstarPath.active.Scan();
        }
    }

    public bool BuildLevel()
    {
        bool towerCorrectlyBuild = false;
        roomBuiltNumber = 0;
        towerGrid = new Room.RoomPart[towerHeight, towerWidth];
        zoneChecked = new bool[towerHeight,towerWidth];

        Debug.Log("Beginning Tower creation");
        nextRoomStart = startPositionIndexes;
        nextRoomOpeningDirection = 3;
        bool openingRemaining = true;

        do
        {
            Debug.Log("Start building room number " + (roomBuiltNumber + 1));
            if(!PlaceNextRoom(nextRoomStart, nextRoomOpeningDirection))
            {
                freeOpeningsUnusable.Add(nextRoomStart);
                Debug.Log("The tile at " + nextRoomStart.x + ", " + nextRoomStart.y + " cannot be used as a connection. Added to unusable list");
            }
            else
            {
                roomBuiltNumber++;
            }

            openingRemaining = FindNextOpening();
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

        StartCoroutine(CreateTower());

        return towerCorrectlyBuild;
    }

    public bool PlaceNextRoom(Vector2 gridIndexes, int openingDirection)
    {
        bool[] roomsTested = new bool[roomList.Count];
        bool[] deadEndsTested = new bool[deadEndList.Count];
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
                if(!isDeadEnd)
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
                            selectedRoom = potentialRoom;
                            Debug.Log("Room found ! The room " + selectedRoom.name + " has all needed features. Placed at Floor " + (int)gridIndexes.x + " Zone " + (int)gridIndexes.y);
                        }
                    }
                }
            }
        }


        if (selectedRoom != null)
        {
            for (int floorNumber = 0; floorNumber < selectedRoom.roomParts.GetLength(0); floorNumber++)
            {
                for (int zoneNumber = 0; zoneNumber < selectedRoom.roomParts.GetLength(1); zoneNumber++)
                {
                    Vector2 relativeIndexes = new Vector2(floorNumber - connectedPartIndexes.x, zoneNumber - connectedPartIndexes.y);
                    if (selectedRoom.roomParts[floorNumber, zoneNumber] != null)
                    {
                        towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] = selectedRoom.roomParts[floorNumber, zoneNumber];
                        Debug.Log(selectedRoom.name + " part placed on " + (int)gridIndexes.x + (int)relativeIndexes.x + ", " + (int)gridIndexes.y + (int)relativeIndexes.y);
                    }
                    else
                    {
                        towerGrid[(int)gridIndexes.x + (int)relativeIndexes.x, (int)gridIndexes.y + (int)relativeIndexes.y] = null;
                        Debug.Log(selectedRoom.name + " hole placed on " + (int)gridIndexes.x + (int)relativeIndexes.x + ", " + (int)gridIndexes.y + (int)relativeIndexes.y);
                    }
                }
            }
        }

        return !noMoreRoomAvailable;
    }

    public bool FindNextOpening()
    {
        bool openingAvailable = false;
        Vector2 openingIndexes = Vector2.zero;
        int openingDirection = -1;
        int lookedFloor = 0;
        while(!openingAvailable && lookedFloor < towerHeight)
        {
            Debug.Log("Checking floor " + lookedFloor + " for openings");
            int lookedZone = 0;
            while (!openingAvailable && lookedZone < towerWidth)
            {
                if (towerGrid[lookedFloor, lookedZone] != null && !zoneChecked[lookedFloor, lookedZone])
                {
                    Debug.Log("Looking zone " + lookedZone);
                    Room.RoomPart lookedPart = towerGrid[lookedFloor, lookedZone];
                    if (lookedPart.openings.Length > 0)
                    {
                        Debug.Log("Openings found on Floor " + lookedFloor + " at Zone " + lookedZone);
                        bool freeOpeningsOnZone = false;
                        int lookedOpening = 0;
                        while (!openingAvailable && lookedOpening < 4)
                        {
                            if (lookedPart.openings[lookedOpening])
                            {
                                switch (lookedOpening)
                                {
                                    case 0:
                                        if ((lookedFloor + 1) < towerHeight)
                                        {
                                            if (towerGrid[lookedFloor + 1, lookedZone] == null
                                            && !freeOpeningsUnusable.Contains(new Vector2(lookedFloor + 1, lookedZone)))
                                            {
                                                openingAvailable = true;
                                                openingDirection = 1;
                                                openingIndexes = new Vector2(lookedFloor + 1, lookedZone);
                                                Debug.Log("Free opening found on Zone " + lookedZone + ", floor " + lookedFloor + ". Direction : Up");
                                            }
                                        }
                                        break;

                                    case 1:
                                        if ((lookedFloor - 1) >= 0)
                                        {
                                            if (towerGrid[lookedFloor - 1, lookedZone] == null
                                                && !freeOpeningsUnusable.Contains(new Vector2(lookedFloor - 1, lookedZone)))
                                            {
                                                openingAvailable = true;
                                                openingDirection = 0;
                                                openingIndexes = new Vector2(lookedFloor - 1, lookedZone);
                                                Debug.Log("Free opening found on Zone " + lookedZone + ", floor " + lookedFloor + ". Direction : Down");
                                            }
                                        }
                                        break;

                                    case 2:
                                        if ((lookedZone + 1) < towerWidth)
                                        {
                                            if (towerGrid[lookedFloor, lookedZone + 1] == null
                                                && !freeOpeningsUnusable.Contains(new Vector2(lookedFloor, lookedZone + 1)))
                                            {
                                                openingAvailable = true;
                                                openingDirection = 3;
                                                openingIndexes = new Vector2(lookedFloor, lookedZone + 1);
                                                Debug.Log("Free opening found on Zone " + lookedZone + ", floor " + lookedFloor + ". Direction : Right");
                                            }
                                        }
                                        break;

                                    case 3:
                                        if ((lookedZone - 1) >= 0)
                                        {
                                            if (towerGrid[lookedFloor, lookedZone - 1] == null
                                            && !freeOpeningsUnusable.Contains(new Vector2(lookedFloor, lookedZone - 1)))
                                            {
                                                openingAvailable = true;
                                                openingDirection = 2;
                                                openingIndexes = new Vector2(lookedFloor, lookedZone - 1);
                                                Debug.Log("Free opening found on Zone " + lookedZone + ", floor " + lookedFloor + ". Direction : Left");
                                            }
                                        }
                                        break;
                                    default:
                                        Debug.Log("The opening direction found does correspond to any known");
                                        break;
                                }

                                if (!openingAvailable)
                                {
                                    Debug.Log("The opening found at " + lookedFloor + ", " + lookedZone + " is blocked");
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
                            zoneChecked[lookedFloor, lookedZone] = true;
                            Debug.Log("Zone " + lookedFloor + ", " + lookedZone + " fully blocked : added to skipList");
                        }
                    }
                    else
                    {
                        Debug.Log("No openings on Zone " + lookedFloor + ", " + lookedZone + " : added to skipList");
                        zoneChecked[lookedFloor, lookedZone] = true;
                    }
                }
                else
                {
                    //Debug.Log(" Zone " + lookedZone + " Floor " + lookedFloor + " is empty");
                }
                lookedZone++;
            }
            lookedFloor++;
        }

        if(openingAvailable)
        {
            nextRoomStart = openingIndexes;
            nextRoomOpeningDirection = openingDirection;
        }
        else
        {
            Debug.Log("No free opening Available. Mission failed :,(");
        }

        return openingAvailable;
    }

    /// <summary>
    /// Return the direction of a free opening find with a roompart at a specified tile. Return -1 if no free opening is found
    /// </summary>
    /// <param name="part">The roompart which will be checked</param>
    /// <param name="floorToCheck">The floor number where the part will be checked</param>
    /// <param name="zoneToCheck">The zone number where the part will be checked</param>
    /// <returns></returns>
    public int CheckFreeOpening(Room.RoomPart part ,int floorToCheck, int zoneToCheck)
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
                        Debug.Log("The opening direction found does correspond to any known");
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

    public IEnumerator CreateTower()
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

    public void CreateTile(int gridFloor, int gridZone)
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
    }
}
