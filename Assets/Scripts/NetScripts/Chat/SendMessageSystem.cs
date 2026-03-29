using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial class SendMessageSystem : SystemBase
{
    int specifiedTarget;
    FixedString128Bytes text;
    bool hasPendingMessage;

    protected override void OnCreate()
    {
        UIHandler.sendMessage.AddListener(SetMsgValues);

        RequireForUpdate<EntitiesReferences>();
        RequireForUpdate<NetworkId>();
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
        if (!hasPendingMessage)
            return;

        hasPendingMessage = false;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        int localClientId = -1;

        foreach (var ghostOwner in SystemAPI
                     .Query<RefRO<GhostOwner>>()
                     .WithAll<GhostOwnerIsLocal>())
        {
            localClientId = ghostOwner.ValueRO.NetworkId;
            break;
        }

        var rpc = new MessageRpc
        {
            message = text,
            sender = localClientId.ToString(),
            targetId = specifiedTarget
        };

        var rpcEntity = ecb.CreateEntity();

        ecb.AddComponent(rpcEntity, rpc);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

partial struct MessageRpc : IRpcCommand
{
    public FixedString64Bytes sender;
    public int targetId;
    public FixedString128Bytes message;
}