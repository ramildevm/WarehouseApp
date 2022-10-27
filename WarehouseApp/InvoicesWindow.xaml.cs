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

namespace WarehouseApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class InvoicesWindow : Window
    {
        public InvoicesWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new EntityModel())
            {
                List<Invoice> invoices = db.Invoice.ToList();
                foreach(var invoice in invoices)
                {
                    var mainPanel = new Grid();
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150)});
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition());
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) });

                    mainPanel.RowDefinitions.Add(new RowDefinition());
                    mainPanel.RowDefinitions.Add(new RowDefinition());
                    mainPanel.RowDefinitions.Add(new RowDefinition());

                    var txtId = new TextBlock() { Foreground = Brushes.Orange, Text = "№ " };
                    var txtName = new TextBlock() { Text = "Получатель: " };
                    var txtAddress = new TextBlock() { Text = "Пункт назначения: " };

                    var gridGoods = new DataGrid() { IsReadOnly = true, AutoGenerateColumns = false, Margin = new Thickness(10)};
                    var goodsList = new ObservableCollection<DataObject>();

                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Товар", Binding = new Binding("Name") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Категория", Binding = new Binding("Category") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Количество", Binding = new Binding("Quantity") });
                    gridGoods.Columns.Add(new DataGridTextColumn() { Header = "Цена", Binding = new Binding("Price") });

                    int price = 0;
                    foreach(var productInvoice in db.InvoiceProduct.Where(v => v.InvoiceId == invoice.InvoiceId))
                    {
                        var good = db.Product.Find(productInvoice.ProductId);
                        price += productInvoice.Price;
                        goodsList.Add(new DataObject() {Name = good.Name, Category= good.Category, Quantity = productInvoice.Quantity.ToString() + " шт.", Price = productInvoice.Price.ToString() + " руб." });
                    }
                    gridGoods.ItemsSource = goodsList;

                    var txtPrice = new TextBlock() { Foreground = Brushes.Orange, Text = $"Общая цена: {price} руб.", TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };

                    txtId.Text += invoice.InvoiceId.ToString();
                    txtName.Text += db.Recipient.Find(invoice.RecipientId).Name;
                    txtAddress.Text += db.Destination.Find(invoice.DestinitonId).Address;

                    Grid.SetRow(txtName, 1);
                    Grid.SetRow(txtAddress, 2);
                    Grid.SetRow(txtPrice, 2);
                    Grid.SetColumn(txtPrice, 2);
                    Grid.SetColumn(gridGoods, 1);
                    Grid.SetRowSpan(gridGoods, 3);

                    mainPanel.Children.Add(txtId);
                    mainPanel.Children.Add(new Control());
                    mainPanel.Children.Add(new Control());
                    mainPanel.Children.Add(txtName);
                    mainPanel.Children.Add(new Control());
                    mainPanel.Children.Add(new Control());
                    mainPanel.Children.Add(txtAddress);
                    mainPanel.Children.Add(gridGoods);
                    mainPanel.Children.Add(txtPrice);

                    InvoicesDataGrid.Children.Add(mainPanel);
                }
            }

        }

        private void ButtonGoods_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonInvoices_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonDestinations_Click(object sender, RoutedEventArgs e)
        {

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
