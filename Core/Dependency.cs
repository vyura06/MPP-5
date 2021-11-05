using System;
using System.Linq;

namespace Core
{
    public class Dependency
    {
        public DependenciesContainer Container { get; } //ссылка на контейнер в котором находится эта зависимость
        public Type Type { get; } //тип реализации
        public LifeCycle LifeCycle { get; } //тип создания экземпляра класса

        private object _instance; //используется для singleton

        public Dependency(DependenciesContainer container,
                          Type type,
                          LifeCycle lifeCycle = LifeCycle.Instance,
                          object instance = null)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            LifeCycle = lifeCycle;
            _instance = instance;
        }

        private object Create() //создание обьекта с помощью класса
        {
            var constructors = Type.GetConstructors();
            if (constructors.Length == 0)
                throw new InvalidOperationException("No constructors present");
            var constructor = constructors[0];
            var cParams = constructor.GetParameters()
                .Select(p =>
                {
                    //параметра конструктора должны быть зарегестрированны в container 
                    if (Container.Dependencies.TryGetValue(p.ParameterType, out var dependencies)) 
                    {
                        return dependencies[0].GetInstance();
                    }
                    throw new InvalidOperationException("No dependency registered for parameter");
                })
                .ToArray();
            return constructor.Invoke(cParams);
        }
        public virtual object GetInstance()
        {
            lock (this) //синхронизация для многопоточности
            {
                return LifeCycle switch
                {
                    LifeCycle.Instance => Create(), //создаем новый
                    LifeCycle.Singleton => _instance ??= Create(), //если instance == null создаем её иначе возвращаем готовую
                    _ => throw new ArgumentException(null, nameof(LifeCycle)),
                };
            }
        }
    }
}
