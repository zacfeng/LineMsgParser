using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ZacSharp;
using ZacSharp.Extensions.DataSet;
using System.Windows.Forms.DataVisualization.Charting;

namespace LineMsgParser
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private DataTable dt;
        private DateTime firstdate;
        private DateTime lastdate;

        private void Initial()
        {
            string path = tbFilepath.Text +@"\"+ cBoxMsgPick.Text;
            string content = ZacSharp.Utility.TextFile.ReadTextFile(path);

            dt = new DataTable();
            string date = string.Empty; //2014/08/30（六）
            string[] colnames = { "日期", "時間", "發話者", "訊息" };
            dt.AddColumns(colnames);

            foreach (string line in content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                //Regex regex = new Regex(@"(19|20)\d\d/\d\d/\d\d(（[一二三四五六日]）|\([Sun|Mon|Tue|Wed|Thu|Fri|Sat])\)");
                Regex regex = new Regex(@"(19|20)\d\d/\d\d/\d\d\((Sun|Mon|Tue|Wed|Thu|Fri|Sat)\)|（[一二三四五六日]）");
                Match match = regex.Match(line);
                if (match.Success && line.Length <=15)
                {
                    date = line;
                }
                else if (date != string.Empty && line != string.Empty)
                {
                    LineMessage lm = new LineMessage(line);
                    DataRow dr = dt.NewRow();
                    dr["日期"] = date;
                    dr["時間"] = lm.Time;
                    dr["發話者"] = lm.Liner;
                    dr["訊息"] = lm.Message;
                    dt.Rows.Add(dr);
                }
            }

            if(dt.Rows.Count == 0)
            {
                throw new Exception("您的LINE對話紀錄格式不符。 "+ Environment.NewLine+
                                    path + Environment.NewLine + 
                                    "請將部分記錄附件提供給開發人員: fengying0709@gmail.com");
            }

            string tmpforfirstdatestring = dt.Rows[0]["日期"].ToString().Split('\t')[0];
            string tmpforlastdatestring = dt.Rows[dt.Rows.Count - 1]["日期"].ToString().Split('\t')[0];

            string firstdatetimestring = tmpforfirstdatestring.Substring(0, Math.Min(tmpforfirstdatestring.Length, 10));
            string lastdatetimestring = tmpforlastdatestring.Substring(0, Math.Min(tmpforlastdatestring.Length, 10));

            firstdate = DateTime.ParseExact(firstdatetimestring, "yyyy/MM/dd", null);
            lastdate = DateTime.ParseExact(lastdatetimestring, "yyyy/MM/dd", null);

            dateTimePicker_Start.MaxDate = new DateTime(9000, 01, 01);
            dateTimePicker_End.MaxDate = new DateTime(9000, 01, 01);
            dateTimePicker_Start.MinDate = new DateTime(1800, 01, 01);
            dateTimePicker_End.MinDate = new DateTime(1800, 01, 01);

            dateTimePicker_Start.MaxDate = lastdate;
            dateTimePicker_End.MaxDate = lastdate;
            dateTimePicker_End.Value = lastdate;

            dateTimePicker_Start.MinDate = firstdate;
            dateTimePicker_End.MinDate = firstdate;
            dateTimePicker_Start.Value = firstdate;

            dataGridView1.DataSource = dt;
            dataGridView1.Columns[3].Width = 600;

            //將原來的DataTable做Distinct並複製到新DataTable，ToTable的第一個參數是設定是否
            //要做Distinct，當然要設成true，其他參數是要做Group By的欄位名稱
            DataTable dtGroup = dt.DefaultView.ToTable(true, "日期");

            //開始加欄位
            dtGroup.Columns.Add("計數");

            for (int i = 0; i < dtGroup.Rows.Count; i++)
            {
                //取資料，用String是因為上方加欄位時，沒指定型別為數字
                string strCount = dt.Select("日期='" + dtGroup.Rows[i]["日期"].ToString() + "'").Length.ToString();

                //設定資料
                dtGroup.Rows[i]["計數"] = (strCount == "" ? "0" : strCount);

                chart1.Series[0].Points.AddY(int.Parse(dtGroup.Rows[i]["計數"].ToString()));
            }

            dataGridView2.DataSource = dtGroup;
            chart1.Series[0].ChartType = SeriesChartType.FastLine;
            chart1.Series[0].IsValueShownAsLabel = true;

            List<string> SpeakerList = new List<string>();
            SpeakerList.Add("(All)");
            DataTable dtSpeaker = dt.DefaultView.ToTable(true, "發話者");
            for (int i = 0; i < dtSpeaker.Rows.Count; i++)
            {
                if (!SpeakerList.Contains(dtSpeaker.Rows[i]["發話者"].ToString().Trim())) SpeakerList.Add(dtSpeaker.Rows[i]["發話者"].ToString().Trim());
            }
            SpeakerList.Remove(string.Empty);
            SpeakerList.Sort();
            cBoxSpeaker.DataSource = SpeakerList;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbialog = new FolderBrowserDialog();
            if (fbialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbFilepath.Text = fbialog.SelectedPath; 
                List<string> filelist = new List<string>();
                DirectoryInfo dinfo = new DirectoryInfo(fbialog.SelectedPath);
                foreach(FileInfo f in dinfo.GetFiles("[LINE]*.txt", SearchOption.AllDirectories))
                {
                    filelist.Add(f.Name);
                }
                cBoxMsgPick.DataSource = filelist;
            }
        }

        private void OnChangeDatagridview()
        {
            string contentfilter = String.Format("'%{0}%'", tbSearch.Text.ToLower());
            string speakerfilter = String.Format("'{0}'", cBoxSpeaker.Text);
            string startdatefilter = string.Format("'{0}'", firstdate.ToString("yyyy/MM/dd"));
            string lastdatefilter = string.Format("'{0}'", lastdate.AddDays(1).ToString("yyyy/MM/dd"));
            if (speakerfilter == "'(All)'") speakerfilter = "[發話者]";

            DataView dv = new DataView(dt);
            dv.RowFilter = string.Format("訊息 LIKE {0} AND 發話者={1} AND 日期 >= {2} AND 日期 <={3}",
                                        contentfilter,speakerfilter,startdatefilter,lastdatefilter);
            dataGridView1.DataSource = dv;
        }

        private void cBoxMsgPick_SelectedIndexChanged(object sender, EventArgs e)
        {
            Initial();
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            OnChangeDatagridview();
        }

        private void cBoxSpeaker_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnChangeDatagridview();
        }

        private void dateTimePicker_Start_ValueChanged(object sender, EventArgs e)
        {
            firstdate = dateTimePicker_Start.Value;
            OnChangeDatagridview();
            Console.WriteLine("dateTimePicker_Start_ValueChanged");
        }

        private void dateTimePicker_End_ValueChanged(object sender, EventArgs e)
        {
            lastdate = dateTimePicker_End.Value;
            OnChangeDatagridview();
            Console.WriteLine("dateTimePicker_End_ValueChanged");
        }

        private void dgGrid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }


    }

}

