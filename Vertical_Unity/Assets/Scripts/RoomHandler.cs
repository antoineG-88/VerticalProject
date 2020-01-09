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

    public bool islocked;

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
        if (currentEnemies.Count == 0 && islocked)
        {
            islocked = false;
        }
        else if (!islocked && currentEnemies.Count > 0)
        {
            islocked = true;
        }


        foreach (RoomDoors door in doors)
        {
            door.doors.isLocked = islocked;
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
                        if (enemy != null && !currentEnemies.Contains(enemy))
                        {
                            currentEnemies.Add(enemy);
                            enemy.room = this;
                            if(originRoom.name == "Le DR 1")
                            {
                                Debug.Log("Added " + enemy.gameObject.name + " > " + currentEnemies.Count);
                            }
                        }
                    }
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
