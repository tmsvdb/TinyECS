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
            Console.WriteLine("BasicUpdateTest system added");

            ITinyEntity entity = manager.CreateEntity();
            Console.WriteLine("BasicUpdateTest New entity created");

            entity.AddComponent<CutomComponent_A>();
            Console.WriteLine("BasicUpdateTest Component A added");

            entity.AddComponent<CutomComponent_B>();
            Console.WriteLine("BasicUpdateTest Component B added");

            entity.AddComponent<CutomComponent_C>();
            Console.WriteLine("BasicUpdateTest Component C added");

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
