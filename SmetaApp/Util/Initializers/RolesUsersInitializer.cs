using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using SmetaApp.Models;

namespace SmetaApp.Util
{
    public class RolesUsersInitializer
    {
        static public void Seed(SmetaContext context)
        {
            SeedRoles(context);
            //after RoleInit
            SeedUsers(context);
        }
        static public void SeedRoles(SmetaContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            //Obs can watch user's and admin's progress
            var roleObs = new IdentityRole { Name = RolesNames.Observer };
            //Admin can add new prices and jobs
            var roleAdm = new IdentityRole { Name = RolesNames.Admin };
            //User can browse jobs and prices
            var roleUser = new IdentityRole { Name = RolesNames.User };
            //Registrator can register new users
            var roleReg = new IdentityRole { Name = RolesNames.Registrator };

            roleManager.Create(roleObs);
            roleManager.Create(roleAdm);
            roleManager.Create(roleUser);
            roleManager.Create(roleReg);
        }
        static public void SeedUsers(SmetaContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            //admin
            var admin = new ApplicationUser { UserName = "admin" };
            string password = "admin123";
            var result = userManager.Create(admin, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(admin.Id, RolesNames.Admin);
                userManager.AddToRole(admin.Id, RolesNames.User);
                userManager.AddToRole(admin.Id, RolesNames.Registrator);
            }

            //observer
            var obs = new ApplicationUser { UserName = "observer" };
            password = "observer";
            result = userManager.Create(obs, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(obs.Id, RolesNames.Observer);
                userManager.AddToRole(obs.Id, RolesNames.User);
                userManager.AddToRole(obs.Id, RolesNames.Registrator);
            }

            //testing region, comment it after testing
            #region testing
            var su = new ApplicationUser { UserName = "superuser" };
            password = "superuser";
            result = userManager.Create(su, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(su.Id, RolesNames.Observer);
                userManager.AddToRole(su.Id, RolesNames.Admin);
                userManager.AddToRole(su.Id, RolesNames.User);
                userManager.AddToRole(su.Id, RolesNames.Registrator);
            }
            #endregion
        }
    }
}