using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    Damaged,
    Dead,
    Respawn,
}
public interface IMessageReceiver
{
    void OnReceiveMessage(MessageType type, object sender, object msg);
}

