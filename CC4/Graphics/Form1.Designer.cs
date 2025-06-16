namespace Graphics
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            button1 = new Button();
            panel1 = new Panel();
            panel3 = new Panel();
            button_PDF = new Button();
            panel2 = new Panel();
            textBox_output = new RichTextBox();
            button2 = new Button();
            treeView1 = new TreeView();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Top;
            textBox1.Location = new Point(0, 0);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(800, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "{ xa = 5; y = (xa + 2) * 3; z = xa < y}";
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Dock = DockStyle.Left;
            button1.Location = new Point(0, 0);
            button1.Name = "button1";
            button1.Size = new Size(407, 31);
            button1.TabIndex = 2;
            button1.Text = "Create Tree";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(textBox1);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 80);
            panel1.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.Controls.Add(button_PDF);
            panel3.Controls.Add(button1);
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 49);
            panel3.Margin = new Padding(3, 2, 3, 2);
            panel3.Name = "panel3";
            panel3.Size = new Size(800, 31);
            panel3.TabIndex = 2;
            // 
            // button_PDF
            // 
            button_PDF.Dock = DockStyle.Right;
            button_PDF.Location = new Point(410, 0);
            button_PDF.Name = "button_PDF";
            button_PDF.Size = new Size(390, 31);
            button_PDF.TabIndex = 3;
            button_PDF.Text = "ToPDF";
            button_PDF.UseVisualStyleBackColor = true;
            button_PDF.Click += button_PDF_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(textBox_output);
            panel2.Controls.Add(button2);
            panel2.Location = new Point(0, 356);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(800, 94);
            panel2.TabIndex = 4;
            // 
            // textBox_output
            // 
            textBox_output.Dock = DockStyle.Top;
            textBox_output.Location = new Point(0, 0);
            textBox_output.Margin = new Padding(3, 2, 3, 2);
            textBox_output.Name = "textBox_output";
            textBox_output.Size = new Size(800, 68);
            textBox_output.TabIndex = 5;
            textBox_output.Text = "";
            // 
            // button2
            // 
            button2.Dock = DockStyle.Bottom;
            button2.Location = new Point(0, 72);
            button2.Margin = new Padding(3, 2, 3, 2);
            button2.Name = "button2";
            button2.Size = new Size(800, 22);
            button2.TabIndex = 1;
            button2.Text = "Postprefix";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // treeView1
            // 
            treeView1.Location = new Point(12, 86);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(788, 265);
            treeView1.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(treeView1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TextBox textBox1;
        private Button button1;
        private Panel panel1;
        private Panel panel2;
        private Button button2;
        private RichTextBox textBox_output;
        private Panel panel3;
        private Button button_PDF;
        private TreeView treeView1;
    }
}
