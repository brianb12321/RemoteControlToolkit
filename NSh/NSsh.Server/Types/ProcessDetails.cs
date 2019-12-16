using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NSsh.Server.Types
{
    public class ProcessDetails : IDisposable
    {
        public int ProcessId { get; set; }

        public Process Process { get; set; }

        public StreamReader StandardError { get; set; }

        public StreamWriter StandardInput { get; set; }

        public StreamReader StandardOutput { get; set; }

        ~ProcessDetails()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (StandardInput != null) StandardInput.Dispose();
                if (StandardOutput != null) StandardOutput.Dispose();
                if (StandardError != null) StandardError.Dispose();
            }
        }

        #endregion
    }
}
