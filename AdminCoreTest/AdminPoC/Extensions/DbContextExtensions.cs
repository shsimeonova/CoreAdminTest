namespace AdminPoC.Extensions
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public static class DbContextExtensions
    {
        public static object Set(this DbContext db, Type type)
            => db.GetType()
                .GetMethod("Set", Array.Empty<Type>())
                ?.MakeGenericMethod(type)
                .Invoke(db, null);
    }
}