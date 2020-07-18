using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    /// <summary>
    /// Allows the construction of a <see cref="RctProcess"/> Can be very complex, you should only interact with the process builder if creating a factory.
    /// </summary>
    public interface IProcessBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="RctProcess"/> from the provided configuration options defined in the builder.
        /// </summary>
        /// <returns></returns>
        RctProcess Build();
        /// <summary>
        /// The name the process will be displayed as when queried.
        /// </summary>
        /// <param name="nameProviderFunction">A function for providing a process name..</param>
        /// <returns></returns>
        IProcessBuilder SetProcessName(Func<string, string> nameProviderFunction);
        /// <summary>
        /// The code that will be executed when the process is started.
        /// </summary>
        /// <param name="action">The code to be executed.</param>
        /// <returns></returns>
        IProcessBuilder SetAction(ProcessDelegate action);
        /// <summary>
        /// Creates a child-parent relationship within the process hierarchy. If set, the child process will inherit the parent's security principal, connection context, and standard IO.
        /// </summary>
        /// <param name="process">The parent process to attach to the child.</param>
        /// <returns></returns>
        IProcessBuilder SetParent(RctProcess process);
        /// <summary>
        /// Sets the security context for this process.
        /// </summary>
        /// <param name="principal">The security principal to be attached to this process.</param>
        /// <returns></returns>
        IProcessBuilder SetSecurityPrincipal(IPrincipal principal);
        /// <summary>
        /// Configures the connection context for this process.
        /// </summary>
        /// <param name="session">The connection context to be set.</param>
        /// <returns></returns>
        IProcessBuilder SetInstanceSession(IInstanceSession session);

        IProcessBuilder SetThreadApartmentMode(ApartmentState mode);
        IProcessBuilder AddProcessExtension(IExtensionProvider<RctProcess> extension);
        IProcessBuilder AddProcessExtensions(IEnumerable<IExtensionProvider<RctProcess>> extensions);
    }
}