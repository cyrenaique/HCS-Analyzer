﻿namespace HCSAnalyzer
{
    partial class FormForImportExcel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormForImportExcel));
            this.buttonOk = new System.Windows.Forms.Button();
            this.dataGridViewForImport = new System.Windows.Forms.DataGridView();
            this.contextMenuStripDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.numericUpDownRows = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownColumns = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonChangeMode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewForImport)).BeginInit();
            this.contextMenuStripDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownColumns)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(480, 600);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(149, 30);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // dataGridViewForImport
            // 
            this.dataGridViewForImport.AllowUserToAddRows = false;
            this.dataGridViewForImport.AllowUserToDeleteRows = false;
            this.dataGridViewForImport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewForImport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewForImport.ContextMenuStrip = this.contextMenuStripDataGrid;
            this.dataGridViewForImport.Location = new System.Drawing.Point(12, 127);
            this.dataGridViewForImport.Name = "dataGridViewForImport";
            this.dataGridViewForImport.Size = new System.Drawing.Size(617, 467);
            this.dataGridViewForImport.TabIndex = 3;
            // 
            // contextMenuStripDataGrid
            // 
            this.contextMenuStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.unselectAllToolStripMenuItem});
            this.contextMenuStripDataGrid.Name = "contextMenuStripDataGrid";
            this.contextMenuStripDataGrid.Size = new System.Drawing.Size(137, 48);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // unselectAllToolStripMenuItem
            // 
            this.unselectAllToolStripMenuItem.Name = "unselectAllToolStripMenuItem";
            this.unselectAllToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.unselectAllToolStripMenuItem.Text = "Unselect All";
            this.unselectAllToolStripMenuItem.Click += new System.EventHandler(this.unselectAllToolStripMenuItem_Click);
            // 
            // numericUpDownRows
            // 
            this.numericUpDownRows.Location = new System.Drawing.Point(96, 68);
            this.numericUpDownRows.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this.numericUpDownRows.Name = "numericUpDownRows";
            this.numericUpDownRows.Size = new System.Drawing.Size(82, 20);
            this.numericUpDownRows.TabIndex = 2;
            this.numericUpDownRows.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // numericUpDownColumns
            // 
            this.numericUpDownColumns.Location = new System.Drawing.Point(96, 29);
            this.numericUpDownColumns.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.numericUpDownColumns.Name = "numericUpDownColumns";
            this.numericUpDownColumns.Size = new System.Drawing.Size(82, 20);
            this.numericUpDownColumns.TabIndex = 1;
            this.numericUpDownColumns.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 34;
            this.label2.Text = "Rows";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "Columns";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDownColumns);
            this.groupBox1.Controls.Add(this.numericUpDownRows);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 109);
            this.groupBox1.TabIndex = 38;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plate Dimensions";
            // 
            // buttonChangeMode
            // 
            this.buttonChangeMode.Enabled = false;
            this.buttonChangeMode.Location = new System.Drawing.Point(503, 12);
            this.buttonChangeMode.Name = "buttonChangeMode";
            this.buttonChangeMode.Size = new System.Drawing.Size(126, 27);
            this.buttonChangeMode.TabIndex = 39;
            this.buttonChangeMode.Text = "Well Position Mode";
            this.buttonChangeMode.UseVisualStyleBackColor = true;
            this.buttonChangeMode.Click += new System.EventHandler(this.buttonChangeMode_Click);
            // 
            // FormForImportExcel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 642);
            this.Controls.Add(this.buttonChangeMode);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridViewForImport);
            this.Controls.Add(this.buttonOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormForImportExcel";
            this.Text = "Import";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewForImport)).EndInit();
            this.contextMenuStripDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownColumns)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        public System.Windows.Forms.DataGridView dataGridViewForImport;
        public System.Windows.Forms.NumericUpDown numericUpDownRows;
        public System.Windows.Forms.NumericUpDown numericUpDownColumns;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDataGrid;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unselectAllToolStripMenuItem;
        private System.Windows.Forms.Button buttonChangeMode;
    }
}