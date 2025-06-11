using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using PdfSharp.Fonts;




namespace Graphics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private int ComputeSubtreeLeafCounts(��2.ParseNode node)
        {
            if (node.Children.Count == 0)
            {
                subtreeLeafCounts[node] = 1;
                return 1;
            }

            int count = 0;
            foreach (var child in node.Children)
            {
                count += ComputeSubtreeLeafCounts(child);
            }

            subtreeLeafCounts[node] = count;
            return count;
        }

        private TreeNode ConvertParseNodeToTreeNode(��2.ParseNode node)
        {
            TreeNode treeNode = new TreeNode(node.Name);
            foreach (var child in node.Children)
            {
                treeNode.Nodes.Add(ConvertParseNodeToTreeNode(child));
            }
            return treeNode;
        }
        private int GetTreeDepth(��2.ParseNode node)
        {
            if (node.Children.Count == 0)
                return 1;
            int maxChildDepth = 0;
            foreach (var child in node.Children)
            {
                int childDepth = GetTreeDepth(child);
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }
            return maxChildDepth + 1;
        }
        private int GetSubtreeLeafCount(��2.ParseNode node)
        {
            if (node.Children.Count == 0)
                return 1;
            int count = 0;
            foreach (var child in node.Children)
            {
                count += GetSubtreeLeafCount(child);
            }
            return count;
        }



        string inputText;
        ��2.Parser parser;
        ��2.ParseNode root;
        //��� ������������� ������ ����������� ����� ��������
        //      ��� �������������� treeView1
        private void CreateTree()
        {
            inputText = textBox1.Text;

            parser = new ��2.Parser(new ��2.Lexer(inputText));
            root = parser.Program();

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(ConvertParseNodeToTreeNode(root));
            treeView1.ExpandAll();
        }
        private void GeneratePdfTree(��2.ParseNode node, string filename)
        {
            string projectDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(projectDir, @"..\..\.."));

            subtreeLeafCounts = new Dictionary<��2.ParseNode, int>();
            ComputeSubtreeLeafCounts(node);

            double totalLeafCount = subtreeLeafCounts[node];
            double baseSpacing = 40;
            double textPadding = 50; // �������������� ������ ��� ������

            double horizontalSpacingUnit = baseSpacing + textPadding;
            double treeWidth = totalLeafCount * horizontalSpacingUnit;
            double margin = 50;

            int treeDepth = GetTreeDepth(node);
            double verticalSpacing = 100;
            double treeHeight = treeDepth * verticalSpacing + 2 * margin;

            PdfDocument document = new PdfDocument();
            document.Info.Title = "Tree";

            PdfPage page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;

            // ��������� ������ � ������ ��������
            page.Width = Math.Max(treeWidth + 2 * margin, page.Width);
            page.Height = Math.Max(treeHeight, page.Height);

            XGraphics gfx = XGraphics.FromPdfPage(page);

            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();
            }

            XFont font = new XFont("MyVerdana", 8, XFontStyleEx.Regular);

            double startX = page.Width / 2;
            double startY = margin;

            // horizontalSpacingUnit ��� ���� ����
            DrawNode(gfx, font, node, startX, startY, horizontalSpacingUnit, verticalSpacing);

            string filePath = Path.Combine(projectRoot, "tree.pdf");
            document.Save(filePath);
        }


        /* private void GeneratePdfTree(��2.ParseNode node, string filename)
         {
             string projectDir = AppDomain.CurrentDomain.BaseDirectory;
             string projectRoot = Path.GetFullPath(Path.Combine(projectDir, @"..\..\.."));

             subtreeLeafCounts = new Dictionary<��2.ParseNode, int>();
             ComputeSubtreeLeafCounts(node);

             double totalLeafCount = subtreeLeafCounts[node];
             double minSpacing = 40;
             double treeWidth = totalLeafCount * minSpacing;
             double margin = 50;

             int treeDepth = GetTreeDepth(node);
             double verticalSpacing = 100;
             double treeHeight = treeDepth * verticalSpacing + 2 * margin;

             PdfDocument document = new PdfDocument();
             document.Info.Title = "Tree";

             PdfPage page = document.AddPage();
             page.Orientation = PdfSharp.PageOrientation.Landscape;

             // ��������� ������ � ������ ��������
             page.Width = Math.Max(treeWidth + 2 * margin, page.Width);
             page.Height = Math.Max(treeHeight, page.Height); // <= ���������� ������

             XGraphics gfx = XGraphics.FromPdfPage(page);

             if (GlobalFontSettings.FontResolver == null)
             {
                 GlobalFontSettings.FontResolver = new CustomFontResolver();
             }

             XFont font = new XFont("MyVerdana", 8, XFontStyleEx.Regular);
             double startX = page.Width / 2;
             double startY = margin;

             double horizontalSpacingUnit = Math.Max(minSpacing, (page.Width - 2 * margin) / totalLeafCount)+25;

             DrawNode(gfx, font, node, startX, startY, horizontalSpacingUnit, verticalSpacing);


             string filePath = Path.Combine(projectRoot, "tree.pdf");
             document.Save(filePath);
         }
 */



        private void Form1_Load(object sender, EventArgs e)
        {
            this.Controls.Add(treeView1);
            this.Controls.Add(panel1);

            panel1.Dock = DockStyle.Top;
            panel1.Height = 100;
            panel2.Height = 100;
            panel2.Dock = DockStyle.Bottom;
            treeView1.Dock = DockStyle.Fill;
            CreateTree();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            inputText = textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateTree();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string rpn = parser.ToRPN(root);
            textBox_output.Text = rpn;
        }

        private XRect rectanglePage;
        private void button_PDF_Click(object sender, EventArgs e)
        {
            //PdfDocument document = new PdfDocument();

            //PdfPage page = document.AddPage();
            //page.Orientation = PdfSharp.PageOrientation.Landscape;
            //XGraphics gfx = XGraphics.FromPdfPage(page);
            //gfx.DrawRectangle(XBrushes.Red, rectanglePage);
            //Rectangle rectangle = new Rectangle(0, 0, (int)page.Width.Point, (int)page.Height.Point);

            //Bitmap bmp = new Bitmap(rectangle.Width, rectangle.Height);
            //XGraphics g = XGraphics.FromImage(bmp);
            //this.chart.Printing.PrintPaint(g, rectangle);

            //XImage img = XImage.FromGdiPlusImage(bmp);
            //gfx.DrawImage(img, 0, 0, page.Width, page.Height);
            GeneratePdfTree(root, "tree.pdf");
            MessageBox.Show("��� ������!");
        }
        private double d;
        private double r;
        private Dictionary<��2.ParseNode, int> subtreeLeafCounts;

        private void DrawNode(XGraphics gfx, XFont font, ��2.ParseNode node, double x, double y, double hSpacingUnit, double vSpacing)
        {
            double r = 22; 
            gfx.DrawEllipse(XPens.Black, XBrushes.LightBlue, x - r, y - r, 2 * r, 2 * r);
            var textSize = gfx.MeasureString(node.Name, font);
            gfx.DrawString(node.Name, font, XBrushes.Black, x - textSize.Width / 2, y + textSize.Height / 4);

            if (node.Children.Count == 0)
                return;

            double totalLeafUnits = 0;
            foreach (var child in node.Children)
            {
                totalLeafUnits += subtreeLeafCounts[child];
            }

            double childX = x - hSpacingUnit * totalLeafUnits / 2;

            foreach (var child in node.Children)
            {
                double childLeafUnits = subtreeLeafCounts[child];
                double childSubtreeWidth = hSpacingUnit * childLeafUnits;

                double childCenterX = childX + childSubtreeWidth / 2;
                double childY = y + vSpacing;

                gfx.DrawLine(XPens.Black, x, y + r, childCenterX, childY - r);
                DrawNode(gfx, font, child, childCenterX, childY, hSpacingUnit, vSpacing);

                childX += childSubtreeWidth;
            }
        }

        /*private void DrawNode(XGraphics gfx, XFont font, ��2.ParseNode node, double x, double y, double hSpacingUnit, double vSpacing)
        {
            r = 20;

            // ������ ��� ����
            gfx.DrawEllipse(XPens.Black, XBrushes.LightBlue, x - r, y - r, 2 * r, 2 * r);
            var textSize = gfx.MeasureString(node.Name, font);
            gfx.DrawString(node.Name, font, XBrushes.Black, x - textSize.Width / 2, y + textSize.Height / 4);

            if (node.Children.Count == 0) return;

            // 1. ������� ������ "����" �������� (� ������)
            double totalLeafUnits = 0;
            foreach (var child in node.Children)
            {
                totalLeafUnits += subtreeLeafCounts[child];
            }

            // 2. �������������� ������������� ������ (� ������), ����� ������ �� ���������
            double textPadding = 140;

            // 3. ����� ������ ��������� � ������ padding
            double totalWidth = hSpacingUnit * totalLeafUnits + textPadding * (node.Children.Count - 1);

            // 4. ������ ��������� �� X
            double childX = x - totalWidth / 2;

            // 5. ��������� ���� �������� �����
            foreach (var child in node.Children)
            {
                double childLeafUnits = subtreeLeafCounts[child];
                double childSubtreeWidth = hSpacingUnit * childLeafUnits;

                double childCenterX = childX + childSubtreeWidth / 2;
                double childY = y + vSpacing;

                gfx.DrawLine(XPens.Black, x, y + r, childCenterX, childY - r);
                DrawNode(gfx, font, child, childCenterX, childY, hSpacingUnit, vSpacing);

                childX += childSubtreeWidth + textPadding; // ��������� ������������� ������ ����� ������������
            }
        }*/


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
