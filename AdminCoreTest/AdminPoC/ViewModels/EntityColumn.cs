namespace AdminPoC.ViewModels
{
    using System;

    public class EntityColumn
    {
        public string Name { get; set; }

        public Func<object, string> Func;
    }
}