using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariosData.Models
{
    public class Recebimento
    {
        [Key]
        public int IdRecebimento {  get; set; }

        public DateTime Registro {  get; set; }

        [Required(ErrorMessage = "O nome do recebedor é obrigatório")]
        [StringLength(100)]
        public required string NomeRecebedor { get; set; }

        [Required(ErrorMessage = "O e-mail do recebedor é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(100)]
        public required string EmailRecebedor { get; set; }

        [Required]
        [StringLength(150)]
        public required string LocalRecebimento { get; set; }

        
        public string? NumeroNotaFiscal { get; set; }

        [Required(ErrorMessage = "O nome do fornecedor é obrigatório.")]
        [StringLength(100)]
        public required string NomeFornecedor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DataRecebimento { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly HoraRecebimento { get; set; }

        public string? Observacoes { get; set; }

        [Required]
        [StringLength(50)]
        public required string TipoRecebimento { get; set; }

        [StringLength(100)]
        public string? TitularChavePix { get; set; }

        
        public string? ChavePix { get; set; }

       
        public string? CaminhoFotoNotaFiscal { get; set; }

        
        public string? CaminhoFotoDocumentos { get; set; }
    }
}
