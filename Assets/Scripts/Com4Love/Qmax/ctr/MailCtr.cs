using Com4Love.Qmax.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Qmax.Ctr
{
    public class MailCtr
    {
        public MailCtr()
        {
            GameController.Instance.Client.AddResponseCallback(Net.Module.Mail, OnMailResponse);
        }

        private void OnMailResponse(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)MailCmd.FEEDBACK)
            {
                if (GameController.Instance.ModelEventSystem.OnMailFeedBack != null)
                {
                    Action MailFeedBackThreadCallback = delegate ()
                    {
                        GameController.Instance.ModelEventSystem.OnMailFeedBack(status);
                    };

                    GameController.Instance.InvokeOnMainThread(MailFeedBackThreadCallback);
                }
            }
        }

        public void FeedBack(long actorId, string qq, string mail , string content)
        {
            GameController.Instance.Client.FeedBack(actorId , qq, mail, content);
        }
    }
}
