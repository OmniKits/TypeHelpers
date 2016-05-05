using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

using static TypeMutabilityEnum;

[Flags]
public enum TypeMutabilityEnum
{
    Immutable = 0,
    Writable = 1,
    OpenType = 2,
    OpenGeneric = 4,
}

public static class TypeMutability
{
    static readonly ConcurrentDictionary<Type, Lazy<TypeMutabilityEnum>> MutabilityTable = new ConcurrentDictionary<Type, Lazy<TypeMutabilityEnum>>();

    public static void RegisterMutability(Type type, TypeMutabilityEnum mutability)
        => MutabilityTable[type] = mutability.ToLazy();
    public static TypeMutabilityEnum TryRegisterMutability(Type type, TypeMutabilityEnum mutability)
        => MutabilityTable.GetOrAdd(type, mutability.ToLazy()).Value;

    static TypeMutability()
    {
        RegisterMutability(typeof(bool), Immutable);
        RegisterMutability(typeof(char), Immutable);
        RegisterMutability(typeof(sbyte), Immutable);
        RegisterMutability(typeof(byte), Immutable);
        RegisterMutability(typeof(short), Immutable);
        RegisterMutability(typeof(ushort), Immutable);
        RegisterMutability(typeof(int), Immutable);
        RegisterMutability(typeof(uint), Immutable);
        RegisterMutability(typeof(long), Immutable);
        RegisterMutability(typeof(ulong), Immutable);
        RegisterMutability(typeof(float), Immutable);
        RegisterMutability(typeof(double), Immutable);
        RegisterMutability(typeof(IntPtr), Immutable);
        RegisterMutability(typeof(UIntPtr), Immutable);

        RegisterMutability(typeof(void), Immutable);
        RegisterMutability(typeof(string), Immutable);
        RegisterMutability(typeof(object), OpenType);
        RegisterMutability(typeof(decimal), Immutable);

        RegisterMutability(typeof(Guid), Immutable);
        RegisterMutability(typeof(DateTime), Immutable);
        RegisterMutability(typeof(DateTimeOffset), Immutable);
        RegisterMutability(typeof(TimeSpan), Immutable);

        RegisterMutability(typeof(Nullable<>), OpenGeneric);
    }

    static TypeMutabilityEnum GetMutabilityImpl(Type type)
    {
        TypeMutabilityEnum tmp = Immutable;

        if (type.IsPrimitive)
            return Immutable;

        if (type.IsInterface)
            return OpenType;

        var isClass = type.IsClass;
        if (isClass)
        {
            if (type.IsArray)
                return Writable;

            if (typeof(Delegate).IsAssignableFrom(type))
                return Immutable;

            if (!type.IsSealed)
                tmp |= OpenType;
        }
        else
        {
            if (type.IsEnum)
                return Immutable;
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).AsEnumerable();
        if (isClass)
        {
            if (fields.Any(fi => !fi.IsInitOnly))
                tmp |= Writable;
        }

        var types = fields.Select(fi => fi.FieldType);

        var isGeneric = type.IsGenericType;
        Type generic = null;
        if (type.IsGenericType)
        {
            if (type.IsGenericTypeDefinition)
            {
                types = types.Where(t => !t.IsGenericParameter);
                tmp |= OpenGeneric;
            }
            else
            {
                generic = type.GetGenericTypeDefinition();
                tmp |= (GetMutability(generic) & ~OpenGeneric);
            }
        }

        return types.Distinct().Aggregate(tmp, (a, t) => a | GetMutability(t));
    }
    public static TypeMutabilityEnum GetMutability(this Type type)
    {
        if (type == null) return Immutable;
        return MutabilityTable.GetOrAdd(type, new Lazy<TypeMutabilityEnum>(() => GetMutabilityImpl(type))).Value;
    }
}
