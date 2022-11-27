namespace InterviewService.Services
{
    public class BaseService
    {
        protected DbContext Context;

        protected BaseService(DbContext context)
        {
            this.Context = context;
        }
    }
}
