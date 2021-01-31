using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

using SmetaApp.Models;

namespace SmetaApp.Util
{
    public class InitializersRegistration : DropCreateDatabaseAlways<SmetaContext>
    {
        protected override void Seed(SmetaContext context)
        {
            JobInitializerTesting.Seed(context);
            RolesUsersInitializer.Seed(context);

            base.Seed(context);
        }

    }
}