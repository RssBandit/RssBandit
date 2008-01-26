using System;

namespace RssBandit.Exceptions
{

    #region Feed-/ExceptionEventArgs classes

    /// <summary>
    /// Used to populate exceptions without throwing them. 
    /// </summary>
    [Serializable]
    public class ExceptionEventArgs : EventArgs
    {

        public ExceptionEventArgs()
        {
        }

        public ExceptionEventArgs(Exception exception, string theErrorMessage)
        {
            failureException = exception;
            errorMessage = theErrorMessage;
        }

        private Exception failureException;

        public Exception FailureException
        {
            get
            {
                return failureException;
            }
            set
            {
                failureException = value;
            }
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }
    }

    /// <summary>
    /// Used to populate feed exceptions without throwing them. 
    /// </summary>
    [Serializable]
    public class FeedExceptionEventArgs : ExceptionEventArgs
    {
        ///// <summary>
        ///// We define also the delegate here.
        ///// </summary>
        //public delegate void EventHandler(object sender, FeedExceptionEventArgs e);

        public FeedExceptionEventArgs()
        {
        }

        public FeedExceptionEventArgs(Exception exception, string link, string theErrorMessage)
            : base(exception, theErrorMessage)
        {
            feedLink = link;
        }

        private string feedLink;

        public string FeedLink
        {
            get
            {
                return feedLink;
            }
            set
            {
                feedLink = value;
            }
        }
    }

    #endregion
}

