using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicEntityApiControllers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EntityObject
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
    }
}