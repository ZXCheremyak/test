using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ReceiveMessageClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (rpc, entity) in SystemAPI
            .Query<RefRO<MessageRpc>>()
            .WithAll<ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            Debug.Log($"[CHAT] {rpc.ValueRO.sender}: {rpc.ValueRO.message}");

            if (UIHandler.Instance != null)
                UIHandler.Instance.ShowMessage(rpc.ValueRO.sender, rpc.ValueRO.message);

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}