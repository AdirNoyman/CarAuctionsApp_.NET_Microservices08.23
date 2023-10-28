using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    // Enrich the user profile with data from identity service
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // Get the user ID from Identity service
            var user = await _userManager.GetUserAsync(context.Subject);
            // Get the user data (like the user's full name) from Claims
            var existingClaims = await _userManager.GetClaimsAsync(user);

            // Create the list of user data, taken from the claims
            var claims = new List<Claim>
            {
                new Claim("username", user.UserName)
            };

            // Add the list of user data (claims) to the profile data
            context.IssuedClaims.AddRange(claims);
            // Add the user full name (Name) to the JWT 
            context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));

        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}