using System.Reflection;

namespace Geta.Optimizely.ProductFeed.Web.Services;

// Stands in for Optimizely Graph's internal ISyncClient when no Graph credentials are configured.
// Every call completes immediately with an empty result rather than null, because Graph dereferences the results
// (responses, streamed results) without null checks.
internal class NoOpSyncClientProxy : DispatchProxy
{
    private static readonly MethodInfo FromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))!;

    private static readonly MethodInfo EmptyAsyncMethod = typeof(AsyncEnumerable).GetMethod(nameof(AsyncEnumerable.Empty))!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var returnType = targetMethod?.ReturnType;

        if (returnType == null || returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GetGenericArguments()[0];
            return FromResultMethod.MakeGenericMethod(resultType).Invoke(null, [CreateEmpty(resultType)]);
        }

        return CreateEmpty(returnType);
    }

    private static object? CreateEmpty(Type type)
    {
        // The only bool results are "exists" checks; answering true stops Graph from (re)building its schema.
        if (type == typeof(bool))
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        {
            return EmptyAsyncMethod.MakeGenericMethod(type.GetGenericArguments()[0]).Invoke(null, null);
        }

        // Value types get their default; classes with a parameterless constructor (Graph's response types) an empty instance.
        return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;
    }
}
