using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Web.SessionState;

namespace DynamicEntityApiControllers
{
    public class Global : System.Web.HttpApplication
    {
        public void Application_Start(object sender, EventArgs e)
        {
            // if you were self hosting, you would get a config differently.
            var config = System.Web.Http.GlobalConfiguration.Configuration;

            // default DefaultHttpControllerTypeResolver intentionally avoids dynamic assemblies.
            // setup a dynamic type supporting type resolver		        
            config.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerTypeResolver), new SupportsDynamicControllerTypeResolver());

            // and setup some default route pattern for these:
            // controller is the name of the collection, e.g. Book for BookEntity or Shop for ShopEntity
            config.Routes.MapHttpRoute("Entity Routes", "{controller}/{key}", new { key = RouteParameter.Optional });

            var containerbuilder = new ContainerBuilder();

            containerbuilder
                .RegisterType<DaoProvider>()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerbuilder
                .RegisterEntityControllers(typeof(EntityObject).Assembly);

            var container = containerbuilder.Build();
            //containerbuilder.RegisterEntityControllers(this.GetType().Assembly);

            // setup dependency resolver.
            config.DependencyResolver = new Autofac.Integration.WebApi.AutofacWebApiDependencyResolver(container);            
        }
    }
}