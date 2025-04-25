namespace BloodDonor.Models
{
    public class City
    {
        public int Id { get; set; }
        public string? CityName { get; set; }
        public int StateID { get; set; } // Foreign key to State
        public StateProprties? State { get; set; } // Navigation property
    }

}
