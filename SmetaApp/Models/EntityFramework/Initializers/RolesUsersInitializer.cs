using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace SmetaApp.Models
{
    public class RolesUsersInitializer : DropCreateDatabaseAlways<SmetaContext>
    {
        protected override void Seed(SmetaContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var role1 = new IdentityRole { Name = "Observer" };
            var role2 = new IdentityRole { Name = "Admin" };
            var role3 = new IdentityRole { Name = "User" };
            var role4 = new IdentityRole { Name = "Registrator" };

            roleManager.Create(role1);
            roleManager.Create(role2);
            roleManager.Create(role3);
            roleManager.Create(role4);

            //admin
            var admin = new ApplicationUser { UserName = "admin" };
            string password = "admin123";
            var result = userManager.Create(admin, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(admin.Id, role2.Name);
                userManager.AddToRole(admin.Id, role3.Name);
                userManager.AddToRole(admin.Id, role4.Name);
            }

            //observer
            var obs = new ApplicationUser { UserName = "observer" };
            password = "observer";
            result = userManager.Create(obs, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(obs.Id, role1.Name);
                userManager.AddToRole(obs.Id, role3.Name);
                userManager.AddToRole(obs.Id, role4.Name);
            }

            //testing region, comment it after testing
            #region testing
            var su = new ApplicationUser { UserName = "superuser" };
            password = "superuser";
            result = userManager.Create(su, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(su.Id, role1.Name);
                userManager.AddToRole(su.Id, role2.Name);
                userManager.AddToRole(su.Id, role3.Name);
                userManager.AddToRole(su.Id, role4.Name);
            }

            JobInitializerTesting.Seed(context);
            #endregion

            base.Seed(context);
        }
    }
}