using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string searchText = null;
        private int page = 1;
        private readonly string outputFilename = "output.csv";

        private void Form1_Load(object sender, EventArgs e)
        {
            this.webBrowser.Navigate("https://www.sgs.gov.cn/nameqry/searchTrdUseableName.action");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.searchText = this.cmbCriteria.Text;
            this.page = 1;
            //%CD%F8%C2%E7%BF%C6%BC%BC
            var postData = $"query=query&zihao=&trdName={WebUtility.UrlEncode(this.searchText)}&f_type=%CD%F8%C2%E7%BF%C6%BC%BC&trdId=07&f_sort1=07++++";
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(postData);
            string url = "https://www.sgs.gov.cn/nameqry/searchTrdUseableName.action";
            webBrowser.Navigate(url, string.Empty, bytes, "Content-Type: application/x-www-form-urlencoded");
        }

        private async void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var text = this.webBrowser.Document.Body.InnerHtml;
            //var pattern = @"<tr bgcolor=#FFF8E1 height=20>\s*<td class=p02>(?<name>.*)</td>\s*<td class=p02>(?<type>.*)</td>\s*<td class=p02>(?<category>.*)</td>\s*</tr>";
            var pattern = @"<tr height=""20"" bgcolor=""#fff8e1"">\s*<td class=""p02"">(?<name>.*)</td>\s*<td class=""p02"">(?<type>.*)</td>\s*<td class=""p02"">(?<category>.*)</td>\s*</tr>";
            var matches = Regex.Matches(text, pattern);
            using (var file = File.Open(this.outputFilename, FileMode.Append, FileAccess.Write, FileShare.Write))
            {
                foreach (Match match in matches)
                {
                    var name = match.Groups["name"];
                    var type = match.Groups["type"];
                    var category = match.Groups["category"];
                    var t = $"{name}, {type}, {category}" + Environment.NewLine;
                    var b = Encoding.UTF8.GetBytes(t);
                    file.Write(b, 0, b.Length);
                }
            }

            await Task.Delay(1000);

            page++;
            this.webBrowser.Document.InvokeScript("formsub", new string[] { this.page.ToString() });
            this.label1.Text = $"opening page {this.page}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var csv = new List<Hello>(); // or, List<YourClass>
            var lines = System.IO.File.ReadAllLines(this.outputFilename);
            foreach (string line in lines)
                csv.Add(new Hello( line.Split(','))); // or, populate YourClass          
            string json =  JsonConvert.SerializeObject(csv);
            File.WriteAllText("test.json", json);
        }

        private class Hello
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Category { get; set; }
            public Hello(params string[] values)
            {
                this.Name = values[0].Trim();
                this.Type = values[1].Trim();
                this.Category = values[2].Trim();
            }
        }
    }
}
