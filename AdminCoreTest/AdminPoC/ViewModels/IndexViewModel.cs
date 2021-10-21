namespace AdminPoC.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class IndexViewModel
    {
        public IEnumerable<object> Entities { get; set; }

        public IEnumerable<EntityColumn> Columns { get; set; }
    }
}