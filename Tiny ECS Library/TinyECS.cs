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

        public void ForceUpdate()
        {
        }

        public ITinyEntity CreateEntity ()
        {
            TinyEntity entity = new TinyEntity(ApplyForSystems, RemoveEntity);
            entities.Add(entity);
            return entity;
        }

        private void RemoveEntity (TinyEntity entity)
        {
            entities.Remove(entity);
        }

        private void ApplyForSystems (TinyEntity entity)
        {
            entity.ResetSystemDependencies(systems);
        }

        private class TinyEntity : ITinyEntity
        {
            private List<Type> ComponentTypes = new List<Type>();
            private List<object> ComponentValues = new List<object>();
            private List<ITinySystem> systemDepencies = new List<ITinySystem>();

            private Action<TinyEntity> ApplyForSystemsCallback;
            private Action<TinyEntity> RemoveEntityCallback;

            public TinyEntity (Action<TinyEntity> ApplyForSystems, Action<TinyEntity> RemoveEntity)
            {
                ApplyForSystemsCallback = ApplyForSystems;
                RemoveEntityCallback = RemoveEntity;
            }

            public void Update()
            {
                foreach (ITinySystem system in systemDepencies)
                    system.UpdateEntity(this);
            }

            public T AddComponent<T>()
            {
                T component = CreateNewComponentInstance<T>();
                ApplyForSystemsCallback(this);
                return component;
            }

            public T GetComponent<T>()
            {
                T component = GetComponentValueByType<T>();
                if (component != null) return (T) component;
                // return null depending on the type -> int returns 0
                else return default(T); 
            }

            public void ResetSystemDependencies (List<ITinySystem> systems)
            {
                // loop through all systems and add system that share a type dependency to the systemDependencies list
                foreach (ITinySystem system in systems)
                    foreach (Type type in ComponentTypes)
                        if (system.ComponentDependencies().Contains(type))
                            systemDepencies.Add(system);
                
                // check if all components are in system dependencies, if so update that system
                foreach (ITinySystem system in systemDepencies)
                    if (system.ComponentDependencies().Intersect(ComponentTypes).Count() == system.ComponentDependencies().Count())
                        system.UpdateEntity(this);              
            }

            public void RemoveComponent<T>()
            {
                RemoveComponentfromLists(typeof(T));
            }

            public void Destroy()
            {
                foreach (Type componentType in ComponentTypes)
                    RemoveComponentfromLists(componentType);
                
                ComponentTypes = null;
                ComponentValues = null;
                ApplyForSystemsCallback = null;
                RemoveEntityCallback = null;

                RemoveEntityCallback(this);
            }


            /*
                Private helpers
            */

            private T CreateNewComponentInstance<T>()
            {
                T componentValueObject = Activator.CreateInstance<T>();
                AddComponentToLists(typeof(T), componentValueObject);
                return componentValueObject;
            }


            /*
                Manage component type and value lists
            */

            private T GetComponentValueByType<T>()
            {
                return GetComponentValueByIndex<T>(ComponentTypes.IndexOf(typeof(T)));
            }

            private T GetComponentValueByIndex <T>(int index)
            {
                return (T) ComponentValues[index];
            }

            private void AddComponentToLists(Type compType, object compValue)
            {
                ComponentTypes.Add(compType);
                ComponentValues.Add(compValue);
            }

            private void RemoveComponentfromLists(Type compType)
            {
                RemoveComponentfromListsAtIndex(ComponentTypes.IndexOf(compType));
            }

            private void RemoveComponentfromLists(object compValue)
            {
                RemoveComponentfromListsAtIndex(ComponentValues.IndexOf(compValue));
            }

            private void RemoveComponentfromListsAtIndex(int index)
            {
                ComponentTypes[index] = null;
                ComponentValues[index] = null;

                ComponentTypes.RemoveAt(index);
                ComponentValues.RemoveAt(index);
            }

        }

    }
 
}
