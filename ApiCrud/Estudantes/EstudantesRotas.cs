using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCrud.Data;
using ApiCrud.Estudantes;
using Microsoft.EntityFrameworkCore;

namespace webapi.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {
            // Verbo Post 
            app.MapPost("estudantes", async (AddEstudantesRequest request, AppDbContext context, CancellationToken ct) => 
            {
                var jaExiste = await context.Estudantes.AnyAsync(Estudante => Estudante.Nome == request.Nome, ct);
                if (jaExiste) return Results.Conflict("Já existe!");

                var novoEstudante = new Estudante(request.Nome);

                await context.Estudantes.AddAsync(novoEstudante, ct);
                await context.SaveChangesAsync(ct);

                var estudanteRetorno = new EstudanteDto(novoEstudante.Nome, novoEstudante.Id);

                return Results.Ok(estudanteRetorno);
            });

            // Verbo Get
            app.MapGet("estudantes", async (AppDbContext context, CancellationToken ct) => 
            {
                var estudantes = await context.Estudantes.Where(estudante => estudante.Ativo).Select(estudante => new EstudanteDto(estudante.Nome, estudante.Id)).
                ToListAsync(ct);

                return estudantes;
            });

            // Verbo Put
            app.MapPut("estudantes{id:guid}", async ( Guid id, UpdateEtudanteRequest request, AppDbContext context, CancellationToken ct) => 
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante == null) return Results.NotFound();

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(ct);
                return Results.Ok(new EstudanteDto(estudante.Nome, estudante.Id));
            });

            // Verbo Delete
             app.MapDelete("estudantes/{id:guid}", async (Guid id, AppDbContext context, CancellationToken ct) => 
             {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante == null) return Results.NotFound();

                estudante.Desativar();

                await context.SaveChangesAsync(ct);

                return Results.Ok();
          });
        }
    }
}