How to install RCT Core (RCTC)
======================================

Installation of RCT Core is pretty simple. RCTC has been designed to be self-contained and lightweight.

=================================
System Requirements
=================================

System requirements fore RCTC is fairly lightweight:

* Windows 7+
* At least one NIC installed.
* .NET Framework 4.7+

.. admonition:: .NET Core
   :class: warning

   Although RCTC was built for .NET Core, the current build only supports Windows.
   You may need to build RCTC by hand and remove any dependencies not supported by .NET Core.

-----------
Windows 7+
-----------

RCTC requires a modern OS to function properly. Many different components of RCTC depend on features not included in older versions of Windows.
For example, the audio subsystem requires Windows Vista+ to use the WASAPI api for audio capturing and playback.

----
NIC
----

A NIC (Network interface card) is required for remote-control access. Any modern NIC will work.

.. admonition:: Static IP Address
   :class: warning

   A static IP address is recommended for any server software.

