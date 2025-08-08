using System;
using System.Windows.Input;

namespace StudentBarcodeApp.Services
{
    /// <summary>
    /// Abstraction for scanner input so the UI doesn't care about the concrete implementation.
    /// </summary>
    public interface IBarcodeService
    {
        /// <summary>Fires when a full barcode was captured (usually after Enter).</summary>
        event Action<string>? BarcodeScanned;

        /// <summary>Begin processing key input.</summary>
        void StartListening();

        /// <summary>Stop processing key input and clear any partial buffer.</summary>
        void StopListening();

        /// <summary>True when the service is actively listening.</summary>
        bool IsListening { get; }

        /// <summary>Feed raw key presses to the service (scanner emulates a keyboard).</summary>
        void ProcessKeyInput(Key key);
    }
}
