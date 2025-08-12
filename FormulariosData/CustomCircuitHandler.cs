using Microsoft.AspNetCore.Components.Server.Circuits;

public class CustomCircuitHandler : CircuitHandler
{
    private readonly ILogger<CustomCircuitHandler> _logger;

    public CustomCircuitHandler(ILogger<CustomCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} opened", circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} closed", circuit.Id);

        // Aqui você pode implementar lógica de limpeza se necessário
        // Por exemplo, cancelar operações em andamento, limpar cache, etc.

        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Circuit {CircuitId} connection down", circuit.Id);
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} connection restored", circuit.Id);
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
