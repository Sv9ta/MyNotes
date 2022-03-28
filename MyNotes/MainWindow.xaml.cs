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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //list.ItemsSource = (IEnumerable)list.Resources["array"];

            //----
            //--Непосредственное добавление с помощью ListView.Items.Add() сработало бы в том случае, если бы не было привязки к свойству ItemsSource. В нашем же случае надо добавить новый элемент не напрямую в ListView, а в коллекцию, к которой идет привязка, а затем уже обновить ListView.
            //Team makedonia = new Team() { Country = "Македония", Place = 5, Score = 8 };
            //приведение ресурса teams к типу ArrayList
           // ((ArrayList)lview.Resources["teams"]).Add(makedonia);
            //lview.Items.Refresh();


            //--- корневая папка
            trvStructure.BorderThickness = new Thickness(0);
            //trvStructure.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));  // transparent
            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo("C:\\Users\\Sveta\\Desktop\\test");
            System.IO.DirectoryInfo[] subDirs = rootDir.GetDirectories();
            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                trvStructure.Items.Add(CreateTreeItem(dirInfo));
            }
            ((TreeViewItem)trvStructure.Items[0]).IsSelected = true;

            //trvStructure.ItemContainerStyle.Resources.Add(Margin, 20);

            //-- или всетаки получать всю структуру папок полностью сразу?
            //-- http://professorweb.ru/my/ASP_NET/sites/level2/2_5.php
            

            //--- заполнение заметок
            //List<Item> items = new List<Item>();
            //items.Add(new Item() { Name = "John Doe", Type = "fg" });
            //items.Add(new Item() { Name = "Jane Doe", Type = "fg" });
            //items.Add(new Item() { Name = "Sammy Doe", Type = "fg" });
            //NotesLv.ItemsSource = items;

            //-- для редактирования текста заметки
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };


            this.colorList.ItemsSource = typeof(Brushes).GetProperties();
        }




        public static string NotePath = "";

//В хорошо спроектированном Windows-приложении прикладная логика находится не в обработчиках событий, а закодирована в высокоуровневых методах. 
//Каждый из этих методов представляет одну решаемую приложением "задачу". 
//Каждая задача может полагаться на дополнительные библиотеки (вроде отдельно компилируемых компонентов, в которых инкапсулируется бизнес-логика или доступ к базам данных)

       

//Вообщем помучился еще немного, вы правы смотря откуда копируешь. 
//    Пока что самый подходящий вариант брать из буфера как html и там в заголовке будет ссылка на источник 
//        и байты с какого по какой скопировано (грубо говоря, да и то не все браузеры передают) и самому уже загружать то, что надо.

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string[] countries = { "Россия", "Португалия", "Израиль", "Азербайджан", "Сев. Ирландия", "Люксембург" };
        //    list.ItemsSource = countries;
        //} 


        private void CutCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = (rtb != null) && (rtb.Selection > 0);
            e.CanExecute = (rtb != null);
        }

        private void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtb.Cut();
        }

        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText() || Clipboard.ContainsImage();
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //rtb.Paste();
            string text = "";
            BitmapSource imgText = null;

            text = Clipboard.GetText();

            if (Clipboard.ContainsText(TextDataFormat.Html))
            {
               // text = Clipboard.GetText(TextDataFormat.Html);
            }
            else if (Clipboard.ContainsText(TextDataFormat.Rtf))
            {
                text = Clipboard.GetText(TextDataFormat.Rtf);
            }
            //else if (Clipboard.ContainsText(TextDataFormat.Text))
            //{
            //    text = Clipboard.GetText(TextDataFormat.Text);
            //}

            if (Clipboard.ContainsImage())
            {
                imgText = Clipboard.GetImage();
            }

            //text = "<HTML><BODY><H1>Welcome</H1><CENTER><H2>Overview<H2></CENTER><P>Be sure to <B>Refresh</B> this page.</P></BODY></HTML>";

            //TextSelection ts = rtb.Selection;
            TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
            //if (textRange.CanLoad(DataFormats.Rtf))
            //{
           //textRange.Load(new MemoryStream(Encoding.Default.GetBytes(text)), DataFormats.Rtf);
            //textRange.Load(new MemoryStream(Encoding.Default.GetBytes(text)), DataFormats.Html);
                //SetDefaultFontColor(textRange);
            //}

           //textRange.Load(imgText, DataFormats.Bitmap);
           //textRange.Load(new MemoryStream();

           byte[] bytes;

           //bytes = GetImageByteArray(imgText);
          // textRange.Save(new MemoryStream(bytes), DataFormats.Rtf);
           //I see that if I paste an image into the RichTextBox control and save that control's contents using the TextRange, it does indeed save the image.

            //DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
            //rtb.Paste(format);


           // Судя по всему в TextRange может быть только текст. если выделить текст с картинкой, то в TextRange только текст

            rtb.AppendText(text); //-- ставляется только текст без картинки
           // rtb.SetValue()
           //rtb.Paste();

            //---------------------------
            //Image image = new Image();
            //image.Source = imgText;
            ////-- Вставка картинки на место курсора
            //TextPointer tp = rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            //Floater floater = new Floater(new BlockUIContainer(image), tp);
            //floater.HorizontalAlignment = HorizontalAlignment.Center;
            //floater.Width = image.Width;
            //-------------------------------------

            /*          
           Элемент Floater можно применять и для вывода рисунков. Но, как ни странно, не существует элемента вывода потокового содержимого, предназначенного для этой задачи. Поэтому элемент Image придется применять вместе с элементом BlockUIContainer или InlineUIContainer.

           Здесь есть один опасный момент. При вставке плавающего окошка, заключающего в себе изображение, потоковый документ предполагает, что рисунок должен быть по ширине таким же, как и вся колонка текста. Размер находящегося внутри элемента Image будет изменен, что может привести к возникновению проблем, если придется сильно уменьшить или увеличить растровое изображение.

           С помощью свойства Image.Stretch можно запретить изменение размеров изображения, хотя в этом случае плавающее окошко все равно займет всю колонку по ширине — просто вокруг рисунка останутся пустые места. Единственным подходящим решением при внедрении растрового изображения в потоковый документ является указание фиксированных размеров плавающего окошка. После этого с помощью свойства Image.Stretch можно определить, как будет меняться размер изображения в этом окошке.
           http://professorweb.ru/my/WPF/documents_WPF/level28/28_4.php
           */

            //rtb.Paste(DataFormats.Bitmap);
            //HtmlToXamlConverter
          // myWebBrowser.DataContext 
            //txtEditor.AppendText(text);

            //-- Вставка в rtb1
            rtb1.Paste();

            //-- Вставка в браузер
            //myWebBrowser.NavigateToString(text);


           // ImageFromClipboardDib();


            //MemoryStream ms = Clipboard.GetDataObject() as MemoryStream;
            //if (ms != null) {
            //    byte[] dibBuffer = new byte[ms.Length];
            //    ms.Read(dibBuffer, 0, dibBuffer.Length);

            //}


            //-- Посмотреть здесь!!
            //http://stackoverflow.com/questions/8442085/receiving-an-image-dragged-from-web-page-to-wpf-window

            //--- TEST
            DataObject retrievedData1 = (DataObject)Clipboard.GetDataObject();
            if (retrievedData1.GetDataPresent(typeof(BitmapSource)))
            {
                BitmapSource bs1 = retrievedData1.GetData(typeof(BitmapSource)) as BitmapSource;
            }
            
            if (retrievedData1.GetDataPresent(typeof(string)))
            {
                string ss1 = retrievedData1.GetData(typeof(string)) as string;
            }


            //------------------------------------------
            // data to the Clipboard in multiple formats.
            DataObject data = new DataObject();

            // Add a Customer object using the type as the format.
            data.SetData(new Customer("Customer as Customer object"));

            data.SetImage(imgText);

            data.SetText("jdjgdgdgdg",TextDataFormat.Text);

            Clipboard.SetDataObject(data);
            DataObject retrievedData = (DataObject)Clipboard.GetDataObject();

            if (retrievedData.GetDataPresent(typeof(Customer)))
            {
                Customer customer =
                    retrievedData.GetData(typeof(Customer)) as Customer;
                if (customer != null)
                {
                    MessageBox.Show(customer.Name);
                }
            }
            if (retrievedData.GetDataPresent(typeof(BitmapSource)))
            {
                BitmapSource bs = retrievedData.GetData(typeof(BitmapSource)) as BitmapSource;
            }
            if (retrievedData.GetDataPresent(typeof(string)))
            {
                string ss = retrievedData.GetData(typeof(string)) as string;
            }

        }


        [Serializable]
        public class Customer
        {
            private string nameValue = string.Empty;
            public Customer(String name)
            {
                nameValue = name;
            }
            public string Name
            {
                get { return nameValue; }
                set { nameValue = value; }
            }
        }

        // Demonstrates SetData, ContainsData, and GetData
        // using a custom format name and a business object.
        public Customer TestCustomFormat
        {
            get
            {
                Clipboard.SetData("CustomerFormat", new Customer("Customer Name"));
                if (Clipboard.ContainsData("CustomerFormat"))
                {
                    return Clipboard.GetData("CustomerFormat") as Customer;
                }
                return null;
            }
        }

        //--------------




        private ImageSource ImageFromClipboardDib()
        {
            MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.EndInit();
            return bmp;
        }

        //private ImageSource ImageFromClipboardDib()
        //{
        //    MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
        //    if (ms != null)
        //    {
        //        byte[] dibBuffer = new byte[ms.Length];
        //        ms.Read(dibBuffer, 0, dibBuffer.Length);

        //        BITMAPINFOHEADER infoHeader =
        //            BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

        //        int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
        //        int infoHeaderSize = infoHeader.biSize;
        //        int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

        //        BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
        //        fileHeader.bfType = BITMAPFILEHEADER.BM;
        //        fileHeader.bfSize = fileSize;
        //        fileHeader.bfReserved1 = 0;
        //        fileHeader.bfReserved2 = 0;
        //        fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

        //        byte[] fileHeaderBytes =
        //            BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

        //        MemoryStream msBitmap = new MemoryStream();
        //        msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
        //        msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
        //        msBitmap.Seek(0, SeekOrigin.Begin);

        //        return BitmapFrame.Create(msBitmap);
        //    }
        //    return null;
        //}



        //public class BITMAPINFOHEADER
        //{
        //    public uint size;
        //    public int width;
        //    public int height;
        //    public ushort biPlanes;
        //    public ushort biBitCount;
        //    public uint biCompression;
        //    public uint biSizeImage;
        //    public int biXPelsPerMeter;
        //    public int biYPelsPerMeter;
        //    public uint biClrUsed;
        //    public uint biClrImportant;
        //}


        //[StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        //[StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        //public static class BinaryStructConverter
        //{
        //    public static T FromByteArray<T>(byte[] bytes) where T : struct
        //    {
        //        IntPtr ptr = IntPtr.Zero;
        //        try
        //        {
        //            int size = Marshal.SizeOf(typeof(T));
        //            ptr = Marshal.AllocHGlobal(size);
        //            Marshal.Copy(bytes, 0, ptr, size);
        //            object obj = Marshal.PtrToStructure(ptr, typeof(T));
        //            return (T)obj;
        //        }
        //        finally
        //        {
        //            if (ptr != IntPtr.Zero)
        //                Marshal.FreeHGlobal(ptr);
        //        }
        //    }

        //    public static byte[] ToByteArray<T>(T obj) where T : struct
        //    {
        //        IntPtr ptr = IntPtr.Zero;
        //        try
        //        {
        //            int size = Marshal.SizeOf(typeof(T));
        //            ptr = Marshal.AllocHGlobal(size);
        //            Marshal.StructureToPtr(obj, ptr, true);
        //            byte[] bytes = new byte[size];
        //            Marshal.Copy(ptr, bytes, 0, size);
        //            return bytes;
        //        }
        //        finally
        //        {
        //            if (ptr != IntPtr.Zero)
        //                Marshal.FreeHGlobal(ptr);
        //        }
        //    }
        //}








        //public void btnMarkUp_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (btnMarkUp.IsChecked.Value)
        //    {
        //        xamlTb.Visibility = System.Windows.Visibility.Visible;
        //        xamlTb.IsTabStop = true;
        //       // xamlTb.Text = rtb.Xaml;
        //    }
        //    else
        //    {
        //       // rtb.Xaml = xamlTb.Text;
        //        xamlTb.Visibility = System.Windows.Visibility.Collapsed;
        //        xamlTb.IsTabStop = false;
        //    }

        //}

        private byte[] GetImageByteArray(BitmapImage src)
        {
            MemoryStream stream = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)src));
            encoder.Save(stream);
            stream.Flush();
            return stream.ToArray();
        }

        private byte[] GetImageByteArray(BitmapSource src)
        {
            MemoryStream stream = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));
            encoder.Save(stream);
            stream.Flush();
            return stream.ToArray();
        }

       // private static void OnPaste(object sender, ExecutedRoutedEventArgs e)
        //{
        ////    RichTextBox richTextBox = sender as RichTextBox;
        ////    if (richTextBox == null) { return; }

        //    var dataObj = (IDataObject)Clipboard.GetDataObject();
        //    if (dataObj == null) { return; }

        //    if (Clipboard.ContainsText(TextDataFormat.Rtf))
        //    {
        //        string rtfText = Clipboard.GetText(TextDataFormat.Rtf);
        ////        TextSelection ts = richTextBox.Selection;
        ////        TextRange textRange = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
        ////        if (textRange.CanLoad(DataFormats.Rtf))
        ////        {
        ////            textRange.Load(new MemoryStream(_encoding.GetBytes(rtfText)), DataFormats.Rtf);
        ////            SetDefaultFontColor(textRange);
        ////        }
        //    }

        ////    e.Handled = true;
        //}


        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
            //if (sfd.ShowDialog() == true)
            //{
                TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                //using (FileStream fs = File.Create(sfd.FileName))
                using (FileStream fs = File.Create(NotePath))
                {
                    if (System.IO.Path.GetExtension(NotePath).ToLower() == ".rtf")
                    {
                        doc.Save(fs, DataFormats.Rtf);
                    }
                    else if (System.IO.Path.GetExtension(NotePath).ToLower() == ".txt")
                    {
                        doc.Save(fs, DataFormats.Text);
                    }
                    else
                    {
                        doc.Save(fs, DataFormats.Xaml);
                    }
                }
            //}
        }




        private void buttonOpenFile0_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";

            string fileName = null;
            if (ofd.ShowDialog() == true)
            {
                fileName = ofd.FileName;
            }
            bool isExists = File.Exists(fileName) ? true : false;

            string strrrrr = "";

            TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

            if (isExists)
            {
                try
                {
                    //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    //{
                    //    if (fs.Length != 0)
                    //    {
                    //        string s = "";
                    //        using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                    //        {
                    //            for (int i = 0; !sr.EndOfStream; i++)
                    //            {
                    //                // slistF.Add(i, sr.ReadLine());
                    //                s += sr.ReadLine();
                    //                //if (s.Trim(new Char[] { ' ', '\t' }).Length > 0)
                    //                //    slistF.Add(i, s);
                    //                //else
                    //                //    i--;
                    //            }
                    //        }
                    //        strrrrr = s;
                    //    }

                    //    doc.Load(fs, DataFormats.Rtf);
                    //}
                    //--

                    var dataFormat = DataFormats.Rtf;
                    if (System.IO.Path.GetExtension(fileName).ToLower() == ".txt")
                        dataFormat = DataFormats.Text;
                    else if (System.IO.Path.GetExtension(fileName).ToLower() == ".rtf")
                        dataFormat = DataFormats.Rtf;
                    else if (System.IO.Path.GetExtension(fileName).ToLower() == ".bmp")
                        dataFormat = DataFormats.Bitmap;

                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    doc.Load(fs, dataFormat);
                    fs.Close();
                }
                catch (FileNotFoundException ioEx)
                {
                    string ex = ioEx.Message;
                }
            }

            //------------
            //TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);



            //doc.Load(new System.IO.FileStream("my.rtf", System.IO.FileMode.Open), DataFormats.Rtf);



            //var document = new FlowDocument();
            //var paragraph = new Paragraph();
            //SolidColorBrush br = Brushes.White;

            //paragraph.Background = br;
            //paragraph.Inlines.Add(strrrrr);
            //document.Blocks.Add(paragraph);
            //document.Background = Brushes.White;
            //rtb.Document = document;

            
        }

        

        private void OpenNotebooks()
        {
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";

            //string fileName = null;
            //if (ofd.ShowDialog() == true)
            //{
            //    fileName = ofd.FileName;
            //}

            string dir1 = System.IO.Path.GetDirectoryName("C:\\Users\\Sveta\\Desktop\\test");
            System.IO.Path.GetFileName("C:\\Users\\Sveta\\Desktop\\test");

            //var dir = new System.IO.DirectoryInfo(dir + @"\Шаблоны\" + spg + @"\");
            var dir = new System.IO.DirectoryInfo("C:\\Users\\Sveta\\Desktop\\test");
            //var files = dir.GetFiles("*.*");
            FileInfo[] files = dir.GetFiles("*.*");


            //var root = dir;
            //DirectoryInfo[] di = root.GetDirectories("*.*", System.IO.SearchOption.AllDirectories);

            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo("C:\\Users\\Sveta\\Desktop\\test");
            WalkDirectoryTree(rootDir);

            //list1.Items.Clear();
            //list1.ItemsSource = files;
            //list1.DisplayMemberPath = "Name";

        }


        static void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;


            ArrayList listItems = new ArrayList(); //---?????


            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                //log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                //Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                Item itm = null;
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we 
                    // want to open, delete or modify the file, then 
                    // a try-catch block is required here to handle the case 
                    // where the file has been deleted since the call to TraverseTree().
                    //Console.WriteLine(fi.FullName);
                    itm = new Item();
                    itm.Name = fi.FullName;
                    itm.Type = "note";
                    listItems.Add(itm);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }

        private void buttonOpenNotebooks_Click(object sender, RoutedEventArgs e)
        {
            OpenNotebooks();
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);
                try
                {
                    //--добавление подпапок
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateTreeItem(subDir));
                    //--добавление файлов
                    //foreach(System.IO.FileInfo file in expandedDir.GetFiles("*.*"))
                    //    item.Items.Add(CreateTreeItem(file));
                    if (expandedDir.GetFiles("*.*") != null)
                        fillNotes(expandedDir.GetFiles("*.*"), expandedDir.Name);
                }
                catch { }
            }
        }

        private void fillNotes(System.IO.FileInfo[] files, string notebookName)
        {
            List<Item> items = new List<Item>();
            foreach (System.IO.FileInfo file in files)
            {
                //items.Add(new Item() { Name = file.ToString(), Type = file.Name });
                //--
                TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
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

                items.Add(new Item() { 
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

            NotesLv.ItemsSource = items;
            //--выделяем первую заметку
            NotesLv.SelectedIndex = 0;

            //-- задаем название блокнота
            NotebookNameTbl.Text = notebookName;
            //-- название блокнота в заметке
            NotePaneltbl1.Text = notebookName;
        }

        private TreeViewItem CreateTreeItem(object o)
        {
            //--Есть ли вложенные папки
            bool isStack = false;
            if (o is DirectoryInfo)
                if ((o as DirectoryInfo).GetDirectories().Length > 0)
                    isStack = true;

            TreeViewItem item = new TreeViewItem();
            StackPanel stp = new StackPanel();
            
            Image img = new Image();
            if (isStack)
                img.Source = new BitmapImage(new Uri(@".\Images\stack.png", UriKind.Relative));
            else
                img.Source = new BitmapImage(new Uri(@".\Images\notebook2_icon.png", UriKind.Relative));
            img.Stretch = Stretch.None;

            TextBlock tbx = new TextBlock();
            tbx.Margin = new Thickness(5, 0, 0, 0);
            tbx.Text = o.ToString();

            //item.Header = o.ToString();
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

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            DirectoryInfo selectedDir = null;
            TreeViewItem item = e.Source as TreeViewItem;
            if (item.Tag is DirectoryInfo)
                selectedDir = (item.Tag as DirectoryInfo);
            if (selectedDir.GetFiles("*.*") != null)
                fillNotes(selectedDir.GetFiles("*.*"), selectedDir.Name);
        }

        private void NotesLv_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Item item = ((sender as ListView).SelectedItem as Item);
            if (item != null)
            {
                //--выводим заметку на экран
                TextRange note = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                Stream outputStream = new MemoryStream();
                outputStream.Write(item.NoteBytes, 0, item.NoteBytes.Length);
                note.Load(outputStream, item.DataFormat);
                //-- информация о заметке
                NotePaneltbl2.Inlines.Clear();
                Run rn;
                rn = new Run();
                rn.Text = "Создана: ";
                rn.Foreground = Brushes.Black;
                NotePaneltbl2.Inlines.Add(rn);
                rn = new Run();
                rn.Text = item.DTimeCreate.ToShortDateString() + " " + item.DTimeCreate.ToShortTimeString();
                NotePaneltbl2.Inlines.Add(rn);
                rn = new Run();
                rn.Text = "   Изменена: ";
                rn.Foreground = Brushes.Black;
                NotePaneltbl2.Inlines.Add(rn);
                rn = new Run();
                rn.Text = item.DTimeModified.ToShortDateString() + " " + item.DTimeModified.ToShortTimeString();
                NotePaneltbl2.Inlines.Add(rn);

                NoteNameTbx.Text = item.Name;

                NotePath = item.Path;
            }
        }


        #region Панелька редактирования текста
        
        private void rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            EditNote.Visibility = Visibility.Visible;

            object temp = rtb.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = rtb.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = rtb.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;
            temp = rtb.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.Text = temp.ToString();
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtb.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtb.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
        }

        private void buttonColorPick_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = true;
        }

        private void colorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //---посмотреть----------
            //System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();

            //if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    BrushConverter conv = new BrushConverter();

            //    string strColor = System.Drawing.ColorTranslator.ToHtml(dlg.Color);

            //    btnColor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(strColor));
            //}
            //-----------------------

            Brush selectedColor = (Brush)(e.AddedItems[0] as System.Reflection.PropertyInfo).GetValue(null, null);
            rtb.Selection.ApplyPropertyValue(Inline.ForegroundProperty, selectedColor);
            //colorRect.Fill = selectedColor;
            buttonColorPick.Background = selectedColor;

            //http://www.c-sharpcorner.com/UploadFile/87b416/create-color-picker-in-wpf/
            //http://www.codeproject.com/Articles/779105/Color-Canvas-and-Color-Picker-WPF-Toolkit
        }

        #endregion

        private void ImgSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //--переименование
            int c = NotePath.LastIndexOf('\\');
            string path = NotePath.Substring(0, c+1);
            string NotePath2 = path + NoteNameTbx.Text + ".rtf";
            File.Move(NotePath, NotePath2);

            TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
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
            items = (List<Item>)NotesLv.ItemsSource;
            foreach (Item itm in items)
            {
                if (itm.Path == NotePath)
                {
                    fs.Position = 0;
                    byte[] bytes = new byte[fs.Length];
                    int numBytesToRead = (int)fs.Length;
                    int n = fs.Read(bytes, 0, numBytesToRead);

                    itm.NoteBytes = bytes;
                    itm.Fs = fs;
                    itm.DTimeModified = DateTime.Today;
                    //itm.DataFormat --??
                    itm.Name = NoteNameTbx.Text;
                    //itm.Type
                    itm.Path = NotePath2;
                    NotePath = NotePath2;

                    break;
                }
            }
            fs.Close();
        }

        private void ImgDell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

    }


    class State
    {
        public string Name { get; set; }
        public string Capital { get; set; }

        public State(string name, string capital)
        {
            this.Name = name;
            this.Capital = capital;
        }

        public State()
        { }
    }



    public class Team
    {
        public string Country { get; set; }
        public int Place { get; set; }
        public int Score { get; set; }
    }


    public class Item
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime DTimeModified { get; set; }
        public DateTime DTimeCreate { get; set; }
        public string DataFormat { get; set; }
        public FileStream Fs { get; set; }
        public byte[] NoteBytes { get; set; }
        public string Path {get; set; }

        //public override string ToString()
        //{
        //    return this.Name + ", " + this.Type + " ffffff";
        //}
    }

}
