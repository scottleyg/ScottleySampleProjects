using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DynamicEntityApiControllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the POCO type.</typeparam>
    public class DaoEntity<T> : DaoEntity
        where T : class
    {
        private T data;

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T Data
        {
            get
            {
                return this.data ?? (this.data = JsonConvert.DeserializeObject<T>(this.JsonData));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>    
    public abstract class DaoEntity
    {
        private static readonly MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider();
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        private string entityTag;
        private string jsonData;

        /// <summary>
        /// Gets the JSON data.
        /// </summary>
        /// <value>
        /// The JSON data.
        /// </value>
        public string JsonData
        {
            get { return this.jsonData; }
            set { this.jsonData = value; }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; protected set; }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <value>
        /// The collection.
        /// </value>
        public string Collection { get; protected set; }

        /// <summary>
        /// Gets the entity tag.
        /// </summary>
        /// <value>
        /// The entity tag.
        /// </value>
        public string EntityTag
        {
            get
            {
                return entityTag ?? (entityTag = GetEntityTag(this.JsonData));
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.GetHashCode() == this.GetHashCode();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return 23 * this.EntityTag.GetHashCode()
                    * 29 * (this.Collection ?? string.Empty).GetHashCode()
                    * 37 * (this.Key ?? string.Empty).GetHashCode();
            }

        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("Key: '", this.Key, "', ETag: '", this.EntityTag, "', Data: ", this.JsonData);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(DaoEntity lhs, DaoEntity rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(DaoEntity lhs, DaoEntity rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                return false;
            }

            return !lhs.Equals(rhs);
        }

        private static string GetEntityTag(string jsonSerializedObjectData)
        {
            var stringBytes = Encoding.UTF8.GetBytes(jsonSerializedObjectData);
            var hashBytes = Md5.ComputeHash(stringBytes);
            var etag = Convert.ToBase64String(hashBytes, 0, hashBytes.Length);
            return etag;
        }

        internal static DaoEntity<T> Create<T>(string collection, string key)
            where T : class
        {
            var entity = new DaoEntity<T>();
            entity.Collection = collection;
            entity.Key = key;
            entity.JsonData = string.Empty;

            return entity;
        }


        /// <summary>
        /// Creates the specified entity for the specified <paramref name="collection"/> 
        /// with a generated key
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="data">The POCO.</param>
        /// <returns></returns>
        public static DaoEntity<T> Create<T>(string collection, T data)
            where T : EntityObject
        {
            if (data.Key == null)
            {
                data.Key = GetEntityKey(collection);
            }

            return DaoEntity.Create(collection, data.Key, data);
        }

        /// <summary>
        /// Creates the specified entity for the specified <paramref name="collection"/> 
        /// with the specified <paramref name="key"/>
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="jsonData">The JSON data.</param>
        /// <returns></returns>
        public static DaoEntity<T> Create<T>(string collection, string key, string jsonData)
            where T : EntityObject
        {
            var poco = JsonConvert.DeserializeObject<T>(jsonData);

            if (poco.Key != key)
            {
                poco.Key = key;
            }

            return DaoEntity.Create(collection, key, poco);
        }

        /// <summary>
        /// Creates the specified entity for the specified <paramref name="collection"/> 
        /// with the specified <paramref name="key"/>
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="data">The poco.</param>
        /// <returns></returns>
        public static DaoEntity<T> Create<T>(string collection, string key, T data)
            where T : class
        {
            var entity = DaoEntity.Create<T>(collection, key);
            entity.JsonData = JsonConvert.SerializeObject(data, SerializerSettings);
            return entity;
        }

        private static string GetEntityKey(string collection)
        {
            return Guid.NewGuid().ToString();
        }
    }
}