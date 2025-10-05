using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OXDesk.Core.Identity;

namespace OXDesk.Tests.Helpers;

public static class TestHelpers
{
    // Test user IDs
    public static readonly Guid TestUserId1 = Guid.Parse("10000000-0000-0000-0000-000000010001");
    public static readonly Guid TestUserId2 = Guid.Parse("10000000-0000-0000-0000-000000010002");
    public static readonly Guid TestUserId3 = Guid.Parse("10000000-0000-0000-0000-000000010003");
    

    public static UserManager<ApplicationUser> MockUserManager()
    {
        var store = new Mock<IUserPasswordStore<ApplicationUser>>();
        
        // Setup required methods for IUserStore
        store.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(IdentityResult.Success));
        store.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(IdentityResult.Success));
        store.Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<ApplicationUser?>(new ApplicationUser()));
        store.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<ApplicationUser?>(new ApplicationUser()));
        
        // Setup required methods for IUserPasswordStore
        store.Setup(x => x.SetPasswordHashAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(IdentityResult.Success));
        store.Setup(x => x.GetPasswordHashAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<string?>("hashedPassword"));
        store.Setup(x => x.HasPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));
            
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        passwordHasher.Setup(x => x.HashPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns("hashedPassword");
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();
        
        return new UserManager<ApplicationUser>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);
    }

    public static RoleManager<ApplicationRole> MockRoleManager()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        var roleValidators = new List<IRoleValidator<ApplicationRole>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<ApplicationRole>>>();
        
        return new RoleManager<ApplicationRole>(
            store.Object,
            roleValidators,
            keyNormalizer.Object,
            errors.Object,
            logger.Object);
    }
}
