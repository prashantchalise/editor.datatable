using Autofac;
using David.Model;

namespace David.UI.Modules
{

	public class EFModule : Autofac.Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType(typeof(RolpoContext)).As(typeof(IContext)).InstancePerLifetimeScope();
		}
	}
}