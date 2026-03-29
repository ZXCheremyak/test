using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class PlayerInputSystem : SystemBase
{

    DefaultInputActions input;
    protected override void OnCreate()
    {
        input = new DefaultInputActions();
        Debug.Log(input.Player.Move + " ");
        input.Enable();
        RequireForUpdate<NetworkStreamInGame>();
        RequireForUpdate<PlayerInput>();
    }

    protected override void OnUpdate()
    {
        InputSystem.Update();

        float2 moveVector = new float2(input.Player.Move.ReadValue<UnityEngine.Vector2>().x, input.Player.Move.ReadValue<UnityEngine.Vector2>().y);

        foreach (RefRW<PlayerInput> playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {

            playerInput.ValueRW.vector = moveVector;
        }
    }

    protected override void OnDestroy()
    {
        input.Disable();
        input.Dispose();
        base.OnDestroy();
    }
}
