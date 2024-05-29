using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTeleport : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private KeyCode keyCode = KeyCode.T;
    private void Update()
    {
        if (Input.GetKeyDown(keyCode))
            player.transform.position = transform.position;
    }
}
