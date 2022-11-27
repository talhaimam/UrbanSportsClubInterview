using Hangfire.Dashboard;

namespace InterviewService
{
    public class DisableAuthorization : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
