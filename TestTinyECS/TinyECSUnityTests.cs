using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyECS;

namespace TestTinyECS
{
    [TestClass]
    public class TestECS
    {
        [TestMethod]
        public void BasicSystemUpdateTest()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem_A system = new CustomSystem_A();

            // act
            manager.AddSystem(system);

            ITinyEntity entity = manager.CreateEntity();

            entity.AddComponent<CustomComponent_A>();
            entity.AddComponent<CustomComponent_B>();
            entity.AddComponent<CustomComponent_C>();

            // assert
            Assert.IsNotNull(system.entityStagedForSystemUpdate);
        }

        [TestMethod]
        public void SystemUpdateOnAddAllDependentComponents()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem_A system_01 = new CustomSystem_A();
            CustomSystem_B system_02 = new CustomSystem_B();

            // act
            manager.AddSystem(system_01);
            manager.AddSystem(system_02);

            ITinyEntity entity_01 = manager.CreateEntity();

            entity_01.AddComponent<CustomComponent_A>();
            entity_01.AddComponent<CustomComponent_B>();

            Assert.AreEqual(entity_01, system_01.entityStagedForSystemUpdate);

            ITinyEntity entity_02 = manager.CreateEntity();

            entity_02.AddComponent<CustomComponent_B>();
            entity_02.AddComponent<CustomComponent_C>();

            // assert
            Assert.AreEqual(entity_02, system_02.entityStagedForSystemUpdate);
        }

        [TestMethod]
        public void InitComponentValues()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem_A system_01 = new CustomSystem_A();

            // act
            manager.AddSystem(system_01);

            ITinyEntity entity_01 = manager.CreateEntity();

            CustomComponent_A component_A = entity_01.AddComponent<CustomComponent_A>();
            CustomComponent_B component_B = entity_01.AddComponent<CustomComponent_B>();

            // assert
            Assert.AreEqual(25, component_A.value_A);
            Assert.AreEqual("hello world", component_B.value_B);
        }

        [TestMethod]
        public void SystemUpdateOnChangeComponentValues()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem_A system_01 = new CustomSystem_A();

            // act
            manager.AddSystem(system_01);

            ITinyEntity entity_01 = manager.CreateEntity();

            entity_01.AddComponent<CustomComponent_A>();
            entity_01.AddComponent<CustomComponent_B>();

            CustomComponent_A component_A = entity_01.GetComponent<CustomComponent_A>();

            component_A.value_A = 12;
            entity_01.Update();

            // assert
            Assert.AreEqual(12, system_01.entityStagedForSystemUpdate.GetComponent<CustomComponent_A>().value_A);
            Assert.AreEqual(12, system_01.lastValueA);
            Assert.AreEqual("hello world", system_01.lastValueB);
        }
    }

    public class CustomComponent_A
    {
        public int value_A = 25;
    }

    public class CustomComponent_B
    {
        public string value_B = "hello world";
    }

    public class CustomComponent_C
    {
        public bool value_C = true;
    }

    public class CustomSystem_A : ITinySystem
    {
        public List<Type> ComponentDependencies()
        {
            return new List<Type> () { typeof(CustomComponent_A), typeof(CustomComponent_B) };
        }

        public void UpdateEntity(ITinyEntity entity)
        {
            entityStagedForSystemUpdate = entity;
            lastValueA = entity.GetComponent<CustomComponent_A>().value_A;
            lastValueB = entity.GetComponent<CustomComponent_B>().value_B;
        }

        public ITinyEntity entityStagedForSystemUpdate;
        public int lastValueA = 0;
        public string lastValueB = "";

    }

    public class CustomSystem_B : ITinySystem
    {
        public List<Type> ComponentDependencies()
        {
            return new List<Type>() { typeof(CustomComponent_B), typeof(CustomComponent_C) };
        }

        public void UpdateEntity(ITinyEntity entity)
        {
            entityStagedForSystemUpdate = entity;
        }

        public ITinyEntity entityStagedForSystemUpdate;
    }
}
