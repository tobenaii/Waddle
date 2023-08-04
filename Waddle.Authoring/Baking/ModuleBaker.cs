using UnityEngine;

namespace Waddle.Authoring.Baking
{
    public abstract class ModuleBaker
    {
        internal abstract void Bake(GameObject gameObject, IModuleWrapper module);
    }
    
    public abstract class ModuleBaker<T> : ModuleBaker where T : IModuleWrapper
    {
        private GameObject _gameObject;
        
        internal override void Bake(GameObject gameObject, IModuleWrapper module)
        {
            _gameObject = gameObject;
            Bake((T)module);
        }

        protected abstract void Bake(T module);

        protected TV AddComponent<TV>() where TV : Component
        {
            var existingComponent = _gameObject.GetComponent<TV>();
            return existingComponent != null ? existingComponent : _gameObject.AddComponent<TV>();
        }

    }
}