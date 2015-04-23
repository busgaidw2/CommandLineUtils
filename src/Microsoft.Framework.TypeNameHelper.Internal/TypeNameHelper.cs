// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Framework.Internal
{
    internal class TypeNameHelper
    {
        private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
            {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" }
            };

        public static string GetTypeDisplayName(Type type, bool fullname = true)
        {
            var sb = new StringBuilder();
            ProcessTypeName(type, sb, fullname);
            return sb.ToString();
        }

        private static void AppendGenericArguments(Type[] args, int startIndex, int nArgsToAppend, StringBuilder sb, bool fullName)
        {
            var nArgs = args.Length;
            if (nArgs >= startIndex + nArgsToAppend)
            {
                sb.Append("<");
                for (int i = startIndex; i < startIndex + nArgsToAppend; i++)
                {
                    ProcessTypeName(args[i], sb, fullName);
                    if (i + 1 < startIndex + nArgsToAppend)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(">");
            }
        }

        private static void ProcessTypeName(Type t, StringBuilder sb, bool fullName)
        {
            if (t.GetTypeInfo().IsGenericType)
            {
                ProcessNestedGenericTypes(t, sb, fullName);
                return;
            }
            if (_builtInTypeNames.ContainsKey(t))
            {
                sb.Append(_builtInTypeNames[t]);
            }
            else
            {
                sb.Append(fullName ? t.FullName : t.Name);
            }
        }

        private static void ProcessNestedGenericTypes(Type t, StringBuilder sb, bool fullName)
        {
            var genericFullName = t.GetGenericTypeDefinition().FullName;
            var genericSimpleName = t.GetGenericTypeDefinition().Name;
            var parts = genericFullName.Split('+');
            var genericArguments = t.GetTypeInfo().GenericTypeArguments;
            var index = 0;
            var nParts = parts.Length;
            if (nParts == 1)
            {
                var part = parts[0];
                var num = part.IndexOf('`');
                if (num == -1) return;

                var name = part.Substring(0, num);
                var nGenericTypeArgs = int.Parse(part.Substring(num + 1));
                sb.Append(fullName ? name : genericSimpleName.Substring(0, genericSimpleName.IndexOf('`')));
                AppendGenericArguments(genericArguments, index, nGenericTypeArgs, sb, fullName);
                return;
            }
            for (var i = 0; i < nParts; i++)
            {
                var part = parts[i];
                var num = part.IndexOf('`');
                if (num != -1)
                {
                    var name = part.Substring(0, num);
                    var nGenericTypeArgs = int.Parse(part.Substring(num + 1));
                    if (fullName || i == nParts - 1)
                    {
                        sb.Append(name);
                        AppendGenericArguments(genericArguments, index, nGenericTypeArgs, sb, fullName);
                    }
                    if (fullName && i != nParts - 1)
                    {
                        sb.Append("+");
                    }
                    index += nGenericTypeArgs;
                }
                else
                {
                    if (fullName || i == nParts - 1)
                    {
                        sb.Append(part);
                    }
                    if (fullName && i != nParts - 1)
                    {
                        sb.Append("+");
                    }
                }
            }
        }
    }
}
