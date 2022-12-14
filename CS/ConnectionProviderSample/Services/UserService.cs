using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ConnectionProviderSample.Services {
    public interface IUserService {
        int GetCurrentUserId();
        bool IsCurrentUserAdministrator();
    }

    public class UserService : IUserService {
        readonly IEnumerable<Claim> currentUserClaims;

        public UserService(IHttpContextAccessor httpContextAccessor) {
            if(httpContextAccessor == null) 
                throw new ArgumentNullException(nameof(httpContextAccessor));
            this.currentUserClaims = httpContextAccessor.HttpContext?.User.Claims;
        }

        public int GetCurrentUserId() {
            var sidClaim = currentUserClaims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid);
            return Convert.ToInt32(sidClaim?.Value, CultureInfo.InvariantCulture);
        }

        public bool IsCurrentUserAdministrator() {
            return currentUserClaims?
                .Where(x => x.Type == ClaimTypes.Role)
                .Any(x => x.Value == "Admin")
                ?? false;
        }
    }
}
