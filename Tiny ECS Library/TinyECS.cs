using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyECS
{
    public interface ITinySystem
    {
        List<Type> ComponentDependencies();
        void UpdateEntity(ITinyEntity entity);
    }

    public interface ITinyEntity
    {
        T AddComponent<T>();
        T GetComponent<T>();
        void RemoveComponent<T>();
        void Destroy();
    }

    public class ECSManager
    {
        private List<ITinySystem> systems = new List<ITinySystem>();
        private List<TinyEntity> entities = new List<TinyEntity>();

        public void AddSystem (ITinySystem system)
        {
            systems.Add(system);
        }

        public ITinyEntity CreateEntity ()
        {
            TinyEntity entity = new TinyEntity();
            entity.stageForApplication += ApplyForSystems;
            entity.stageForDestruction += RemoveEntity;
            entities.Add(entity);
            return entity;
        }

        private void RemoveEntity (TinyEntity entity)
        {
            entity.stageForApplication -= ApplyForSystems;
            entity.stageForDestruction -= RemoveEntity;
            entities.Add(entity);
        }

        private void ApplyForSystems (TinyEntity entity)
        {
            foreach (ITinySystem system in systems)
            {
                entity.CheckAndAddSystemReferences(system);
            }
        }




        private class TinyEntity : ITinyEntity
        {



            private interface ITinyComponent
            {
                Type GetComponentType();
                List<ITinySystem> GetDependentSystems();
                object GetComponentValueObject();
                void Destroy();
            }

            private class TinyComponent<T> : ITinyComponent
            {
                public Type componentType;
                public List<ITinySystem> dependentSystems;
                public T componentValueObject;

                public Type GetComponentType()
                {
                    return componentType;
                }

                public List<ITinySystem> GetDependentSystems()
                {
                    return dependentSystems;
                }

                public object GetComponentValueObject()
                {
                    return (T) componentValueObject;
                }

                public void Destroy ()
                {
                    componentType = null;
                    dependentSystems = null;
                    componentValueObject = default(T);
                }
            }




            private List<ITinyComponent> components;

            public event Action<TinyEntity> stageForApplication;
            public event Action<TinyEntity> stageForDestruction;

            public T AddComponent<T>()
            {
                TinyComponent<T> new_component = new TinyComponent<T>();
                new_component.componentType = typeof(T);
                new_component.componentValueObject = Activator.CreateInstance<T>();
                components.Add(new_component);
                stageForApplication(this);
                return new_component.componentValueObject;
            }

            public T GetComponent<T>()
            {
                foreach (T component in components)
                    if (component is T) return component;
                return default(T); // return null depending on the type -> int returns 0
            }

            public void CheckAndAddSystemReferences (ITinySystem system)
            {
                foreach (ITinyComponent component in  components)
                {

                    if (system.ComponentDependencies().Contains(component.GetComponentType()))
                    {
                        component.GetDependentSystems().Add(system);
                        system.UpdateEntity(this);
                    }
                    else
                        component.GetDependentSystems().Remove(system);
                }
            }

            public void RemoveComponent<T>()
            {
                ITinyComponent component = (ITinyComponent) GetComponent<T>();
                component.Destroy();
                components.Remove(component);
            }

            public void Destroy()
            {
                foreach (ITinyComponent component in components)
                    component.Destroy();
                components = null;

                stageForDestruction(this);
            }
        }
    }

 
}
