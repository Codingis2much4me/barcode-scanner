using System;

namespace StudentBarcodeApp.Services
{
    public interface IBarcodeService
    {
        event Action<string>? BarcodeScanned;
        void StartListening();
        void StopListening();
        bool IsListening { get; }
    }
}
