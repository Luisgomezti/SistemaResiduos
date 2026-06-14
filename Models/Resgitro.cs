namespace SistemaResiduos.Models
{
    public class Registro
    {
        public int Id { get; set; }

        public string Hotel { get; set; }

        public string Area { get; set; }

        public string Turno { get; set; }

        public decimal Composta { get; set; }

        public decimal NoCompostable { get; set; }

        public decimal Inorganica { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}