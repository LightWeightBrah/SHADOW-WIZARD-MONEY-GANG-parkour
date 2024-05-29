using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedOnText : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private TextMeshProUGUI text;
    private void LateUpdate()
    {
        text.text = $"speed: {rigidbody.velocity.magnitude.ToString("F1")} ";
    }

}
