using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]

partial struct GoInGameServerSystem : ISystem
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
    var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

    foreach ((RefRO<ReceiveRpcCommandRequest> req, Entity entity)
        in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
        .WithAll<GoInGameRequestRpc>()
        .WithEntityAccess())
    {
        var connection = req.ValueRO.SourceConnection;

        ecb.AddComponent<NetworkStreamInGame>(connection);

        var networkId = SystemAPI.GetComponent<NetworkId>(connection);

        var random = new Unity.Mathematics.Random((uint)(networkId.Value * 1234 + 1));
        float x = random.NextFloat(-10f, 10f);

        Entity player = ecb.Instantiate(entitiesReferences.playerPrefabEntity);

        ecb.SetComponent(player, LocalTransform.FromPosition(new float3(x, 0.5f, 0)));

        ecb.AddComponent(player, new GhostOwner
        {
            NetworkId = networkId.Value
        });

        if (!SystemAPI.HasBuffer<LinkedEntityGroup>(connection))
        {
            ecb.AddBuffer<LinkedEntityGroup>(connection);
        }

        ecb.AppendToBuffer(connection, new LinkedEntityGroup
        {
            Value = player
        });

        ecb.DestroyEntity(entity);
    }

    ecb.Playback(state.EntityManager);
    ecb.Dispose();
}
}
