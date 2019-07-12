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

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="RegistrationExtensions" />
    /// </summary>
    public static class RegistrationExtensions
    {
        #region 方法

        /// <summary>
        /// The AsInheritedClasses
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <param name="registration">The registration<see cref="IRegistrationBuilder{TLimit, ScanningActivatorData, DynamicRegistrationStyle}"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{TLimit, ScanningActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> AsInheritedClasses<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            return registration.As(t => GetInheritedClasses(t));
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="@delegate">The delegate<see cref="Func{IComponentContext, IEnumerable{Parameter}, T}"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{T, SimpleActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilderWrapper builder, Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            return builder.ContainerBuilder.Register<T>(@delegate);
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="@delegate">The delegate<see cref="Func{IComponentContext, T}"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{T, SimpleActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilderWrapper builder, Func<IComponentContext, T> @delegate)
        {
            return builder.ContainerBuilder.Register<T>(@delegate);
        }

        /// <summary>
        /// The RegisterAdapter
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="adapter">The adapter<see cref="Func{TFrom, TTo}"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(this ContainerBuilderWrapper builder, Func<TFrom, TTo> adapter)
        {
            return builder.ContainerBuilder.RegisterAdapter<TFrom, TTo>(adapter);
        }

        /// <summary>
        /// The RegisterAssemblyTypes
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="assemblies">The assemblies<see cref="Assembly[]"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{object, ScanningActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyTypes(this ContainerBuilderWrapper builder, params Assembly[] assemblies)
        {
            return builder.ContainerBuilder.RegisterAssemblyTypes(assemblies);
        }

        /// <summary>
        /// The RegisterDecorator
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="decorator">The decorator<see cref="Func{TService, TService}"/></param>
        /// <param name="fromKey">The fromKey<see cref="object"/></param>
        /// <param name="toKey">The toKey<see cref="object"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{TService, LightweightAdapterActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(this ContainerBuilderWrapper builder, Func<TService, TService> decorator, object fromKey, object toKey = null)
        {
            return builder.ContainerBuilder.RegisterDecorator<TService>(decorator, fromKey, toKey);
        }

        /// <summary>
        /// The RegisterGeneratedFactory
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory<TDelegate>(this ContainerBuilderWrapper builder)
            where TDelegate : class
        {
            return builder.ContainerBuilder.RegisterGeneratedFactory<TDelegate>();
        }

        /// <summary>
        /// The RegisterGeneratedFactory
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="delegateType">The delegateType<see cref="Type"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory(this ContainerBuilderWrapper builder, Type delegateType)
        {
            return builder.ContainerBuilder.RegisterGeneratedFactory(delegateType);
        }

        /// <summary>
        /// The RegisterGeneric
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="implementor">The implementor<see cref="Type"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{object, ReflectionActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> RegisterGeneric(this ContainerBuilderWrapper builder, Type implementor)
        {
            return builder.ContainerBuilder.RegisterGeneric(implementor);
        }

        /// <summary>
        /// The RegisterGenericDecorator
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="decoratorType">The decoratorType<see cref="Type"/></param>
        /// <param name="decoratedServiceType">The decoratedServiceType<see cref="Type"/></param>
        /// <param name="fromKey">The fromKey<see cref="object"/></param>
        /// <param name="toKey">The toKey<see cref="object"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle> RegisterGenericDecorator(this ContainerBuilderWrapper builder, Type decoratorType, Type decoratedServiceType, object fromKey, object toKey = null)
        {
            return builder.ContainerBuilder.RegisterGenericDecorator(decoratorType, decoratedServiceType, fromKey, toKey);
        }

        /// <summary>
        /// The RegisterInstance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="instance">The instance<see cref="T"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{T, SimpleActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterInstance<T>(this ContainerBuilderWrapper builder, T instance)
            where T : class
        {
            return builder.ContainerBuilder.RegisterInstance<T>(instance);
        }

        /// <summary>
        /// The RegisterModule
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        public static void RegisterModule<TModule>(this ContainerBuilderWrapper builder)
            where TModule : IModule, new()
        {
            builder.ContainerBuilder.RegisterModule<TModule>();
        }

        /// <summary>
        /// The RegisterModule
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="module">The module<see cref="IModule"/></param>
        public static void RegisterModule(this ContainerBuilderWrapper builder, IModule module)
        {
            builder.ContainerBuilder.RegisterModule(module);
        }

        /// <summary>
        /// The RegisterType
        /// </summary>
        /// <typeparam name="TImplementor"></typeparam>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<TImplementor, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TImplementor>(this ContainerBuilderWrapper builder)
        {
            return builder.ContainerBuilder.RegisterType<TImplementor>();
        }

        /// <summary>
        /// The RegisterType
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="implementationType">The implementationType<see cref="Type"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{object, ConcreteReflectionActivatorData, SingleRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(this ContainerBuilderWrapper builder, Type implementationType)
        {
            return builder.ContainerBuilder.RegisterType(implementationType);
        }

        /// <summary>
        /// The RegisterTypes
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="implementationTypes">The implementationTypes<see cref="Type []"/></param>
        /// <returns>The <see cref="IRegistrationBuilder{object, ScanningActivatorData, DynamicRegistrationStyle}"/></returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterTypes(this ContainerBuilderWrapper builder, params Type[] implementationTypes)
        {
            return builder.ContainerBuilder.RegisterTypes(implementationTypes);
        }

        /// <summary>
        /// The GetInheritedClasses
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="List{Type}"/></returns>
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

        #endregion 方法
    }
}