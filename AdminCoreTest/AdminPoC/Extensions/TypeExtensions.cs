namespace AdminPoC.Extensions
{
    using System;

    public static class TypeExtensions
    {
        public static bool IsSubclassOfRawGeneric(this Type type, Type genericParent)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericParent == cur)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}