﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyECS
{
    public delegate void StageForSystemApplication(TinyEntity entity);

    public class ECSManager
    {
        public List<TinySystem> systems;
        public List<TinyEntity> entities;

        public void AddSystem (TinySystem system)
        {
            if (systems == null)
                systems = new List<TinySystem>();

            systems.Add(system);
        }

        public void AddEntity (TinyEntity entity)
        {
            if (entities == null)
                entities = new List<TinyEntity>();

            entity.Stage += new StageForSystemApplication(ApplyForSystems);
            entities.Add(entity);
        }

        public void UdateSystems ()
        {
            foreach (TinySystem system in systems)
            {
                system.Update();
            }
        }

        private void ApplyForSystems (TinyEntity entity)
        {
            foreach (TinySystem system in systems)
            {
                int numComponentCompare = 0;

                foreach (ITinyComponent sysCompDep in system.componentDependencies)
                    foreach (ITinyComponent entityComp in entity)
                        if (entityComp.type_id == sysCompDep.type_id)
                            numComponentCompare++;

                if (numComponentCompare == system.componentDependencies.Count)
                    system.AddEntityReference(entity);
                else
                    system.RemoveEntityReference(entity);
            }
        }
    }

    public class TinySystem
    {
        public List<ITinyComponent> componentDependencies = new List<ITinyComponent>();
        private List<TinyEntity> entityReferences = new List<TinyEntity>();

        public void AddEntityReference(TinyEntity entity)
        {
            if (!entityReferences.Contains(entity))
                entityReferences.Add(entity);
        }

        public void RemoveEntityReference(TinyEntity entity)
        {
            if (entityReferences.Contains(entity))
                entityReferences.Remove(entity);
        }

        public void Update ()
        {
            foreach (TinyEntity entity in entityReferences)
                UpdateEntity(entity);
        }

        public virtual void UpdateEntity(TinyEntity entity)
        {
            
        }
    }

    public class TinyEntity : List<ITinyComponent>
    { 
        public event StageForSystemApplication Stage;

        public ITinyComponent AddComponent (ITinyComponent component)
        {
            Console.Write("TinyEntity: AddComponent");

            Add(component);
            Stage(this);
            return component;
        }

        public bool HasComponents (List<ITinyComponent> components)
        {
            return true;
        }
    }

    public interface ITinyComponent
    {
        string type_id { get; }
    }
}
