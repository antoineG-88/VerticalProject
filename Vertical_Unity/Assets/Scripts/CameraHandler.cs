using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Follow settings")]
    public bool useWallAvoidance;
    public Vector2 followCenterOffset;
    [Range(0.0f,1f)] public float baseLerpSpeed;
    public float edgePointOffset;
    public float aimOffsetLength;
    [Header("Debug settings")]
    public float roomWidth;
    [Header("Debug settings")]
    public bool displayDebugs;
    public List<GameObject> edges;
    public List<Transform> roomsPos;

    private Camera mainCamera;
    private Vector2 cameraTarget;
    private Vector2 cameraFinalPos;
    [HideInInspector] public bool followPlayer;

    private List<Vector2> rooms = new List<Vector2>();

    private void Start()
    {
        mainCamera = Camera.main;
        followPlayer = true;
        foreach (Transform roomPos in roomsPos)
        {
            rooms.Add(roomPos.position);
        }
    }

    private void FixedUpdate()
    {
        UpdateCameraTarget();

        MoveCamera(cameraFinalPos);
    }

    private Vector2 OffsetForCamera(Vector2 targetPos, List<Vector2> zonePositions, float zoneWidth)
    {
        Vector2 offset = Vector2.zero;
        List<Vector2> edgePoints = new List<Vector2>
        {
            new Vector2(targetPos.x + (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), targetPos.y + (mainCamera.orthographicSize - edgePointOffset)), // 1 : Haut droite
            new Vector2(targetPos.x - (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), targetPos.y + (mainCamera.orthographicSize - edgePointOffset)), // 2 : Haut gauche
            new Vector2(targetPos.x + (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), targetPos.y - (mainCamera.orthographicSize - edgePointOffset)), // 3 : Bas droite
            new Vector2(targetPos.x - (mainCamera.orthographicSize * 16 / 9 - edgePointOffset), targetPos.y - (mainCamera.orthographicSize - edgePointOffset))  // 4 : Bas gauche
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
            edgeOffset[i] = OffsetFromZones(edgePoints[i], zonePositions, zoneWidth);
        }

        float horizontalOffset = (edgeOffset[0].x == edgeOffset[2].x && edgeOffset[0].x != 0 ? edgeOffset[0].x : 0) + (edgeOffset[1].x == edgeOffset[3].x && edgeOffset[1].x != 0 ? edgeOffset[1].x : 0);
        float verticalOffset = (edgeOffset[0].y == edgeOffset[1].y && edgeOffset[1].y != 0 ? edgeOffset[0].y : 0) + (edgeOffset[2].y == edgeOffset[3].y && edgeOffset[2].y != 0 ? edgeOffset[0].y : 0);

        offset = new Vector2(horizontalOffset, verticalOffset);

        if (displayDebugs)
            Debug.DrawLine(transform.position, (Vector2)transform.position - offset);

        return offset;
    }

    private Vector2 OffsetFromZones(Vector2 point, List<Vector2> zonePositions, float zoneWidth)
    {
        Vector2 offset = Vector2.zero;
        Vector2[] zoneOffsets = new Vector2[zonePositions.Count];
        bool offsetted = true;
        int i = 0;
        while(offsetted && i < zonePositions.Count)
        {
            zoneOffsets[i] = Vector2.zero;
            zoneOffsets[i] = point.x > zonePositions[i].x ?
                point.x > zonePositions[i].x + zoneWidth / 2 ? new Vector2((zonePositions[i].x + zoneWidth / 2) - point.x, zoneOffsets[i].y) : zoneOffsets[i]
                : point.x < zonePositions[i].x - zoneWidth / 2 ? new Vector2((zonePositions[i].x - zoneWidth / 2) - point.x, zoneOffsets[i].y) : zoneOffsets[i];

            zoneOffsets[i] = point.y > zonePositions[i].y ?
                point.y > zonePositions[i].y + zoneWidth / 2 ? new Vector2(zoneOffsets[i].x, (zonePositions[i].y + zoneWidth / 2) - point.y) : zoneOffsets[i]
                : point.y < zonePositions[i].y - zoneWidth / 2 ? new Vector2(zoneOffsets[i].x, (zonePositions[i].y - zoneWidth / 2) - point.y) : zoneOffsets[i];

            if(zoneOffsets[i] == Vector2.zero)
            {
                offsetted = false;
            }
            i++;
        }

        if(offsetted)
        {
            float minOffset = -1;
            foreach(Vector2 zoneOffset in zoneOffsets)
            {
                if(minOffset < 0 || zoneOffset.magnitude < minOffset)
                {
                    minOffset = zoneOffset.magnitude;
                    offset = zoneOffset;
                }
            }

            if (displayDebugs)
                Debug.DrawLine(point, point + offset);
        }

        return offset;
    }

    private void MoveCamera(Vector2 targetCameraPos)
    {
        Vector2 lerpPos = Vector2.Lerp(mainCamera.transform.position, targetCameraPos, baseLerpSpeed);
        mainCamera.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10.0f);
    }

    private void UpdateCameraTarget()
    {
        if (followPlayer)
        {
            cameraTarget = (Vector2)GameData.playerMovement.transform.position + followCenterOffset + CameraAimOffset(GameData.playerGrapplingHandler.aimDirection);
            cameraFinalPos = useWallAvoidance ? cameraTarget + OffsetForCamera(cameraTarget, rooms, roomWidth) : cameraTarget;
            if (displayDebugs)
                Debug.DrawLine(cameraTarget, cameraTarget + Vector2.up, Color.red);
        }
    }

    private Vector2 CameraAimOffset(Vector2 aimDirection)
    {
        Vector2 offset = Vector2.zero;

        offset = new Vector2(aimDirection.x, - aimDirection.y) * aimOffsetLength;

        return offset;
    }
}
