using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;

namespace EstiloMB.MVC
{
    public class HasClaim : Attribute, IAuthorizationFilter
    {
        private readonly string[] _types;

        //public HasClaim(string type, object value)
        //{
        //    _type = type;
        //    _value = value.GetType().IsEnum ? ((int)value).ToString() : value.ToString();
        //}

        public HasClaim(params string[] types)
        {
            _types = types;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }

            bool hasClaim = context.HttpContext.User.Claims.Any(c => _types.Contains(c.Type));
            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    //public class HasClaim : TypeFilterAttribute
    //{
    //    public HasClaim(string claimType, string claimValue) : base(typeof(ClaimRequirementFilter))
    //    {
    //        Arguments = new object[] { new Claim(claimType, claimValue) };
    //    }
    //}    

    //public class ClaimRequirementFilter : IAuthorizationFilter
    //{
    //    private readonly Claim _claim;

    //    public ClaimRequirementFilter(Claim claim)
    //    {
    //        _claim = claim;
    //    }

    //    public void OnAuthorization(AuthorizationFilterContext context)
    //    {
    //        bool hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
    //        if (!hasClaim)
    //        {
    //            context.Result = new ForbidResult();
    //        }
    //    }
    //}
}