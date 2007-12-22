using System;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using System.Collections.Generic;

using RssBandit.WinGui.Forms;
using RssBandit.Resources;
using NewsComponents.Collections;
using NewsComponents;

using Logger = RssBandit.Common.Logging;

namespace RssBandit.WinGui{
    class TopStoriesThreadHandler: EntertainmentThreadHandlerBase {

        private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(TopStoriesThreadHandler));

        /// <summary>
        /// The path to the file that will be the Top Stories HTML page. 
        /// </summary>
        private string memeFile = null; 

        /// <summary>
        /// The calling instance of RSS Bandit application
        /// </summary>
        private RssBanditApplication rssBanditApp = null;     

        /// <summary>
        /// The list of top stories. 
        /// </summary>
        private List<RelationHRefEntry> TopStories = null;

        /// <summary>
        /// Default constructor cannot be called
        /// </summary>
        private TopStoriesThreadHandler() { ;}

        /// <summary>
        /// Initializes the object with the calling RssBanditApplication instance. 
        /// </summary>
        /// <param name="rssBanditApp">The calling RssBanditApplication</param>
        /// <param name="memeFile">The file to write the Top Stories to after they have been determined.</param>
        public TopStoriesThreadHandler(RssBanditApplication rssBanditApp, string memeFile) {
            this.rssBanditApp = rssBanditApp;
            this.memeFile = memeFile;
        }

        /// <summary>
        /// Does the actual work of determining the top stories
        /// </summary>
        protected override void Run() {

            try {
                TopStories = rssBanditApp.FeedHandler.GetTopStories(new TimeSpan(7, 0, 0, 0), 10);
                this.GenerateTopStoriesPage();
            } catch (ThreadAbortException) {
                // eat up
            } catch (Exception ex) {
                p_operationException = ex;
                _log.Error("GetTopStories() exception", ex);
            } finally {
                WorkDone.Set();
            }
        
        }

        /// <summary>
        /// Generates a the Top Stories HTML page 
        /// </summary>
        private void GenerateTopStoriesPage() {

            XmlWriter writer = XmlWriter.Create(memeFile);

            writer.WriteStartElement("html");
            writer.WriteStartElement("head");
            writer.WriteElementString("title", SR.TopStoriesHtmlPageTitle(RssBanditApplication.Name));
            writer.WriteEndElement();
            writer.WriteStartElement("body");
            writer.WriteStartElement("ol");

            foreach (RelationHRefEntry topStory in TopStories) {
                writer.WriteStartElement("li");
                writer.WriteStartElement("p");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", topStory.HRef);
                writer.WriteString(topStory.Text);
                writer.WriteEndElement(); //a 
                // writer.WriteString(" (" + topStory.Score + ")"); 
                writer.WriteElementString("p", SR.TopStoriesHtmlDiscussionSectionTitle);
                writer.WriteStartElement("ul");
                foreach (NewsItem item in topStory.References) {
                    writer.WriteStartElement("li");
                    writer.WriteString(item.FeedDetails.Title + ": ");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", item.Link);
                    writer.WriteString(item.Title);
                    writer.WriteEndElement(); //a 
                    writer.WriteEndElement(); //li
                }
                writer.WriteEndElement(); //ul
                writer.WriteEndElement(); //p//
                writer.WriteEndElement(); //li
            }//foreach

            writer.WriteEndDocument();
            writer.Close(); 
        }

    }
}
