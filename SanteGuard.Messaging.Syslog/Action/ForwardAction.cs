using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteGuard.Messaging.Syslog.TransportProtocol;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Represents a forwarding action
    /// </summary>
    public class ForwardAction : ISyslogAction
    {
        /// <summary>
        /// Handle invalid message 
        /// </summary>
        public void HandleInvalidMessage(object sender, SyslogMessageReceivedEventArgs e)
        {
            TransportUtil.Current.Forward((sender as SyslogListenerThread).Configuration.Forward, Encoding.UTF8.GetBytes(e.Message.Original));
        }

        /// <summary>
        /// Handle a message being received
        /// </summary>
        public void HandleMessageReceived(object sender, SyslogMessageReceivedEventArgs e)
        {
            TransportUtil.Current.Forward((sender as SyslogListenerThread).Configuration.Forward, Encoding.UTF8.GetBytes(e.Message.Original));
        }
    }
}
