using System;
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

        public GoodsWindow(InvoiceMakeEditWindow invoiceMakeEditWindow)
        {
            InitializeComponent();
            this.invoiceMakeEditWindow = invoiceMakeEditWindow;
            InitializeData();
        }

        private void InitializeData()
        {
            List<Product> list;
            using (var db = new EntityModel())
            {
                list = db.Product.ToList();
            }
            dgridGoods.ItemsSource = list;
            dgridGoods.CanUserAddRows = false;

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
                    if(String.IsNullOrEmpty(txtQuantity.Text) || String.IsNullOrEmpty(txtPrice.Text))
                    {
                        MessageBox.Show("Не все поля заполнены!", "Внимание!");
                        return;
                    }
                    try
                    {
                        if (Convert.ToInt32(txtQuantity.Text) <= 0 || Convert.ToInt32(txtPrice.Text)<=0)
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
            if(invoiceProducts.Count == 0)
            {
                MessageBox.Show("Ни один товар не выбран!", "Внимание!");
                return;
            }
            invoiceMakeEditWindow.invoiceProducts = invoiceProducts;
        }
    }
}
