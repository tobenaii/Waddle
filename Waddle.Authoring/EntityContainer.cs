using UnityEngine;
using Waddle.Authoring.Core;

namespace Waddle.Authoring
{
    public class EntityContainer : ScriptableObject
    {
        [SerializeField] private Entity _entity;

        public Entity Entity => _entity;

        public EntityContainer FromJson(string json)
        {
            _entity = Entity.FromJson(json);
            return this;
        }
    }
}