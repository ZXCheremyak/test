using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct PlayerMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float speed = 0.5f;
        foreach(var (input, transform) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            float2 moveInput = input.ValueRO.vector;
            moveInput = math.normalizesafe(moveInput)*speed;
            transform.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
        }
    }

}
