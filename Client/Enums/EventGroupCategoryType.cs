namespace InterviewService.Client.Enums
{
    /// <summary>
    /// The category type of an <see cref="EventGroup"/> that are used for FE filtering.
    /// <see cref="EventGroupType"/> is used to denote different behavior of the <see cref="EventGroup"/>s
    /// and most of its current values will be removed.
    /// </summary>
    public enum EventGroupCategoryType
    {
        Class = 0,
        Course = 1,
        Workshop = 2,
        Appointment = 3,
        Retreat = 4,
        Preventions = 5,
        OpenSession = 6,
    }
}
