RSSBandit AdsBlocker 1.6.0.1
Only RSS Bandit 1.6.0.1 and higher is supported by this AddIn!

1. How to install
=================

a) You have downloaded the zipped package, now unpack them to a new subfolder
of the RSS Bandit installation folder, e.g. "AddIns".


b) Start RSS Bandit.

c) Open the "Tools" menu and select "Addins..."

d) Press the "Add..." button

e) Select the "AdsBlocker.AddIn2.dll" from the newly created folder.

f) Press "Close", and you are done.


2. How to configure
===================

In the same folder above you will find an "ads.blacklist.txt" file. Add more regular
expressions there to match your ad links to block. Each one must be a separate
line or concatenated with the ";" character. Lines that start with a hash sign "#"
are comments and ignored, same is true if it was used after a regex.

If you have a big blacklist, you may want to (re-)enable some of the blocked
links. Just create a file named "ads.whitelist.txt" in the same folder with the
regex of the links to unblock. Syntax is the same as for the blacklist.