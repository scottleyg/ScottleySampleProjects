using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dispatcher;

namespace DynamicEntityApiControllers
{
    /// <summary>
    /// 
    /// </summary>
    public class SupportsDynamicControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        private static List<Type> DynamicControllerTypes = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportsDynamicControllerTypeResolver"/> class.
        /// </summary>
        public SupportsDynamicControllerTypeResolver()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportsDynamicControllerTypeResolver"/> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public SupportsDynamicControllerTypeResolver(Predicate<Type> predicate)
            : base(predicate)
        {
        }
        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <param name="assembliesResolver">The assemblies resolver.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1" /> of controllers.
        /// </returns>
        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {            
            var fromBase = base.GetControllerTypes(assembliesResolver);

            return fromBase.Union(DynamicControllerTypes).ToList();
        }

        /// <summary>
        /// Adds the type of the dynamic controller for resolution.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        public static void AddDynamicControllerType(Type controllerType)
        {
            DynamicControllerTypes.Add(controllerType);
        }
    }
}