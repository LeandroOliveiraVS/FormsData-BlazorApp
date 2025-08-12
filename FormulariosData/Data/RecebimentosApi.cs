using FormulariosData.Data;
using FormulariosData.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text.Json;

public static class RecebimentosApi
{
    public static void MapRecebimentosApi(this WebApplication app)
    {
        _ = app.MapPost("/api/Recebimentos", async (DataManagerRequest dm, IDbContextFactory<ApplicationDbContext> dbFactory, CancellationToken cancellationToken, HttpContext httpContext) =>
        {
            try
            {
                // Verifica se o usuário está autenticado
                if (!httpContext.User.Identity?.IsAuthenticated ?? true)
                {
                    Console.WriteLine("Usuário não autenticado na API");
                    return Results.Unauthorized();
                }

                Console.WriteLine("======================================================");
                Console.WriteLine("--- Pedido Recebido do DataGrid ---");
                Console.WriteLine(JsonSerializer.Serialize(dm, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine("======================================================");


                // Verifica se a requisição foi cancelada
                cancellationToken.ThrowIfCancellationRequested();

                await using var dbContext = await dbFactory.CreateDbContextAsync(cancellationToken);
                IQueryable<Recebimento> query = dbContext.Recebimentos.AsQueryable();

                // --- LÓGICA DE FILTRAGEM DE COLUNAS CORRIGIDA E ROBUSTA ---
                if (dm.Where != null && dm.Where.Count > 0)
                {
                    Console.WriteLine($"Processando {dm.Where.Count} grupo(s) de filtro(s):");

                    foreach (var grupoFiltro in dm.Where)
                    {
                        // Verifica cancelamento entre operações
                        cancellationToken.ThrowIfCancellationRequested();

                        // Verifica se é um filtro complexo com predicates
                        if (grupoFiltro.IsComplex && grupoFiltro.predicates != null && grupoFiltro.predicates.Count > 0)
                        {
                            Console.WriteLine($"Processando {grupoFiltro.predicates.Count} predicado(s):");

                            foreach (var predicado in grupoFiltro.predicates)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                Console.WriteLine($"Campo: {predicado.Field}, Operador: {predicado.Operator}, Valor: {predicado.value}");

                                if (predicado.value == null || string.IsNullOrEmpty(predicado.value.ToString()) ||
                                    string.IsNullOrEmpty(predicado.Field) || string.IsNullOrEmpty(predicado.Operator))
                                    continue;

                                var valor = predicado.value.ToString();
                                var campo = predicado.Field;

                                query = ApplyFilter(query, campo, predicado.Operator, valor);
                            }
                        }
                        // Filtro simples (não complexo)
                        else if (!grupoFiltro.IsComplex)
                        {
                            Console.WriteLine($"Campo: {grupoFiltro.Field}, Operador: {grupoFiltro.Operator}, Valor: {grupoFiltro.value}");

                            if (grupoFiltro.value == null || string.IsNullOrEmpty(grupoFiltro.value.ToString()) ||
                                string.IsNullOrEmpty(grupoFiltro.Field) || string.IsNullOrEmpty(grupoFiltro.Operator))
                                continue;

                            var valor = grupoFiltro.value.ToString();
                            var campo = grupoFiltro.Field;

                            query = ApplyFilter(query, campo, grupoFiltro.Operator, valor);
                        }
                    }
                }

                // Lógica de Filtragem (da barra de pesquisa geral)
                if (dm.Search != null && dm.Search.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var searchValue = dm.Search[0].Key;
                    Console.WriteLine($"Aplicando pesquisa geral: {searchValue}");

                    query = query.Where(r =>
                        r.NomeFornecedor.Contains(searchValue) ||
                        r.NomeRecebedor.Contains(searchValue) ||
                        r.NumeroNotaFiscal.Contains(searchValue) ||
                        r.EmailRecebedor.Contains(searchValue) ||
                        r.Observacoes.Contains(searchValue)
                    );
                }

                // Lógica de Ordenação
                if (dm.Sorted != null && dm.Sorted.Count > 0)
                {
                    var sort = dm.Sorted[0];
                    Console.WriteLine($"Aplicando ordenação: {sort.Name} {sort.Direction}");
                    query = query.OrderBy($"{sort.Name} {sort.Direction}");
                }
                else
                {
                    query = query.OrderByDescending(r => r.Registro);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var totalRecords = await query.CountAsync(cancellationToken);
                Console.WriteLine($"Total de registros após filtros: {totalRecords}");

                if (dm.Skip != 0)
                {
                    query = query.Skip(dm.Skip);
                }
                if (dm.Take != 0)
                {
                    query = query.Take(dm.Take);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var result = await query.ToListAsync(cancellationToken);
                Console.WriteLine($"Registros retornados: {result.Count}");

                return Results.Ok(new DataResult() { Result = result, Count = totalRecords });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operação cancelada pelo cliente");
                return Results.StatusCode(499); // Client Closed Request
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na API de Recebimentos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Retorna erro interno do servidor com uma mensagem genérica
                return Results.Problem(
                    title: "Erro interno do servidor",
                    detail: "Ocorreu um erro ao processar a requisição",
                    statusCode: 500
                );
            }
        })
        .WithName("GetRecebimentos")
        .AllowAnonymous(); ; // Adiciona autorização à API
    }

    #region Métodos de Filtro

    private static IQueryable<Recebimento> ApplyFilter(IQueryable<Recebimento> query, string campo, string operador, string valor)
    {
        try
        {
            return operador.ToLower() switch
            {
                "contains" => ApplyContainsFilter(query, campo, valor),
                "equal" or "eq" => ApplyEqualsFilter(query, campo, valor),
                "startswith" => ApplyStartsWithFilter(query, campo, valor),
                "endswith" => ApplyEndsWithFilter(query, campo, valor),
                "notequal" or "neq" => ApplyNotEqualsFilter(query, campo, valor),
                "greaterthan" or "gt" => ApplyGreaterThanFilter(query, campo, valor),
                "lessthan" or "lt" => ApplyLessThanFilter(query, campo, valor),
                "greaterthanorequal" or "gte" => ApplyGreaterThanOrEqualFilter(query, campo, valor),
                "lessthanorequal" or "lte" => ApplyLessThanOrEqualFilter(query, campo, valor),
                _ => query
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao aplicar filtro {operador} no campo {campo}: {ex.Message}");
            return query; // Retorna query original em caso de erro
        }
    }

    private static IQueryable<Recebimento> ApplyContainsFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.NomeFornecedor) => query.Where(r => r.NomeFornecedor != null && r.NomeFornecedor.Contains(valor)),
            nameof(Recebimento.NomeRecebedor) => query.Where(r => r.NomeRecebedor != null && r.NomeRecebedor.Contains(valor)),
            nameof(Recebimento.EmailRecebedor) => query.Where(r => r.EmailRecebedor != null && r.EmailRecebedor.Contains(valor)),
            nameof(Recebimento.NumeroNotaFiscal) => query.Where(r => r.NumeroNotaFiscal != null && r.NumeroNotaFiscal.Contains(valor)),
            nameof(Recebimento.LocalRecebimento) => query.Where(r => r.LocalRecebimento != null && r.LocalRecebimento.Contains(valor)),
            nameof(Recebimento.Observacoes) => query.Where(r => r.Observacoes != null && r.Observacoes.Contains(valor)),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyEqualsFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.NomeFornecedor) => query.Where(r => r.NomeFornecedor == valor),
            nameof(Recebimento.NomeRecebedor) => query.Where(r => r.NomeRecebedor == valor),
            nameof(Recebimento.EmailRecebedor) => query.Where(r => r.EmailRecebedor == valor),
            nameof(Recebimento.NumeroNotaFiscal) => query.Where(r => r.NumeroNotaFiscal == valor),
            nameof(Recebimento.LocalRecebimento) => query.Where(r => r.LocalRecebimento == valor),
            nameof(Recebimento.Observacoes) => query.Where(r => r.Observacoes == valor),
            nameof(Recebimento.DataRecebimento) when DateOnly.TryParse(valor, out var data) =>
                query.Where(r => r.DataRecebimento == data),
            nameof(Recebimento.HoraRecebimento) when TimeOnly.TryParse(valor, out var hora) =>
                query.Where(r => r.HoraRecebimento == hora),
            nameof(Recebimento.IdRecebimento) when int.TryParse(valor, out var id) =>
                query.Where(r => r.IdRecebimento == id),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyStartsWithFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.NomeFornecedor) => query.Where(r => r.NomeFornecedor != null && r.NomeFornecedor.StartsWith(valor)),
            nameof(Recebimento.NomeRecebedor) => query.Where(r => r.NomeRecebedor != null && r.NomeRecebedor.StartsWith(valor)),
            nameof(Recebimento.EmailRecebedor) => query.Where(r => r.EmailRecebedor != null && r.EmailRecebedor.StartsWith(valor)),
            nameof(Recebimento.NumeroNotaFiscal) => query.Where(r => r.NumeroNotaFiscal != null && r.NumeroNotaFiscal.StartsWith(valor)),
            nameof(Recebimento.LocalRecebimento) => query.Where(r => r.LocalRecebimento != null && r.LocalRecebimento.StartsWith(valor)),
            nameof(Recebimento.Observacoes) => query.Where(r => r.Observacoes != null && r.Observacoes.StartsWith(valor)),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyEndsWithFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.NomeFornecedor) => query.Where(r => r.NomeFornecedor != null && r.NomeFornecedor.EndsWith(valor)),
            nameof(Recebimento.NomeRecebedor) => query.Where(r => r.NomeRecebedor != null && r.NomeRecebedor.EndsWith(valor)),
            nameof(Recebimento.EmailRecebedor) => query.Where(r => r.EmailRecebedor != null && r.EmailRecebedor.EndsWith(valor)),
            nameof(Recebimento.NumeroNotaFiscal) => query.Where(r => r.NumeroNotaFiscal != null && r.NumeroNotaFiscal.EndsWith(valor)),
            nameof(Recebimento.LocalRecebimento) => query.Where(r => r.LocalRecebimento != null && r.LocalRecebimento.EndsWith(valor)),
            nameof(Recebimento.Observacoes) => query.Where(r => r.Observacoes != null && r.Observacoes.EndsWith(valor)),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyNotEqualsFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.NomeFornecedor) => query.Where(r => r.NomeFornecedor != valor),
            nameof(Recebimento.NomeRecebedor) => query.Where(r => r.NomeRecebedor != valor),
            nameof(Recebimento.EmailRecebedor) => query.Where(r => r.EmailRecebedor != valor),
            nameof(Recebimento.NumeroNotaFiscal) => query.Where(r => r.NumeroNotaFiscal != valor),
            nameof(Recebimento.LocalRecebimento) => query.Where(r => r.LocalRecebimento != valor),
            nameof(Recebimento.Observacoes) => query.Where(r => r.Observacoes != valor),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyGreaterThanFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.DataRecebimento) when DateOnly.TryParse(valor, out var data) =>
                query.Where(r => r.DataRecebimento > data),
            nameof(Recebimento.HoraRecebimento) when TimeOnly.TryParse(valor, out var hora) =>
                query.Where(r => r.HoraRecebimento > hora),
            nameof(Recebimento.IdRecebimento) when int.TryParse(valor, out var id) =>
                query.Where(r => r.IdRecebimento > id),
            nameof(Recebimento.Registro) when DateTime.TryParse(valor, out var registro) =>
                query.Where(r => r.Registro > registro),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyLessThanFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.DataRecebimento) when DateOnly.TryParse(valor, out var data) =>
                query.Where(r => r.DataRecebimento < data),
            nameof(Recebimento.HoraRecebimento) when TimeOnly.TryParse(valor, out var hora) =>
                query.Where(r => r.HoraRecebimento < hora),
            nameof(Recebimento.IdRecebimento) when int.TryParse(valor, out var id) =>
                query.Where(r => r.IdRecebimento < id),
            nameof(Recebimento.Registro) when DateTime.TryParse(valor, out var registro) =>
                query.Where(r => r.Registro < registro),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyGreaterThanOrEqualFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.DataRecebimento) when DateOnly.TryParse(valor, out var data) =>
                query.Where(r => r.DataRecebimento >= data),
            nameof(Recebimento.HoraRecebimento) when TimeOnly.TryParse(valor, out var hora) =>
                query.Where(r => r.HoraRecebimento >= hora),
            nameof(Recebimento.IdRecebimento) when int.TryParse(valor, out var id) =>
                query.Where(r => r.IdRecebimento >= id),
            nameof(Recebimento.Registro) when DateTime.TryParse(valor, out var registro) =>
                query.Where(r => r.Registro >= registro),
            _ => query
        };
    }

    private static IQueryable<Recebimento> ApplyLessThanOrEqualFilter(IQueryable<Recebimento> query, string campo, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return query;

        return campo switch
        {
            nameof(Recebimento.DataRecebimento) when DateOnly.TryParse(valor, out var data) =>
                query.Where(r => r.DataRecebimento <= data),
            nameof(Recebimento.HoraRecebimento) when TimeOnly.TryParse(valor, out var hora) =>
                query.Where(r => r.HoraRecebimento <= hora),
            nameof(Recebimento.IdRecebimento) when int.TryParse(valor, out var id) =>
                query.Where(r => r.IdRecebimento <= id),
            nameof(Recebimento.Registro) when DateTime.TryParse(valor, out var registro) =>
                query.Where(r => r.Registro <= registro),
            _ => query
        };
    }

    #endregion
}