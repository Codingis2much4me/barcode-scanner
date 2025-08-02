using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Windows.Input;

namespace StudentBarcodeApp.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly ILogger<BarcodeService> _logger;
        private readonly StringBuilder _barcodeBuffer;
        private DateTime _lastKeyPress;
        private readonly TimeSpan _barcodeTimeout = TimeSpan.FromMilliseconds(100);
        private bool _isListening;

        public event Action<string>? BarcodeScanned;
        public bool IsListening => _isListening;

        public BarcodeService(ILogger<BarcodeService> logger)
        {
            _logger = logger;
            _barcodeBuffer = new StringBuilder();
            _lastKeyPress = DateTime.Now;
        }

        public void StartListening()
        {
            if (!_isListening)
            {
                _isListening = true;
                _logger.LogInformation("Barcode scanner listening started");
            }
        }

        public void StopListening()
        {
            if (_isListening)
            {
                _isListening = false;
                _barcodeBuffer.Clear();
                _logger.LogInformation("Barcode scanner listening stopped");
            }
        }

        public void ProcessKeyInput(Key key)
        {
            if (!_isListening) return;

            var now = DateTime.Now;
            
            // If too much time has passed, clear the buffer (new scan)
            if (now - _lastKeyPress > _barcodeTimeout)
            {
                _barcodeBuffer.Clear();
            }

            _lastKeyPress = now;

            // Handle Enter key (end of barcode)
            if (key == Key.Enter || key == Key.Return)
            {
                if (_barcodeBuffer.Length > 0)
                {
                    var barcode = _barcodeBuffer.ToString();
                    _barcodeBuffer.Clear();
                    
                    _logger.LogInformation("Barcode scanned: {Barcode}", barcode);
                    BarcodeScanned?.Invoke(barcode);
                }
                return;
            }

            // Convert key to character
            var character = KeyToChar(key);
            if (character.HasValue)
            {
                _barcodeBuffer.Append(character.Value);
            }
        }

        private char? KeyToChar(Key key)
        {
            // Handle numbers
            if (key >= Key.D0 && key <= Key.D9)
            {
                return (char)('0' + (key - Key.D0));
            }

            // Handle numpad numbers
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (char)('0' + (key - Key.NumPad0));
            }

            // Handle letters
            if (key >= Key.A && key <= Key.Z)
            {
                return (char)('A' + (key - Key.A));
            }

            // Handle some special characters commonly found in barcodes
            switch (key)
            {
                case Key.OemMinus:
                case Key.Subtract:
                    return '-';
                case Key.OemPeriod:
                case Key.Decimal:
                    return '.';
                case Key.Space:
                    return ' ';
                default:
                    return null;
            }
        }
    }
}
