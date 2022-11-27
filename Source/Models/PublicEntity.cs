using System;

namespace InterviewService.Models
{
    /// <summary>
    /// Items that need to sync to other systems should inherit off this.
    /// </summary>
    public abstract class PublicEntity
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// When this item was created or last updated.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// When this item was created.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// Who created this item (user ID/Service name)  
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// When this item was updated 
        /// </summary>
        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// Who updated this item (user ID/Service name)  
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// If null, this item has not been deleted. Otherwise, the date tells us when it was deleted.
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }
    }
}
