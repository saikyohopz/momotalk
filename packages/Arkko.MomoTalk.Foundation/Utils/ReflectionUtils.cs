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

        if (nullableAttr != null) {
            byte flag = nullableAttr.NullableFlags.FirstOrDefault();

            return flag == 0b10;
        }

        return true;
    }

    public async static Task<object?> AwaitObjectAsync(object? obj) {
        if (obj == null) {
            return null;
        }

        Type type = obj.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
            dynamic awaitable = obj;
            await awaitable;
            return awaitable.Result;
        }

        if (type == typeof(Task)) {
            await (Task)obj;

            return null;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>)) {
            dynamic awaitable = obj;
            await awaitable;
            return awaitable.Result;
        }

        if (type == typeof(ValueTask)) {
            await (ValueTask)obj;

            return null;
        }

        return obj;
    }
}
