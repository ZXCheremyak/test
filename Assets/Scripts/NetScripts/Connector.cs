using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class Connector : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 7979;
        return base.Initialize(defaultWorldName);
    }

}