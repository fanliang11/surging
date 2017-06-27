// Copyright 2007-2010 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.TypeScanning
{
    using System;


    public static class ScannerExtensions
    {
        public static bool Closes(this Type type, Type openType)
        {
            if (!openType.IsGenericType && !openType.IsGenericTypeDefinition)
                return false;

            bool closes = false;

            if (openType.IsInterface)
                closes = type.ImplementsInterfaceTemplate(openType) && type.IsGenericTypeDefinition == false;
            else
            {
                if (type.BaseType != null)
                {
                    if (type.BaseType.IsGenericType)
                        closes = type.BaseType.GetGenericTypeDefinition() == openType;
                    else
                        closes = type.BaseType == openType;
                }
            }
            if (closes)
                return true;
            return type.BaseType == null ? false : type.BaseType.Closes(openType);
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
        }

        public static bool IsConcreteAndAssignableTo(this Type pluggedType, Type pluginType)
        {
            return pluggedType.IsConcrete() && pluginType.IsAssignableFrom(pluggedType);
        }

        public static bool ImplementsInterfaceTemplate(this Type pluggedType, Type templateType)
        {
            if (!pluggedType.IsConcrete())
                return false;

            foreach (var interfaceType in pluggedType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == templateType)
                    return true;
            }

            return false;
        }

        public static bool IsConcrete(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }
    }
}