using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace FormulariosData.Models
{
    public class CheckListVeiculos
    {
        [Key]
        public int IdCheckListVeiculo { get; set; }
        public DateTime Registro {  get; set; }
        [StringLength(100)]
        public required string NomeRecebedor {  get; set; }
        [StringLength(100)]
        public required string EmailRecebedor { get; set; }
        [StringLength(100)]
        public required string EmailResponsavel { get; set; }
        [StringLength(100)]
        public required string NomeResponsavel { get; set; }
        public int NumeroVT { get; set; }
        [StringLength(100)]
        public required string PlacaVeiculo { get; set; }
        public required int KmVeiculo { get; set; }
        public required DateOnly Data { get; set; }
        public required TimeOnly Hora { get; set; }
        [StringLength(50)]
        public required string ChecagemFarol { get; set; }
        [StringLength(50)]
        public required string ChecagemLuzesIntermintentes { get; set; }
        [StringLength(50)]
        public required string ChecagemLanternas { get; set; }
        [StringLength(50)]
        public required string ChecagemLuzesInternas { get; set; }
        [StringLength(50)]
        public required string ChecagemSirenes { get; set; }
        [StringLength(50)]
        public required string ChecagemAcessorios { get; set; }
        public string? Observacoes { get; set; }
        public string? RelatoAvarias { get; set; }
        public string? PastaImagens { get; set; }
        public string? Relatorio { get; set; }
    }
}
