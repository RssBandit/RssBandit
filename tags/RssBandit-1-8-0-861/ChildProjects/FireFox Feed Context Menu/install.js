const APP_DISPLAY_NAME   = "RSS Feed Subscriber";
const APP_NAME           = "feedsubscriber";
const APP_PACKAGE        = "/Torsten Rendelmann/feedsubscriber";
const APP_VERSION        = "0.1";

const APP_JAR_FILE       = "feedsubscriber.jar";
const APP_CONTENT_FOLDER = "content/feedsubscriber/";
const APP_LOCALE_FOLDER  = "locale/en-US/feedsubscriber/";

const APP_SUCCESS_MSG    = APP_DISPLAY_NAME + " installation successful. \n\n Restart browser, then right-click on a link to subscribe. "

initInstall(APP_NAME, APP_PACKAGE, APP_VERSION);

//Get installation directory
var installdir;
if(confirm("Install this into the application directory?  (Cancel will install into your profile directory)"))
	installdir = getFolder("Chrome");
else
	installdir = getFolder("Profile", "chrome");

	logComment(APP_NAME + ": Installation target: " + installdir + "\n");

	// Add file and register chrome
	var err = addFile(APP_PACKAGE, APP_VERSION, APP_JAR_FILE, installdir, null);

	if ( err == SUCCESS)
	{
		var jar = getFolder(installdir, APP_JAR_FILE);
		registerChrome(CONTENT | DELAYED_CHROME, jar, APP_CONTENT_FOLDER);
		registerChrome(LOCALE  | DELAYED_CHROME, jar, APP_LOCALE_FOLDER);

		//Install
		if(getLastError() == SUCCESS)
		{			logComment(APP_NAME + ": Begin performInstall **");
			
			// Do the deed
			err = performInstall();

			if(!(err == SUCCESS || err == 999)) {

				logComment(APP_NAME + ": performInstall failed with error: \n" + err);

				alert("An error occured during installation !\nErrorcode: " + err);
			}
			else
 				logComment(APP_NAME + ": End performInstall **\n"); 				alert(APP_SUCCESS_MSG);		}
		else
		{			// Failed to register
			logComment(APP_NAME + ": Installation failed registering folder with error: " + getLastError());
			alert("An error occurred, installation will be canceled.\nErrorcode: " + getLastError());
			cancelInstall(getLastError());
		}
	
	}
	else
	{
			//Problem with the XPI
			logComment(APP_NAME + ": Error creating file: " + err);
			alert("AddFile failed with error: " + err);
			cancelInstall(err);	
	}