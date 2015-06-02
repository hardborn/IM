using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface ISimpleServiceLocator
    {
        T Get<T>();
        Type Get(Type type);
        T Get<T>(string key);
        void Inject<T>(T instance);
        void InjectAsSingleton<T>(T instance);
        void Register<TInterface, TImplementor>() where TImplementor : TInterface;
        void Register<TType>();
        void Register<TInterface, TImplementor>(string key) where TImplementor : TInterface;
        void RegisterAsSingleton<TInterface, TImplementor>() where TImplementor : TInterface;
        void RegisterAsSingleton(Type contract, Type implementor);
        void RegisterAsSingleton<TInterface>(TInterface instance);
        void Register(Type interfaceType, Type implementorType, string key);
        void Register(Type interfaceType, Type implementorType);
    }
}
