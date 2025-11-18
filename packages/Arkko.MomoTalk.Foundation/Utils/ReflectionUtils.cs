using System.Reflection;
using System.Runtime.CompilerServices;

namespace Arkko.MomoTalk.Foundation.Utils;

public static class ReflectionUtils {
    public static IEnumerable<Type> FindTypesWithAttribute<TAttribute>() where TAttribute : Attribute {
        return from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.GetCustomAttribute<TAttribute>(true) != null
            select type;
    }

    public static List<MethodInfo> FindMethodsWithAttribute<TAttribute>()
    where TAttribute : Attribute {
        return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            from method in type.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly
            )
            where method.IsDefined(typeof(TAttribute), false)
            select method).ToList();
    }

    public static ReturnInfo AnalyzeReturn(MethodInfo method) {
        ArgumentNullException.ThrowIfNull(method);

        Type returnType = method.ReturnType;
        bool isAsync = method.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false);

        if (returnType == typeof(void)) {
            return new ReturnInfo(false, null, isAsync, false);
        }

        if (returnType == typeof(Task) || returnType == typeof(ValueTask)) {
            return new ReturnInfo(false, null, isAsync, true);
        }

        if (returnType.IsGenericType) {
            Type genericTypeDef = returnType.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(Task<>) || genericTypeDef == typeof(ValueTask<>)) {
                Type actualReturnType = returnType.GetGenericArguments()[0];

                return new ReturnInfo(true, actualReturnType, isAsync, true);
            }
        }

        return new ReturnInfo(true, returnType, isAsync, false);
    }

    public static bool IsParameterNullable(ParameterInfo parameter) {
        ArgumentNullException.ThrowIfNull(parameter);

        Type parameterType = parameter.ParameterType;

        if (parameterType.IsValueType) {
            return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        NullableAttribute? nullableAttr = parameter.GetCustomAttribute<NullableAttribute>();

        if (nullableAttr == null) {
            MethodInfo? method = parameter.Member as MethodInfo;

            if (method != null) {
                nullableAttr = method.GetCustomAttribute<NullableAttribute>();
            }
        }

        byte? flag = nullableAttr?.NullableFlags.FirstOrDefault();

        return flag == 0b10;
    }

    public static bool IsParamsParameter(this ParameterInfo param) {
        ArgumentNullException.ThrowIfNull(param);

        return param.GetCustomAttribute<ParamArrayAttribute>() != null && param.ParameterType.IsValidParamsType();
    }

    private static bool IsValidParamsType(this Type type) {
        if (type.IsValueType) {
            return false;
        }

        if (type.IsArray) {
            return true;
        }

        Type? enumerableType = type.GetGenericIEnumerableType();
        Type? elementType = enumerableType?.GetGenericArguments()[0];

        if (elementType == null) {
            return false;
        }

        return type.HasPublicParameterlessConstructor()
            && type.HasValidAddMethod(elementType);
    }

    private static Type? GetGenericIEnumerableType(this Type type) {
        // 直接实现的接口 + 基类实现的接口
        IEnumerable<Type> allInterfaces = type.GetInterfaces().Concat(
            type.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>()
        );

        // 找到泛型 IEnumerable<T>（排除非泛型 IEnumerable）
        return allInterfaces.FirstOrDefault(i
            => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        );
    }

    private static bool HasPublicParameterlessConstructor(this Type type) {
        return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes) != null;
    }

    private static bool HasValidAddMethod(this Type type, Type elementType) {
        MethodInfo? addMethod = type.GetMethod(
            name: "Add",
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            types: [elementType]
        );

        if (addMethod == null) {
            addMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m =>
                    m.Name == "Add" &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == elementType.MakeArrayType() &&
                    m.GetParameters()[0].GetCustomAttribute<ParamArrayAttribute>() != null
                );
        }

        return addMethod != null;
    }
}
