using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler
{
    public Room originRoom;
    public List<Vector2> zonesCenterPos;
    public bool discovered;
    public List<EnemyHandler> currentEnemies;
    public List<Doors> doors;
    public Vector2 center;

    public RoomHandler(Room _originRoom, int floorNumber, int zoneNumber)
    {
        originRoom = _originRoom;
        zonesCenterPos = new List<Vector2>();
        discovered = false;
        currentEnemies = new List<EnemyHandler>();
        doors = new List<Doors>();
    }

    public void Pause()
    {
        foreach (EnemyHandler enemy in currentEnemies)
        {
            enemy.gameObject.SetActive(false);
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

    public void RemoveEnemy(EnemyHandler enemy)
    {
        currentEnemies.Remove(enemy);
        UpdateRoomLockState();
    }

    public void SetCenter(Vector2 bottomLeftZoneCenter)
    {
        center = bottomLeftZoneCenter;
        UpdateRoomLockState();
    }

    public void UpdateRoomLockState()
    {
        if (currentEnemies.Count == 0)
        {
            foreach (Doors door in doors)
            {
                door.isLocked = false;
            }
        }
    }
}
