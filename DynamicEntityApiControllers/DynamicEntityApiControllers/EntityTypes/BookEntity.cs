using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicEntityApiControllers.EntityTypes
{
    public class BookEntity : EntityObject
    {
        public string Title { get; set; }

        public int PageCount { get; set; }
    }
}