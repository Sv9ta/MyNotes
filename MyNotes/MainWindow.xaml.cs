using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.IO;
using System.Collections;
using Microsoft.Win32;
//using System.Globalization;
//using System.Windows.Forms; // для FolderBrowserDialog



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

            // Открыть папку с Записными книжками
            string folderName  = ParseNotes.ChooseNotebookFolder();

            trvStructure.BorderThickness = new Thickness(0);
            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo(folderName);
            System.IO.DirectoryInfo[] subDirs = rootDir.GetDirectories();
            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                trvStructure.Items.Add(ParseNotes.CreateTreeItem(dirInfo));
            }
            ((TreeViewItem)trvStructure.Items[0]).IsSelected = true;
                  
            //-- для редактирования текста заметки
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            this.colorList.ItemsSource = typeof(Brushes).GetProperties();
        }


        public static string NotePath = "";


        #region Верхнее меню

        // Меню - закрыть приложение
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            // Close this window
            this.Close();
        }

        // кнопка меню - открыть записную книжку
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            string folderName = ParseNotes.ChooseNotebookFolder();

            // очистка
            trvStructure.Items.Clear();

            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo(folderName);
            System.IO.DirectoryInfo[] subDirs = rootDir.GetDirectories();
            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                trvStructure.Items.Add(ParseNotes.CreateTreeItem(dirInfo));
            }
            ((TreeViewItem)trvStructure.Items[0]).IsSelected = true;
        }

        #endregion


        #region Операции с деревом директорий (Записных книжек)

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;

            DirectoryInfo expandedDir = ParseNotes.TreeViewItemExpanded(item);
            if (expandedDir.GetFiles("*.*") != null)
            {
                NotesLv.ItemsSource = ParseNotes.FillNotes(expandedDir.GetFiles("*.*"), expandedDir.Name);
                //--выделяем первую заметку
                NotesLv.SelectedIndex = 0;
                //-- задаем название блокнота
                NotebookNameTbl.Text = expandedDir.Name;
                //-- название блокнота в заметке
                NotePaneltbl1.Text = "Записная книжка: " + expandedDir.Name;
            }
        }

        //--Выбор элемента из дерева папок
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            DirectoryInfo selectedDir = null;
            TreeViewItem item = e.Source as TreeViewItem;
            if (item.Tag is DirectoryInfo)
                selectedDir = (item.Tag as DirectoryInfo);
            if (selectedDir.GetFiles("*.*") != null)
            {
                NotesLv.ItemsSource = ParseNotes.FillNotes(selectedDir.GetFiles("*.*"), selectedDir.Name);
                //--выделяем первую заметку
                NotesLv.SelectedIndex = 0;
                //-- задаем название блокнота
                NotebookNameTbl.Text = selectedDir.Name;
                //-- название блокнота в заметке
                NotePaneltbl1.Text = "Записная книжка: " + selectedDir.Name;
            }
        }

        //-- 2022 вывод заметки на экран
        private void NotesLv_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Item item = ((sender as ListView).SelectedItem as Item);
            if (item != null)
            {
                //--выводим заметку на экран
                TextRange note = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                           
                Stream outputStream = new MemoryStream();
                if (outputStream.CanWrite)
                {
                    outputStream.Write(item.NoteBytes, 0, item.NoteBytes.Length);
                }
                try
                {
                    // Предполагаются файлы в формате Rich Text Format
                    if (note.CanLoad(item.DataFormat))
                        note.Load(outputStream, item.DataFormat);
                }
                catch (Exception ex) { }

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

        #endregion


        #region сохранить/удалить заметку

        //-- Иконка "сохранить заметку"
        private void ImgSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            ParseNotes.SaveNote(NotePath, NoteNameTbx.Text, doc, (List<Item>)NotesLv.ItemsSource);
            NotesLv.Items.Refresh();
        }


        //-- Иконка "удалить заметку"
        private void ImgDell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // диалоговое окно для подтверждения удаления
            DeleteNoteWindow deleteNoteWindow = new DeleteNoteWindow();

            if (deleteNoteWindow.ShowDialog() == true)
            {
                try
                {
                    File.Delete(NotePath);

                    // Обновить ----------
                    int a = NotePath.LastIndexOf("\\") + 1;
                    int b = NotePath.LastIndexOf(".");
                    string dellNoteName = NotePath.Substring(a, b - a);

                    List<Item> items = (List<Item>)NotesLv.ItemsSource;
                    Item itm = new Item();
                    foreach (Item item in items)
                    {
                        if (item.Name.Equals(dellNoteName))
                        {
                            itm = item;
                            break;
                        }
                    }

                    int ind = items.IndexOf(itm);
                    items.RemoveAt(ind);

                    NotesLv.Items.Refresh();
                    //--выделяем первую заметку
                    NotesLv.SelectedIndex = 0;
                }
                catch (Exception ex) { }
            }
        }

        #endregion


        #region Cut/Paste

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

        //-- вставка из буфера. Можно вставить текст, можно картинку
        //-- метод реализован для того, чтобы в дальнейшем расширить вставку ctrl+v и удаление Dell
        //-- сейчас, как для ctrl+v можно вставить или текст или картинку. Если в буфере и текст и картинка вставляется только текст
        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //-- Вставка в rtb
            rtb.Paste();
            
            //-- расширение метода на будущее. не готово ==========================
            /*
            string text = "";
            BitmapSource imgText = null;
            
            //-- текст из буфера
            text = Clipboard.GetText();
            //-- для разных форматов --
            //if (Clipboard.ContainsText(TextDataFormat.Html))
            //{
            //    text = Clipboard.GetText(TextDataFormat.Html);
            //}
            //else if (Clipboard.ContainsText(TextDataFormat.Rtf))
            //{
            //    text = Clipboard.GetText(TextDataFormat.Rtf);
            //}
            //else if (Clipboard.ContainsText(TextDataFormat.Text))
            //{
            //    text = Clipboard.GetText(TextDataFormat.Text);
            //}

            //-- картинка из буфера
            if (Clipboard.ContainsImage())
            {
                imgText = Clipboard.GetImage();
            }

            TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
            //if (textRange.CanLoad(DataFormats.Rtf))
            //{
            //textRange.Load(new MemoryStream(Encoding.Default.GetBytes(text)), DataFormats.Rtf);
            //textRange.Load(new MemoryStream(Encoding.Default.GetBytes(text)), DataFormats.Html);
            //SetDefaultFontColor(textRange);
            //}

            //textRange.Load(imgText, DataFormats.Bitmap);
            //textRange.Load(new MemoryStream();

            //byte[] bytes;
            //bytes = GetImageByteArray(imgText);
            // textRange.Save(new MemoryStream(bytes), DataFormats.Rtf);
            //I see that if I paste an image into the RichTextBox control and save that control's contents using the TextRange, it does indeed save the image.

            //DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
            //rtb.Paste(format);
            // в TextRange может быть только текст. если выделить текст с картинкой, то в TextRange только текст

            //rtb.AppendText(text); //-- вставляется только текст без картинки
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
                     
           //Элемент Floater можно применять и для вывода рисунков. Но, как ни странно, не существует элемента вывода потокового содержимого, предназначенного для этой задачи. Поэтому элемент Image придется применять вместе с элементом BlockUIContainer или InlineUIContainer.
           //Здесь есть один опасный момент. При вставке плавающего окошка, заключающего в себе изображение, потоковый документ предполагает, что рисунок должен быть по ширине таким же, как и вся колонка текста. Размер находящегося внутри элемента Image будет изменен, что может привести к возникновению проблем, если придется сильно уменьшить или увеличить растровое изображение.
           //С помощью свойства Image.Stretch можно запретить изменение размеров изображения, хотя в этом случае плавающее окошко все равно займет всю колонку по ширине — просто вокруг рисунка останутся пустые места. Единственным подходящим решением при внедрении растрового изображения в потоковый документ является указание фиксированных размеров плавающего окошка. После этого с помощью свойства Image.Stretch можно определить, как будет меняться размер изображения в этом окошке.
           
            //rtb.Paste(DataFormats.Bitmap);
            //HtmlToXamlConverter
          // myWebBrowser.DataContext 
            //txtEditor.AppendText(text);

            //-- Вставка в rtb1
            //rtb1.Paste();

            //-- Вставка в браузер
            //myWebBrowser.NavigateToString(text);
           // ImageFromClipboardDib();

            //MemoryStream ms = Clipboard.GetDataObject() as MemoryStream;
            //if (ms != null) {
            //    byte[] dibBuffer = new byte[ms.Length];
            //    ms.Read(dibBuffer, 0, dibBuffer.Length);
            //}

            //--- TEST
            //DataObject retrievedData1 = (DataObject)Clipboard.GetDataObject();
            //if (retrievedData1.GetDataPresent(typeof(BitmapSource)))
            //{
            //    BitmapSource bs1 = retrievedData1.GetData(typeof(BitmapSource)) as BitmapSource;
            //}
            
            //if (retrievedData1.GetDataPresent(typeof(string)))
            //{
            //    //-- 2022 из буфера тоже....
            //    string ss1 = retrievedData1.GetData(typeof(string)) as string;
            //}

            //------------------------------------------
            // data to the Clipboard in multiple formats.
            //DataObject data = new DataObject();

            // Add a Customer object using the type as the format.
            //data.SetData(new Customer("Customer as Customer object"));

            //if(imgText != null)
            //    data.SetImage(imgText);

            //data.SetText("jdjgdgdgdg",TextDataFormat.Text);

            //Clipboard.SetDataObject(data);
            //DataObject retrievedData = (DataObject)Clipboard.GetDataObject();

            //if (retrievedData.GetDataPresent(typeof(Customer)))
            //{
            //    Customer customer =
            //        retrievedData.GetData(typeof(Customer)) as Customer;
            //    if (customer != null)
            //    {
            //        // во время отладки зависает из-за этого
            //        //MessageBox.Show(customer.Name);
            //    }
            //}
            //if (retrievedData.GetDataPresent(typeof(BitmapSource)))
            //{
            //    BitmapSource bs = retrievedData.GetData(typeof(BitmapSource)) as BitmapSource;
            //}
            //if (retrievedData.GetDataPresent(typeof(string)))
            //{
            //    string ss = retrievedData.GetData(typeof(string)) as string;
            //}
            */
        }

        #endregion




        //-- для кнопки "Открыть"
        // открывает выбранный файл в выбранную заметку, можно заметку сохранить с новым содержимым
        private void buttonOpenFile0_Click(object sender, RoutedEventArgs e)
        {
            // окно открытия файла
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|bmp Files (*.bmp)|*.bmp|All files (*.*)|*.*";
            ofd.Filter = "RichText Files (*.rtf)|*.rtf|All files (*.*)|*.*";

            string fileName = null;
            if (ofd.ShowDialog() == true)
            {
                fileName = ofd.FileName;
            }
            bool isExists = File.Exists(fileName) ? true : false;

            // выводим на экран
            TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (isExists)
            {
                try
                {           
                    var dataFormat = DataFormats.Rtf;
                    //if (System.IO.Path.GetExtension(fileName).ToLower() == ".txt")
                    //    dataFormat = DataFormats.Text;
                    //else if (System.IO.Path.GetExtension(fileName).ToLower() == ".rtf")
                    //    dataFormat = DataFormats.Rtf;
                    //else if (System.IO.Path.GetExtension(fileName).ToLower() == ".bmp")
                    //    dataFormat = DataFormats.Bitmap;

                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    doc.Load(fs, dataFormat);
                    fs.Close();
                }
                catch (FileNotFoundException ioEx)
                {
                    string ex = ioEx.Message;
                }
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


       




        






   

     


        #region Панелька редактирования текста
        
        private void rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            EditNote.Visibility = Visibility.Visible;
            //-- "\r\n0\r\n0\r\n0\r\n0\r\n0\r\n0\r\n0\r\n \r\n\r\n"
            if (rtb.Selection != null && rtb.Selection.Text.Trim().Length > 0)
            {
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
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtb.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                rtb.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
            }
            catch (Exception ex) {
                rtb.Selection.ApplyPropertyValue(Inline.FontSizeProperty, "12");
                //System.Windows.Controls.TextChangedEventArgs
            }
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

        //-- 2022 сохранение заметки
        //-- 2022 сохраняет в RTF но потом открывает белиберду
        //-- добавить подсказку при наведении мыши и вообще как-то выделить окно редактирования заметки
        //private void ImgSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    //--переименование
        //    //-- добавить проверку, может это уже rtf
        //    int c = NotePath.LastIndexOf('\\');
        //    string path = NotePath.Substring(0, c+1);
        //    string NotePath2 = path + NoteNameTbx.Text + ".rtf";
        //    File.Move(NotePath, NotePath2);

        //    TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        //    FileStream fs = File.Create(NotePath2);
        //    //using (FileStream fs = File.Create(NotePath))
        //    //{
        //        if (System.IO.Path.GetExtension(NotePath2).ToLower() == ".rtf")
        //        {
        //            doc.Save(fs, DataFormats.Rtf);
        //        }
        //        else if (System.IO.Path.GetExtension(NotePath2).ToLower() == ".txt")
        //        {
        //            doc.Save(fs, DataFormats.Text);
        //        }
        //        else
        //        {
        //            doc.Save(fs, DataFormats.Xaml);
        //        }
        //    //}
        //    //-- обновление заметки в списке заметок
        //    List<Item> items = new List<Item>();
        //    items = (List<Item>)NotesLv.ItemsSource;
        //    foreach (Item itm in items)
        //    {
        //        if (itm.Path == NotePath)
        //        {
        //            fs.Position = 0;
        //            byte[] bytes = new byte[fs.Length];
        //            int numBytesToRead = (int)fs.Length;
        //            int n = fs.Read(bytes, 0, numBytesToRead);

        //            itm.Name = NoteNameTbx.Text;
        //            itm.DTimeModified = DateTime.Today; //-- взять время сохранения
        //            itm.NoteBytes = bytes;
        //            itm.Fs = fs;
        //            itm.Path = NotePath2;
        //            NotePath = NotePath2;

        //            break;
        //        }
        //    }
        //    fs.Close();
        //}

       



  

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


    //-- 2022 заметка-файл
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
