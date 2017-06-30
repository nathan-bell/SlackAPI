using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackAPI
{
    public class Message : SlackSocketMessage
    {
        public string channel;
        public DateTime ts;
        public string user;
        public bool mrkdwn;
        /// <summary>
        /// Isn't always set. Should look up if not set.
        /// </summary>

        public string username;
        public string text;
        public bool is_starred;
        public string permalink;
        public Reaction[] reactions;

        public Attachment[] attachments;
        //Wibblr? Not really sure what this applies to.  :<
    }
}
