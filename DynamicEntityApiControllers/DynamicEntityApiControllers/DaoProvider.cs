using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicEntityApiControllers
{
    public class DaoProvider : IDaoProvider
    {
        private static readonly Dictionary<string, Dictionary<string,object>> data = new Dictionary<string,Dictionary<string,object>>(StringComparer.OrdinalIgnoreCase);

        public DaoEntity<T> Input<T>(DaoEntity<T> daoEntity) where T : EntityObject
        {
            var collection = daoEntity.Collection;
            Dictionary<string, object> colData;
            if (data.ContainsKey(collection))
            {
                colData = data[collection];
            }
            else 
            {
                colData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                data.Add(collection, colData);
            }

            if (colData.ContainsKey(daoEntity.Key))
            {
                colData[daoEntity.Key] = daoEntity;
            }
            else
            {
                colData.Add(daoEntity.Key, daoEntity);
            }

            return daoEntity;
        }

        public IEnumerable<T> Input<T>(IEnumerable<DaoEntity<T>> daoEntities) where T : EntityObject
        {
            foreach (var group in daoEntities.GroupBy(d => d.Collection))
            {                
                Dictionary<string, object> colData;
                if (data.ContainsKey(group.Key))
                {
                    colData = data[group.Key];
                    colData.Clear();
                }
                else
                {
                    colData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    data.Add(group.Key, colData);
                }

                foreach (var daoEntity in group)
                {
                    colData.Add(daoEntity.Key, daoEntity);
                }
            }

            return daoEntities.Select(d => d.Data);
        }

        public DaoEntity<T> Output<T>(string collection, string key) where T : EntityObject
        {
            var daoEntity = (T)data[collection][key];
            return DaoEntity.Create(collection, key, daoEntity);
        }

        public IEnumerable<DaoEntity<T>> Output<T>(string collection, IDictionary<string, IEnumerable<string>> fieldQueryValues, int currentPage, int pageSize) where T : EntityObject
        {
            throw new NotImplementedException();
        }

        public void Delete(string collection, string key)
        {
            if (data.ContainsKey(collection))
            {
                var col = data[collection];
                if (col.ContainsKey(key))
                {
                    col.Remove(key);
                }
            }
        }
    }
}