using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<NetworkId> networkId, Entity entity) in SystemAPI
                 .Query<RefRO<NetworkId>>()
                 .WithNone<NetworkStreamInGame>()
                 .WithEntityAccess())
        {
            ecb.AddComponent<NetworkStreamInGame>(entity);

            Entity rpcEntity = ecb.CreateEntity();

            ecb.AddComponent(rpcEntity, new GoInGameRequestRpc());

            ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
            {
                TargetConnection = entity
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct GoInGameRequestRpc: IRpcCommand
{ }
