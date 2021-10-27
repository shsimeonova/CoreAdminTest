namespace AdminPoC.ViewModels
{
    using System;
    using System.Collections.Generic;
    using AdminPoC.Models;

    public class IndexViewModel
    {
        public IEnumerable<EntityBase> Entities { get; set; }

        public IEnumerable<EntityColumn> Columns { get; set; }

        public IEnumerable<EntityAction> Actions { get; set; }
    }
}