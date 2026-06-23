using System;
using System.IO;
using System.Text;

namespace AttnSoft.AutoUpdate
{
    internal class TimestampWriter : TextWriter
    {
        private readonly TextWriter _fileWriter;
        private readonly TextWriter? _fallbackWriter;

        public TimestampWriter(TextWriter fileWriter, TextWriter? fallbackWriter = null)
        {
            _fileWriter = fileWriter;
            _fallbackWriter = fallbackWriter;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {value}";
            _fileWriter.WriteLine(line);
            _fileWriter.Flush();
            _fallbackWriter?.WriteLine(line);
            _fallbackWriter?.Flush();
        }

        public override void Flush()
        {
            _fileWriter.Flush();
            _fallbackWriter?.Flush();
        }
    }
}
