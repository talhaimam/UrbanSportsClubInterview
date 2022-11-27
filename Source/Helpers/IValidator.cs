namespace InterviewService.Helpers
{
    /// <summary>
    /// A simple interface that will be implemented by any settings that require validation
    /// </summary>
    public interface IValidator
    {
        void Validate();
    }
}