namespace API.DTO
{
    public class TarefaCreateDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Estado { get; set; } = "Pendente";
        public DateTime? Data_Entrega { get; set; }
        public int ID_Utilizador { get; set; }
        public int? ID_Calendario { get; set; }

        //Etiquetas
        public List<int>? ID_Etiquetas { get; set; }
    }
}
