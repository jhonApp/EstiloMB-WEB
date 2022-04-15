using System;
using System.Security.Claims;
using System.Security.Principal;

namespace EstiloMB.MVC
{
    public static class IdentityExtension
    {
        public static bool HasClaim(this IIdentity identity, string claimType)
        {
            return (identity as ClaimsIdentity).HasClaim(c => c.Type == claimType);
        }

        public static string GetClaim(this IIdentity identity, string claimType)
        {
            Claim claim = (identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? claim.Value : null;
        }

        public static int GetInt32Claim(this IIdentity identity, string claimType)
        {
            Claim claim = (identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }

        public static bool GetClaimBool(this IIdentity identity, string claimType)
        {
            Claim claim = (identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? Convert.ToBoolean(claim.Value) : false;
        }

        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? claim.Value : null;
        }

        public static int GetClaimInt32(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }

        public static bool GetClaimBool(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? Convert.ToBoolean(claim.Value) : false;
        }

        public static T GetClaim<T>(this ClaimsPrincipal claimsPrincipal, string claimType) where T : struct
        {
            Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).FindFirst(claimType);
            return claim != null ? Enum.Parse<T>(claim.Value) : default(T);
        }

        public static string GetSessionID(this IIdentity identity)
        {
            Claim claim = (identity as ClaimsIdentity).FindFirst("SessionID");
            return claim?.Value ?? null;
        }

        public static string GetSessionID(this ClaimsPrincipal claimsPrincipal)
        {
            Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).FindFirst("SessionID");
            return claim?.Value ?? null;
        }
    }
}