using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Modelo
{
    public class Utilizador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Utilizador { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Password_Encriptado { get; set; }

        // Navegação
        public ICollection<Calendario>? Calendarios { get; set; }
        public ICollection<Tarefa>? Tarefas { get; set; }

        //Etiquetas
        public ICollection<Etiqueta>? Etiquetas { get; set; }


    }

    public class Calendario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Calendario { get; set; }
        public DateTime? Data { get; set; }

        public int ID_Utilizador { get; set; }
        public Utilizador Utilizador { get; set; }

        // Navegação
        public ICollection<Tarefa> Tarefas { get; set; }
    }

    public class Tarefa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Tarefa { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Estado { get; set; } // 'Pendente', 'Em Progresso', 'Concluída' 
        public DateTime? Data_Entrega { get; set; }

        public int ID_Utilizador { get; set; }
        public Utilizador? Utilizador { get; set; }

        public int? ID_Calendario { get; set; }
        public Calendario? Calendario { get; set; }

        // Navegação muitos para muitos
        public ICollection<Tem>? Tem { get; set; }

        
    }

    public class Etiqueta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Etiqueta { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; }

        // Navegação muitos para muitos
        public ICollection<Tem> Tem { get; set; }

        //Utilizadores
        public int ID_Utilizador { get; set; }

        public Utilizador Utilizador { get; set; }
    }

    public class Tem
    {
        [Key, Column(Order = 0)]
        public int ID_Tarefa { get; set; }
        public Tarefa Tarefa { get; set; }

        [Key,Column(Order = 1)]
        public int ID_Etiqueta { get; set; }
        public Etiqueta Etiqueta { get; set; }
    }
}
