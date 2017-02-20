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

            private class TinyComponent
            {
                public Type componentType;
                public List<ITinySystem> dependentSystems;
                public object componentValueObject;

                public Type GetComponentType()
                {
                    return componentType;
                }

                public List<ITinySystem> GetDependentSystems()
                {
                    if (dependentSystems == null) dependentSystems = new List<ITinySystem>();
                    return dependentSystems;
                }

                public object GetComponentValueObject()
                {
                    return componentValueObject;
                }

                public void Destroy ()
                {
                    componentType = null;
                    dependentSystems = null;
                    componentValueObject = null;
                }
            }




            private List<TinyComponent> components = new List<TinyComponent>();

            public event Action<TinyEntity> stageForApplication;
            public event Action<TinyEntity> stageForDestruction;

            public T AddComponent<T>()
            {
                TinyComponent new_component = new TinyComponent();
                new_component.componentType = typeof(T);
                new_component.componentValueObject = Activator.CreateInstance<T>();
                components.Add(new_component);
                stageForApplication(this);
                return (T) new_component.componentValueObject;
            }

            public T GetComponent<T>()
            {
                TinyComponent component = GetTinyComponentByType<T>();
                if (component != null) return (T) component.componentValueObject;
                else return default(T); // return null depending on the type -> int returns 0
            }

            private TinyComponent GetTinyComponentByType<T>()
            {
                foreach (TinyComponent component in components)
                    if (component.componentType.Equals(typeof(T)))
                        return component;

                return null;
            }

            public void CheckAndAddSystemReferences (ITinySystem system)
            {
                foreach (TinyComponent component in  components)
                {
                    Console.WriteLine("CheckAndAddSystemReferences Component Type => " + component.GetComponentType().ToString());

                    foreach(Type dependency in system.ComponentDependencies())
                        Console.WriteLine("CheckAndAddSystemReferences Sysem dependency Type => " + dependency.ToString());

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
                TinyComponent component = GetTinyComponentByType<T>();
                component.Destroy();
                components.Remove(component);
            }

            public void Destroy()
            {
                foreach (TinyComponent component in components)
                    component.Destroy();
                components = null;

                stageForDestruction(this);
            }
        }
    }

 
}
