using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMessage", menuName = "Phone/Message")]
public class PhoneMessage : ScriptableObject
{
    public string Sender;
    public DateTime Timestamp;
    public string Content;

    public Objective Objective = null; // optional

    private void Awake()
    {
        Timestamp = DateTime.Now;
    }
}
