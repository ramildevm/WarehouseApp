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

namespace WarehouseApp
{
    /// <summary>
    /// Логика взаимодействия для InvoicesWindow.xaml
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

                    txtId.Text += invoice.InvoiceId.ToString();
                    txtDate.Text += invoice.Date.ToString();
                    txtName.Text += db.Recipient.Find(invoice.RecipientId).Name;
                    txtAddress.Text += db.Destination.Find(invoice.DestinitonId).Address;

                    Grid.SetRow(txtAddress, 1);
                    Grid.SetRow(gridGoods, 2);
                    Grid.SetRow(btnEdit, 1);
                    Grid.SetRow(btnDelete, 2);
                    Grid.SetRow(txtPrice, 3);
                    Grid.SetColumn(txtPrice, 1);
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
                    mainPanel.Children.Add(txtAddress);
                    mainPanel.Children.Add(gridGoods);
                    mainPanel.Children.Add(txtPrice);

                    InvoicesPanel.Children.Add(mainPanel);
                }
            }

        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить это объект?","Внимание", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                using(var db = new EntityModel())
                {
                    var invoice = (sender as Button).Tag as Invoice;
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

        private void ButtonGoods_Click(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void ButtonInvoices_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonDestinations_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonMakeInvoice_Click(object sender, RoutedEventArgs e)
        {
            new InvoiceMakeEditWindow().Show();
            this.Close();
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
