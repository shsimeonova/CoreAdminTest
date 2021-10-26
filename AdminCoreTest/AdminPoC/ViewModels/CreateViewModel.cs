namespace AdminPoC.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CreateViewModel
    {
        public IEnumerable<InputType> Properties { get; set; }

        public IEnumerable<ComplexInputType> ComplexProperties { get; set; }
    }

    public class InputType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class ComplexInputType
        : InputType
    {
        public IEnumerable<SelectListItem> Values { get; set; }
    }
}