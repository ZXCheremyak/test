using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Burst;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ReceiveMessageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (rpc, entity) in SystemAPI
            .Query<RefRO<MessageRpc>>()
            .WithAll<ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            // broadcast всем (targetId == 0)
            if (rpc.ValueRO.targetId == 0)
            {
                foreach (var (networkId, connEntity) in SystemAPI
                    .Query<RefRO<NetworkId>>()
                    .WithEntityAccess())
                {
                    var newRpc = ecb.CreateEntity();
                    ecb.AddComponent(newRpc, rpc.ValueRO);
                    ecb.AddComponent(newRpc, new SendRpcCommandRequest { TargetConnection = connEntity });
                }
            }
            else // приватное сообщение
            {
                Entity targetConn = Entity.Null;
                foreach (var (networkId, connEntity) in SystemAPI
                    .Query<RefRO<NetworkId>>()
                    .WithEntityAccess())
                {
                    if (networkId.ValueRO.Value == rpc.ValueRO.targetId)
                    {
                        targetConn = connEntity;
                        break;
                    }
                }

                if (targetConn != Entity.Null)
                {
                    var newRpc = ecb.CreateEntity();
                    ecb.AddComponent(newRpc, rpc.ValueRO);
                    ecb.AddComponent(newRpc, new SendRpcCommandRequest { TargetConnection = targetConn });
                }
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}