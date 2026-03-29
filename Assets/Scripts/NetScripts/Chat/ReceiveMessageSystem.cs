using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ReceiveMessageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (rpc, entity) in SystemAPI
                     .Query<RefRO<MessageRpc>>()
                     .WithAll<ReceiveRpcCommandRequest>()
                     .WithEntityAccess())
        {
            if (rpc.ValueRO.targetId == 0)
            {
                foreach (var (networkId, connEntity) in SystemAPI
                             .Query<RefRO<NetworkId>>()
                             .WithEntityAccess())
                {
                    var rpcEntity = ecb.CreateEntity();

                    var newRpc = new MessageRpc
                    {
                        message = rpc.ValueRO.message,
                        sender = rpc.ValueRO.sender,
                        targetId = 0
                    };

                    ecb.AddComponent(rpcEntity, newRpc);
                    ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
                    {
                        TargetConnection = connEntity
                    });
                }
            }
            else
            {
                Entity targetConnection = Entity.Null;

                foreach (var (networkId, connEntity) in SystemAPI
                             .Query<RefRO<NetworkId>>()
                             .WithEntityAccess())
                {
                    if (networkId.ValueRO.Value == rpc.ValueRO.targetId)
                    {
                        targetConnection = connEntity;
                        break;
                    }
                }

                if (targetConnection != Entity.Null)
                {
                    var rpcEntity = ecb.CreateEntity();

                    var newRpc = new MessageRpc
                    {
                        message = rpc.ValueRO.message,
                        sender = rpc.ValueRO.sender,
                        targetId = rpc.ValueRO.targetId
                    };

                    ecb.AddComponent(rpcEntity, newRpc);
                    ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
                    {
                        TargetConnection = targetConnection
                    });
                }
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
