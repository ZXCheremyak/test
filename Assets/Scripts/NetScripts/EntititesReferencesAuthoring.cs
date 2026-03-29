using Unity.Entities;
using UnityEngine;

public class EntititesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;

    public class Baker : Baker<EntititesReferencesAuthoring>

    {
        public override void Bake(EntititesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
            });

        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity playerPrefabEntity;
}

