using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler
{
    public Room originRoom;
    public List<Vector2> zonesCenterPos;
    public bool discovered;
    public List<EnemyHandler> currentEnemies;
    public List<RoomDoors> doors;
    public GameObject healTerminal;
    public Vector2 center;
    public List<GameObject> roomParts;

    public RoomHandler(Room _originRoom, int floorNumber, int zoneNumber)
    {
        originRoom = _originRoom;
        zonesCenterPos = new List<Vector2>();
        discovered = false;
        currentEnemies = new List<EnemyHandler>();
        doors = new List<RoomDoors>();
        roomParts = new List<GameObject>();
    }

    public void Pause()
    {
        foreach (EnemyHandler enemy in currentEnemies)
        {
            if(enemy != null)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    public void Play()
    {
        foreach (EnemyHandler enemy in currentEnemies)
        {
            enemy.gameObject.SetActive(true);
            enemy.provoked = false;
        }
    }

    public void Activate()
    {
        foreach (GameObject part in roomParts)
        {
            if (part != null)
                part.SetActive(true);
        }
    }

    public void DeActivate()
    {
        foreach (GameObject part in roomParts)
        {
            if(part != null)
                part.SetActive(false);
        }
    }

    public void RemoveEnemy(EnemyHandler enemy)
    {
        currentEnemies.Remove(enemy);
    }

    public void SetCenter(Vector2 bottomLeftZoneCenter)
    {
        center = bottomLeftZoneCenter;
    }

    public void UpdateRoomLockState()
    {
        if (currentEnemies.Count == 0)
        {
            foreach (RoomDoors door in doors)
            {
                door.doors.isLocked = false;
            }
        }
        else
        {
            foreach (RoomDoors door in doors)
            {
                door.doors.isLocked = true;
            }
        }
    }

    public void InitializeRoomObjects()
    {
        foreach (GameObject newRoomObject in roomParts)
        {
            for (int i = 0; i < newRoomObject.transform.childCount; i++)
            {
                Transform child = newRoomObject.transform.GetChild(i);
                if (child.name == "Enemies")
                {
                    for (int t = 0; t < child.childCount; t++)
                    {
                        EnemyHandler enemy = child.GetChild(t).GetComponent<EnemyHandler>();
                        if (enemy != null)
                        {
                            currentEnemies.Add(enemy);
                            enemy.room = this;
                        }
                        else
                        {
                            Debug.LogError("No EnemyHandler script found on " + child.GetChild(t).name);
                        }
                    }
                }
                else if (child.name == "FinalRing")
                {
                    GameData.levelHandler.finalRing = child.gameObject;
                }
                else if (child.name == "HealTerminal")
                {
                    healTerminal = child.gameObject;
                    healTerminal.SetActive(false);
                }
            }
        }
    }

    public class RoomDoors
    {
        public Doors doors;
        public int direction;

        public RoomDoors(Doors _doors, int _direction)
        {
            doors = _doors;
            direction = _direction;
        }
    }
}
