using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformConnection : MonoBehaviour
{
    [HideInInspector] public PlatformHandler attachedPlatformHandler;
    public List<PlatformConnection> connectedConnections;
}
