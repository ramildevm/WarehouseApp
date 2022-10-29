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

        private void ButtonMakeStat_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonInvoices_Click(object sender, RoutedEventArgs e)
        {
            new InvoicesWindow().Show();
            this.Close();
        }
        private void ButtonDestinations_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAll_Click(object sender, RoutedEventArgs e)
        {
            contentPanel.Children.Clear();
            LoadData("all");
        }
    }
}
