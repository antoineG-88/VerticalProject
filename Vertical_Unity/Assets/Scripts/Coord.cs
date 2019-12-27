using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Coord
{
    public int x;
    public int y;

    public Coord(int xGridPos, int yGridPos)
    {
        x = xGridPos;
        y = yGridPos;
    }

    /// <summary>
    /// Return the union of two list, all items common in both lists
    /// </summary>
    /// <param name="coordList1">The tiniest list</param>
    /// <param name="coordList2">The bigger list</param>
    /// <returns></returns>
    public static List<Coord> GetListUnion(List<Coord> coordList1, List<Coord> coordList2)
    {
        List<Coord> unionList = new List<Coord>();

        foreach (Coord list1Zone in coordList1)
        {
            if (Coord.GetZone(coordList2, list1Zone.x, list1Zone.y) != null)
            {
                unionList.Add(list1Zone);
            }
        }

        return unionList;
    }

    public static List<Coord> CreateFullZoneGrid(int towerH, int towerW)
    {
        List<Coord> zones = new List<Coord>();

        for (int i = 0; i < towerH; i++)
        {
            for (int t = 0; t < towerW; t++)
            {
                zones.Add(new Coord(i, t));
            }
        }

        return zones;
    }

    /// <summary>
    /// Return the zone from the list with the same coordinates, if not found return null
    /// </summary>
    /// <param name="zones">The list where the zone will be searched</param>
    /// <param name="floor">x coordinate in the tower</param>
    /// <param name="zone">y coordinate in the tower</param>
    /// <returns></returns>
    public static Coord GetZone(List<Coord> zones, int floor, int zone)
    {
        Coord zoneFound = null;
        int i = 0;
        while (i < zones.Count && zoneFound == null)
        {
            if (zones[i].x == floor && zones[i].y == zone)
            {
                zoneFound = zones[i];
                //Debug.Log("GetZone() successfull : Zone found at " + i);
                break;
            }
            i++;
        }

        if (zoneFound == null)
        {
            //Debug.Log("The zone " + floor + ", " + zone + " is not in the list ");
        }

        return zoneFound;
    }

    public static Vector2 ZoneToTowerPos(Coord zone, LevelBuilder level)
    {
        Vector2 pos = Vector2.zero;

        Vector2 originGridPos = new Vector2(level.bottomCenterTowerPos.x - level.towerWidth * level.tileLength / 2 + level.tileLength / 2, level.bottomCenterTowerPos.y + level.tileLength / 2);

        pos = new Vector2(originGridPos.x + zone.y * level.tileLength, originGridPos.y + zone.x * level.tileLength);

        return pos;
    }

    public void Set(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public override string ToString()
    {
        return x + ", " + y;
    }

    public static Coord operator +(Coord a, Coord b)
        => new Coord(a.x + b.x, a.y + b.y);
}
