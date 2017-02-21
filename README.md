# TinyECS
Lightweight easy to use generic ECS object management system.

IMPORTANT: This project is still under construction and does not give the expected results!

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
        
        // change your component values
        // TODO: still under construction
        // for now use component.value = "World";
    }

    /*
        Create components of any type
    */
    public class CustomComponent
    {
        public string value = "Hello";
    }

    /*
        Create your own systems
        
        Your custom system must have and interface called ITinySystem
        so the systems manager can find it's dependencies and call it if an update occurred
    */
    public class CustomSystem : ITinySystem
    {
        /*
            Set the components this system depends on.

            This is a manditory interface methode.
            
            @returns a List<Type> of all the dependency Types the ECS should listen to
        */
        public List<Type> ComponentDependencies()
        {
            // this is where and how you add your systems dependencies
            return new List<Type> () { typeof(CustomComponent_A), typeof(CustomComponent_B) };
        }

        /* 
            Entities that comtain components which value has been changed 
            are handled by the systems UpdateEntity methode.
            This methode is only called if the enity who's value has changed has a component
            that this system depends on.

            This is a manditory interface methode.

            @param entity: the entity in question who's component has been updated.
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

Completed:
- Custom systems can be added to the TinyECS.manager
- Entities are created by the TinyECS.manager this to protect system code from users.
- Components of any type can be added to enities.

Failed tests:
- Components do not trigger dependent systems on component update.
- Components trigger all systems that depend on that component > currently by design

