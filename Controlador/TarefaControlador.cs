using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Modelo;
using API.DTO;
using API.DTO.Tarefa;
using API.DTO.Etiqueta;
using Microsoft.AspNetCore.Authorization;

namespace API.TarefaControlador
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class TarefasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TarefasController(AppDbContext context)
        {
            _context = context;
        }

        //GET de todas as tarefas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetTarefas()
        {
            var tarefas = await _context.Tarefas
                .Include(t => t.Tem)
                    .ThenInclude(te => te.Etiqueta)
                .ToListAsync();

            var dtos = tarefas.Select(t => new TarefaDTO
            {
                ID_Tarefa = t.ID_Tarefa,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Estado = t.Estado,
                Data_Entrega = t.Data_Entrega,
                ID_Utilizador = t.ID_Utilizador,
                ID_Calendario = t.ID_Calendario,
                Etiquetas = t.Tem?
                    .Select(te => new EtiquetaDTO
                    {
                        ID_Etiqueta = te.ID_Etiqueta,
                        Nome = te.Etiqueta?.Nome ?? string.Empty,
                        Cor = te.Etiqueta?.Cor
                    })
                    .ToList() ?? new List<EtiquetaDTO>()
            }).ToList();

            return dtos;
        }

        //GET por ID

        [HttpGet("{id}")]
        public async Task<ActionResult<TarefaDTO>> GetTarefa(int id)
        {
            var tarefa = await _context.Tarefas
                .Include(t => t.Tem)
                    .ThenInclude(te => te.Etiqueta)
                .FirstOrDefaultAsync(t => t.ID_Tarefa == id);

            if (tarefa == null)
                return NotFound();

            // Mapeia para DTO manualmente
            var dto = new TarefaDTO
            {
                ID_Tarefa = tarefa.ID_Tarefa,
                Nome = tarefa.Nome,
                Descricao = tarefa.Descricao,
                Estado = tarefa.Estado,
                Data_Entrega = tarefa.Data_Entrega,
                ID_Utilizador = tarefa.ID_Utilizador,
                ID_Calendario = tarefa.ID_Calendario,
                Etiquetas = tarefa.Tem?
                    .Select(te => new EtiquetaDTO
                    {
                        ID_Etiqueta = te.ID_Etiqueta,
                        Nome = te.Etiqueta?.Nome ?? string.Empty
                    })
                    .ToList() ?? new List<EtiquetaDTO>()
            };

            return dto;
        }

        //GET tarefas por utilizador
        [HttpGet("TarefaUtilizador/{idUtilizador}")]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetMinhasTarefas(int idUtilizador)
        {
            var tarefas = await _context.Tarefas
            .Where(t => t.ID_Utilizador == idUtilizador) // filtra pelo utilizador
            .Include(t => t.Tem)
            .ThenInclude(te => te.Etiqueta)
        .ToListAsync();

            var dtos = tarefas.Select(t => new TarefaDTO
            {
                ID_Tarefa = t.ID_Tarefa,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Estado = t.Estado,
                Data_Entrega = t.Data_Entrega,
                ID_Utilizador = t.ID_Utilizador,
                ID_Calendario = t.ID_Calendario,
                Etiquetas = t.Tem?
                    .Select(te => new EtiquetaDTO
                    {
                        ID_Etiqueta = te.ID_Etiqueta,
                        Nome = te.Etiqueta?.Nome ?? string.Empty,
                        Cor = te.Etiqueta?.Cor
                    })
                    .ToList() ?? new List<EtiquetaDTO>()
            }).ToList();

            return dtos;
        }



        // POST
        [HttpPost]
        public async Task<ActionResult<Tarefa>> PostTarefa([FromBody] TarefaCreateDTO dto)
        {
            var tarefa = new Tarefa
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Estado = dto.Estado,
                Data_Entrega = dto.Data_Entrega,
                ID_Utilizador = dto.ID_Utilizador,
                ID_Calendario = dto.ID_Calendario
            };

            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            // Associa etiquetas
            if (dto.ID_Etiquetas != null)
            {
                foreach (var id in dto.ID_Etiquetas)
                {
                    _context.Tem.Add(new Tem
                    {
                        ID_Tarefa = tarefa.ID_Tarefa,
                        ID_Etiqueta = id
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Recarrega tarefa com etiquetas
            var tarefaComEtiquetas = await _context.Tarefas
                .Include(t => t.Tem)
                    .ThenInclude(te => te.Etiqueta)
                .FirstOrDefaultAsync(t => t.ID_Tarefa == tarefa.ID_Tarefa);

            // Converte para DTO
            var tarefaDTO = new TarefaDTO
            {
                ID_Tarefa = tarefaComEtiquetas.ID_Tarefa,
                Nome = tarefaComEtiquetas.Nome,
                Descricao = tarefaComEtiquetas.Descricao,
                Estado = tarefaComEtiquetas.Estado,
                Data_Entrega = tarefaComEtiquetas.Data_Entrega,
                ID_Utilizador = tarefaComEtiquetas.ID_Utilizador,
                ID_Calendario = tarefaComEtiquetas.ID_Calendario,
                Etiquetas = tarefaComEtiquetas.Tem.Select(te => new EtiquetaDTO
                {
                    ID_Etiqueta = te.ID_Etiqueta,
                    Nome = te.Etiqueta.Nome,
                    Cor = te.Etiqueta.Cor
                }).ToList()
            };

            return CreatedAtAction("GetTarefa", new { id = tarefaDTO.ID_Tarefa }, tarefaDTO);
        }



        // PUT por ID de tarefa
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarefa(int id, TarefaCreateDTO dto)
        {
            var tarefa = await _context.Tarefas
                .Include(t => t.Tem)
                .FirstOrDefaultAsync(t => t.ID_Tarefa == id);

            if (tarefa == null)
                return NotFound();

            // Atualizar campos
            tarefa.Nome = dto.Nome;
            tarefa.Descricao = dto.Descricao;
            tarefa.Estado = dto.Estado;
            tarefa.Data_Entrega = dto.Data_Entrega;
            tarefa.ID_Calendario = dto.ID_Calendario;

            // Só atualizar etiquetas se vierem no DTO
            if (dto.ID_Etiquetas != null)
            {
                _context.Tem.RemoveRange(tarefa.Tem);

                foreach (var idEt in dto.ID_Etiquetas)
                {
                    _context.Tem.Add(new Tem
                    {
                        ID_Tarefa = tarefa.ID_Tarefa,
                        ID_Etiqueta = idEt
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE por ID de tarefa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
                return NotFound();

            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //Filtrar tarefas

        [HttpGet("Filtrar")]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> FiltrarTarefas(
    [FromQuery] string? pesquisa,
    [FromQuery] string? estado,
    [FromQuery] string? etiquetas, 
    [FromQuery] int? utilizador   
)
        {

            var query = _context.Tarefas
        .Include(t => t.Tem)
            .ThenInclude(te => te.Etiqueta)
        .AsQueryable();

            // Pesquisa por nome da tarefa
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                query = query.Where(t =>
                    t.Nome.Contains(pesquisa) ||
                    t.Descricao.Contains(pesquisa)
                );
            }

            //  Filtrar por estado
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            // Filtrar por utilizador
            if (utilizador.HasValue)
            {
                query = query.Where(t => t.ID_Utilizador == utilizador);
            }

            // Filtrar por etiquetas
            if (!string.IsNullOrWhiteSpace(etiquetas))
            {
                var ids = etiquetas.Split(',').Select(int.Parse).ToList();

                query = query.Where(t =>
                    t.Tem.Any(e => ids.Contains(e.ID_Etiqueta))
                );
            }

            var tarefas = await query.ToListAsync();

            var dtos = tarefas.Select(t => new TarefaDTO
            {
                ID_Tarefa = t.ID_Tarefa,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Estado = t.Estado,
                Data_Entrega = t.Data_Entrega,
                ID_Utilizador = t.ID_Utilizador,
                ID_Calendario = t.ID_Calendario,
                Etiquetas = t.Tem?
                    .Select(te => new EtiquetaDTO
                    {
                        ID_Etiqueta = te.ID_Etiqueta,
                        Nome = te.Etiqueta.Nome,
                        Cor = te.Etiqueta.Cor
                    })
                    .ToList() ?? new List<EtiquetaDTO>()
            }).ToList();

            return dtos;
        }


        //Mostrar tarefas no calendario

        [HttpGet("Calendario")]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetTarefasDoMes([FromQuery] int ano, [FromQuery] int mes, [FromQuery] int? utilizador)
        {
            var query = _context.Tarefas
                .Include(t => t.Tem)
                    .ThenInclude(te => te.Etiqueta)
                .AsQueryable();

            // Filtra por ano e mês
            query = query.Where(t => t.Data_Entrega.HasValue &&
                         t.Data_Entrega.Value.Year == ano &&
                         t.Data_Entrega.Value.Month == mes);

            //Filtrar por utilizador
            if (utilizador.HasValue)
            {
                query = query.Where(t => t.ID_Utilizador == utilizador.Value);
            }

            var tarefas = await query.ToListAsync();

            var dtos = tarefas.Select(t => new TarefaDTO
            {
                ID_Tarefa = t.ID_Tarefa,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Estado = t.Estado,
                Data_Entrega = t.Data_Entrega,
                ID_Utilizador = t.ID_Utilizador,
                ID_Calendario = t.ID_Calendario,
                Etiquetas = t.Tem?
                    .Select(te => new EtiquetaDTO
                    {
                        ID_Etiqueta = te.ID_Etiqueta,
                        Nome = te.Etiqueta.Nome,
                        Cor = te.Etiqueta.Cor
                    })
                    .ToList() ?? new List<EtiquetaDTO>()
            }).ToList();

            return dtos;
        }

    }
}
