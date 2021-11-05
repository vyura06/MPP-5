using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class DependenciesContainer
    {
        private static bool IsBaseTypeOrInterface(Type type, Type parent) //проверка наследуется ли класс от другого
        {
            return Equals(type.BaseType, parent) ||
                type.GetInterfaces() //или реализует интерфейс
                .Any(t => t.Equals(parent));
        }
        private static void ValidateTypes(Type depType, Type implType) //проверка правильности типов
        {
            if (depType is null)
            {
                throw new ArgumentNullException(nameof(depType));
            }

            if (implType is null)
            {
                throw new ArgumentNullException(nameof(implType));
            }

            if (!IsBaseTypeOrInterface(implType, depType))
            {
                throw new ArgumentException($"Implementation type: '{implType}" +
                    $"' not implements service type: '{depType}'");
            }
        }
        internal IDictionary<Type, IList<Dependency>> Dependencies { get; } = 
            new Dictionary<Type, IList<Dependency>>(); //словарь где ключ это тип абстракции а знвчение это список типов реализации

        public void Register<TDependency>(LifeCycle lifetime) where TDependency : class
        {
            Register(typeof(TDependency), lifetime);
        } 
        public void Register(Type depType, LifeCycle lifetime) //регистрация одного класса
        {
            if (depType is null)
            {
                throw new ArgumentNullException(nameof(depType));
            }

            if (!Dependencies.TryGetValue(depType, out var dependencies))
            {
                dependencies = new List<Dependency>();
                Dependencies.Add(depType, dependencies);
            }
            var dependency = (lifetime) switch
            {
                LifeCycle.Instance => new Dependency(this, depType),
                LifeCycle.Singleton => new Dependency(this, depType, LifeCycle.Singleton),
                _ => throw new ArgumentException(null, nameof(lifetime)),
            };
            dependencies.Add(dependency);
        }

        public void Register<TDependency, TImplementation>(LifeCycle lifetime)
        {
            Register(typeof(TDependency), typeof(TImplementation), lifetime);
        }
        public void Register(Type depType, Type implType, LifeCycle lifetime) //регистрация пары тип - тип реализации
        {
            ValidateTypes(depType, implType);
            if (!Dependencies.TryGetValue(depType, out var dependencies))
            {
                dependencies = new List<Dependency>();
                Dependencies.Add(depType, dependencies);
            }
            var dependency = (lifetime) switch
            {
                LifeCycle.Instance => new Dependency(this, implType),
                LifeCycle.Singleton => new Dependency(this, implType, LifeCycle.Singleton),
                _ => throw new ArgumentException(null, nameof(lifetime)),
            };
            dependencies.Add(dependency);
        }

        public void RegisterSingleton<TDependency>(TDependency implementationInstance) where TDependency : class
        {
            RegisterSingleton(typeof(TDependency), implementationInstance);
        }
        public void RegisterSingleton(Type depType, object implementationInstance) //регистрация singleton с помошью уже созданного экземпляра
        {
            ValidateTypes(depType, implementationInstance?.GetType());
            if (!Dependencies.TryGetValue(depType, out var dependencies))
            {
                dependencies = new List<Dependency>();
                Dependencies.Add(depType, dependencies);
            }
            dependencies.Add(new Dependency(this, depType, LifeCycle.Singleton, implementationInstance));
        }

        public TDependency Resolve<TDependency>()
        {
            return (TDependency)Resolve(typeof(TDependency));
        }
        public object Resolve(Type depType) //получение реализации по типу
        {
            if (Dependencies.TryGetValue(depType, out var dependencies))
            {
                return dependencies[0].GetInstance(); //получение самой первой реализации
            }
            throw new InvalidOperationException("No such type is registered");
        }

        public IEnumerable<TDependency> ResolveAll<TDependency>()
        {
            return ResolveAll(typeof(TDependency)).Cast<TDependency>();
        }
        public IEnumerable<object> ResolveAll(Type depType) //получение списка всех реализаций
        {
            if (Dependencies.TryGetValue(depType, out var dependencies))
            {
                return dependencies.Select(d => d.GetInstance()).ToArray();
            }
            throw new InvalidOperationException("No such type is registered");
        }
    }
}
