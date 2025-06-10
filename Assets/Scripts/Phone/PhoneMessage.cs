using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMessage", menuName = "Phone/Message")]
public class PhoneMessage : ScriptableObject
{
    public string Sender;
    public DateTime Timestamp;
    public string Content;

    private void Awake()
    {
        Timestamp = DateTime.Now;
    }
}
