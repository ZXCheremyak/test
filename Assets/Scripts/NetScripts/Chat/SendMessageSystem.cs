using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Burst;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class SendMessageSystem : SystemBase
{
    private FixedString128Bytes text;
    private int specifiedTarget;
    private bool hasPendingMessage;

    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<EntitiesReferences>();
        RequireForUpdate<NetworkStreamInGame>();
        UIHandler.sendMessage.AddListener(SetMsgValues);
    }

    protected override void OnDestroy()
    {
        UIHandler.sendMessage.RemoveListener(SetMsgValues);
    }

    void SetMsgValues(FixedString128Bytes text, int value)
    {
        this.text = text;
        this.specifiedTarget = value;
        hasPendingMessage = true;
    }

    protected override void OnUpdate()
    {
        if (!hasPendingMessage) return;
        hasPendingMessage = false;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        int localClientId = -1;
        foreach (var ghostOwner in SystemAPI.Query<RefRO<GhostOwner>>().WithAll<GhostOwnerIsLocal>())
        {
            localClientId = ghostOwner.ValueRO.NetworkId;
            break;
        }

        var rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, new MessageRpc
        {
            message = text,
            sender = new FixedString64Bytes(localClientId.ToString()),
            targetId = specifiedTarget
        });
        ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

// RPC (оставляем здесь)
public struct MessageRpc : IRpcCommand
{
    public FixedString64Bytes sender;
    public int targetId;
    public FixedString128Bytes message;
}