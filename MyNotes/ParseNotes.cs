using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using Microsoft.Win32;
using System.Globalization;

namespace MyNotes
{
    internal class ParseNotes
    {


        //--Bозвращает дерево всех вложенных директорий TreeViewItem
        public static TreeViewItem CreateTreeItem(object o)
        {
            //--Есть ли вложенные папки 
            bool isStack = false;
            if (o is DirectoryInfo)
                if ((o as DirectoryInfo).GetDirectories().Length > 0)
                    isStack = true;

            TreeViewItem item = new TreeViewItem();
            StackPanel stp = new StackPanel();

            //-- Назначаем иконку "папка с вложенными папками" или "блокнот"
            Image img = new Image();
            if (isStack)
                img.Source = new BitmapImage(new Uri(@".\Images\stack.png", UriKind.Relative));
            else
                img.Source = new BitmapImage(new Uri(@".\Images\notebook2_icon.png", UriKind.Relative));
            img.Stretch = Stretch.None;

            //-- вложенная папка
            TextBlock tbx = new TextBlock();
            tbx.Margin = new Thickness(5, 0, 0, 0);
            tbx.Text = o.ToString();

            item.Header = new StackPanel
            {
                Children = { img, tbx },
                Orientation = Orientation.Horizontal
            };

            item.Tag = o;
            if (!(o is FileInfo))
                item.Items.Add("Loading...");

            return item;
        }


        //-- Раскрытие элемента дерева
        public static DirectoryInfo TreeViewItemExpanded(TreeViewItem item)
        {
            DirectoryInfo expandedDir = null;

            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);

                try
                {
                    //--добавление подпапок
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateTreeItem(subDir));
                }
                catch { }
            }
            return expandedDir;
        }


        //-- Возвращает все заметки из выбранной директории списком List<Item>
        public static List<Item> FillNotes(System.IO.FileInfo[] files, string notebookName)
        {
            List<Item> items = new List<Item>();
            //--обход всех заметок-файлов в папке
            foreach (System.IO.FileInfo file in files)
            {
                var dataFormat = DataFormats.Rtf;
                if (System.IO.Path.GetExtension(file.Name).ToLower() == ".txt")
                    dataFormat = DataFormats.Text;
                else if (System.IO.Path.GetExtension(file.Name).ToLower() == ".rtf")
                    dataFormat = DataFormats.Rtf;
                else if (System.IO.Path.GetExtension(file.Name).ToLower() == ".bmp")
                    dataFormat = DataFormats.Bitmap;

                FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                byte[] bytes = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;
                int n = fs.Read(bytes, 0, numBytesToRead);
                fs.Close();

                items.Add(new Item()
                {
                    Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length),
                    Type = file.LastWriteTime.ToShortDateString(),
                    DataFormat = dataFormat,
                    DTimeModified = file.LastWriteTime,
                    DTimeCreate = file.CreationTime,
                    Fs = fs,
                    NoteBytes = bytes,
                    Path = file.FullName
                });
            }

            return items;
        }


        //-- Сохранение заметки
        public static void SaveNote(string NotePath, string NoteName, TextRange doc, List<Item> ItemsSource)
        {
            //--переименование (Название заметки)
            int c = NotePath.LastIndexOf('\\');
            string path = NotePath.Substring(0, c + 1);
            string NotePath2 = path + NoteName + ".rtf";
            File.Move(NotePath, NotePath2);

            FileStream fs = File.Create(NotePath2);
            //using (FileStream fs = File.Create(NotePath))
            //{
            if (System.IO.Path.GetExtension(NotePath2).ToLower() == ".rtf")
            {
                doc.Save(fs, DataFormats.Rtf);
            }
            else if (System.IO.Path.GetExtension(NotePath2).ToLower() == ".txt")
            {
                doc.Save(fs, DataFormats.Text);
            }
            else
            {
                doc.Save(fs, DataFormats.Xaml);
            }
            //}
            //-- обновление заметки в списке заметок
            List<Item> items = new List<Item>();
            items = ItemsSource;
            //-- добавить проверку на null
            foreach (Item itm in items)
            {
                if (itm.Path == NotePath)
                {
                    fs.Position = 0;
                    byte[] bytes = new byte[fs.Length];
                    int numBytesToRead = (int)fs.Length;
                    int n = fs.Read(bytes, 0, numBytesToRead);

                    itm.Name = NoteName;
                    itm.DTimeModified = DateTime.Today; //-- взять время сохранения
                    itm.NoteBytes = bytes;
                    itm.Fs = fs;
                    itm.Path = NotePath2;
                    NotePath = NotePath2;

                    break;
                }
            }
            fs.Close();
        }






    }
}
