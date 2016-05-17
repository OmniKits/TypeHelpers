using System;
using System.Linq;
using System.Reflection;

#if NO_TYPEINFO
using Type = System.Type;
using TypeInfo = System.Type;
#endif

public static class TypeHelpers
{
#if NO_TYPEINFO
    internal static TypeInfo GetTypeInfo(this Type type) => type;
#endif

    public const BindingFlags InstanceMemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    public const BindingFlags InstanceLocalMemberBindingFlags = InstanceMemberBindingFlags | BindingFlags.DeclaredOnly;

    public const BindingFlags StaticMemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    public const BindingFlags StaticLocalMemberBindingFlags = StaticMemberBindingFlags | BindingFlags.DeclaredOnly;

    public const BindingFlags AllMemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    public const BindingFlags AllLocalMemberBindingFlags = AllMemberBindingFlags | BindingFlags.DeclaredOnly;

    public static bool IsVisibleToDerived(this MethodAttributes attributes, bool sameAssembly = false)
    {
        switch (attributes & MethodAttributes.MemberAccessMask)
        {
            case MethodAttributes.Public:
            case MethodAttributes.Family:
            case MethodAttributes.FamORAssem:
                return true;
            case MethodAttributes.Assembly:
            case MethodAttributes.FamANDAssem:
                return sameAssembly;
        }
        return false;
    }
    public static bool IsVisibleToDerived(this MethodBase self, bool sameAssembly = false)
        => self.Attributes.IsVisibleToDerived(sameAssembly);

    public static bool IsVisibleToDerived(this FieldAttributes attributes, bool sameAssembly = false)
    {
        switch (attributes & FieldAttributes.FieldAccessMask)
        {
            case FieldAttributes.Public:
            case FieldAttributes.Family:
            case FieldAttributes.FamORAssem:
                return true;
            case FieldAttributes.Assembly:
            case FieldAttributes.FamANDAssem:
                return sameAssembly;
        }
        return false;
    }
    public static bool IsVisibleToDerived(this FieldInfo self, bool sameAssembly = false)
        => self.Attributes.IsVisibleToDerived(sameAssembly);

    public static bool IsVisibleToDerived(this TypeAttributes attributes, bool sameAssembly = false)
    {
        switch (attributes & TypeAttributes.VisibilityMask)
        {
            case TypeAttributes.Public:
            case TypeAttributes.NestedPublic:
            case TypeAttributes.NestedFamily:
            case TypeAttributes.NestedFamORAssem:
                return true;
            case TypeAttributes.NotPublic:
            case TypeAttributes.NestedAssembly:
            case TypeAttributes.NestedFamANDAssem:
                return sameAssembly;
        }
        return false;
    }
    public static bool IsVisibleToDerived(this TypeInfo self, bool sameAssembly = false)
        => self.Attributes.IsVisibleToDerived(sameAssembly);
#if !NO_TYPEINFO
    public static bool IsVisibleToDerived(this Type self, bool sameAssembly = false)
        => self.GetTypeInfo().IsVisibleToDerived(sameAssembly);
#endif

#if !NO_BINDER
    public static ConstructorInfo GetInheritableConstructor(this Type self, params Type[] types)
    {
#if FOR_REFERENCE
        throw new NotImplementedException();
#else
        var candidates = from item in self.GetConstructors(InstanceMemberBindingFlags)
                         where item.IsVisibleToDerived()
                         select item;

        return Type.DefaultBinder.SelectMethod(InstanceMemberBindingFlags, 
            candidates.ToArray(), types, null) as ConstructorInfo;
#endif
    }
#endif
}
