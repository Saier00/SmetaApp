using SmetaApp.Models;
using Ninject.Modules;

namespace SmetaApp.Util
{
    public class NinjectRegistrations : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobRepository>().To<EFRepository>().InTransientScope();
            Bind<IMatPriceRepository>().To<EFRepository>().InTransientScope();
            Bind<IMatRepository>().To<EFRepository>().InTransientScope();
            Bind<IMechPriceRepository>().To<EFRepository>().InTransientScope();
            Bind<IMechRepository>().To<EFRepository>().InTransientScope();
            Bind<IMechNameMapRepository>().To<EFRepository>().InTransientScope();
            Bind<IMatNameMapRepository>().To<EFRepository>().InTransientScope();
        }
    }
}