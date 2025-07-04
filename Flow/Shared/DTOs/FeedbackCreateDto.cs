using System.ComponentModel.DataAnnotations;

namespace Flow.Shared.Dtos
{
    public class FeedbackCreateDto
    {
        [Required(ErrorMessage = "O conteúdo é obrigatório.")]
        [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "O destinatário é obrigatório.")]
        public string ReceiverId { get; set; } = string.Empty;
    }
}