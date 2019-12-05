using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public string name;
    public List<Floor> floors;
    [HideInInspector]
    public RoomPart[,] roomParts;

    public void Rearrange()
    {
        roomParts = new RoomPart[floors.Count, floors[0].roomPartsByFloor.Count];
        int floorIndex = 0;
        foreach(Floor floor in floors)
        {
            int partIndex = 0;
            foreach(RoomPart part in floor.roomPartsByFloor)
            {
                if(part.partPrefab != null)
                {
                    roomParts[floorIndex, partIndex] = part;
                }
                else
                {
                    roomParts[floorIndex, partIndex] = null;
                }
                partIndex++;
            }
            floorIndex++;
        }
    }

    [System.Serializable]
    public class Floor
    {
        public List<RoomPart> roomPartsByFloor;
    }

    [System.Serializable]
    /// <summary>
    /// 0 > Up,  1 > Down,  2 > Right,  3 > Left
    /// </summary>
    public class RoomPart
    {
        public GameObject partPrefab;
        public bool[] openings = new bool[4];
        [HideInInspector]
        public int roomIndex;
    }
}
