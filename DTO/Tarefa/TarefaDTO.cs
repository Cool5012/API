using API.DTO.Etiqueta;

namespace API.DTO.Tarefa
{
    public class TarefaDTO
    {
        public int ID_Tarefa { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Estado { get; set; }
        public DateTime? Data_Entrega { get; set; }
        public int ID_Utilizador { get; set; }
        public int? ID_Calendario { get; set; }

        public List<EtiquetaDTO>? Etiquetas { get; set; } = new();
    }
}
