using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct ClientMessageReceiver : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (rpc, entity) in SystemAPI
                     .Query<RefRO<MessageRpc>>()
                     .WithAll<ReceiveRpcCommandRequest>()
                     .WithEntityAccess())
        {
            Debug.Log($"MSG from {rpc.ValueRO.sender}: {rpc.ValueRO.message}");
            UIHandler.Instance.ShowMessage(rpc.ValueRO.sender, rpc.ValueRO.message);
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
