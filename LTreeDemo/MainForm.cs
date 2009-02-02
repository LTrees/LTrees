/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace LTreeDemo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (base.DesignMode)
                return;

            for (int i = 0; i < xnaControl.ProfileNames.Count; i++)
            {
                profileBox.Items.Add(xnaControl.ProfileNames[i]);
            }
            profileBox.SelectedIndex = 0;
            UpdateOptions(null, null);
        }

        private void profileBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            xnaControl.ProfileIndex = profileBox.SelectedIndex;
        }

        private void UpdateOptions(object sender, EventArgs e)
        {
            xnaControl.EnableLeaves = leavesBox.Checked;
            xnaControl.EnableLight1 = light1Box.Checked;
            xnaControl.EnableLight2 = light2Box.Checked;
            xnaControl.EnableTrunk = branchesBox.Checked;
            xnaControl.EnableWind = windBox.Checked;
            xnaControl.EnableBones = bonesBox.Checked;
            xnaControl.EnableGround = groundBox.Checked;
        }

        private void seedBox_ValueChanged(object sender, EventArgs e)
        {
            xnaControl.Seed = (int)seedBox.Value;
        }

        Random random = new Random();
        private void randomButton_Click(object sender, EventArgs e)
        {
            seedBox.Value = random.Next((int)seedBox.Maximum);
        }

        private void xnaControl_OnTreeUpdated(object sender, EventArgs e)
        {
            verticesLabel.Text = "" + xnaControl.Tree.TrunkMesh.NumberOfVertices;
            polygonsLabel.Text = "" + xnaControl.Tree.TrunkMesh.NumberOfTriangles;
            leavesLabel.Text = "" + xnaControl.Tree.Skeleton.Leaves.Count;
            bonesLabel.Text = "" + xnaControl.Tree.Skeleton.Bones.Count;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Image files (*.png, *.jpg, *.bmp, *.tga)|*.png;*.jpg;*.bmp;*.tga|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                xnaControl.SaveTreeImage(dialog.FileName, GetImageFileFormatFromFilename(dialog.FileName));
            }
        }

        private ImageFileFormat GetImageFileFormatFromFilename(string filename)
        {
            String ext = filename.Substring(filename.LastIndexOf(".")).ToLowerInvariant();
            switch (ext)
            {
                case ".png":
                    return ImageFileFormat.Png;
                case ".jpg":
                    return ImageFileFormat.Jpg;
                case ".bmp":
                    return ImageFileFormat.Bmp;
                case ".tga":
                    return ImageFileFormat.Tga;
                default:
                    return ImageFileFormat.Png; // Just use this as default
            }
        }

    }
}
