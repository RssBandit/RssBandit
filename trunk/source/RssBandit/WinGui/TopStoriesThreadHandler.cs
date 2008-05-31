#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Threading;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Collections.Generic;

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
        private string memeFile; 

        /// <summary>
        /// The calling instance of RSS Bandit application
        /// </summary>
        private readonly RssBanditApplication rssBanditApp;     

        /// <summary>
        /// The list of top stories. 
        /// </summary>
        private List<RelationHRefEntry> TopStories;

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
                TopStories = new  List<RelationHRefEntry>(rssBanditApp.FeedHandler.GetTopStories(new TimeSpan(7, 0, 0, 0), 10));
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
            writer.WriteElementString("title", String.Format(SR.TopStoriesHtmlPageTitle, RssBanditApplication.Name));
            writer.WriteEndElement();
            writer.WriteStartElement("body");

            if (TopStories.Count == 0)
            {
                writer.WriteString(SR.NoTopStoriesMessage);
            }
            else
            {
                writer.WriteElementString("h1", SR.TopStoriesHtmlPageTitle.Replace("{0}",String.Empty));
                writer.WriteElementString("p", SR.TopStoriesDescription); 
                writer.WriteStartElement("ol");

                for (int i = 0; i < TopStories.Count; i++)
                {
                    RelationHRefEntry topStory = TopStories[i];
                    writer.WriteStartElement("li");
                    writer.WriteStartElement("p");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", topStory.HRef);
                    writer.WriteString(topStory.Text);
                    writer.WriteEndElement(); //a 
                    //writer.WriteString(" (" + topStory.Score + ")"); 

                    writer.WriteStartElement("map");
                    writer.WriteAttributeString("name", "discussion" + i);
                    writer.WriteStartElement("area");
                    writer.WriteAttributeString("shape", "rect");
                    writer.WriteAttributeString("coords", "0,0,16,16");
                    writer.WriteAttributeString("alt", SR.MenuCatchUpOnAllCaption);
                    writer.WriteAttributeString("href", "fdaction:?action=markdiscussionread&storyid=" + topStory.HRef);
                    writer.WriteEndElement(); //area                
                    writer.WriteEndElement(); //map

                    writer.WriteStartElement("p");
                    writer.WriteString(SR.TopStoriesHtmlDiscussionSectionTitle);
                    writer.WriteStartElement("img");
                    writer.WriteAttributeString("border", "0");
                    writer.WriteAttributeString("usemap", "#discussion" + i);
                    writer.WriteAttributeString("src", Path.Combine(Application.StartupPath, @"templates\images\read.gif"));
                    writer.WriteEndElement(); //img
                    writer.WriteEndElement(); //p


                    writer.WriteStartElement("ul");
                    foreach (NewsItem item in topStory.References)
                    {
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

            }//else

            writer.WriteEndDocument();
            writer.Close(); 
        }

    }
}
