using Core;
using System;
using Xunit;

namespace Tests
{
    public class DependenciesContainerTests
    {
        interface IObj
        {
            int Value { get; }
        }

        interface IAgregateObj
        {
            int Value { get; }
            IObj Obj { get; }
        }

        interface IGenericObj<T>
        {
            int Value { get; }
            T TObj { get; }
        }

        class GenericImpl1 : IGenericObj<IObj>
        {
            private int _value = new Random().Next();
            public int Value => _value;

            private IObj _obj;
            public IObj TObj => _obj;
        }

        class GenericImpl2 : IGenericObj<IObj>
        {
            private int _value = new Random().Next();
            public int Value => _value;

            private IObj _obj;
            public IObj TObj => _obj;
        }

        class ObjImpl : IObj
        {
            private int _value = new Random().Next();
            public int Value => _value;
        }

        class AgregateObj : IAgregateObj
        {
            private int _value = new Random().Next();
            public int Value => _value;


            private IObj _obj;
            public IObj Obj => _obj;

            public AgregateObj(IObj obj)
            {
                _obj = obj;
            }
        }

        [Fact]
        public void Test_DifferentValues_Instance()
        {
            DependenciesContainer provider = new();
            provider.Register<IObj, ObjImpl>(LifeCycle.Instance);
            int inst1 = provider.Resolve<IObj>().Value;
            for (int i = 0; i < 100; i++)
            {
                int inst2 = provider.Resolve<IObj>().Value;
                Assert.NotEqual(inst1, inst2);
            }
        }

        [Fact]
        public void Test_SameValues_Singleton()
        {
            DependenciesContainer provider = new();
            provider.Register<IObj, ObjImpl>(LifeCycle.Singleton);
            int inst1 = provider.Resolve<IObj>().Value;
            for (int i = 0; i < 100; i++)
            {
                int inst2 = provider.Resolve<IObj>().Value;
                Assert.Equal(inst1, inst2);
            }
        }

        [Fact]
        public void Test_Instance_Instance()
        {
            DependenciesContainer provider = new();
            provider.Register<IObj, ObjImpl>(LifeCycle.Instance);
            provider.Register<IAgregateObj, AgregateObj>(LifeCycle.Instance);

            IAgregateObj inst1 = provider.Resolve<IAgregateObj>();
            for (int i = 0; i < 100; i++)
            {
                IAgregateObj inst2 = provider.Resolve<IAgregateObj>();
                Assert.NotEqual(inst1.Value, inst2.Value);
                Assert.NotEqual(inst1.Obj.Value, inst2.Obj.Value);
            }
        }

        [Fact]
        public void Test_Instance_Singleton()
        {
            DependenciesContainer provider = new();
            provider.Register<IObj, ObjImpl>(LifeCycle.Instance);
            provider.Register<IAgregateObj, AgregateObj>(LifeCycle.Singleton);
            int val1 = provider.Resolve<IObj>().Value;
            IAgregateObj inst1 = provider.Resolve<IAgregateObj>();
            for (int i = 0; i < 100; i++)
            {
                int val2 = provider.Resolve<IObj>().Value;
                IAgregateObj inst2 = provider.Resolve<IAgregateObj>();
                Assert.NotEqual(val1, val2);
                Assert.Equal(inst1.Value, inst2.Value);
                Assert.Equal(inst1.Obj.Value, inst2.Obj.Value);
            }
        }

        [Fact]
        public void Test_Singleton_Instance()
        {
            DependenciesContainer provider = new();
            provider.Register<IObj, ObjImpl>(LifeCycle.Singleton);
            provider.Register<IAgregateObj, AgregateObj>(LifeCycle.Instance);
            int val1 = provider.Resolve<IObj>().Value;
            IAgregateObj inst1 = provider.Resolve<IAgregateObj>();
            for (int i = 0; i < 100; i++)
            {
                int val2 = provider.Resolve<IObj>().Value;
                IAgregateObj inst2 = provider.Resolve<IAgregateObj>();
                Assert.Equal(val1, val2);
                Assert.NotEqual(inst1.Value, inst2.Value);
                Assert.Equal(inst1.Obj.Value, inst2.Obj.Value);
            }
        }

        [Fact]
        public void Test_Singleton_Singleton()
        {
            DependenciesContainer provider = new();

            provider.Register<IObj, ObjImpl>(LifeCycle.Singleton);
            provider.Register<IAgregateObj, AgregateObj>(LifeCycle.Singleton);
            int val1 = provider.Resolve<IObj>().Value;
            IAgregateObj inst1 = provider.Resolve<IAgregateObj>();
            for (int i = 0; i < 100; i++)
            {
                int val2 = provider.Resolve<IObj>().Value;
                IAgregateObj inst2 = provider.Resolve<IAgregateObj>();
                Assert.Equal(val1, val2);
                Assert.Equal(inst1.Value, inst2.Value);
                Assert.Equal(inst1.Obj.Value, inst2.Obj.Value);
            }
        }
    }
}