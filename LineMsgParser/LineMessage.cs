using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineMsgParser
{
    class LineMessage
    {
        private string _time;
        private string _liner;
        private string _message;

        public LineMessage(string msg)
        {
            string[] seg = msg.Split('\t');
            switch (seg.Length)
            {
                case 2:
                    this._time = seg[0];
                    //Zac邀請hsinyi加入群組
                    //2014/06/15(Sun)
                    //14:49	Daniel J.Lee joined the chat.
                    //2014/06/17(Tue)
                    //19:45	黃小華 invited 王俊豪 to the group.

                    if (seg[1].Contains("邀請") && seg[1].Contains("加入群組"))
                    {
                        int index = seg[1].IndexOf("邀請");
                        this._liner = seg[1].Substring(0, index);
                    }
                    else if (seg[1].Contains("joined the chat."))
                    {
                        this._liner = seg[1].Replace("joined the chat.", string.Empty);
                    }
                    else
                    {
                        this._liner = "系統";
                    }

                    this._message = seg[1];
                    break;
                case 3:
                    this._time = seg[0];
                    this._liner = seg[1];
                    this._message = seg[2];
                    break;
            }
      
        }

        public string Time
        {
            get
            {
                return _time;
            }
        }

        public string Liner
        {
            get
            {
                return _liner;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }


    }
}
