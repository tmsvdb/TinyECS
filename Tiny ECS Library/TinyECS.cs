using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyECS
{
    public delegate void StageForSystemApplication(TinyEntity entity);

    public class ECSManager
    {
        public List<TinySystem> systems = new List<TinySystem>();
        public List<TinyEntity> entities = new List<TinyEntity>();

        public void AddSystem (TinySystem system)
        {
            systems.Add(system);
        }

        public void AddEntity (TinyEntity entity)
        {
            entity.Stage += new StageForSystemApplication(ApplyForSystems);
            entities.Add(entity);
        }

        /*
        public void UdateSystems ()
        {
            foreach (TinySystem system in systems)
            {
                system.Update();
            }
        }
        */

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
        // TODO: render entity through system using dependent Componets

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
        // TODO: keeps track of Components and systems_dependancies

        public event StageForSystemApplication Stage;

        public T AddComponent<T> (T component) where T : ITinyComponent
        {
            Add(Activator.CreateInstance<T>());
            Stage(this);
            return component;
        }

        public bool HasComponents (List<ITinyComponent> components)
        {
            foreach (ITinyComponent requested_comp in components)
            {
                bool has_comp = false;
                foreach (ITinyComponent local_comp in this)
                    if (local_comp.GetType().Equals(requested_comp.GetType()))
                    {
                        has_comp = true;
                        break;
                    }
                if (!has_comp) return false;
            }
            return true;
        }
    }

    public interface ITinyComponent
    {
        string type_id { get; }

        // TODO: keeps track of Enity types and values
    }
}
