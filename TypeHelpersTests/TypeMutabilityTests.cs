using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using static TypeMutabilityEnum;

public class TypeMutabilityTests
{
    enum MyEnum { }
    struct MyStruct
    {
    }

    struct MyStruct<T>
    {
        T Data;
    }

    sealed class MyClass
    {
        int Value;
    }

    class MyClass<T>
    {
        readonly T Value;
    }

    [Theory]
    [InlineData(typeof(DBNull), Immutable)]
    [InlineData(typeof(Nullable<>), OpenGeneric)]
    [InlineData(typeof(string), Immutable)]
    [InlineData(typeof(void), Immutable)]
    [InlineData(typeof(bool), Immutable)]
    [InlineData(typeof(bool?), Immutable)]
    [InlineData(typeof(Tuple<bool>), OpenType)]
    [InlineData(typeof(Tuple<object>), OpenType)]
    [InlineData(typeof(byte[]), Writable)]
    [InlineData(typeof(List<byte>), Writable | OpenType)]
    [InlineData(typeof(Tuple<byte[]>), OpenType | Writable)]
    [InlineData(typeof(MyEnum), Immutable)]
    [InlineData(typeof(MyStruct), Immutable)]
    [InlineData(typeof(MyStruct<>), OpenGeneric)]
    [InlineData(typeof(MyStruct<object>), OpenType)]
    [InlineData(typeof(MyStruct<byte>), Immutable)]
    [InlineData(typeof(MyClass), Writable)]
    [InlineData(typeof(MyClass<>), OpenGeneric | OpenType)]
    [InlineData(typeof(MyClass<IntPtr>), OpenType)]
    [InlineData(typeof(MyClass<object>), OpenType)]
    public void Regular(Type type, TypeMutabilityEnum mutability)
    => Assert.Equal(mutability, TypeMutability.GetMutability(type));
}
