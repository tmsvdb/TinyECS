# TinyECS
Lightweight easy to use generic ECS object management system.

IMPORTANT: This project is up and runnig but it still needs more tests.
I want to take a look at other ways of event subscription such as Action method subscriptions => systems that want to update their entities and entities that have been changed and need their system to update. 

What is this tool used for?
--
As the description implies this tool is a Entity Component System. 
The core of this ECS is event based, this means that the systems are only updated if the components values have changed.

How to use this tool
--
EXAMPLE CODE:

    public class MyCustomClass
    {
        // Create a new instance of the TinyECS manager
        ECSManager manager = new ECSManager();

        // Create a custom system and add it to the manager
        CustomSystem system = new CustomSystem();
        manager.AddSystem(system);

        // Create new entities by requesting a new entity from the manager
        ITinyEntity entity = manager.CreateEntity();
        
        // Add a component of any type to the entity
        CustomComponent component = entity.AddComponent<CustomComponent>();
       
        // If you would need the component settings later in the game use it like this
        CustomComponent component = entity.GetComponent<CustomComponent>();
        
        // Change values of the component on an entity
        component.customValue = "My New Value";
        
        // Push the entities changes through its dependent systems
        entity.Update();
       
    }

    /*
        Create components of any type
    */
    public class CustomComponent
    {
        public string customValue = "Hello";
    }

    /*
        Create your own systems
        
        Your custom system must have and interface called ITinySystem
        so the systems manager can find it's dependencies and call it if an update occurred
    */
    public class CustomSystem : ITinySystem
    {
        /*
            <This is a manditory interface methode>
            Set the components this system depends on.
            
            @returns a List<Type> of all the dependency Types the ECS should listen to
        */
        public List<Type> ComponentDependencies()
        {
            // this is where and how you add your systems dependencies
            return new List<Type> () { typeof(CustomComponent_A), typeof(CustomComponent_B) };
        }

        /* 
            <This is a manditory interface methode>
            Entities that comtain components which value has been changed 
            are handled by the systems UpdateEntity methode.
            This methode is only called if the enity who's value has changed has a component
            that this system depends on.

            @param ITinyEntity entity: the entity in question who's component has been updated.
        */
        public void UpdateEntity(ITinyEntity entity)
        {
            // if an entities values have been changed it will go through this methode
            // you can use the new values to do your calculations
            // For instance:
            // If this would be your game's Movement System and the position component was changed
            // this is the place where you could do somthing with the new location.
        }
    }

State of this project
--
Up to version v0.2: the code base of the v0.1 is becoming too nested, which will eventualy make the Parsing of al large amount of Enities slow.
The auto update of components as is does not work, and makes the nested Component <> System implementation unnecessary.
Whats in the new version:
- Simple loop through systems in order in which they have been added.
- Entity keeps track of system dependencies.
- Entity is going to get an Update methode so it will be parsed through the system loop.
Interfaces:
+ ECSManager:
   - ITinySystem AddSystem(ITinySystem);
   - ITinyEntity CreateEntity();
   - void ForceUpdate();
+ ITinySystem:
   - List<Type> ComponentDependencies;
   - void UpdateEntity (ITinyEntity);
+ ITinyEntity:
   - T AddComponent<T> ();
   - T GetComponent<T> ();
   - void RemoveComponent<T> ();
   - void Destroy();
   - void Update();

