using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access"),

        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
          // Client for dev only. This one is for Postman only
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                // What services will this token be used for
                AllowedScopes = {"openid", "profile", "auctionApp"},
                // Mockes redirect back to resource app asking the user for the token
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
                // What the user is suppose to deliver in order to recieve the token (user + password)
                // ResourceOwner = user
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword},
            }

        };
}
