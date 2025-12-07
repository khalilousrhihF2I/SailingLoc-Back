using System;

namespace Core.Entities
{
    /// <summary>
    /// Represents a period of availability/unavailability for a boat.
    /// Used to store blocked periods, maintenance windows or other manual availability overrides.
    /// </summary>
    public class BoatAvailability
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// FK to the Boat this availability applies to
        /// </summary>
        public int BoatId { get; set; }

        /// <summary>
        /// Start date/time of the period (inclusive)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date/time of the period (exclusive)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// When false, this record marks the boat as unavailable for the period.
        /// When true, it can be used to explicitly mark availability (rare).
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Optional short reason (e.g. "maintenance", "owner_block").
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Optional reference type indicating the origin of this record (e.g. "booking", "blocked").
        /// </summary>
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Optional external reference id (for example the booking id) stored as string.
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Optional details or notes about the period.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// When the record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation to the Boat entity
        /// </summary>
        public Boat Boat { get; set; } = null!;
    }
}
