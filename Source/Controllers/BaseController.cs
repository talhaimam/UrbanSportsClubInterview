using System;
using System.Linq;
using System.Security.Claims;
using InterviewService.Dto.Output;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BaseController : Controller
    {
        protected DbContext Context;

        protected BaseController(DbContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Check to see if the user is authenticated.
        /// Will throw an exception if authentication was attempted (a Bearer token was sent) but the user is still unauthenticated.
        /// </summary>
        protected bool IsAuthenticated()
        {
            // Probably running in a test if this is null or equals test user uuid .
            try
            {
                if (HttpContext == null || this.GetUUIDFromToken(HttpContext.User) == Guid.Parse(Constants.TestUuid))
                    return true;
            }
            // This means the token is not ours and the person is not authenticated
            catch
            {
                return false;
            }

            // https://github.com/aspnet/Security/issues/1310
            var result = HttpContext.AuthenticateAsync().Result;

            // Should cause a 401 error if Failure is not null.
            // Currently will cause a 500 error instead.
            // Perhaps action filter attributes could be setup: https://damienbod.com/2015/09/15/asp-net-5-action-filters/
            if (result.Failure != null)
                throw new System.Exception("Error authenticating.");

            // Succeeded means that they are authenticated.
            return result.Succeeded;
        }

        protected OkObjectResult OkMarkedAsDeleted() => Ok("Marked as deleted.");

        protected AcceptedResult AcceptedJobCreated(string jobId) => Accepted(new MessageDto
        {
            Message = $"Created job with ID `{jobId}`.",
        });

        protected NotFoundObjectResult NotFoundBooking(object id) => base.NotFound(new MessageDto
        {
            Message = $"Booking not found for ID `{id}`.",
        });

        protected NotFoundObjectResult NotFoundBookings(Guid[] ids) => base.NotFound(new MessageDto
        {
            Message = $"Bookings not found for IDs `{ids}`.",
        });

        protected bool HasRole(ClaimsPrincipal user, Guid providerId)
        {
            try
            {
                if (IsAdmin(user))
                {
                    return true;
                }

                Guid uuid = this.GetUUIDFromToken(user);
                var provider = this.Context.Providers.FirstOrDefault(p => p.Id == providerId);

                if (provider == null)
                    return false;

                var role = this.Context.Roles
                    .Where(r => r.ProviderId == provider.Id)
                    .Where(r => r.UserId == uuid)
                    .FirstOrDefault();

                return role != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool IsAdmin(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == "user_type")?.Value == "admin";
        }

        protected Guid GetUUIDFromToken(ClaimsPrincipal user)
        {
            var user_uuid = user.Claims.FirstOrDefault(c => c.Type == "user_uuid")
                ?? throw new UnauthorizedAccessException("Claim user_uuid does not exist");

            Guid.TryParse(user_uuid.Value, out Guid uuid);

            return uuid;
        }
    }
}
