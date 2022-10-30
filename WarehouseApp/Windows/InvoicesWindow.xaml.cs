using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WarehouseApp.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace WarehouseApp
{
    /// <summary>
    /// Логика взаимодействия для InvoicesWindow.xaml
    /// </summary>
    public partial class InvoicesWindow : Window
    {
        public InvoicesWindow(int flag)
        {
            InitializeComponent();
            if (flag == 1)
                ButtonInvoices_Click(this, new RoutedEventArgs());
        }
        public InvoicesWindow()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            InvoicesPanel.Children.Clear();
            using (var db = new EntityModel())
            {
                List<Invoice> invoices = db.Invoice.ToList();
                foreach(var invoice in invoices)
                {
                    var mainPanel = new Grid();
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100)});
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition());
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(180) });

                    mainPanel.RowDefinitions.Add(new RowDefinition());
                    mainPanel.RowDefinitions.Add(new RowDefinition());
                    mainPanel.RowDefinitions.Add(new RowDefinition());
                    mainPanel.RowDefinitions.Add(new RowDefinition());

                    var txtId = new TextBlock() { Foreground = Brushes.Orange, Text = "№ ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold};
                    var txtDate = new TextBlock() {Text = "Дата: \n", FontWeight = FontWeights.Bold};
                    var txtName = new TextBlock() { Text = "Получатель: " };
                    var txtAddress = new TextBlock() { Text = "Пункт назначения: " };

                    var gridGoods = new DataGrid() { IsReadOnly = true, AutoGenerateColumns = false, FontFamily = new FontFamily("Comic Sans MS"), Margin = new Thickness(10)};
                    var goodsList = new ObservableCollection<DataObject>();

                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Товар", Binding = new Binding("Name") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Категория", Binding = new Binding("Category") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Количество", Binding = new Binding("Quantity") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Цена", Binding = new Binding("Price") });

                    int price = 0;
                    foreach(var productInvoice in db.InvoiceProduct.Where(obj => obj.InvoiceId == invoice.InvoiceId))
                    {
                        var good = db.Product.Find(productInvoice.ProductId);
                        price += productInvoice.Price;
                        goodsList.Add(new DataObject() {Name = good.Name, Category= good.Category, Quantity = productInvoice.Quantity.ToString() + " шт.", Price = productInvoice.Price.ToString() + " руб." });
                    }
                    gridGoods.ItemsSource = goodsList;

                    var txtPrice = new TextBlock() {FontWeight = FontWeights.Bold, Text = $"Общая цена: {price} руб.", TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
                    var btnEdit = new Button() { Content = "Изменить", Tag = invoice };
                    btnEdit.Click += BtnEdit_Click;
                    var btnDelete = new Button() { Content = "Удалить", Tag = invoice };
                    btnDelete.Click += BtnDelete_Click;
                    var btnMakeDoc = new Button() {Margin = new Thickness(5,10,0,10),HorizontalAlignment=HorizontalAlignment.Left, Width = 160, Background=Brushes.LightGray, Content = "Сохранить в PDF", Tag = invoice };
                    btnMakeDoc.Click += BtnMakeDoc_Click;

                    txtId.Text += invoice.InvoiceId.ToString();
                    txtDate.Text += invoice.Date.ToString();
                    txtName.Text += db.Recipient.Find(invoice.RecipientId).Name;
                    txtAddress.Text += db.Destination.Find(invoice.DestinitonId).Address;

                    Grid.SetRow(txtAddress, 1);
                    Grid.SetRow(gridGoods, 2);
                    Grid.SetRow(btnEdit, 1);
                    Grid.SetRow(btnDelete, 2);
                    Grid.SetRow(txtPrice, 3);
                    Grid.SetRow(btnMakeDoc, 3);
                    Grid.SetColumn(txtPrice, 1);
                    Grid.SetColumn(btnMakeDoc, 1);
                    Grid.SetColumn(txtDate, 2);
                    Grid.SetColumn(btnEdit, 2);
                    Grid.SetColumn(btnDelete, 2);
                    Grid.SetColumn(txtName, 1);
                    Grid.SetColumn(txtAddress, 1);
                    Grid.SetColumn(gridGoods, 1);

                    mainPanel.Children.Add(txtId);
                    mainPanel.Children.Add(txtDate);
                    mainPanel.Children.Add(txtName);
                    mainPanel.Children.Add(btnEdit);
                    mainPanel.Children.Add(btnDelete);
                    mainPanel.Children.Add(txtPrice);
                    mainPanel.Children.Add(btnMakeDoc);
                    mainPanel.Children.Add(txtAddress);
                    mainPanel.Children.Add(gridGoods);

                    InvoicesPanel.Children.Add(mainPanel);
                }
            }

        }

        private void BtnMakeDoc_Click(object sender, RoutedEventArgs e)
        {
            var invoice = (sender as Button).Tag as Invoice;
            var app = new Word.Application();
            Word.Document document = app.Documents.Add();

            Word.Paragraph paragraph = document.Paragraphs.Add();
            Word.Range range = paragraph.Range;
            range.Text = "№" + invoice.InvoiceId.ToString();
            range.Font.Size = 16;
            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            range.Font.Bold = 1;
            range.InsertParagraphAfter();

            Word.Paragraph paragraphOther = document.Paragraphs.Add();
            paragraphOther.Range.Text = "Дата: " + invoice.Date.ToString();
            paragraphOther.Range.Font.Size = 14;
            paragraphOther.Range.Font.Bold = 1;
            paragraphOther.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            paragraphOther.Range.InsertParagraphAfter();

            paragraphOther = document.Paragraphs.Add();
            paragraphOther.Range.Bold = 0;
            if (invoice.Recipient.Type == "Физическое лицо")
                paragraphOther.Range.Text = $"Грузополучатель: {invoice.Recipient.Name}, {invoice.Recipient.DocSeries} {invoice.Recipient.DocNumber}, № {invoice.Recipient.BankNumber} \"{invoice.Recipient.BankName}\"";
            else
                paragraphOther.Range.Text = $"Грузополучатель: {invoice.Recipient.Name}, № {invoice.Recipient.BankNumber} \"{invoice.Recipient.BankName}\"";
            paragraphOther.Range.Font.Size = 14;
            paragraphOther.Range.InsertParagraphAfter();

            paragraphOther = document.Paragraphs.Add();
            paragraphOther.Range.Bold = 0;
            paragraphOther.Range.Text = $"Пункт назначения: {invoice.Destination.Address}";
            paragraphOther.Range.Font.Size = 14;
            paragraphOther.Range.InsertParagraphAfter();

            using (var db = new EntityModel())
            {
                var allProducts = db.InvoiceProduct.Where(obj => obj.InvoiceId == invoice.InvoiceId).ToList();
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table regionsTable = document.Tables.Add(tableRange, allProducts.Count() + 1, 4);
                regionsTable.Borders.InsideLineStyle = regionsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                regionsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                Word.Range cellRange;
                cellRange = regionsTable.Cell(1, 1).Range;
                cellRange.Text = "Название товара";

                cellRange = regionsTable.Cell(1, 2).Range;
                cellRange.Text = "Категория";
                cellRange = regionsTable.Cell(1, 3).Range;
                cellRange.Text = "Количество";
                cellRange = regionsTable.Cell(1, 4).Range;
                cellRange.Text = "Цена";
                regionsTable.Rows[1].Range.Bold = 1;
                regionsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                int iter = 1;
                foreach(var product in allProducts)
                {
                    cellRange = regionsTable.Cell(iter + 1, 1).Range;
                    cellRange.Text = product.Product.Name;
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    cellRange.Font.Bold = 0;
                    cellRange = regionsTable.Cell(iter + 1, 2).Range;
                    cellRange.Text = product.Product.Category;
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    cellRange.Font.Bold = 0;
                    cellRange = regionsTable.Cell(iter + 1, 3).Range;
                    cellRange.Text = product.Quantity.ToString();
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    cellRange.Font.Bold = 0;
                    cellRange = regionsTable.Cell(iter + 1, 4).Range;
                    cellRange.Text = product.Price.ToString();
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    cellRange.Font.Bold = 0;
                    iter++;
                }
                paragraphOther = document.Paragraphs.Add();
                paragraphOther.Range.Text = $"Общая цена: {allProducts.Sum(obj=>(int?)obj.Price)??0} руб.";
                paragraphOther.Range.Font.Size = 14;
                paragraphOther.Range.Font.Bold = 1;
                paragraphOther.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            }
            app.Visible = true;
            string fileName = "invoice№"+invoice.InvoiceId.ToString();
            document.SaveAs2($@"D:\{fileName}.pdf", Word.WdExportFormat.wdExportFormatPDF);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить это объект?","Внимание", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                using(var db = new EntityModel())
                {
                    var invoice = (sender as Button).Tag as Invoice;
                    db.Entry(invoice.Recipient).State = System.Data.Entity.EntityState.Deleted;
                    db.Entry(invoice.Destination).State = System.Data.Entity.EntityState.Deleted;
                    db.Entry(invoice).State = System.Data.Entity.EntityState.Deleted;
                    foreach (var invoiceProduct in db.InvoiceProduct.Where(obj => obj.InvoiceId == invoice.InvoiceId))
                    {
                        db.Entry(invoiceProduct).State = System.Data.Entity.EntityState.Deleted;
                    }
                    db.SaveChanges();
                }
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var invoice = (sender as Button).Tag as Invoice;
            new InvoiceMakeEditWindow(invoice).Show();
            this.Close();
        }

        
        private void ButtonDestinations_Click(object sender, RoutedEventArgs e)
        {
            new DestinationsWindow().Show();
            this.Close();
        }

        private void ButtonMakeInvoice_Click(object sender, RoutedEventArgs e)
        {
            new InvoiceMakeEditWindow().Show();
            this.Close();
        }

        private void ButtonInvoices_Click(object sender, RoutedEventArgs e)
        {
            btnMakeInvoice.Visibility = Visibility.Visible;
            txtGreating.Visibility = Visibility.Collapsed;
            LoadData();
        }
    }
    public class DataObject
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
    }
}
