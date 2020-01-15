Building RCTC from Source
=================================

There may be a time when you would like to configure the initialization process of RCTC.

==========
Hooks?
==========

| RCTC provides a hook mechanism for plugin developers. You can implement the :title:`IApplicationStartup` interface to register services.
| But, some people would like to completely reconfigure the startup and application hosting process. This is where you must build RCTC from source.

================
The entry-point
================

| In any application, the operating system will call its entry-point.
| In C#, the entry point would look like the following:

.. code-block:: csharp
    :linenos:

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace ConsoleApp1
    {
        class Program
        {
            static void Main(string[] args)
            {
            }
        }
    }

| When RCTC starts, its entry-point will be called. The system will create an instance of `IHostApplication` to house all of its internal parts.
| To aid in the construction of the app-host, an `IAppBuilder` aids in calling all hooks, creating the DI container, and injecting dependencies into the application.
| The entry-point is responsible in the construction of the app-builder and the app-host.
| The default entry-point for RCTC is the following:

.. literalinclude:: /CodeExamples/defaultEntryPoint.cs
   :language: csharp
   :tab-width: 5
   :linenos:

