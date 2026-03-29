using UnityEngine;
using Unity.NetCode;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Entities;

public class PlayerInputAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerInput());
        }

    }
}

public struct PlayerInput : IInputComponentData
{
    public float2 vector;
}