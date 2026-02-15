using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace HerkesYazarOlsun.Portal.Helpers.Extensions
{
    public class UserClaimProvider : IClaimsTransformation
    {
        ClaimsIdentity? identity;
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            identity = principal.Identity as ClaimsIdentity;
            Claim claim = null;
            if (principal.HasClaim(x => x.Type == "email"))
            {
                var mail = principal.Claims.FirstOrDefault(x => x.Type.Equals("email"));
                claim = new Claim("email", mail?.Value.ToString());
                identity.AddClaim(claim);
            }


            return principal;
        }
        public Claim? GETTransformAsync()
        {
            return identity?.Claims.FirstOrDefault();
        }


    }
}
