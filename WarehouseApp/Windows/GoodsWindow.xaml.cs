using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для GoodsWindow.xaml
    /// </summary>
    public partial class GoodsWindow : Window
    {

        List<InvoiceProduct> invoiceProducts = new List<InvoiceProduct>();
        private readonly InvoiceMakeEditWindow invoiceMakeEditWindow;
        private readonly bool isEditMode;

        public GoodsWindow(InvoiceMakeEditWindow invoiceMakeEditWindow, bool _isEditMode)
        {
            InitializeComponent();
            this.invoiceMakeEditWindow = invoiceMakeEditWindow;
            isEditMode = _isEditMode;
            InitializeData();
        }
        public class InvoiceProductObject
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public int Quantity { get; set; }
            public int Price { get; set; }
            public InvoiceProductObject makeProduct(Product product)
            {
                return new InvoiceProductObject()
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Category = product.Category,
                    Quantity = 0,
                    Price = 0
                };
            }
            public InvoiceProductObject makeProduct(Product product, InvoiceProduct ip)
            {
                return new InvoiceProductObject()
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Category = product.Category,
                    Quantity = ip.Quantity,
                    Price = ip.Price
                };
            }
        }
        private void InitializeData()
        {
            List<InvoiceProductObject> list = new List<InvoiceProductObject>();
            using (var db = new EntityModel())
            {
                foreach(var p in db.Product)
                {
                    list.Add()
                }
            }
            dgridGoods.ItemsSource = list;
            dgridGoods.CanUserAddRows = false;

            if (isEditMode)
            {
                invoiceProducts = invoiceMakeEditWindow.invoiceProducts;
                int i = 0;
                foreach (var p in list)
                {
                    var ip = invoiceProducts.Find(obj => obj.ProductId == p.ProductId && obj.InvoiceId == invoiceMakeEditWindow.invoice.InvoiceId);
                    if (ip != null)
                    {
                        DataGridRow row = (DataGridRow)dgridGoods.ItemContainerGenerator.ContainerFromIndex(i);
                        MessageBox.Show(i.ToString());
                        MessageBox.Show((row is null).ToString());
                        var txtQuantity = dgridGoods.Columns[3].GetCellContent(row) as TextBox;
                        var txtPrice = dgridGoods.Columns[4].GetCellContent(row) as TextBox;
                        var btnChoosed = dgridGoods.Columns[5].GetCellContent(row) as Button;
                        btnChoosed.Content = "Добавлено";
                        btnChoosed.IsEnabled = false;
                        txtPrice.Text = ip.Price.ToString();
                        txtQuantity.Text = ip.Quantity.ToString();
                    }
                    i++;
                }
            }

        }
        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }
        private void RowDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    var item = row.Item as Product;
                    var FW_element_ChoosedCount = dgridGoods.Columns[5].GetCellContent(row);
                    var btnChoosed = ((DataGridTemplateColumn)dgridGoods.Columns[5]).CellTemplate.FindName("btnChoosed", FW_element_ChoosedCount) as Button;
                    btnChoosed.Content = "Добавить";
                    btnChoosed.IsEnabled = true;
                    invoiceProducts.Remove(invoiceProducts.Find(obj => obj.ProductId == item.ProductId));
                }
        }
        private void RowButton_Click(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    var item = row.Item as Product;
                    var FW_element_Quantity = dgridGoods.Columns[3].GetCellContent(row);
                    var FW_element_Price = dgridGoods.Columns[4].GetCellContent(row);
                    var txtQuantity = ((DataGridTemplateColumn)dgridGoods.Columns[3]).CellTemplate.FindName("txtQuantity", FW_element_Quantity) as TextBox;
                    var txtPrice = ((DataGridTemplateColumn)dgridGoods.Columns[4]).CellTemplate.FindName("txtPrice", FW_element_Price) as TextBox;
                    var btnChoosed = sender as Button;
                    if (String.IsNullOrEmpty(txtQuantity.Text) || String.IsNullOrEmpty(txtPrice.Text))
                    {
                        MessageBox.Show("Не все поля заполнены!", "Внимание!");
                        return;
                    }
                    try
                    {
                        if (Convert.ToInt32(txtQuantity.Text) <= 0 || Convert.ToInt32(txtPrice.Text) <= 0)
                        {
                            MessageBox.Show("Значения в полях должны быть больше нуля!", "Внимание!");
                            return;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Введены некорректные данные!", "Внимание!");
                        return;
                    }
                    invoiceProducts.Add(new InvoiceProduct()
                    {
                        Quantity = Convert.ToInt32(txtQuantity.Text),
                        Price = Convert.ToInt32(txtPrice.Text),
                        ProductId = item.ProductId
                    });
                    btnChoosed.Content = "Добавлено";
                    btnChoosed.IsEnabled = false;
                    break;
                }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (invoiceProducts.Count == 0)
            {
                MessageBox.Show("Ни один товар не выбран!", "Внимание!");
                return;
            }
            invoiceMakeEditWindow.invoiceProducts = invoiceProducts;
            this.Close();
        }
    }
}
