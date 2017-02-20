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
        public void BasicUpdateTest()
        {
            // arrange
            ECSManager manager = new ECSManager();
            CustomSystem system = new CustomSystem();

            // act
            manager.AddSystem(system);
            ITinyEntity entity = manager.CreateEntity();

            entity.AddComponent<CutomComponent_A>();
            entity.AddComponent<CutomComponent_B>();
            entity.AddComponent<CutomComponent_C>();

            // assert
            Assert.IsNotNull(system.entityStagedForSystemUpdate);
        }
    }

    public class CutomComponent_A
    {
        public int value_A = 25;
    }

    public class CutomComponent_B
    {
        public string value_B = "hello world";
    }

    public class CutomComponent_C
    {
        public bool value_C = true;
    }

    public class CustomSystem : ITinySystem
    {
        public List<Type> ComponentDependencies()
        {
            return new List<Type> () { typeof(CutomComponent_A) };
        }

        public void UpdateEntity(ITinyEntity entity)
        {
            entityStagedForSystemUpdate = entity;
        }

        public ITinyEntity entityStagedForSystemUpdate;
    }
}
