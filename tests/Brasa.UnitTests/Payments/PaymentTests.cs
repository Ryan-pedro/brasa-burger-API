using Brasa.Domain.Payments;

namespace Brasa.UnitTests.Payments;

public class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Pix_online_nasce_pendente_com_prazo_e_so_aceita_uma_cobranca()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.PixOnline, 30m, Now);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(Now + PixPaymentPolicy.PaymentWindow, payment.ExpiresAt);

        payment.AttachCharge("fake", "c1", "00020126");

        Assert.True(payment.HasCharge);
        Assert.Throws<InvalidOperationException>(() => payment.AttachCharge("fake", "c2", "00020126"));
    }

    [Fact]
    public void Pagamento_na_entrega_nao_tem_prazo_nem_cobranca()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.Cash, 30m, Now);

        Assert.Null(payment.ExpiresAt);
        Assert.False(payment.IsDueAt(Now.AddDays(1)));
        Assert.Throws<InvalidOperationException>(() => payment.AttachCharge("fake", "c1", "00020126"));
        Assert.Throws<InvalidOperationException>(payment.Expire);
    }

    [Fact]
    public void Pix_vencido_expira_e_pagamento_depois_disso_fica_como_tardio()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.PixOnline, 30m, Now);

        Assert.False(payment.IsDueAt(Now + PixPaymentPolicy.PaymentWindow - TimeSpan.FromSeconds(1)));
        Assert.True(payment.IsDueAt(Now + PixPaymentPolicy.PaymentWindow));

        payment.Expire();
        Assert.Throws<InvalidOperationException>(() => payment.MarkPaid(Now));

        payment.MarkPaidLate(Now.AddHours(1));
        Assert.Equal(PaymentStatus.PaidLate, payment.Status);
        Assert.Equal(Now.AddHours(1), payment.PaidAt);
    }

    [Fact]
    public void Aviso_de_pagamento_conta_falhas_e_para_de_tentar_no_limite()
    {
        var webhookEvent = new PaymentWebhookEvent("fake", "evt_1", "c1", Now);

        for (var attempt = 0; attempt < PaymentWebhookEvent.MaxAttempts; attempt++)
        {
            Assert.True(webhookEvent.IsWaiting);
            webhookEvent.RecordFailure("gateway fora do ar");
        }

        Assert.False(webhookEvent.IsWaiting);
        Assert.Equal("gateway fora do ar", webhookEvent.LastError);
    }
}
