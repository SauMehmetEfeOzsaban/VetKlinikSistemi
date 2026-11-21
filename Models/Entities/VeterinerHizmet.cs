namespace VetKlinikSistemi.Models.Entities
{
    // Hangi veterinerin hangi hizmetleri verebildigini tutan ara tablo
    public class VeterinerHizmet
    {
        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }
    }
}