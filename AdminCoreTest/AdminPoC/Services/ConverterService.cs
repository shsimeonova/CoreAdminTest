namespace AdminPoC.Services
{
    using System;

    public class ConverterService
    {
        public object ConvertToType(string value, Type t)
        {
            return t.IsEnum
                ? Enum.Parse(t, value)
                : Convert.ChangeType(value, t);
        }
    }
}