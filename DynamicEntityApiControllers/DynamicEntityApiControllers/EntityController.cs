using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace DynamicEntityApiControllers
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityController<T> : ApiController
        where T : EntityObject
    {
        private readonly IDaoProvider daoProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityController{T}"/> class.
        /// </summary>
        /// <param name="daoProvider">The DAO provider.</param>
        protected EntityController(IDaoProvider daoProvider)
        {
            this.daoProvider = daoProvider;
        }

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        protected string CollectionName
        {
            get
            {
                return (string)this.RequestContext.RouteData.Values["controller"];
            }
        }

        /// <summary>
        /// Gets the specified entity by key from the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get(string key)
        {            
            return this.daoProvider.Output<T>(this.CollectionName, key).Data;
        }

        /// <summary>
        /// Creates or updates the specified entity by key in the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="updating">The updating.</param>
        /// <returns></returns>
        public T Put(string key, [FromBody] T updating)
        {
            var daoObject = DaoEntity.Create(this.CollectionName, key, updating);
            return this.daoProvider.Input<T>(daoObject).Data;
        }

        /// <summary>
        /// Creates or Updates the collection specified entirely.
        /// </summary>
        /// <param name="replacingCollection">The replacing collection.</param>
        /// <returns></returns>
        public IEnumerable<T> Put([FromBody] IEnumerable<T> replacingCollection)
        {
            return this.daoProvider.Input<T>(replacingCollection
                            .Select(o => DaoEntity.Create(this.CollectionName, o)));
        }

        /// <summary>
        /// creates a new entity in the specified collection.
        /// </summary>
        /// <param name="creating">The creating.</param>
        /// <returns></returns>
        public T Post([FromBody] T creating)
        {
            var daoEntity = DaoEntity.Create(this.CollectionName, creating);
            return this.daoProvider.Input(daoEntity).Data;
        }

        /// <summary>
        /// Deletes the specified entity by key from the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Delete(string key)
        {
            this.daoProvider.Delete(this.CollectionName, key);
        }
    }
}