using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Word = Microsoft.Office.Interop.Word;

namespace WarehouseApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для DestinationsWindow.xaml
    /// </summary>
    public partial class DestinationsWindow : Window
    {
        public DestinationsWindow()
        {
            InitializeComponent();
            LoadButtons();
            LoadData("all");
        }

        private void LoadButtons()
        {
            using (var db = new EntityModel())
            {
                var allCountries = db.Destination.OrderBy(obj => obj.Country).GroupBy(obj => obj.Country).ToList();
                foreach (var country in allCountries)
                {
                    spanelLeftMenu.Children.Add(new TextBlock() { Margin=new Thickness(0,2,0,2), FontWeight = FontWeights.Bold, Padding =new Thickness(5,0,0,0), Foreground=Brushes.White, Background = Brushes.Orange, Text = country.Key });
                    var allRegions = db.Destination.Where(obj=>obj.Country==country.Key).OrderBy(obj => obj.Region).GroupBy(obj => obj.Region).ToList();
                    foreach (var region in allRegions)
                    {
                        var btnRegion = new Button();
                        btnRegion.Content = region.Key;
                        btnRegion.Click += BtnRegion_Click;
                        spanelLeftMenu.Children.Add(btnRegion);
                    }
                }
            }
        }

        private void BtnRegion_Click(object sender, RoutedEventArgs e)
        {
            contentPanel.Children.Clear();
            LoadData((sender as Button).Content.ToString());
        }

        private void LoadData(string filter)
        {
            List<Destination> destinationsList;
            using(var db = new EntityModel())
            {
                if (filter == "all")
                    destinationsList = db.Destination.OrderBy(obj => obj.Address).ToList();
                else
                    destinationsList = db.Destination.Where(obj => obj.Region == filter).OrderBy(obj => obj.Address).ToList();
            }
            foreach(var destination in destinationsList)
            {
                var mainPanel = new Grid();
                mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                mainPanel.ColumnDefinitions.Add(new ColumnDefinition());

                mainPanel.RowDefinitions.Add(new RowDefinition());
                mainPanel.RowDefinitions.Add(new RowDefinition());

                var txtAddress = new TextBlock() {FontWeight=FontWeights.Bold, Text = "Адресс: "+ destination.Address };
                var leftPanel = new StackPanel();
                var txtCountry = new TextBlock() {Text ="Страна: "+ destination.Country };
                var txtRegion = new TextBlock() { Text = "Регион: " + destination.Region };
                var txtLocality = new TextBlock() { Text = "Населенный пункт: " + destination.Locality };
                leftPanel.Children.Add(txtCountry);
                leftPanel.Children.Add(txtRegion);
                leftPanel.Children.Add(txtLocality);

                var rightPanel = new StackPanel();
                var txtTableText = new TextBlock() { Text = "Отправленные товары:" };
                var gridGoods = new DataGrid() { IsReadOnly = true, AutoGenerateColumns = false, FontFamily = new FontFamily("Comic Sans MS"), Margin = new Thickness(10) };
                var goodsList = new ObservableCollection<DataObject>();

                gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Товар", Binding = new Binding("Name") });
                gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Категория", Binding = new Binding("Category") });
                gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Количество", Binding = new Binding("Quantity") });
                gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Цена", Binding = new Binding("Price") });

                int price = 0;
                using (var db = new EntityModel())
                {
                    var invoice = db.Invoice.ToList().Find(obj => obj.DestinitonId == destination.DestinationId);
                    if(invoice!=null)
                    foreach (var productInvoice in db.InvoiceProduct.Where(obj => obj.InvoiceId == invoice.InvoiceId))
                    {
                        var good = db.Product.Find(productInvoice.ProductId);
                        price += productInvoice.Price;
                        goodsList.Add(new DataObject() { Name = good.Name, Category = good.Category, Quantity = productInvoice.Quantity.ToString() + " шт.", Price = productInvoice.Price.ToString() + " руб." });
                    }
                }
                gridGoods.ItemsSource = goodsList;
                rightPanel.Children.Add(txtTableText);
                rightPanel.Children.Add(gridGoods);

                Grid.SetRow(leftPanel, 1);
                Grid.SetRow(rightPanel, 1);
                Grid.SetColumn(rightPanel, 1);
                Grid.SetColumnSpan(txtAddress, 2);

                mainPanel.Children.Add(txtAddress);
                mainPanel.Children.Add(rightPanel);
                mainPanel.Children.Add(leftPanel);
                contentPanel.Children.Add(mainPanel);
            }
        }
        class ProductSumObject
        {
            public string Name { get; set; }
            public int Sum { get; set; }
            public ProductSumObject(string name, int sum)
            {
                this.Name = name;
                this.Sum = sum;
            }
        }
        private void ButtonMakeStat_Click(object sender, RoutedEventArgs e)
        {

            var app = new Word.Application();
            Word.Document document = app.Documents.Add();

            using (var db = new EntityModel())
            {
                var allCountries = db.Destination.OrderBy(obj => obj.Country).GroupBy(obj => obj.Country).ToList();
                foreach (var country in allCountries)
                {
                    Word.Paragraph paragraphFirst = document.Paragraphs.Add();
                    paragraphFirst.set_Style("Заголовок 1");
                    Word.Range range = paragraphFirst.Range;
                    range.Text = country.Key;                    
                    range.InsertParagraphAfter();

                    var countryInvoiceProducts = db.InvoiceProduct.Where(obj => obj.Invoice.Destination.Country == country.Key)
                        .GroupBy(obj => obj.Product.Name)
                        .ToList()
                        .Select(obj =>new KeyValuePair<string,int>( obj.Key, obj.Sum(o => (int?)o.Quantity) ?? 0 ))
                        .ToDictionary(obj=>obj.Key, obj=>obj.Value);
                    var countryMostSelledProduct = countryInvoiceProducts.OrderBy(obj=>obj.Value).Last();

                    Word.Paragraph paragraph = document.Paragraphs.Add();
                    range = paragraph.Range;
                    range.Text = $"Самый продаваемый товар в стране: {countryMostSelledProduct.Key} ({countryMostSelledProduct.Value}шт.)";
                    range.Font.Size = 14;
                    range.InsertParagraphAfter();

                    paragraph = document.Paragraphs.Add();
                    range = paragraph.Range;
                    range.Text = "Доля спроса на категории товаров по регионам:";
                    range.Font.Size = 14;
                    range.InsertParagraphAfter();

                    var allRegions = db.Destination.Where(obj => obj.Country == country.Key).OrderBy(obj => obj.Region).GroupBy(obj => obj.Region).Select(obj => obj.Key).ToList();

                    var productCategories = db.Product.ToList().GroupBy(obj => obj.Category).Select(obj => obj.Key).ToList();
                    int[] productCategoriesTotalNumber = new int[productCategories.Count()];

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table regionsTable = document.Tables.Add(tableRange, allRegions.Count() + 1, productCategories.Count+1);
                    regionsTable.Borders.InsideLineStyle = regionsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    regionsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    Word.Range cellRange;
                    cellRange = regionsTable.Cell(1, 1).Range;
                    cellRange.Text = "Название региона";
                    for (int i = 1; i < productCategories.Count + 1; i++)
                    {
                        cellRange = regionsTable.Cell(1, i+1).Range;
                        string category = productCategories[i - 1];
                        cellRange.Text = category;
                        productCategoriesTotalNumber[i - 1] = db.InvoiceProduct.Where(obj => obj.Product.Category == category && obj.Invoice.Destination.Country == country.Key).Select(obj => (int?)obj.Quantity).Sum(obj => obj)??0;
                    }
                    regionsTable.Rows[1].Range.Bold = 1;
                    regionsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    Dictionary<string, int[]> regionStat = new Dictionary<string, int[]>();
                    foreach (var region in allRegions)
                    {
                        var regionInvoiceProducts = db.InvoiceProduct.Where(obj => obj.Invoice.Destination.Region == region && obj.Invoice.Destination.Country == country.Key);
                        int[] regionCategoryCount = new int[productCategories.Count];
                        for(int i = 0; i<productCategories.Count;i++)
                        {
                            string category = productCategories[i];
                            regionCategoryCount[i] = regionInvoiceProducts.Where(obj => obj.Product.Category == category).Select(obj=>(int?)obj.Quantity).Sum(obj=>obj)??0;
                        }
                        regionStat.Add(region, regionCategoryCount);
                    }
                    
                    int iter = 1;
                    foreach (var region in regionStat)
                    {
                        cellRange = regionsTable.Cell(iter + 1, 1).Range;
                        cellRange.Text = region.Key;
                        cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        for (int i = 1; i < productCategories.Count+1; i++)
                        {
                            cellRange = regionsTable.Cell(iter + 1, i + 1).Range;
                            cellRange.Text = FindPercent(region.Value[i - 1], productCategoriesTotalNumber[i - 1]);
                            cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                        iter++;
                    }
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }
            }
            app.Visible = true;
            document.SaveAs2(@"D:\outputFileWord.pdf", Word.WdExportFormat.wdExportFormatPDF);
        }

        private string FindPercent(int number, int totalnNumber)
        {
            if (totalnNumber == 0)
                return $"0%";
            double result = ((double)number * 100.0d) / (double)totalnNumber;
            return $"{Math.Round(result,2,MidpointRounding.AwayFromZero)}%";
        }

        private void ButtonInvoices_Click(object sender, RoutedEventArgs e)
        {
            new InvoicesWindow().Show();
            this.Close();
        }

        private void ButtonAll_Click(object sender, RoutedEventArgs e)
        {
            contentPanel.Children.Clear();
            LoadData("all");
        }
    }
}
