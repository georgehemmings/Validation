using System.Collections.Generic;
using Funq;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.FluentValidation;

namespace Validation
{
    // Run before AppHost.Configure()
    public class ConfigureAuth : IConfigureAppHost
    {
        public void Configure(IAppHost appHost)
        {
            var AppSettings = appHost.AppSettings;
            appHost.Plugins.Add(new AuthFeature(() => new CustomUserSession(),
                new IAuthProvider[] {
                    new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
                }));

            appHost.Plugins.Add(new RegistrationFeature());

            //override the default registration validation with your own custom implementation
            appHost.RegisterAs<CustomRegistrationValidator, IValidator<Register>>();

            appHost.Register<IAuthRepository>(new InMemoryAuthRepository());

            CreateUser(appHost, "admin@email.com", "Admin User", "p@55wOrd", roles:new[]{ RoleNames.Admin });
        }

        // Add an additional User to the configured Auth Repository
        public void CreateUser(IAppHost appHost, string email, string name, string password, string[] roles)
        {
            var authRepo = appHost.TryResolve<IAuthRepository>();
            var newAdmin = new UserAuth { Email = email, DisplayName = name };
            var user = authRepo.CreateUserAuth(newAdmin, password);
            authRepo.AssignRoles(user, roles);
        }
    }
    
    // Type class to store additional metadata in Users Session
    public class CustomUserSession : AuthUserSession {}
    
    // Custom Validator to add custom validators to built-in /register Service requiring DisplayName and ConfirmPassword
    public class CustomRegistrationValidator : RegistrationValidator
    {
        public CustomRegistrationValidator()
        {
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.DisplayName).NotEmpty();
                RuleFor(x => x.ConfirmPassword).NotEmpty();
            });
        }
    }

}