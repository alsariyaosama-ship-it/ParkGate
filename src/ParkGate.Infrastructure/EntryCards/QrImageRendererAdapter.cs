using ParkGate.Application.Ports.EntryCards;

namespace ParkGate.Infrastructure.EntryCards;

public sealed class QrImageRendererAdapter : IQrImageRenderer
{
    public byte[] RenderPng(string qrText, int pixelsPerModule = 10)
    {
        return QrImageRenderer.RenderPng(qrText, pixelsPerModule);
    }
}
