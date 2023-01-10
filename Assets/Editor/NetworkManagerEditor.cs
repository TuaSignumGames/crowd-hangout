using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetworkManagerEditor
{
    [MenuItem("Networking/Stop All Entities")]
    static void StopAllEntities()
    {
        NetworkManager.DisconnectAll();
    }
}
