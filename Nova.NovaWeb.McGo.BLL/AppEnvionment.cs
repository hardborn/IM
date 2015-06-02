using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public sealed class AppEnvionment : ISimpleServiceLocator, IStoreData
    {
        private readonly UnityContainer _unityContainer;
        private Dictionary<string, object> _globalData = new Dictionary<string, object>();

        private static AppEnvionment _default;

        private AppEnvionment()
        {
            _unityContainer = new UnityContainer();
        }

        public static AppEnvionment Default
        {
            get
            {
                return (_default ?? (_default = new AppEnvionment()));
            }
        }

        public void Reset()
        {
            _default = null;
        }

        public T Get<T>()
        {
            if (_unityContainer.IsRegistered<T>())
                return _unityContainer.Resolve<T>();
            else
                return default(T);
        }

        public Type Get(Type type)
        {
            return (Type)_unityContainer.Resolve(type);
        }

        public T Get<T>(string key)
        {
            return _unityContainer.Resolve<T>(key);
        }

        public void Inject<T>(T instance)
        {
            _unityContainer.RegisterInstance(instance, new ExternallyControlledLifetimeManager());
        }

        public void InjectAsSingleton<T>(T instance)
        {
            _unityContainer.RegisterInstance(instance);
        }

        public void Register<TInterface, TImplementor>() where TImplementor : TInterface
        {
            _unityContainer.RegisterType<TInterface, TImplementor>();
        }

        public void Register<TType>()
        {
            _unityContainer.RegisterType(typeof(TType), new ExternallyControlledLifetimeManager());
        }

        public void Register<TInterface, TImplementor>(string key) where TImplementor : TInterface
        {
            _unityContainer.RegisterType<TInterface, TImplementor>(key);
        }

        public void RegisterAsSingleton<TInterface, TImplementor>() where TImplementor : TInterface
        {
            _unityContainer.RegisterType<TInterface, TImplementor>(new ContainerControlledLifetimeManager());
        }

        public void RegisterAsSingleton(Type contract, Type implementor)
        {
            _unityContainer.RegisterType(contract, implementor, new ContainerControlledLifetimeManager());
        }

        public void RegisterAsSingleton<TInterface>(TInterface instance)
        {
            _unityContainer.RegisterInstance(instance, new ContainerControlledLifetimeManager());
        }

        public void Register(Type interfaceType, Type implementorType, string key)
        {
            _unityContainer.RegisterType(interfaceType, implementorType, key);
        }

        public void Register(Type interfaceType, Type implementorType)
        {
            _unityContainer.RegisterType(interfaceType, implementorType);
        }

        public void ClearData()
        {
            _globalData.Clear();
        }

        public object this[string key]
        {
            get
            {
                object objectData;
                if (!string.IsNullOrEmpty(key) && _globalData.TryGetValue(key, out objectData))
                {
                    return objectData;
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _globalData[key] = value;
                }
            }
        }
    }
}
