namespace LTreeDemo
{
    partial class MainForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bonesBox = new System.Windows.Forms.CheckBox();
            this.light2Box = new System.Windows.Forms.CheckBox();
            this.light1Box = new System.Windows.Forms.CheckBox();
            this.windBox = new System.Windows.Forms.CheckBox();
            this.leavesBox = new System.Windows.Forms.CheckBox();
            this.branchesBox = new System.Windows.Forms.CheckBox();
            this.randomButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.seedBox = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.profileBox = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bonesLabel = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.leavesLabel = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.polygonsLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.verticesLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.xnaControl = new LTreeDemo.TreeDemoControl();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.seedBox)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.randomButton);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.seedBox);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.profileBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(676, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 616);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.bonesBox);
            this.groupBox1.Controls.Add(this.light2Box);
            this.groupBox1.Controls.Add(this.light1Box);
            this.groupBox1.Controls.Add(this.windBox);
            this.groupBox1.Controls.Add(this.leavesBox);
            this.groupBox1.Controls.Add(this.branchesBox);
            this.groupBox1.Location = new System.Drawing.Point(9, 128);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(179, 161);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Display Options";
            // 
            // bonesBox
            // 
            this.bonesBox.AutoSize = true;
            this.bonesBox.Location = new System.Drawing.Point(6, 134);
            this.bonesBox.Name = "bonesBox";
            this.bonesBox.Size = new System.Drawing.Size(56, 17);
            this.bonesBox.TabIndex = 5;
            this.bonesBox.Text = "Bones";
            this.bonesBox.UseVisualStyleBackColor = true;
            this.bonesBox.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // light2Box
            // 
            this.light2Box.AutoSize = true;
            this.light2Box.Checked = true;
            this.light2Box.CheckState = System.Windows.Forms.CheckState.Checked;
            this.light2Box.Location = new System.Drawing.Point(6, 111);
            this.light2Box.Name = "light2Box";
            this.light2Box.Size = new System.Drawing.Size(58, 17);
            this.light2Box.TabIndex = 4;
            this.light2Box.Text = "Light 2";
            this.light2Box.UseVisualStyleBackColor = true;
            this.light2Box.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // light1Box
            // 
            this.light1Box.AutoSize = true;
            this.light1Box.Checked = true;
            this.light1Box.CheckState = System.Windows.Forms.CheckState.Checked;
            this.light1Box.Location = new System.Drawing.Point(6, 88);
            this.light1Box.Name = "light1Box";
            this.light1Box.Size = new System.Drawing.Size(58, 17);
            this.light1Box.TabIndex = 3;
            this.light1Box.Text = "Light 1";
            this.light1Box.UseVisualStyleBackColor = true;
            this.light1Box.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // windBox
            // 
            this.windBox.AutoSize = true;
            this.windBox.Checked = true;
            this.windBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.windBox.Location = new System.Drawing.Point(6, 65);
            this.windBox.Name = "windBox";
            this.windBox.Size = new System.Drawing.Size(51, 17);
            this.windBox.TabIndex = 2;
            this.windBox.Text = "Wind";
            this.windBox.UseVisualStyleBackColor = true;
            this.windBox.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // leavesBox
            // 
            this.leavesBox.AutoSize = true;
            this.leavesBox.Checked = true;
            this.leavesBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.leavesBox.Location = new System.Drawing.Point(6, 42);
            this.leavesBox.Name = "leavesBox";
            this.leavesBox.Size = new System.Drawing.Size(61, 17);
            this.leavesBox.TabIndex = 1;
            this.leavesBox.Text = "Leaves";
            this.leavesBox.UseVisualStyleBackColor = true;
            this.leavesBox.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // branchesBox
            // 
            this.branchesBox.AutoSize = true;
            this.branchesBox.Checked = true;
            this.branchesBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.branchesBox.Location = new System.Drawing.Point(6, 19);
            this.branchesBox.Name = "branchesBox";
            this.branchesBox.Size = new System.Drawing.Size(71, 17);
            this.branchesBox.TabIndex = 0;
            this.branchesBox.Text = "Branches";
            this.branchesBox.UseVisualStyleBackColor = true;
            this.branchesBox.CheckedChanged += new System.EventHandler(this.UpdateOptions);
            // 
            // randomButton
            // 
            this.randomButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.randomButton.Location = new System.Drawing.Point(52, 99);
            this.randomButton.Name = "randomButton";
            this.randomButton.Size = new System.Drawing.Size(82, 23);
            this.randomButton.TabIndex = 8;
            this.randomButton.Text = "Random";
            this.randomButton.UseVisualStyleBackColor = true;
            this.randomButton.Click += new System.EventHandler(this.randomButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Seed";
            // 
            // seedBox
            // 
            this.seedBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.seedBox.Location = new System.Drawing.Point(9, 73);
            this.seedBox.Maximum = new decimal(new int[] {
            2000000,
            0,
            0,
            0});
            this.seedBox.Name = "seedBox";
            this.seedBox.Size = new System.Drawing.Size(179, 20);
            this.seedBox.TabIndex = 6;
            this.seedBox.ValueChanged += new System.EventHandler(this.seedBox_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 594);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Use mouse wheel to zoom.";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 572);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(196, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Hold left mouse button to rotate camera.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tree Profile";
            // 
            // profileBox
            // 
            this.profileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.profileBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profileBox.FormattingEnabled = true;
            this.profileBox.Location = new System.Drawing.Point(9, 25);
            this.profileBox.Name = "profileBox";
            this.profileBox.Size = new System.Drawing.Size(179, 21);
            this.profileBox.TabIndex = 0;
            this.profileBox.SelectedIndexChanged += new System.EventHandler(this.profileBox_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.bonesLabel);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.leavesLabel);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.polygonsLabel);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.verticesLabel);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(685, 295);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(179, 105);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statistics";
            // 
            // bonesLabel
            // 
            this.bonesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bonesLabel.Location = new System.Drawing.Point(105, 82);
            this.bonesLabel.Name = "bonesLabel";
            this.bonesLabel.Size = new System.Drawing.Size(64, 13);
            this.bonesLabel.TabIndex = 10;
            this.bonesLabel.Text = "123";
            this.bonesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(2, 82);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(37, 13);
            this.label14.TabIndex = 9;
            this.label14.Text = "Bones";
            // 
            // leavesLabel
            // 
            this.leavesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.leavesLabel.Location = new System.Drawing.Point(105, 60);
            this.leavesLabel.Name = "leavesLabel";
            this.leavesLabel.Size = new System.Drawing.Size(64, 13);
            this.leavesLabel.TabIndex = 8;
            this.leavesLabel.Text = "123";
            this.leavesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(2, 60);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(42, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "Leaves";
            // 
            // polygonsLabel
            // 
            this.polygonsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.polygonsLabel.Location = new System.Drawing.Point(105, 38);
            this.polygonsLabel.Name = "polygonsLabel";
            this.polygonsLabel.Size = new System.Drawing.Size(64, 13);
            this.polygonsLabel.TabIndex = 6;
            this.polygonsLabel.Text = "123";
            this.polygonsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(2, 38);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Trunk polygons";
            // 
            // verticesLabel
            // 
            this.verticesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.verticesLabel.Location = new System.Drawing.Point(105, 16);
            this.verticesLabel.Name = "verticesLabel";
            this.verticesLabel.Size = new System.Drawing.Size(64, 13);
            this.verticesLabel.TabIndex = 4;
            this.verticesLabel.Text = "123";
            this.verticesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(2, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Trunk vertices";
            // 
            // xnaControl
            // 
            this.xnaControl.CameraDistance = 0F;
            this.xnaControl.CameraOrbitAngle = 0F;
            this.xnaControl.CameraPitchAngle = 0F;
            this.xnaControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xnaControl.EnableBones = false;
            this.xnaControl.EnableLeaves = false;
            this.xnaControl.EnableLight1 = false;
            this.xnaControl.EnableLight2 = false;
            this.xnaControl.EnableTrunk = false;
            this.xnaControl.EnableWind = false;
            this.xnaControl.Location = new System.Drawing.Point(0, 0);
            this.xnaControl.Name = "xnaControl";
            this.xnaControl.ProfileIndex = 0;
            this.xnaControl.Seed = 0;
            this.xnaControl.Size = new System.Drawing.Size(676, 616);
            this.xnaControl.TabIndex = 1;
            this.xnaControl.Text = "xnaControl";
            this.xnaControl.TreeUpdated += new System.EventHandler(this.xnaControl_OnTreeUpdated);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(876, 616);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.xnaControl);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "LTrees Demo";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.seedBox)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox profileBox;
        private System.Windows.Forms.Button randomButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown seedBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox bonesBox;
        private System.Windows.Forms.CheckBox light2Box;
        private System.Windows.Forms.CheckBox light1Box;
        private System.Windows.Forms.CheckBox windBox;
        private System.Windows.Forms.CheckBox leavesBox;
        private System.Windows.Forms.CheckBox branchesBox;
        private TreeDemoControl xnaControl;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label bonesLabel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label leavesLabel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label polygonsLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label verticesLabel;
        private System.Windows.Forms.Label label8;
    }
}

