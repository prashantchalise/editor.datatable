using Autofac;
using System.Linq;
using System.Reflection;

namespace David.UI.Modules
{
	public class ServiceModule : Autofac.Module
	{

		protected override void Load(ContainerBuilder builder)
		{

			builder.RegisterAssemblyTypes(Assembly.Load("David.Service"))

					  .Where(t => t.Name.EndsWith("Service"))

					  .AsImplementedInterfaces()

					  .InstancePerLifetimeScope();

		}

	}
}