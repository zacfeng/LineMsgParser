using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace LineMsgParser
{
    class LineDailyMessages
    {
        private string _originText;
        private string _date;
        private List<LineMessage> _msglist;

        public LineDailyMessages(string date)
        {
            this._date = date;
            this._msglist = new List<LineMessage>();
        }

        public bool Add(string msg)
        {
            bool success = false;
            LineMessage lm = new LineMessage(msg);
            _msglist.Add(lm);
            return success;
        }


    }
}
