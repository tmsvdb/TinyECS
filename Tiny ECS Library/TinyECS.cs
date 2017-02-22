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
        void Update();
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

        // TODO: Implement a ForceUpdate() so that entities can itterate through
        // systems in the ored the where added. This if the user want to use the
        // ECS tool in an update game loop 

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
                // TODO: Try using a getter/setter to auto update when changed       
                public object componentValueObject;
                public List<ITinySystem> dependentSystems = new List<ITinySystem>();
            }
  
            private List<TinyComponent> components = new List<TinyComponent>();
            public event Action<TinyEntity> stageForApplication;
            public event Action<TinyEntity> stageForDestruction;

            // TODO: Move Update() to ECSManager
            public void Update()
            {
                List<ITinySystem> allSystemsToUpdate = new List<ITinySystem>();

                foreach (TinyComponent component in components)
                    foreach (ITinySystem system in component.dependentSystems)
                        if (!allSystemsToUpdate.Contains(system))
                            allSystemsToUpdate.Add(system);

                foreach (ITinySystem system in allSystemsToUpdate)
                    system.UpdateEntity(this);
            }

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
                bool aChangeWasMade = false;
                List<Type> componentTypes = new List<Type>();

                foreach (TinyComponent component in components)
                {
                    componentTypes.Add(component.componentType);

                    if (system.ComponentDependencies().Contains(component.componentType))
                    {
                        component.dependentSystems.Add(system);
                        aChangeWasMade = true;
                    }
                }

                if (aChangeWasMade) {
                    bool hasAllDependencies = system.ComponentDependencies().Intersect(componentTypes).Count() == system.ComponentDependencies().Count();
                    if (hasAllDependencies) system.UpdateEntity(this);
                }
            }

            public void RemoveComponent<T>()
            {
                TinyComponent component = GetTinyComponentByType<T>();
                components.Remove(component);
            }

            public void Destroy()
            {
                foreach (TinyComponent component in components)
                {
                    component.componentType = null;
                    component.componentValueObject = null;
                    component.dependentSystems = null;
                }
                components = null;

                stageForDestruction(this);
            }
        }
    }

 
}
