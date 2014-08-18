using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicEntityApiControllers
{
    public interface IDaoProvider
    {
        /// <summary>
        /// Inputs the specified data into the cache.
        /// </summary>
        /// <param name="daoEntity">The DAO Entity.</param>
        /// <returns>the Entity Tag (ETag) of the object</returns>
        DaoEntity<T> Input<T>(DaoEntity<T> daoEntity)
            where T : EntityObject;

        /// <summary>
        /// Inputs the specified data into the cache.
        /// </summary>
        /// <param name="daoEntities">The DAO Entities.</param>
        /// <returns>the Entity Tag (ETag) of the object</returns>
        IEnumerable<T> Input<T>(IEnumerable<DaoEntity<T>> daoEntities)
            where T : EntityObject;

        /// <summary>
        /// Outputs the single object by collection and key
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        DaoEntity<T> Output<T>(string collection, string key)
            where T : EntityObject;

        /// <summary>
        /// Outputs the specified object by field query values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="fieldQueryValues">The field query values.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        IEnumerable<DaoEntity<T>> Output<T>(string collection, IDictionary<string, IEnumerable<string>> fieldQueryValues, int currentPage, int pageSize)
            where T : EntityObject;

        /// <summary>
        /// Deletes the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        void Delete(string collection, string key);
    }
}