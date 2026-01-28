using API.Data;
using API.DTO.Etiqueta;
using API.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controlador
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class EtiquetasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EtiquetasController(AppDbContext context)
        {
            _context = context;
        }

        // GET todas as etiquetas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EtiquetaDTO>>> Get()
        {
            return await _context.Etiquetas
                .Select(e => new EtiquetaDTO
                {
                    ID_Etiqueta = e.ID_Etiqueta,
                    Nome = e.Nome,
                    Cor = e.Cor
                    
                })
                .ToListAsync();
        }
        
        //GET por id da etiqueta

        [HttpGet("{id}")]
        public async Task<ActionResult<EtiquetaDTO>> Get(int id)
        {
            var etiqueta = await _context.Etiquetas
        .FirstOrDefaultAsync(e => e.ID_Etiqueta == id);

            if (etiqueta == null)
                return NotFound();

            return new EtiquetaDTO
            {
                ID_Etiqueta = etiqueta.ID_Etiqueta,
                Nome = etiqueta.Nome,
                Cor = etiqueta.Cor
            };
        }

        //GET POR UTILIZADOR

        [HttpGet("EtiquetaUtilizador/{idUtilizador}")]
        public async Task<ActionResult<IEnumerable<EtiquetaDTO>>> GetEtiquetasPorUtilizador(int idUtilizador)
        {
            var etiquetas = await _context.Etiquetas
                .Where(e => e.ID_Utilizador == idUtilizador)
                .Select(e => new EtiquetaDTO
                {
                    ID_Etiqueta = e.ID_Etiqueta,
                    Nome = e.Nome,
                    Cor = e.Cor
                })
                .ToListAsync();

            if (etiquetas == null || etiquetas.Count == 0)
                return NotFound("Nenhuma etiqueta encontrada para este utilizador.");

            return Ok(etiquetas);
        }


        // POST
        [HttpPost]
        public async Task<ActionResult> Create(EtiquetaCriarDTO dto)
        {
            var etiqueta = new Etiqueta
            {
                Nome = dto.Nome,
                Cor = dto.Cor
            };

            _context.Etiquetas.Add(etiqueta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Etiqueta criada com sucesso!", etiqueta.ID_Etiqueta });
        }

        //POST por utilizador
        [HttpPost("Utilizador/{idUtilizador}")]
        public async Task<ActionResult<EtiquetaCriarDTO>> CriarEtiquetaParaUtilizador(int idUtilizador, EtiquetaCriarDTO dto)
        {
            var utilizador = await _context.Utilizadores.FindAsync(idUtilizador);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            var etiqueta = new Etiqueta
            {
                Nome = dto.Nome,
                Cor = dto.Cor,
                ID_Utilizador = idUtilizador
            };

            _context.Etiquetas.Add(etiqueta);
            await _context.SaveChangesAsync();

            var resultado = new EtiquetaDTO
            {
                ID_Etiqueta = etiqueta.ID_Etiqueta,
                Nome = etiqueta.Nome,
                Cor = etiqueta.Cor
            };


            return Ok(resultado);
        }


        // PUT
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, EtiquetaEditarDTO dto)
        {
            var etiqueta = await _context.Etiquetas.FindAsync(id);
            if (etiqueta == null) return NotFound();

            etiqueta.Nome = dto.Nome;
            etiqueta.Cor = dto.Cor;

            await _context.SaveChangesAsync();
            return Ok("Etiqueta atualizada.");
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // Buscar a etiqueta com tracking para permitir eliminar
            var etiqueta = await _context.Etiquetas
                .FirstOrDefaultAsync(e => e.ID_Etiqueta == id);

            if (etiqueta == null)
                return NotFound("Etiqueta não encontrada.");

            // Buscar relações e remover
            var relacoes = await _context.Tem
                .Where(t => t.ID_Etiqueta == id)
                .ToListAsync();

            if (relacoes.Any())
                _context.Tem.RemoveRange(relacoes);

            _context.Etiquetas.Remove(etiqueta);

            await _context.SaveChangesAsync();

            return Ok("Etiqueta apagada com sucesso.");
        }


        //Associação das tarefas com as etiquetas


        //POST
        [HttpPost("{idTarefa}/etiquetas/{idEtiqueta}")]
        public async Task<ActionResult> AddEtiqueta(int idTarefa, int idEtiqueta)
        {
            var tarefa = await _context.Tarefas.FindAsync(idTarefa);
            var etiqueta = await _context.Etiquetas.FindAsync(idEtiqueta);

            if (tarefa == null || etiqueta == null)
                return NotFound();

            var jaExiste = await _context.Tem
                .AnyAsync(t => t.ID_Tarefa == idTarefa && t.ID_Etiqueta == idEtiqueta);

            if (jaExiste)
                return BadRequest("A etiqueta já está associada à tarefa.");

            _context.Tem.Add(new Tem
            {
                ID_Tarefa = idTarefa,
                ID_Etiqueta = idEtiqueta
            });

            await _context.SaveChangesAsync();
            return Ok("Etiqueta associada à tarefa.");
        }

        //DELETE
        [HttpDelete("{idTarefa}/etiquetas/{idEtiqueta}")]
        public async Task<ActionResult> RemoveEtiqueta(int idTarefa, int idEtiqueta)
        {
            var rel = await _context.Tem
                .FirstOrDefaultAsync(t => t.ID_Tarefa == idTarefa && t.ID_Etiqueta == idEtiqueta);

            if (rel == null) return NotFound();

            _context.Tem.Remove(rel);
            await _context.SaveChangesAsync();

            return Ok("Etiqueta removida.");
        }

    }

}
