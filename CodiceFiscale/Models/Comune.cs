using SQLite;

namespace CodiceFiscale.Models
{
    public class Comune
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed] // Indicizzato per ricerche veloci
        public string Nome { get; set; }

        [Indexed(Unique = true)]
        public string CodiceCatastale { get; set; } // Es: H501 per Roma
    }
}
