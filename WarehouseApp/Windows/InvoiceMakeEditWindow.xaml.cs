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
using System.Windows.Shapes;

namespace WarehouseApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для InvoiceMakeEditWindow.xaml
    /// </summary>
    public partial class InvoiceMakeEditWindow : Window
    {
        public List<InvoiceProduct> invoiceProducts = null;
        public InvoiceMakeEditWindow()
        {
            InitializeComponent();
        }
        private void ButtonGoods_Click(object sender, RoutedEventArgs e)
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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonChoiceGoods_Click(object sender, RoutedEventArgs e)
        {
            var fieldsData = new string[] { txtName.Text, txtBankNumber.Text, txtBankName.Text, txtDocNumber.Text, txtDocSeries.Text, txtIndex.Text, txtCountry.Text, txtRegion.Text, txtLocality.Text, txtStreet.Text };

            string result = ValidateData(fieldsData);

            if (result != "Ок")
            {
                MessageBox.Show("Некорректные данные!", "Внимание!");
                return;
            }
            bool check = rbtnNatural.IsChecked ?? false;
            string type = (check) ? rbtnNatural.Content.ToString() : rbtnLegal.Content.ToString();

            var recipient = new Recipient()
            {
                Name = fieldsData[0],
                BankNumber = fieldsData[1],
                BankName = fieldsData[2],
                DocNumber = Convert.ToInt32(fieldsData[3]),
                DocSeries = Convert.ToInt32(fieldsData[4]),
                Type = type
            };
            string address = $"{fieldsData[5]}, {fieldsData[7]}, {fieldsData[8]}, {fieldsData[9]}";
            var destination = new Destination()
            {
                Address = address,
                Country = fieldsData[6],
                Region = fieldsData[7],
                Locality = fieldsData[8]
            };
            new GoodsWindow(this).Show();
            if (invoiceProducts == null)
                return;
            using(var db = new EntityModel())
            {
                recipient = db.Recipient.Add(recipient);
                destination = db.Destination.Add(destination);
                var invoice = db.Invoice.Add(new Invoice()
                {
                    Date = DateTime.Now,
                    RecipientId = recipient.RecipientId,
                    DestinitonId = destination.DestinationId
                });
                foreach(var ip in invoiceProducts)
                {
                    ip.InvoiceId = invoice.InvoiceId;
                    db.InvoiceProduct.Add(ip);
                }
                db.SaveChanges();
            }

        }

        private string ValidateData(string[] list)
        {
            foreach (var item in list)
            {
                if (String.IsNullOrWhiteSpace(item))
                    return "Не все поля заполнены!";
            }
            try
            {
                Convert.ToInt32(list[1]);
                Convert.ToInt32(list[3]);
                Convert.ToInt32(list[4]);
                Convert.ToInt32(list[5]);
            }
            catch
            {
                return "Введены некорректные данные";
            }
            return "Ок";
        }
    }
}
