// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using nanoFramework.Json;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Base class for registries that support conversion and deserialization of objects.
    /// </summary>
    public abstract class RegistryBase
    {
        /// <summary>
        /// Converts a value to the specified primitive type with appropriate type conversion and error handling.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target primitive type to convert to.</param>
        /// <returns>The converted value as the target type.</returns>
        protected static object ConvertToPrimitiveType(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            else if (targetType == typeof(int))
            {
                return Convert.ToInt32(value.ToString());
            }
            else if (targetType == typeof(double))
            {
                return Convert.ToDouble(value.ToString());
            }
            else if (targetType == typeof(bool))
            {
                if (value.ToString().Length == 1)
                {
                    try
                    {
                        return Convert.ToBoolean(Convert.ToByte(value.ToString()));
                    }
                    catch (Exception)
                    {
                    }
                }

                return value.ToString().ToLower() == "true";
            }
            else if (targetType == typeof(long))
            {
                return Convert.ToInt64(value.ToString());
            }
            else if (targetType == typeof(float))
            {
                return Convert.ToSingle(value.ToString());
            }
            else if (targetType == typeof(byte))
            {
                return Convert.ToByte(value.ToString());
            }
            else if (targetType == typeof(short))
            {
                return Convert.ToInt16(value.ToString());
            }
            else if (targetType == typeof(char))
            {
                try
                {
                    return Convert.ToChar(Convert.ToUInt16(value.ToString()));
                }
                catch (Exception)
                {
                    return string.IsNullOrEmpty(value.ToString()) ? '\0' : value.ToString()[0];
                }
            }
            else if (targetType == typeof(uint))
            {
                return Convert.ToUInt32(value.ToString());
            }
            else if (targetType == typeof(ulong))
            {
                return Convert.ToUInt64(value.ToString());
            }
            else if (targetType == typeof(ushort))
            {
                return Convert.ToUInt16(value.ToString());
            }
            else if (targetType == typeof(sbyte))
            {
                return Convert.ToSByte(value.ToString());
            }

            return value;
        }

        /// <summary>
        /// Recursively deserializes a Hashtable into a strongly-typed object by mapping properties and handling nested objects.
        /// </summary>
        /// <param name="hashtable">The Hashtable containing the data to deserialize.</param>
        /// <param name="targetType">The target type to deserialize the data into.</param>
        /// <returns>A new instance of the target type with properties populated from the Hashtable, or null if hashtable or targetType is null.</returns>
        protected static object DeserializeFromHashtable(Hashtable hashtable, Type targetType)
        {
            if (hashtable == null || targetType == null)
            {
                return null;
            }

            if (SkillJsonHelper.IsPrimitiveType(targetType) || targetType == typeof(string))
            {
                return hashtable;
            }

            object instance = CreateInstance(targetType);

            MethodInfo[] methods = targetType.GetMethods();

            foreach (MethodInfo method in methods)
            {
                if (!method.Name.StartsWith("set_") || method.GetParameters().Length != 1)
                {
                    continue;
                }

                string propertyName = method.Name.Substring(4);

                if (!hashtable.Contains(propertyName))
                {
                    continue;
                }

                object value = hashtable[propertyName];
                if (value == null)
                {
                    continue;
                }

                try
                {
                    Type propertyType = method.GetParameters()[0].ParameterType;
                    if (SkillJsonHelper.IsPrimitiveType(propertyType) || propertyType == typeof(string))
                    {
                        object convertedValue = ConvertToPrimitiveType(value, propertyType);
                        method.Invoke(instance, new object[] { convertedValue });
                    }
                    else
                    {
                        if (value is string stringValue)
                        {
                            var nestedHashtable = (Hashtable)JsonConvert.DeserializeObject(stringValue, typeof(Hashtable));
                            object nestedObject = DeserializeFromHashtable(nestedHashtable, propertyType);
                            method.Invoke(instance, new object[] { nestedObject });
                        }
                        else if (value is Hashtable nestedHashtable)
                        {
                            object nestedObject = DeserializeFromHashtable(nestedHashtable, propertyType);
                            method.Invoke(instance, new object[] { nestedObject });
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates an instance of a type using its parameterless constructor.
        /// </summary>
        /// <param name="type">The type to create an instance of.</param>
        /// <returns>A new instance of the type.</returns>
        /// <exception cref="Exception">Thrown when the type does not have a parameterless constructor.</exception>
        protected static object CreateInstance(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                throw new Exception($"Type {type.Name} does not have a parameterless constructor");
            }

            return constructor.Invoke(new object[0]);
        }
    }
}
