using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorPartTab
{
    using System;

    [Serializable]
    public class ListSearchData
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ListName { get; set; }
        public string filterFields { get; set; }
        public string displayFields { get; set; }
    }
    [Serializable]
    public class ListSettings
    {
        public string ListName { get; set; }
        public string filterFields { get; set; }
        public string displayFields { get; set; }
        public string filterFieldsNames { get; set; }
        public string displayFieldsNames { get; set; }
        public string filterFieldsOrders { get; set; }
        public string displayFieldsOrders { get; set; }
    }
}
