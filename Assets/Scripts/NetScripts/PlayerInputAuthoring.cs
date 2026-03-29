using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class PlayerInputAuthoring : MonoBehaviour { }

public class PlayerInputBaker : Baker<PlayerInputAuthoring>
{
    public override void Bake(PlayerInputAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerInput());
    }
}

public struct PlayerInput : IInputComponentData
{
    public float2 vector;
}