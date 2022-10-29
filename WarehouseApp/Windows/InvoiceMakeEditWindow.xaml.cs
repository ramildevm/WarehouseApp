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
        bool isEditMode;
        Recipient recipient;
        Destination destination;
        public Invoice invoice;
        public InvoiceMakeEditWindow(Invoice invoice = null)
        {
            InitializeComponent();
            isEditMode = invoice == null ? false : true;
            this.invoice = invoice;
            if (isEditMode)
                LoadData();
        }

        private void LoadData()
        {
            using (var db = new EntityModel())
            {
                recipient = invoice.Recipient;
                destination = invoice.Destination;
            }
            txtName.Text = recipient.Name;
            txtDocNumber.Text = recipient.DocNumber.ToString();
            txtDocSeries.Text = recipient.DocSeries.ToString();
            txtBankNumber.Text = recipient.BankNumber.ToString();
            txtBankName.Text = recipient.BankName;
            if (recipient.Type == "Физическое лицо")
                rbtnNatural.IsChecked = true;
            else
                rbtnLegal.IsChecked = true;

            string[] address = destination.Address.Split(new char[] { ',' });
            txtIndex.Text = address[0];
            txtCountry.Text = destination.Country;
            txtRegion.Text = destination.Region;
            txtLocality.Text = destination.Locality;
            try
            {
                txtStreet.Text = address[3] + ", " + address[4];
            }
            catch (IndexOutOfRangeException)
            {
                txtStreet.Text = address[3];
            }
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
            new InvoicesWindow().Show();
            this.Close();
        }

        private void ButtonChoiceGoods_Click(object sender, RoutedEventArgs e)
        {
            var fieldsData = new string[] { txtName.Text, txtBankNumber.Text, txtBankName.Text, txtDocNumber.Text, txtDocSeries.Text, txtIndex.Text, txtCountry.Text, txtRegion.Text, txtLocality.Text, txtStreet.Text };
            string result = ValidateData(fieldsData);

            if (result != "Ок")
            {
                MessageBox.Show(result, "Внимание!");
                return;
            }
            bool check = rbtnNatural.IsChecked ?? false;
            string type = (check) ? rbtnNatural.Content.ToString() : rbtnLegal.Content.ToString();

            recipient = isEditMode ? recipient : new Recipient();

            recipient.Name = fieldsData[0];
            recipient.BankNumber = fieldsData[1];
            recipient.BankName = fieldsData[2];
            if (rbtnLegal.IsChecked != true)
            {
                recipient.DocNumber = Convert.ToInt32(fieldsData[3]);
                recipient.DocSeries = Convert.ToInt32(fieldsData[4]);
            }
            recipient.Type = type;

            string address = $"{fieldsData[5]}, {fieldsData[7]}, {fieldsData[8]}, {fieldsData[9]}";
            destination = isEditMode ? destination : new Destination();

            destination.Address = address;
            destination.Country = fieldsData[6];
            destination.Region = fieldsData[7];
            destination.Locality = fieldsData[8];

            if (isEditMode)
            {
                invoiceProducts = new List<InvoiceProduct>();
                using (var db = new EntityModel())
                {
                    foreach (var ip in db.InvoiceProduct.Where(obj => obj.InvoiceId == invoice.InvoiceId))
                    {
                        invoiceProducts.Add(ip);
                    }
                }
            }
            List<InvoiceProduct> oldInvoiceProducts = new List<InvoiceProduct>();
            if (isEditMode)
                oldInvoiceProducts = invoiceProducts.ToList();
            this.Hide();
            new GoodsWindow(this, isEditMode).ShowDialog();
            this.Show();

            if (invoiceProducts == null)
                return;
            else if (Enumerable.SequenceEqual(oldInvoiceProducts, invoiceProducts))
                return;
            if (!isEditMode)
            {
                using (var db = new EntityModel())
                {
                    recipient = db.Recipient.Add(recipient);
                    destination = db.Destination.Add(destination);
                    invoice = db.Invoice.Add(new Invoice()
                    {
                        Date = DateTime.Now,
                        RecipientId = recipient.RecipientId,
                        DestinitonId = destination.DestinationId
                    });
                    foreach (var ip in invoiceProducts)
                    {
                        ip.InvoiceId = invoice.InvoiceId;
                        db.InvoiceProduct.Add(ip);
                    }
                    int res = db.SaveChanges();
                    if (res > 0)
                        MessageBox.Show("Накладная добавлена!", "Внимание!");
                }
            }
            else
            {
                using (var db = new EntityModel())
                {
                    db.Entry(recipient).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(destination).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(invoice).State = System.Data.Entity.EntityState.Modified;
                    invoiceProducts.ForEach(obj => obj.InvoiceId = invoice.InvoiceId);
                    
                    foreach (var p in db.Product)
                    {
                        var ipdb = db.InvoiceProduct.ToList().Find(obj => obj.InvoiceId == invoice.InvoiceId && obj.ProductId == p.ProductId);
                        var ip = invoiceProducts.Find(obj => obj.InvoiceId == invoice.InvoiceId && obj.ProductId == p.ProductId);
                        if (ipdb != null)
                        {
                            if (ip != null)
                            {
                                ipdb.Quantity = ip.Quantity;
                                ipdb.Price = ip.Price;
                                 db.Entry(ipdb).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                                db.Entry(ipdb).State = System.Data.Entity.EntityState.Deleted;
                            continue;
                        }
                        if (ip != null)
                        {
                            ip.InvoiceId = invoice.InvoiceId;
                            db.InvoiceProduct.Add(ip);
                        }
                    }
                    int res = db.SaveChanges();
                    if (res > 0)
                        MessageBox.Show("Накладная обновлена!", "Внимание!");
                }
            }
            new InvoicesWindow().Show();
            this.Close();

        }

        private string ValidateData(string[] list)
        {
            int index = 0;
            foreach (var item in list)
            {
                if (rbtnLegal.IsChecked == true && (index == 3 || index == 4))
                    continue;
                if (String.IsNullOrWhiteSpace(item))
                    return "Не все поля заполнены!";
                index++;
            }
            try
            {
                Convert.ToInt32(list[1]);
                Convert.ToInt32(list[5]);
                if (rbtnLegal.IsChecked != true)
                {
                    Convert.ToInt32(list[3]);
                    Convert.ToInt32(list[4]);
                }
            }
            catch
            {
                return "Введены некорректные данные";
            }
            return "Ок";
        }
    }
}
