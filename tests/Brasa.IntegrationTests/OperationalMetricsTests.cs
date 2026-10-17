using System.Diagnostics.Metrics;
using Brasa.Application.Shared.Observability;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OperationalMetricsTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Amostra_operacional_publica_os_indicadores_de_impressao_e_pagamento()
    {
        await TestPrintAgent.InstallAsync(factory);
        await factory.RunJobAsync<SampleOperationalMetricsJob>();

        var gauges = new Dictionary<string, double>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == BrasaMetrics.MeterName)
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, _, _) => gauges[instrument.Name] = value);
        listener.SetMeasurementEventCallback<long>((instrument, value, _, _) => gauges[instrument.Name] = value);
        listener.Start();
        listener.RecordObservableInstruments();

        Assert.True(gauges["brasa.printing.online_agents"] >= 1);
        Assert.True(gauges["brasa.outbox.oldest_pending_age"] >= 0);
        Assert.Contains("brasa.payments.waiting_pix", gauges.Keys);
    }
}
