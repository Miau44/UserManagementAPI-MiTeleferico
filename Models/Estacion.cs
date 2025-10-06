namespace UserManagementAPI.Models
{
    public class Estacion
    {
        public int EstacionId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Linea { get; set; } = null!;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}
