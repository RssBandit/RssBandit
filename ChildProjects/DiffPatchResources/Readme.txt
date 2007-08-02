A small console application to keep VS7 .resx files in sync. with the main resource.

It expect the path and filename to the main text resource file (.resx, no language LCID)
as a comandline parameter, so you can simply drag/drop the main resource file to the
executable file, e.g.:

	DiffPatchResources.exe "C:\MyProjects\MyApp\Resources\mainText.resx"

It will iterate the "/root/data" entries in the main file (e.g. main.resx) and 
patch each dependent language file (mainText.[LCID].resx) to contain any missing entry.
Further, if the source data contains a comment element and the content of that starts
with "CHANGED" text, it will replace the yet existing target entry with the changed
entry of the source.

It writes a .bak file on each modified resource and reports success/failures and some
statistics to the console.


========= ATTENTION! =============

I will not guarantee for any success and will not be responsible
for any damage resulting of the any usage of this tool!

Have fun, peace!

Torsten Rendelmann
http://www.rendelmann.info
