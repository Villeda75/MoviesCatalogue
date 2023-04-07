using Microsoft.EntityFrameworkCore;
using MoviesCatalogue.Context;
using MoviesCatalogue.Models;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace MoviesCatalogue.Classes
{
    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }

        public static int GetClaimId(ClaimsIdentity identity)
        {
            int claimId = 0;

            try
            {
                if (identity.Claims.Any())
                {

                    var Id = identity.Claims.FirstOrDefault(x => x.Type == "Id").Value;

                    if (int.TryParse(Id, out int userId))
                    {
                        return userId;
                    }
                }

                return claimId;
            }
            catch (Exception)
            {
                return claimId;
            }
        }

    }
}
