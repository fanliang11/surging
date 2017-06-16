using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Surging.Core.System.Module
{
   public static class RegistrationExtensions
    {
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilderWrapper builder, Func<IComponentContext, T> @delegate)
        {
            return builder.ContainerBuilder.Register<T>(@delegate);
        }

        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilderWrapper builder, Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            return builder.ContainerBuilder.Register<T>(@delegate);
        }

        public static void RegisterModule(this ContainerBuilderWrapper builder, IModule module)
        {
            builder.ContainerBuilder.RegisterModule(module);
        }

        public static void RegisterModule<TModule>(this ContainerBuilderWrapper builder)
            where TModule : IModule, new()
        {
            builder.ContainerBuilder.RegisterModule<TModule>();
        }

        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterInstance<T>(this ContainerBuilderWrapper builder, T instance)
            where T : class
        {
            return builder.ContainerBuilder.RegisterInstance<T>(instance);
        }

        public static IRegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TImplementor>(this ContainerBuilderWrapper builder)
        {
            return builder.ContainerBuilder.RegisterType<TImplementor>();
        }

        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(this ContainerBuilderWrapper builder, Type implementationType)
        {
            return builder.ContainerBuilder.RegisterType(implementationType);
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyTypes(this ContainerBuilderWrapper builder, params Assembly[] assemblies)
        {
            return builder.ContainerBuilder.RegisterAssemblyTypes(assemblies);
        }

        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> RegisterGeneric(this ContainerBuilderWrapper builder, Type implementor)
        {
            return builder.ContainerBuilder.RegisterGeneric(implementor);
        }

        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(this ContainerBuilderWrapper builder, Func<TFrom, TTo> adapter)
        {
            return builder.ContainerBuilder.RegisterAdapter<TFrom, TTo>(adapter);
        }

        public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle> RegisterGenericDecorator(this ContainerBuilderWrapper builder, Type decoratorType, Type decoratedServiceType, object fromKey, object toKey = null)
        {
            return builder.ContainerBuilder.RegisterGenericDecorator(decoratorType, decoratedServiceType, fromKey, toKey);
        }

        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(this ContainerBuilderWrapper builder, Func<TService, TService> decorator, object fromKey, object toKey = null)
        {
            return builder.ContainerBuilder.RegisterDecorator<TService>(decorator, fromKey, toKey);
        }


        public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory(this ContainerBuilderWrapper builder, Type delegateType)
        {
            return builder.ContainerBuilder.RegisterGeneratedFactory(delegateType);
        }

        public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory<TDelegate>(this ContainerBuilderWrapper builder)
            where TDelegate : class
        {
            return builder.ContainerBuilder.RegisterGeneratedFactory<TDelegate>();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRepositories(this ContainerBuilderWrapper builder, params Assembly[] repositoriesAssemblies)
        {
            return builder.RegisterAssemblyTypes(repositoriesAssemblies)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsInheritedClasses()
                .AsImplementedInterfaces();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterServices(this ContainerBuilderWrapper builder, params Assembly[] serviceAssemblies)
        {
            return builder.RegisterAssemblyTypes(serviceAssemblies)
                .Where(t => t.Name.EndsWith("Service"))
                .AsInheritedClasses()
                .AsImplementedInterfaces();
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> AsInheritedClasses<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            return registration.As(t => GetInheritedClasses(t));
        }

        private static List<Type> GetInheritedClasses(Type type)
        {
            List<Type> types = new List<Type>();

            Func<Type, Type> GetBaseType = (t) =>
            {
                if (t.GetTypeInfo().BaseType != null && t.GetTypeInfo().BaseType != typeof(object))
                {
                    types.Add(t.GetTypeInfo().BaseType);
                    return t.GetTypeInfo().BaseType;
                }

                return null;
            };

            Type baseType = type;

            do
            {
                baseType = GetBaseType(baseType);
            }
            while (baseType != null);

            return types;
        }
    }
}
