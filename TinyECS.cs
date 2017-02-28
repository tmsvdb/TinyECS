using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyECS
{
    /*
        Create your own systems

        You can add systems of any type to the ECS manager as long as they implement this
        interface, in order for the TinyECS system to communicatie with it!

        This interface (ITinySystem) uses methodes so the systems manager can find it's dependencies 
        and can call your system if an entity update occurred.        
    */
    public interface ITinySystem
    {
        /*
            Set the components this system depends on. This means that only the Entities that 
            share the same component type as defined here will be pushed through this
            systems if an entity update occurred

            @returns a List<Type> of all the dependency Types the ECS should listen to

            EXAMPLE RETURN:
            return new List<Type> () { typeof(CustomComponent_A), typeof(CustomComponent_B) };
        */
        List<Type> ComponentDependencies();

        /* 
            Entities that share the same components, as defined in the ComponentsDependencies methode, 
            are handled by the systems UpdateEntity methode, if the entity i has been updated.
            This is the place to write your systems machanics that will apply to the entity.

            @param ITinyEntity entity: the entity in question who has been updated.
        */
        void UpdateEntity(ITinyEntity entity);
    }

    /*
        Enities as returned by the ECSManager

        An entity whithin this ECS tool are complex packages that manage their own
        components, and hold reference to the systems they depend on.
        This means that entities have lots of internal stucture that is handeled by the ECS tool.
        To prevent users from setting the wrong value's only a few methodes are made public by only
        handing over this interface to the user.
    */
    public interface ITinyEntity
    {
        /*
            Add your own custom component of type T to this entity.

            @returns an instance of the type T you added.
        */
        T AddComponent<T>();

        /*
            Get a component of the specified type T from this entity.

            @returns an instance of a component of type T you previously added.
        */
        T GetComponent<T>();

        /*
            Update this entity
            En entity is only passed through its dependent systems if this methode is called!
        */
        void Update();

        /*
            Remove a component of the specified type T from this entity.
        */
        void RemoveComponent<T>();

        /*
            Destroy this entire entity and all of its components
            If this methode is called, this entity and its components will be nullified, 
            and removed from the ECSManager.
        */
        void Destroy();
    }

    /*
        Keeps track of all the entities and systems that have been created.
        Users can add custom systems of any type to the manager
        Entities need to be created by the ECSManager
    */
    public class ECSManager
    {
        /*
            List of all systems and entities added/created to the manager
        */
        private List<ITinySystem> systems = new List<ITinySystem>();
        private List<TinyEntity> entities = new List<TinyEntity>();

        /*
            Add a new system to the systems list. Systems are parsed in the order that they are added.

            @ param system an new instane of any object type that implenets the interface ITinySystem
        */
        public void AddSystem (ITinySystem system)
        {
            systems.Add(system);
        }

        /*
            Classic ECS Update methode
            Loops through all systems in the order they are added, 
            and push the entities that share dependencies through the systems UpdateEntity methode.
            This version of the classic way of ECS redeereing is a bit slower than the original design pattern.
        */
        public void ForceUpdate()
        {
            foreach(ITinySystem system in systems)
                foreach (TinyEntity entity in entities)
                    if (system.ComponentDependencies().Intersect(entity.ComponentTypes).Count() == system.ComponentDependencies().Count())
                        system.UpdateEntity(entity);
        }

        /*
            Create a new entity and add it to the list of entities
            you cannot create entities by instantiating them yourself, this ECSmanager has therefore been designed
            to keep users for doing so.
            The only way to create new entities is by using this methode.

            @returns the interface ITinyEntity of the newly created instance of an entity.
        */
        public ITinyEntity CreateEntity ()
        {
            TinyEntity entity = new TinyEntity(ApplyForSystems, RemoveEntity);
            entities.Add(entity);
            return entity;
        }


        /*
            ============================
                LOCAL IMPLEMENTATION
            ============================
        */

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
            public List<Type> ComponentTypes = new List<Type>();
            public List<object> ComponentValues = new List<object>();
            public List<ITinySystem> systemDepencies = new List<ITinySystem>();

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
