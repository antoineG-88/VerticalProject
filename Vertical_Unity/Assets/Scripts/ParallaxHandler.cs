using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxHandler : MonoBehaviour
{
    public Vector2 startOffset;
    public GameObject[] layerObjects;
    public float[] layerRelativeHorizontalSpeed;
    public float[] layerRelativeVerticalSpeed;

    [HideInInspector] public float horizontalLevelMultiplier;
    [HideInInspector] public float verticalLevelMultiplier;
    private Transform mainCamera;
    private Vector2 originPos;
    private Vector2[] relativeLayerStartPos = new Vector2[8];

    void Start()
    {
        mainCamera = GameData.cameraHandler.transform;
        SetNewOrigin(mainCamera.position);
    }

    void Update()
    {
        for(int i = 0; i < layerObjects.Length; i++)
        {
            layerObjects[i].transform.position = new Vector2(relativeLayerStartPos[i].x + (mainCamera.position.x - originPos.x) * layerRelativeHorizontalSpeed[i], relativeLayerStartPos[i].y + (mainCamera.position.y - originPos.y) * layerRelativeVerticalSpeed[i]);
        }
    }

    public void SetNewOrigin(Vector2 origin)
    {
        originPos = origin;
        for (int i = 0; i < layerObjects.Length; i++)
        {
            relativeLayerStartPos[i] = new Vector2(originPos.x, originPos.y) + startOffset;
        }
    }
}
