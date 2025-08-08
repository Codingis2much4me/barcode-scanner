using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Windows.Input;

namespace StudentBarcodeApp.Services
{
    /// <summary>
    /// Minimal barcode capture: buffers fast key presses until Enter.
    /// Works well with USB scanners that act like keyboards.
    /// </summary>
    public class BarcodeService : IBarcodeService
    {
        private readonly ILogger<BarcodeService> _logger;
        private readonly StringBuilder _barcodeBuffer = new(); // re-used buffer to avoid allocations
        private DateTime _lastKeyPress = DateTime.Now;
        private readonly TimeSpan _barcodeTimeout = TimeSpan.FromMilliseconds(100); // typing is slower than scanners
        private bool _isListening;

        public event Action<string>? BarcodeScanned;
        public bool IsListening => _isListening;

        public BarcodeService(ILogger<BarcodeService> logger)
        {
            _logger = logger;
        }

        public void StartListening()
        {
            if (_isListening) return;
            _isListening = true;
            _logger.LogInformation("Scanner listening started");
        }

        public void StopListening()
        {
            if (!_isListening) return;
            _isListening = false;
            _barcodeBuffer.Clear();
            _logger.LogInformation("Scanner listening stopped");
        }

        public void ProcessKeyInput(Key key)
        {
            if (!_isListening) return;

            var now = DateTime.Now;

            // If it's been a while, assume a new scan and clear stale characters.
            if (now - _lastKeyPress > _barcodeTimeout)
                _barcodeBuffer.Clear();

            _lastKeyPress = now;

            // Enter means "end of scan"
            if (key == Key.Enter || key == Key.Return)
            {
                if (_barcodeBuffer.Length > 0)
                {
                    var barcode = _barcodeBuffer.ToString();
                    _barcodeBuffer.Clear();
                    _logger.LogInformation("Barcode: {Barcode}", barcode);
                    BarcodeScanned?.Invoke(barcode);
                }
                return;
            }

            // Map Key -> char and append if recognized.
            var ch = KeyToChar(key);
            if (ch.HasValue) _barcodeBuffer.Append(ch.Value);
        }

        // Only the characters we actually need for roll numbers.
        private char? KeyToChar(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9) return (char)('0' + (key - Key.D0));
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return (char)('0' + (key - Key.NumPad0));
            if (key >= Key.A && key <= Key.Z) return (char)('A' + (key - Key.A));
            return key switch
            {
                Key.OemMinus or Key.Subtract => '-',
                Key.OemPeriod or Key.Decimal => '.',
                Key.Space => ' ',
                _ => null
            };
        }
    }
}
