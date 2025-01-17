
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate;

#if NETFRAMEWORK
public interface IServiceProvider
{
    T? GetService<T>();
}
public class ServiceCollection : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, Func<object?>> _transientServices = new ConcurrentDictionary<Type, Func<object?>>();
    private readonly ConcurrentDictionary<Type, object> _singletonServices = new ConcurrentDictionary<Type, object>();

    // 注册瞬态服务（每次请求都创建一个新实例）
    public void AddTransient<TService, TImplementation>() where TImplementation : TService
    {
        _transientServices[typeof(TService)] = () => Activator.CreateInstance<TImplementation>();
    }

    // 注册单例服务（整个应用程序生命周期内只有一个实例）
    public void AddSingleton<TService, TImplementation>() where TImplementation : TService
    {
        _transientServices[typeof(TService)] = () => GetSingletonInstance(typeof(TService), typeof(TImplementation));
    }

    // 获取单例实例
    private object GetSingletonInstance(Type serviceType, Type implementationType)
    {
        return _singletonServices.GetOrAdd(serviceType, _ => Activator.CreateInstance(implementationType));
    }

    // 获取服务实例
    public T? GetService<T>()
    {
        var serviceType = typeof(T);

        // 检查是否是单例服务
        if (_singletonServices.TryGetValue(serviceType, out var singletonInstance))
        {
            return (T)singletonInstance;
        }
        // 检查是否是瞬态服务
        if (_transientServices.TryGetValue(serviceType, out var factory))
        {
            return (T?)factory();
        }

        throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
    }
    public IServiceProvider BuildServiceProvider()
    {
        return this;
    }
}
public class ApplicationServiceCollection : ServiceCollection
{
}

#else
using Microsoft.Extensions.DependencyInjection;
public class ApplicationServiceCollection : ServiceCollection
{
}
#endif

