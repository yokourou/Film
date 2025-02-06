using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace app.Services
{
    public class CustomStateAuthProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = (await localStorage.GetAsync<string>("authToken")).Value;
            var identity = string.IsNullOrEmpty(token) ? new ClaimsIdentity() : GetClaimsIdentity(token);
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
            
        }
        public async Task MarkUserAsAuthenticated(string token) 
        {
            await localStorage.SetAsync("authToken",token);
            var identity = GetClaimsIdentity(token);
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public ClaimsIdentity GetClaimsIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler(); 
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims , "jwt");
        }

        public async Task MarkUserAsLoggedOut()
        {
            await localStorage.DeleteAsync("authToken");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

    }

}