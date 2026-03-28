using System;

namespace Core.Entities
{
    /// <summary>
    /// Tarification saisonnière : prix différent selon la période (haute/basse saison).
    /// </summary>
    public class BoatPricePeriod
    {
        public int Id { get; set; }
        public int BoatId { get; set; }
        public string Label { get; set; } = ""; // e.g. "Haute saison", "Basse saison"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PricePerDay { get; set; }

        public Boat Boat { get; set; } = null!;
    }
}
