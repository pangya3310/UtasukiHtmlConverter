using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Xml;

namespace UtasukiHtmlConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".html")
                {
                    ConvertHtml(file);
                }
            }

            MessageBox.Show("轉換完成！");
        }

        private void ConvertHtml(string htmlPath)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(htmlPath, Encoding.UTF8);

            var titleNodes = doc.DocumentNode
                .SelectNodes("//*[contains(@class,'content-song__text')]");

            if (titleNodes == null) return;

            var results = new List<string>();

            foreach (var titleNode in titleNodes)
            {
                var container = titleNode.Ancestors()
                    .FirstOrDefault(n =>
                        n.Name == "div" || n.Name == "li" || n.Name == "tr");

                if (container == null) continue;

                string title = titleNode.InnerText.Trim();
                string artist = GetText(container, ".//*[contains(@class,'content-artist__text')]");
                string user = GetText(container, ".//*[contains(@class,'content-user__text')]");

                if (string.IsNullOrEmpty(artist)) continue;

                // 預設不顯示使用者
                // results.Add($"{title} / {artist}");
                // 若要顯示使用者：
                results.Add($"{title} / {artist} ｜ {user}");
            }

            // 倒序
            results.Reverse();

            var sb = new StringBuilder();
            for (int i = 0; i < results.Count; i++)
            {
                sb.AppendLine($"{(i + 1):D2}.{results[i]}");
            }

            string outputPath = Path.Combine(
                Path.GetDirectoryName(htmlPath)!,
                Path.GetFileNameWithoutExtension(htmlPath) + "_converted.txt"
            );

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private string GetText(HtmlNode root, string xpath)
        {
            var node = root.SelectSingleNode(xpath);
            return node?.InnerText.Trim() ?? "";
        }
    }
}
