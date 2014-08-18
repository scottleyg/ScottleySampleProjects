using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;

namespace DynamicEntityApiControllers
{
    /// <summary>
    /// 
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Generates dynamic <see cref="EntityController{T}"/> types and registers an adapter to each of them.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="controllerAssemblies">The controller assemblies.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This method is responsible for IL emitting dynamic assemblies and types")]
        public static void RegisterEntityControllers(this ContainerBuilder builder, params Assembly[] controllerAssemblies)
        {
            var entityObjectTypes = controllerAssemblies
                .SelectMany(a =>
                    a.GetTypes()
                        .Where(t =>
                            !t.IsAbstract
                            && !t.IsInterface
                            && t.IsAssignableTo<EntityObject>()))
                .ToList();

            if (entityObjectTypes.Count == 0)
            {
                return;
            }

            var entityControllerAssemblyName = string.Concat("EntityControllerTypes_", Guid.NewGuid());
            var assemblyName = new AssemblyName(entityControllerAssemblyName);

            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            Type baseType = typeof(EntityController<>);

            foreach (var entityObjectType in entityObjectTypes)
            {
                var idxOfEntityWord = entityObjectType.Name.IndexOf("Entity");

                string entityBaseName = idxOfEntityWord < 1 ? entityObjectType.Name : entityObjectType.Name.Substring(0, idxOfEntityWord);

                string controllerTypeName = string.Concat(entityBaseName, "Controller");
                var parent = baseType.MakeGenericType(entityObjectType);

                TypeBuilder typeBuilder = moduleBuilder.DefineType(controllerTypeName, TypeAttributes.Public | TypeAttributes.Class, parent);

                CreatePassThroughConstructors(parent, typeBuilder);

                var generatedType = typeBuilder.CreateType();
                
                builder
                    .RegisterType(generatedType)
                    .AsSelf()
                    .As<System.Web.Http.Controllers.IHttpController>()
                    .InstancePerRequest();

                SupportsDynamicControllerTypeResolver.AddDynamicControllerType(generatedType);
            }
        }

        private static void CreatePassThroughConstructors(Type parent, TypeBuilder typeBuilder)
        {
            foreach (var constructor in parent
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Union(parent
                        .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters.Last().IsDefined(typeof(ParamArrayAttribute), false))
                {
                    //throw new InvalidOperationException("Variadic constructors are not supported");
                    continue;
                }

                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = ctor.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
                    if (((int)parameter.Attributes & (int)ParameterAttributes.HasDefault) != 0)
                    {
                        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                    }

                    foreach (var attribute in BuildCustomAttributes(parameter.GetCustomAttributesData()))
                    {
                        parameterBuilder.SetCustomAttribute(attribute);
                    }
                }

                foreach (var attribute in BuildCustomAttributes(constructor.GetCustomAttributesData()))
                {
                    ctor.SetCustomAttribute(attribute);
                }

                var emitter = ctor.GetILGenerator();
                emitter.Emit(OpCodes.Nop);

                // Load `this` and call base constructor with arguments
                emitter.Emit(OpCodes.Ldarg_0);
                for (var i = 1; i <= parameters.Length; ++i)
                {
                    emitter.Emit(OpCodes.Ldarg, i);
                }
                emitter.Emit(OpCodes.Call, constructor);

                emitter.Emit(OpCodes.Ret);
            }
        }

        private static CustomAttributeBuilder[] BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute =>
            {
                var attributeArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
                var namedPropertyInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
                var namedPropertyValues = attribute.NamedArguments.Where(a => a.MemberInfo is PropertyInfo).Select(a => a.TypedValue.Value).ToArray();
                var namedFieldInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<FieldInfo>().ToArray();
                var namedFieldValues = attribute.NamedArguments.Where(a => a.MemberInfo is FieldInfo).Select(a => a.TypedValue.Value).ToArray();
                return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, namedPropertyInfos, namedPropertyValues, namedFieldInfos, namedFieldValues);
            }).ToArray();
        }
    }

}