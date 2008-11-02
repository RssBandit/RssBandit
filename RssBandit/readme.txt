Source Control Rules
===============

The rules of engagement for the source control system are easy, but we will kick out everyone 
(independent of celebrity or other honorary status) not following them. 

0.  If you are not sure whether you should/could/may add or change something, ask an project 
     administrator. Behave like a professional software developer, please -- even if you aren't one. 

1.  Every "official release" is has its own subdirectory (CVS module). As of this writing, we did have
     v1.2.0.61, v1.2.0.90 and this rule will apply the next time we release again. 
     Elements from these source trees **must not** be modified by anyone without an 
     administrator's explicit permission. They exist only for archiving purposes and to fit in patches. 

2.  All work is done in the "CurrentWork" subtree. When we plan to turn "CurrentWork" into a 
     new release, the snapshot of current work gets copied into a new version directory branch. 

3.  If you want to make changes, UPDATE the whole source tree first and then start modifying 
     select files. If you added new files, ensure to get them ADDED and COMMITED to CVS!
     After a check-in new files, wipe your working/merge directory, re-UPDATE the tree and 
     recompile locally. If that breaks, some files are missed, fix the problem. 

4. If you want to add a feature or fix a bug, we expect that you understand what you're doing. 

5. Checked-In items compile, work as expected and add features, not remove features. 

6. Any new feature that you add should be configurable in the options dialog or App.config. 

7. Please do NOT edit/save any installer related project with .NET 1.1 (Visual Studio 7.1) 
    as long they are of version .NET 1.0 

8. If you add controls/dialogs or other strings that could be visible anytime to an enduser, 
    consider l8n. Use 'Resource.Manager["RES_MyResourceId"]' or 'Resource.Manager["RES_MyResourceMsgId", params]'
    to get localized strings e.g. for MessageBox calls. 

9. If you have GPL code in your pocket coming in, leave it at the door, please. 

10. This list may grow.