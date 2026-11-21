namespace VetKlinikSistemi.Models.Entities
{
    // Veteriner ve UzmanlikAlani arasindaki coktan-coga ara tablo
    public class VeterinerUzmanlik
    {
        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        public int UzmanlikAlaniId { get; set; }
        public UzmanlikAlani? UzmanlikAlani { get; set; }
    }
}