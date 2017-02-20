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
        public void DoesNotHaveCorrespondingComponents()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem_A system_01 = new CustomSystem_A();
            CustomSystem_B system_02 = new CustomSystem_B();

            // act
            manager.AddSystem(system_01);
            manager.AddSystem(system_02);

            ITinyEntity false_entity = manager.CreateEntity();
            false_entity.AddComponent<CustomComponent_A>();
            false_entity.AddComponent<CustomComponent_C>();

            Assert.AreNotEqual(false_entity, system_01.entityStagedForSystemUpdate);
            Assert.AreNotEqual(false_entity, system_02.entityStagedForSystemUpdate);
        }

        [TestMethod]
        public void HasCorrespondingComponents()
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
            Assert.AreNotEqual(entity_01, system_02.entityStagedForSystemUpdate);

            ITinyEntity entity_02 = manager.CreateEntity();

            entity_02.AddComponent<CustomComponent_B>();
            entity_02.AddComponent<CustomComponent_C>();

            // assert
            Assert.AreEqual(entity_02, system_02.entityStagedForSystemUpdate);
            Assert.AreNotEqual(entity_02, system_01.entityStagedForSystemUpdate);
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
        }

        public ITinyEntity entityStagedForSystemUpdate;
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
