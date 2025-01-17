using System;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Reflection;
using System.Reflection.Emit;

namespace o4ko.Helpers
{
    public class BatchReader
    {
        private readonly string _assemblyName = "DynamicClasses";
        private readonly string _moduleName = "DynamicClassesModule";

        /// <summary>
        /// Generates a dynamic class based on the provided highlights.
        /// </summary>
        /// <param name="highlights">List of highlights with their names, data types, and values.</param>
        /// <param name="className">Name of the class to generate.</param>
        /// <returns>The Type of the dynamically created class.</returns>
        public object GenerateClass(List<Highlight> highlights, string className)
        {
            if (highlights == null || highlights.Count == 0)
                throw new ArgumentException("Highlight list cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Class name cannot be null or whitespace.");

            // Create an assembly and module
            var assemblyName = new AssemblyName(_assemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(_moduleName);

            // Define the new class
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);

            // Create a constructor with parameters for each property
            var constructorParams = new List<Type>();
            foreach (var highlight in highlights)
            {
                var type = GetTypeFromString(highlight.DataType);
                AddProperty(typeBuilder, highlight.Name, type);
                constructorParams.Add(type);
            }

            DefineConstructor(typeBuilder, highlights, constructorParams.ToArray());

            // Create the type
            var aboba = typeBuilder.CreateType();
            object dynamicObject = Activator.CreateInstance(aboba, highlights.Select(h => ConvertValue(h.RecognizedText, h.DataType)).ToArray());
            Type dynamicType = dynamicObject.GetType();
            foreach (var highlight in highlights)
            {
                string propertyName = highlight.Name;
                if (dynamicType.GetProperty(propertyName) != null)
                {
                    // Set the property value
                    dynamicType.GetProperty(propertyName).SetValue(dynamicObject, ConvertValue(highlight.RecognizedText, highlight.DataType) ); // Replace "New Value" with the actual value
                }
            }

            return dynamicObject;
        }

        /// <summary>
        /// Adds a property to the TypeBuilder.
        /// </summary>
        /// <param name="typeBuilder">The TypeBuilder to add the property to.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        private void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getterMethodBuilder = typeBuilder.DefineMethod(
                $"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes
            );

            var getterIL = getterMethodBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            var setterMethodBuilder = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { propertyType }
            );

            var setterIL = setterMethodBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethodBuilder);
            propertyBuilder.SetSetMethod(setterMethodBuilder);
        }

        /// <summary>
        /// Defines a constructor for the dynamic type.
        /// </summary>
        /// <param name="typeBuilder">The TypeBuilder to add the constructor to.</param>
        /// <param name="highlights">List of highlights to determine constructor parameters.</param>
        /// <param name="constructorParams">Types of constructor parameters.</param>
        private void DefineConstructor(TypeBuilder typeBuilder, List<Highlight> highlights, Type[] constructorParams)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParams
            );

            var ilGenerator = constructorBuilder.GetILGenerator();

            // Call the base constructor
            ilGenerator.Emit(OpCodes.Ldarg_0);
            var baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes);
            ilGenerator.Emit(OpCodes.Call, baseConstructor);

            // Assign each parameter to its respective field
            //for (int i = 0; i < highlights.Count; i++)
            //{
            //    ilGenerator.Emit(OpCodes.Ldarg_0); // Load 'this'
            //    ilGenerator.Emit(OpCodes.Ldarg_S, i + 1); // Load constructor argument
            //    var field = typeBuilder.GetField($"_{highlights[i].Name}", BindingFlags.NonPublic | BindingFlags.Instance);//
            //    ilGenerator.Emit(OpCodes.Stfld, field); // Store the value in the field
            //}

            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Converts a string representation of a type to its Type object.
        /// </summary>
        /// <param name="dataType">The data type as a string (e.g., "int", "string").</param>
        /// <returns>The corresponding Type object.</returns>
        private Type GetTypeFromString(string dataType)
        {
            return dataType.ToLower() switch
            {
                "int" => typeof(int),
                "string" => typeof(string),
                "double" => typeof(double),
                "bool" => typeof(bool),
                "float" => typeof(float),
                _ => throw new ArgumentException($"Unsupported data type: {dataType}")
            };
        }
        private object ConvertValue(string value, string dataType)
        {
            return dataType.ToLower() switch
            {
                "int" => int.TryParse(value, out int i) ? i : 0,
                "double" => double.TryParse(value, out double d) ? d : 0,
                "bool" => bool.TryParse(value, out bool b) ? b : false,
                "float" => float.TryParse(value, out float f) ? f : 0f,
                _ => value // Default to string
            };
        }
    }
}