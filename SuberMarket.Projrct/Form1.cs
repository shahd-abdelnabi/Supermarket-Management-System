using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SuberMarket.Projrct
{
    // ============================================================
    //  Product: صنف في المنتجات   |   CartItem: صنف داخل السلة
    // ============================================================
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }
    }

    public class CartItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal Total { get { return Price * Qty; } }

        public CartItem(string name, decimal price, int qty)
        {
            Name = name;
            Price = price;
            Qty = qty;
        }
    }

    public partial class Form1 : Form
    {
        // ---------- Settings ----------
        const decimal TaxRate = 0.14m;          // ضريبة 14%
        const string StoreName = "FCAI SUPER MARKET";
        int invoiceNo = 1000;
        string currentCategory = "";

        // ---------- Colors ----------
        static readonly Color Primary = Color.FromArgb(27, 94, 32);
        static readonly Color Accent = Color.FromArgb(255, 143, 0);
        static readonly Color DarkBg = Color.FromArgb(33, 37, 41);
        static readonly Color LightBg = Color.FromArgb(244, 247, 244);

        // ---------- Data ----------
        readonly List<string> categoryOrder = new List<string>();
        readonly Dictionary<string, List<Product>> catalog = new Dictionary<string, List<Product>>();
        readonly Dictionary<string, Color> categoryColor = new Dictionary<string, Color>();
        readonly Dictionary<string, Button> categoryButtons = new Dictionary<string, Button>();
        readonly BindingList<CartItem> cart = new BindingList<CartItem>();

        // ---------- Controls ----------
        readonly DataGridView gridProducts = new DataGridView();
        readonly DataGridView gridCart = new DataGridView();
        readonly Label lblCategory = new Label();
        readonly Label lblCartTitle = new Label();
        readonly Label lblSubtotal = new Label();
        readonly Label lblTax = new Label();
        readonly Label lblTotal = new Label();
        readonly Label lblClock = new Label();
        readonly NumericUpDown nudQty = new NumericUpDown();

        public Form1()
        {
            InitializeComponent();
            BuildCatalog();
            BuildUI();
            ShowCategory(categoryOrder[0]);
            UpdateTotals();
        }
        private void Form1_Load(object sender, EventArgs eق)
        { }

        // ============================================================
        //  1) البيانات: الأقسام والأصناف والأسعار (بالجنيه)
        // ============================================================
        static Product P(string name, decimal price)
        {
            return new Product(name, price);
        }

        void AddCategory(string name, Color color, params Product[] items)
        {
            categoryOrder.Add(name);
            catalog[name] = new List<Product>(items);
            categoryColor[name] = color;
        }

        void BuildCatalog()
        {
            AddCategory("Dairy", Color.FromArgb(2, 136, 209),
                P("Milk 1L", 45m), P("White Cheese 500g", 85m), P("Yogurt Cup", 12m),
                P("Butter 100g", 55m), P("Eggs 12 pcs", 60m), P("Cream Cheese", 30m));

            AddCategory("Vegetables", Color.FromArgb(56, 142, 60),
                P("Tomato 1kg", 25m), P("Potato 1kg", 20m), P("Cucumber 1kg", 18m),
                P("Onion 1kg", 15m), P("Carrot 1kg", 22m), P("Green Pepper 1kg", 35m));

            AddCategory("Fruits", Color.FromArgb(229, 57, 53),
                P("Apple 1kg", 60m), P("Banana 1kg", 40m), P("Orange 1kg", 30m),
                P("Grapes 1kg", 70m), P("Mango 1kg", 80m), P("Strawberry 500g", 45m));

            AddCategory("Bakery", Color.FromArgb(239, 108, 0),
                P("Baladi Bread", 10m), P("Toast Pack", 35m), P("Croissant", 20m),
                P("Cake Slice", 45m), P("Donut", 18m), P("Cookies Box", 40m));

            AddCategory("Drinks", Color.FromArgb(0, 151, 167),
                P("Water 1.5L", 10m), P("Orange Juice 1L", 40m), P("Cola 1L", 25m),
                P("Tea 40 bags", 70m), P("Coffee 200g", 120m), P("Energy Drink", 35m));

            AddCategory("Snacks", Color.FromArgb(123, 31, 162),
                P("Chips", 15m), P("Chocolate Bar", 25m), P("Biscuits", 12m),
                P("Popcorn", 30m), P("Mixed Nuts 100g", 50m), P("Candy Pack", 20m));

            AddCategory("Meat & Poultry", Color.FromArgb(109, 76, 65),
                P("Beef 1kg", 380m), P("Chicken 1kg", 130m), P("Fish 1kg", 150m),
                P("Sausage 250g", 70m), P("Burger Patties", 95m), P("Turkey 1kg", 210m));

            AddCategory("Cleaning", Color.FromArgb(84, 110, 122),
                P("Dish Soap", 40m), P("Laundry Powder 2kg", 120m), P("Floor Cleaner", 45m),
                P("Tissues Box", 25m), P("Trash Bags", 30m), P("Sponge Pack", 15m));
        }

        // ============================================================
        //  2) بناء الواجهة بالكود (من غير Designer)
        // ============================================================
        Button MakeButton(string text, Color back, int x, int y, int w, int h, float size)
        {
            Button b = new Button();
            b.Text = text;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.Font = new Font("Segoe UI", size, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseMnemonic = false;
            return b;
        }

        Label MakeLabel(string text, int x, int y, int w, int h, float size, FontStyle style, Color color, ContentAlignment align)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(w, h);
            l.Font = new Font("Segoe UI", size, style);
            l.ForeColor = color;
            l.TextAlign = align;
            l.UseMnemonic = false;
            return l;
        }

        void StyleGrid(DataGridView g)
        {
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(232, 232, 232);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 42;
            g.ColumnHeadersDefaultCellStyle.BackColor = DarkBg;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = DarkBg;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            g.RowTemplate.Height = 38;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 11f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 224, 178);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
        }

        void SetCol(DataGridView g, string prop, string header, string format, float weight, bool right)
        {
            var col = g.Columns[prop];
            if (col == null) return;
            col.HeaderText = header;
            col.FillWeight = weight;
            if (format != "") col.DefaultCellStyle.Format = format;
            if (right)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        void BuildUI()
        {
            Text = "FCAI Super Market";
            ClientSize = new Size(1280, 760);
            MinimumSize = new Size(1150, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = LightBg;
            Font = new Font("Segoe UI", 10f);

            // ---------------- Header ----------------
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 78;
            header.BackColor = Primary;
            header.Controls.Add(MakeLabel(StoreName, 22, 8, 600, 42, 22f, FontStyle.Bold, Color.White, ContentAlignment.MiddleLeft));
            header.Controls.Add(MakeLabel("Fresh products  |  Best prices  |  Fast checkout", 24, 48, 600, 24, 10.5f, FontStyle.Regular, Color.FromArgb(200, 230, 201), ContentAlignment.MiddleLeft));
            lblClock.Size = new Size(260, 30);
            lblClock.Top = 24;
            lblClock.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblClock.ForeColor = Color.White;
            lblClock.TextAlign = ContentAlignment.MiddleRight;
            header.Controls.Add(lblClock);
            header.Resize += delegate { lblClock.Left = header.Width - lblClock.Width - 25; };
            lblClock.Text = DateTime.Now.ToString("ddd dd MMM yyyy  HH:mm:ss", CultureInfo.InvariantCulture);
            System.Windows.Forms.Timer clock = new System.Windows.Forms.Timer();
            clock.Interval = 1000;
            clock.Tick += delegate { lblClock.Text = DateTime.Now.ToString("ddd dd MMM yyyy  HH:mm:ss", CultureInfo.InvariantCulture); };
            clock.Start();

            // ---------------- Left: categories ----------------
            Panel left = new Panel();
            left.Dock = DockStyle.Left;
            left.Width = 225;
            left.BackColor = DarkBg;
            left.Controls.Add(MakeLabel("CATEGORIES", 20, 16, 180, 28, 11f, FontStyle.Bold, Color.FromArgb(160, 170, 180), ContentAlignment.MiddleLeft));

            int y = 55;
            foreach (string cat in categoryOrder)
            {
                Button b = MakeButton(cat, categoryColor[cat], 0, y, 225, 58, 12.5f);
                b.TextAlign = ContentAlignment.MiddleLeft;
                b.Padding = new Padding(22, 0, 0, 0);
                string name = cat;
                b.Click += delegate { ShowCategory(name); };
                categoryButtons[cat] = b;
                left.Controls.Add(b);
                y += 62;
            }

            // ---------------- Center: products ----------------
            Panel center = new Panel();
            center.Dock = DockStyle.Fill;
            center.Padding = new Padding(18, 12, 18, 12);
            center.BackColor = LightBg;

            lblCategory.Dock = DockStyle.Top;
            lblCategory.Height = 58;
            lblCategory.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
            lblCategory.TextAlign = ContentAlignment.MiddleLeft;
            lblCategory.UseMnemonic = false;

            StyleGrid(gridProducts);
            gridProducts.Dock = DockStyle.Fill;
            gridProducts.CellDoubleClick += delegate (object s, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) AddToCart();
            };

            Panel productBar = new Panel();
            productBar.Dock = DockStyle.Bottom;
            productBar.Height = 70;
            productBar.Controls.Add(MakeLabel("Quantity:", 0, 18, 90, 34, 12f, FontStyle.Bold, DarkBg, ContentAlignment.MiddleLeft));
            nudQty.Location = new Point(92, 18);
            nudQty.Size = new Size(85, 34);
            nudQty.Font = new Font("Segoe UI", 14f);
            nudQty.Minimum = 1;
            nudQty.Maximum = 99;
            nudQty.Value = 1;
            nudQty.TextAlign = HorizontalAlignment.Center;
            productBar.Controls.Add(nudQty);
            Button btnAdd = MakeButton("ADD TO CART", Accent, 195, 12, 210, 48, 12.5f);
            btnAdd.Click += delegate { AddToCart(); };
            productBar.Controls.Add(btnAdd);
            productBar.Controls.Add(MakeLabel("Tip: double-click an item to add it quickly", 425, 18, 380, 34, 10f, FontStyle.Italic, Color.Gray, ContentAlignment.MiddleLeft));

            // الترتيب مهم: الـ Fill الأول وبعدين الأطراف
            center.Controls.Add(gridProducts);
            center.Controls.Add(productBar);
            center.Controls.Add(lblCategory);

            // ---------------- Right: cart ----------------
            Panel cartPanel = new Panel();
            cartPanel.Dock = DockStyle.Right;
            cartPanel.Width = 470;
            cartPanel.Padding = new Padding(10);
            cartPanel.BackColor = Color.White;

            lblCartTitle.Dock = DockStyle.Top;
            lblCartTitle.Height = 50;
            lblCartTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblCartTitle.ForeColor = Primary;
            lblCartTitle.TextAlign = ContentAlignment.MiddleLeft;

            StyleGrid(gridCart);
            gridCart.Dock = DockStyle.Fill;
            gridCart.DataSource = cart;
            SetCol(gridCart, "Name", "Item", "", 150f, false);
            SetCol(gridCart, "Price", "Price", "N2", 70f, true);
            SetCol(gridCart, "Qty", "Qty", "", 45f, true);
            SetCol(gridCart, "Total", "Total", "N2", 80f, true);

            Panel totals = new Panel();
            totals.Dock = DockStyle.Bottom;
            totals.Height = 262;

            totals.Controls.Add(MakeLabel("Subtotal:", 10, 12, 200, 30, 12f, FontStyle.Regular, DarkBg, ContentAlignment.MiddleLeft));
            totals.Controls.Add(MakeLabel("VAT 14%:", 10, 44, 200, 30, 12f, FontStyle.Regular, DarkBg, ContentAlignment.MiddleLeft));
            Panel line = new Panel();
            line.BackColor = Color.FromArgb(210, 210, 210);
            line.Location = new Point(10, 80);
            line.Size = new Size(430, 2);
            totals.Controls.Add(line);
            totals.Controls.Add(MakeLabel("TOTAL:", 10, 88, 150, 40, 15f, FontStyle.Bold, Primary, ContentAlignment.MiddleLeft));

            lblSubtotal.Location = new Point(200, 12);
            lblSubtotal.Size = new Size(240, 30);
            lblSubtotal.Font = new Font("Segoe UI", 12f);
            lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
            lblTax.Location = new Point(200, 44);
            lblTax.Size = new Size(240, 30);
            lblTax.Font = new Font("Segoe UI", 12f);
            lblTax.TextAlign = ContentAlignment.MiddleRight;
            lblTotal.Location = new Point(150, 88);
            lblTotal.Size = new Size(290, 40);
            lblTotal.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTotal.ForeColor = Primary;
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            totals.Controls.Add(lblSubtotal);
            totals.Controls.Add(lblTax);
            totals.Controls.Add(lblTotal);

            Button btnRemove = MakeButton("Remove Selected", Color.FromArgb(198, 40, 40), 10, 140, 210, 44, 11f);
            btnRemove.Click += delegate { RemoveSelected(); };
            Button btnClear = MakeButton("Clear Cart", Color.FromArgb(96, 125, 139), 230, 140, 210, 44, 11f);
            btnClear.Click += delegate { ClearCart(); };
            Button btnCheckout = MakeButton("CHECKOUT & PRINT INVOICE", Primary, 10, 194, 430, 56, 13f);
            btnCheckout.Click += delegate { Checkout(); };
            totals.Controls.Add(btnRemove);
            totals.Controls.Add(btnClear);
            totals.Controls.Add(btnCheckout);

            cartPanel.Controls.Add(gridCart);
            cartPanel.Controls.Add(totals);
            cartPanel.Controls.Add(lblCartTitle);

            // ---------------- Add to form ----------------
            Controls.Add(center);      // Fill أول واحد
            Controls.Add(cartPanel);
            Controls.Add(left);
            Controls.Add(header);      // آخر واحد: ياخد العرض كله فوق
        }

        // ============================================================
        //  3) المنطق
        // ============================================================
        void ShowCategory(string name)
        {
            currentCategory = name;
            Color c = categoryColor[name];

            lblCategory.Text = name;
            lblCategory.ForeColor = c;

            gridProducts.DataSource = null;
            gridProducts.DataSource = new List<Product>(catalog[name]);
            SetCol(gridProducts, "Name", "Item", "", 200f, false);
            SetCol(gridProducts, "Price", "Price (EGP)", "N2", 90f, true);
            gridProducts.ColumnHeadersDefaultCellStyle.BackColor = c;
            gridProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = c;

            // الزر المختار يبقى أبيض بخط ملون
            foreach (string cat in categoryOrder)
            {
                Button b = categoryButtons[cat];
                if (cat == name)
                {
                    b.BackColor = Color.White;
                    b.ForeColor = categoryColor[cat];
                }
                else
                {
                    b.BackColor = categoryColor[cat];
                    b.ForeColor = Color.White;
                }
            }
            nudQty.Value = 1;
        }

        void AddToCart()
        {
            if (gridProducts.CurrentRow == null) return;
            Product p = gridProducts.CurrentRow.DataBoundItem as Product;
            if (p == null) return;

            int qty = (int)nudQty.Value;
            CartItem existing = cart.FirstOrDefault(c => c.Name == p.Name);
            if (existing != null)
            {
                existing.Qty += qty;
                cart.ResetBindings();
            }
            else
            {
                cart.Add(new CartItem(p.Name, p.Price, qty));
            }
            nudQty.Value = 1;
            UpdateTotals();
        }

        void RemoveSelected()
        {
            if (gridCart.CurrentRow == null) return;
            CartItem item = gridCart.CurrentRow.DataBoundItem as CartItem;
            if (item == null) return;
            cart.Remove(item);
            UpdateTotals();
        }

        void ClearCart()
        {
            if (cart.Count == 0) return;
            DialogResult r = MessageBox.Show("Remove all items from the cart?", "Clear Cart",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                cart.Clear();
                UpdateTotals();
            }
        }

        static string M(decimal v)
        {
            return v.ToString("N2", CultureInfo.InvariantCulture);
        }

        void UpdateTotals()
        {
            decimal sub = cart.Sum(c => c.Total);
            decimal tax = Math.Round(sub * TaxRate, 2);
            decimal total = sub + tax;
            int items = cart.Sum(c => c.Qty);

            lblSubtotal.Text = M(sub) + " EGP";
            lblTax.Text = M(tax) + " EGP";
            lblTotal.Text = M(total) + " EGP";
            lblCartTitle.Text = "YOUR CART  (" + items + (items == 1 ? " item)" : " items)");
        }

        // ============================================================
        //  4) الفاتورة
        // ============================================================
        static string Center(string s, int width)
        {
            return s.PadLeft((width + s.Length) / 2).PadRight(width);
        }

        string BuildInvoiceText()
        {
            const int W = 44;
            StringBuilder sb = new StringBuilder();
            string eq = new string('=', W);
            string dash = new string('-', W);

            sb.AppendLine(eq);
            sb.AppendLine(Center(StoreName, W));
            sb.AppendLine(Center("Faculty of Computers & AI - Sohag", W));
            sb.AppendLine(eq);
            sb.AppendLine("Invoice No : INV-" + invoiceNo);
            sb.AppendLine("Date       : " + DateTime.Now.ToString("yyyy-MM-dd  HH:mm", CultureInfo.InvariantCulture));
            sb.AppendLine(dash);
            sb.AppendLine(string.Format("{0,-18}{1,4}{2,10}{3,12}", "Item", "Qty", "Price", "Total"));
            sb.AppendLine(dash);

            foreach (CartItem c in cart)
            {
                string n = c.Name.Length > 17 ? c.Name.Substring(0, 17) : c.Name;
                sb.AppendLine(string.Format("{0,-18}{1,4}{2,10}{3,12}", n, c.Qty, M(c.Price), M(c.Total)));
            }

            decimal sub = cart.Sum(c => c.Total);
            decimal tax = Math.Round(sub * TaxRate, 2);
            decimal total = sub + tax;

            sb.AppendLine(dash);
            sb.AppendLine(string.Format("{0,-24}{1,20}", "Subtotal", M(sub) + " EGP"));
            sb.AppendLine(string.Format("{0,-24}{1,20}", "VAT 14%", M(tax) + " EGP"));
            sb.AppendLine(eq);
            sb.AppendLine(string.Format("{0,-24}{1,20}", "TOTAL", M(total) + " EGP"));
            sb.AppendLine(eq);
            sb.AppendLine(Center("Thank you for shopping with us!", W));
            sb.AppendLine(Center("See you again :)", W));
            return sb.ToString();
        }

        void Checkout()
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty.\nPlease add some items first.", "Cart is empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            invoiceNo++;
            string text = BuildInvoiceText();

            Form f = new Form();
            f.Text = "Invoice INV-" + invoiceNo;
            f.ClientSize = new Size(540, 700);
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.BackColor = Color.White;

            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.ReadOnly = true;
            rtb.BorderStyle = BorderStyle.None;
            rtb.BackColor = Color.White;
            rtb.Font = new Font("Consolas", 11f);
            rtb.Text = text;

            Panel wrap = new Panel();
            wrap.Dock = DockStyle.Fill;
            wrap.Padding = new Padding(30, 20, 20, 10);
            wrap.BackColor = Color.White;
            wrap.Controls.Add(rtb);

            Panel bar = new Panel();
            bar.Dock = DockStyle.Bottom;
            bar.Height = 74;
            bar.BackColor = LightBg;

            Button btnPrint = MakeButton("PRINT", Accent, 20, 14, 160, 46, 12f);
            btnPrint.Click += delegate { PrintInvoice(text); };
            Button btnNew = MakeButton("NEW ORDER", Primary, 190, 14, 180, 46, 12f);
            btnNew.Click += delegate
            {
                cart.Clear();
                UpdateTotals();
                f.Close();
            };
            Button btnClose = MakeButton("CLOSE", Color.FromArgb(96, 125, 139), 380, 14, 140, 46, 12f);
            btnClose.Click += delegate { f.Close(); };
            bar.Controls.Add(btnPrint);
            bar.Controls.Add(btnNew);
            bar.Controls.Add(btnClose);

            f.Controls.Add(wrap);
            f.Controls.Add(bar);
            f.ShowDialog(this);
            f.Dispose();
        }

        void PrintInvoice(string text)
        {
            try
            {
                PrintDocument doc = new PrintDocument();
                doc.PrintPage += (s, e) =>
                {
                    using (Font font = new Font("Consolas", 10f))
                    {
                        if (e.Graphics != null)
                            e.Graphics.DrawString(text, font, Brushes.Black, 50, 50);
                    }
                    e.HasMorePages = false;
                };

                using (PrintPreviewDialog dlg = new PrintPreviewDialog())
                {
                    dlg.Document = doc;
                    dlg.Width = 900;
                    dlg.Height = 750;
                    dlg.ShowDialog();   // من هنا زر الطباعة (أو Save as PDF)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the print preview.\nMake sure a printer is installed " +
                    "(e.g. Microsoft Print to PDF).\n\n" + ex.Message, "Print error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
