using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler
{
    public Room originRoom;
    public List<Vector2> zonesCenterPos;
    public bool discovered;
    public List<EnemyHandler> currentEnemies;
    public Vector2 center;

    public RoomHandler(Room _originRoom, int floorNumber, int zoneNumber)
    {
        originRoom = _originRoom;
        zonesCenterPos = new List<Vector2>();
        discovered = false;
        currentEnemies = new List<EnemyHandler>();
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
    }

    public void SetCenter(Vector2 bottomLeftZoneCenter)
    {
        center = bottomLeftZoneCenter;
    }
}
