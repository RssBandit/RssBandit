A small console application to keep VS7 *.[LCID].resx files 
in sync. with the main resource.

	DiffPatchResources.exe -?
	
Will print little parameter help to the console window.
	
It expect the path and filename to the main text resource file (.resx, no language LCID)
as a comandline parameter, so you can simply drag/drop the main resource file to the
executable file, e.g.:

	DiffPatchResources.exe "C:\MyProjects\MyApp\Resources\mainText.resx"

It will iterate the "/root/data" entries in the main file (e.g. main.resx) and 
patch each dependent language file (mainText.[LCID].resx) to contain any missing entry.
Further, if the source data contains a comment element and the content of that starts
with "CHANGED" text, it will replace the yet existing target entry with the changed
entry of the source.

It is able to convert resource files (Forms and also "normal" resource files) back and forth 
from .NET 1.0, 1.1 and back from 2.0 (see cmdline option -c)

It is able to cleanup data nodes in the *.[LCID].resx files, that are no longer exist in
the main resource file. Furthe it removes also any "*.Location" and "*.Size" elements.
(see cmdline option -d)

It writes a .bak file on each modified resource and reports success/failures and some
statistics to the console.

Cmdline option -t switches on a test/verbose-mode, where no files are modified and only 
steps/nodes reported that are applied and changed.

Option -s generates a .strings file from the provided input file to be used with the famous
StringResourceTool.

========= ATTENTION! =============

I will not guarantee for any success and will not be responsible
for any damage resulting of the any usage of this tool!

Have fun, peace!

Torsten Rendelmann
http://www.rendelmann.info


PS: the sources are created while working on the Open Source project RSS Bandit,
http://www.rssbandit.org.

You can retrieve the sources from sourceforge via anonymous CVS access at
http://dev.rssbandit.org