using System;
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
            TinyEntity entity = new TinyEntity();
            CustomSystem system = new CustomSystem();      

            ITinyComponent a = new CutomComponent_A();
            ITinyComponent b = new CutomComponent_B();
            ITinyComponent c = new CutomComponent_C();

            // act
            manager.AddSystem(system);
            manager.AddEntity(entity);

            entity.AddComponent(a);
            entity.AddComponent(b);
            entity.AddComponent(c);

            manager.UdateSystems();

            // assert
            Assert.IsNotNull(system.entityStagedForSystemUpdate);
        }
    }

    public class CutomComponent_A : ITinyComponent
    {
        public string type_id { get { return "Component A"; }  }
        public int value_A = 25;
    }

    public class CutomComponent_B : ITinyComponent
    {
        public string type_id { get { return "Component B"; } }
        public string value_B = "hello world";
    }

    public class CutomComponent_C : ITinyComponent
    {
        public string type_id { get { return "Component C"; } }
        public bool value_C = true;
    }

    public class CustomSystem : TinySystem
    {
        public TinyEntity entityStagedForSystemUpdate = null;

        public CustomSystem ()
        {
            ITinyComponent a = new CutomComponent_A();
            ITinyComponent b = new CutomComponent_B();
            ITinyComponent c = new CutomComponent_C();

            this.componentDependencies.Add(b);
            this.componentDependencies.Add(c);
        }

        public override void UpdateEntity(TinyEntity entity)
        {
            Console.Write("CustomSystem: UpdateEntity");
            entityStagedForSystemUpdate = entity;
        }
    }
}
