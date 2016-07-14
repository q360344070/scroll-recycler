using UnityEngine;
using System.Collections;

public enum LC
{
    // Do not use 'LC.Nothing' for logs that you're not sure about, try using 'LC.Trace' instead
    Nothing = 0,
    Everything = -1,
    Editor = (1 << 1),
    Trace = (1 << 2),
    Database = (1 << 3),
    FileSystem = (1 << 4),
    Graphics = (1 << 5),
    Battle = (1 << 6),
    Player = (1 << 7),
    Analytics = (1 << 8),
    Audio = (1 << 9),
    FatalError = (1 << 10),
    Server = (1 << 11),
    UI = (1 << 12),
    IAP = (1 << 13),
    Simulation = (1 << 14),
    Profiling = (1 << 15),
    Campaign = (1 << 16),
    HQ = (1 << 17),
    Camera = (1 << 18),
    Notifications = (1 << 19),
    Quality = (1 << 20),
    Hardware = (1 << 21),
    Memory = (1 << 22),
    Rpc = (1 << 23),
    Websocket = (1 << 24),
    Loc = (1 << 25),
    Social = (1 << 26),
    Crafting = (1 << 27),
    BuildProcess = (1 << 28),
    Rewards = (1 << 29),
    Pooling = (1 << 30),
    // Don't add more than 30
    // Break Unity tools for 'Flags'
};

public static class MfLog
{
    public static void Error(LC channel, object msg) {}
}
