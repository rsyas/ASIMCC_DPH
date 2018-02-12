Purpose:
When the Web Service attempts to make an entry in the log, it encounters the System.Security.SecurityException.
It complains about not being able to find the Event Source.  It fails to create the Event Source.
Because by default ASPNET/Network Services does not have the correct user rights to create an event source.

To solve this problem:
Using the EventLogInstaller class in the System.Diagnostics namespace create an Event source.
This Dll "SendSSHIE.Integration.aEventLogInstaller.dll" is used to create Event Sources used by the WebSite1 Service windows event viewer log -SendSSHIE.

How to Do:
Open command prompt - 'Run as administrator'.

Change to the directory where the "SendSSHIE.Integration.aEventLogInstaller.dll" is located.
Then execute the following command:

InstallUtil SendSSHIE.Integration.EventLogInstaller.dll

InstallUtil displays the progress of creating a new Event Sources.
It also logs the progress in the SendSSHIE.Integration.EventLogInstaller.InstallLog file 
that is created in the same location as the SendSSHIE.Integration.aEventLogInstaller.dll.
After the commit phase, it displays the message:

The Commit phase completed successfully.
The transacted install has completed.

Now that the Event Source is successfully created. we can find it in Registry Editor.

Registry key path = HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\eventlog\SendSSHIE
