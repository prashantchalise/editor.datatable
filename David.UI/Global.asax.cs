using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using David.UI.Modules;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace David.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            //Autofac Configuration
            var builder = new Autofac.ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();


            builder.RegisterModule(new ServiceModule());
            builder.RegisterModule(new EFModule());


            //var x = new ApplicationDbContext();
            //builder.Register<ApplicationDbContext>(c => x);
            //builder.Register<UserStore<ApplicationUser>>(c => new UserStore<ApplicationUser>(x)).AsImplementedInterfaces();
            //builder.Register<IdentityFactoryOptions<ApplicationUserManager>>(c => new IdentityFactoryOptions<ApplicationUserManager>()
            //{
            //	DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("ApplicationName")
            //});
            //builder.RegisterType<ApplicationUserManager>();

            var container = builder.Build();

            //DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Set the dependency resolver for MVC.
            var mvcResolver = new AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(mvcResolver);

            // Set the dependency resolver for Web API.
            var webApiResolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;

        }
    }
}
