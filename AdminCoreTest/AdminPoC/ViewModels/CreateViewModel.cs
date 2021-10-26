namespace AdminPoC.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class CreateViewModel
    {
        public IEnumerable<InputType> Properties { get; set; }

        public string EntityName { get; set; }
    }

    public class InputType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}