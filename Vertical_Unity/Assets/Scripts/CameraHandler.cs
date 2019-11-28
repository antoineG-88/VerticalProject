using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Follow settings")]
    public float edgePointOffset;
    [Header("Debug settings")]
    public bool displayDebugs;
    public List<GameObject> edges;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        OffsetForCamera(transform.position, new List<Vector2>(), 5);
    }

    private Vector2 OffsetForCamera(Vector2 cameraPos, List<Vector2> zonePositions, float zoneWidth)
    {
        Vector2 offset = Vector2.zero;
        List<Vector2> edgePoints = new List<Vector2>
        {
            new Vector2(mainCamera.transform.position.x + (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), mainCamera.transform.position.y + (mainCamera.orthographicSize - edgePointOffset)), // 1 : Haut droite
            new Vector2(mainCamera.transform.position.x - (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), mainCamera.transform.position.y + (mainCamera.orthographicSize - edgePointOffset)), // 2 : Haut gauche
            new Vector2(mainCamera.transform.position.x + (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), mainCamera.transform.position.y - (mainCamera.orthographicSize - edgePointOffset)), // 3 : Bas droite
            new Vector2(mainCamera.transform.position.x - (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), mainCamera.transform.position.y - (mainCamera.orthographicSize - edgePointOffset))  // 4 : Bas gauche
        };

        if(displayDebugs)
        {
            for (int i = 0; i < 4; i++)
            {
                edges[i].transform.position = edgePoints[i];
            }
        }

        Vector2[] edgeOffset = new Vector2[4];

        for (int i = 0; i < 4; i++)
        {
            edgeOffset[i] = OffsetFromZone(edgePoints[i], zonePositions, zoneWidth);
        }

        return offset;
    }

    private Vector2 OffsetFromZone(Vector2 point, List<Vector2> zonePositions, float zoneWidth)
    {
        return Vector2.zero;
    }
}
