using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxHandler : MonoBehaviour
{
    public Vector2 startOffset;
    public Vector2 elementsStartOffset;
    public int layerNumber;
    public GameObject[] layerObjects;
    public GameObject[] layerDuplicates;
    public GameObject[] verticalElements;
    public float[] elementsMovingSpeed;
    public Vector2[] vElemVerticalInterval;
    public Vector2[] vElemHorizontalInterval;
    public Vector2[] layerRelativeSpeed;
    public float spriteWidth;

    [HideInInspector] public float horizontalLevelMultiplier;
    [HideInInspector] public float verticalLevelMultiplier;
    private Transform mainCamera;
    private Vector2 originPos;
    private Vector2[] layerRelativeStartPos;
    private List<GameObject>[] elements;
    private List<Vector2>[] elementsPrefabPos;

    void Start()
    {
        mainCamera = GameData.cameraHandler.transform;
        layerRelativeStartPos = new Vector2[layerNumber];
        SetNewOrigin((Vector2)mainCamera.position + startOffset);
        elements = new List<GameObject>[layerNumber];

        for (int i = 0; i < layerNumber; i++)
        {
            elements[i] = new List<GameObject>();
        }

        elementsPrefabPos = new List<Vector2>[layerNumber];
        for (int i = 0; i < layerNumber; i++)
        {
            elementsPrefabPos[i] = new List<Vector2>();
        }

        CreateElements();
    }

    void Update()
    {
        if(GameData.levelBuilder != null)
        {
            if(GameData.levelBuilder.towerCreated)
            {
                UpdateLayersPos();

                UpdateElementsMovement();
            }
        }
        else
        {
            UpdateLayersPos();

            UpdateElementsMovement();
        }
    }

    public void SetNewOrigin(Vector2 origin)
    {
        originPos = origin;
        for (int i = 0; i < layerNumber; i++)
        {
            layerRelativeStartPos[i] = originPos + startOffset;
        }
    }

    private void UpdateLayersPos()
    {
        for (int i = 0; i < layerNumber; i++)
        {
            layerObjects[i].transform.position = new Vector2(layerRelativeStartPos[i].x + (mainCamera.position.x - originPos.x) * layerRelativeSpeed[i].x, layerRelativeStartPos[i].y + (mainCamera.position.y - originPos.y) * layerRelativeSpeed[i].y);

            if ((layerRelativeStartPos[i] - (Vector2)mainCamera.transform.position).x > 0)
            {
                layerDuplicates[i].transform.localPosition = new Vector2(-spriteWidth, layerDuplicates[i].transform.localPosition.y);
            }
            else
            {
                layerDuplicates[i].transform.localPosition = new Vector2(spriteWidth, layerDuplicates[i].transform.localPosition.y);
            }

            if (layerObjects[i].transform.position.x + layerRelativeStartPos[i].x - mainCamera.transform.position.x < -spriteWidth)
            {
                layerRelativeStartPos[i].x += spriteWidth;
            }
            else if (layerObjects[i].transform.position.x + layerRelativeStartPos[i].x - mainCamera.transform.position.x > spriteWidth)
            {
                layerRelativeStartPos[i].x -= spriteWidth;
            }
        }
    }

    private void CreateElements()
    {
        float height = 0;
        if(GameData.levelBuilder != null)
        {
            height = GameData.levelBuilder.towerHeight * GameData.levelBuilder.tileLength;
        }
        else
        {
            height = 200;
        }

        for (int i = 0; i < layerNumber; i++)
        {
            if (verticalElements[i] != null)
            {
                float heightReached = 0;
                while (heightReached < height * (1 -layerRelativeSpeed[i].y))
                {
                    float addedHeight = Random.Range(vElemVerticalInterval[i].x, vElemVerticalInterval[i].y);
                    GameObject newElement = Instantiate(verticalElements[i], new Vector2(Random.Range(vElemHorizontalInterval[i].x, vElemHorizontalInterval[i].y), heightReached + addedHeight) + elementsStartOffset, Quaternion.identity, layerObjects[i].transform);
                    //bool isFlipped = Random.Range(0, 2) == 0 ? true : false;
                    bool isFlipped = false;
                    newElement.GetComponent<SpriteRenderer>().flipX = isFlipped;
                    if (isFlipped)
                        newElement.transform.localPosition += new Vector3(-verticalElements[i].transform.position.x * 2, 0);
                    heightReached += addedHeight;
                    elementsPrefabPos[i].Add(!isFlipped ? verticalElements[i].transform.position : -verticalElements[i].transform.position);
                    elements[i].Add(newElement);
                    newElement.GetComponent<SpriteRenderer>().sortingOrder = layerObjects[i].GetComponentInChildren<SpriteRenderer>().sortingOrder;
                }
            }
        }
    }

    private void UpdateElementsMovement()
    {
        for (int i = 0; i < layerNumber; i++)
        {
            if (verticalElements[i] != null)
            {
                for (int y = 0; y < elements[i].Count; y++)
                {
                    elements[i][y].transform.position += new Vector3(elementsMovingSpeed[i] * Time.deltaTime, 0, 0);

                    if (elementsMovingSpeed[i] > 0 && (elements[i][y].transform.position.x + elementsPrefabPos[i][y].x - layerObjects[i].transform.position.x) > spriteWidth / 2)
                    {
                        elements[i][y].transform.position -= new Vector3(spriteWidth, 0, 0);
                    }
                    else if (elementsMovingSpeed[i] < 0 && (elements[i][y].transform.position.x + elementsPrefabPos[i][y].x - layerObjects[i].transform.position.x) < -(spriteWidth / 2))
                    {
                        elements[i][y].transform.position += new Vector3(spriteWidth, 0, 0);
                    }
                }
            }
        }
    }
}