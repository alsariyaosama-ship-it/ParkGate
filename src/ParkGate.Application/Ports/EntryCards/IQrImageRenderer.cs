namespace ParkGate.Application.Ports.EntryCards;

public interface IQrImageRenderer
{
    byte[] RenderPng(string qrText, int pixelsPerModule = 10);
}
