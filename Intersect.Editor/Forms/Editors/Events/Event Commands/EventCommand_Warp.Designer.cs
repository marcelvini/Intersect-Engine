using DarkUI.Controls;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands
{
    partial class EventCommandWarp
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpWarp = new DarkUI.Controls.DarkGroupBox();
            this.grpWarpSettings = new DarkUI.Controls.DarkGroupBox();
            this.nudScrollY = new DarkUI.Controls.DarkNumericUpDown();
            this.btnVisual = new DarkUI.Controls.DarkButton();
            this.rdoSpecificMap = new DarkUI.Controls.DarkRadioButton();
            this.lblY = new System.Windows.Forms.Label();
            this.lblX = new System.Windows.Forms.Label();
            this.rdoUserVariable = new DarkUI.Controls.DarkRadioButton();
            this.lblMap = new System.Windows.Forms.Label();
            this.rdoServerVariable = new DarkUI.Controls.DarkRadioButton();
            this.lblDir = new System.Windows.Forms.Label();
            this.rdoGuildVariable = new DarkUI.Controls.DarkRadioButton();
            this.cmbDirection = new DarkUI.Controls.DarkComboBox();
            this.rdoPlayerVariable = new DarkUI.Controls.DarkRadioButton();
            this.cmbMap = new DarkUI.Controls.DarkComboBox();
            this.nudScrollX = new DarkUI.Controls.DarkNumericUpDown();
            this.cmbScrollX = new DarkUI.Controls.DarkComboBox();
            this.cmbScrollY = new DarkUI.Controls.DarkComboBox();
            this.grpInstanceSettings = new DarkUI.Controls.DarkGroupBox();
            this.cmbInstanceType = new DarkUI.Controls.DarkComboBox();
            this.lblInstanceType = new System.Windows.Forms.Label();
            this.chkChangeInstance = new System.Windows.Forms.CheckBox();
            this.btnCancel = new DarkUI.Controls.DarkButton();
            this.btnSave = new DarkUI.Controls.DarkButton();
            this.grpWarp.SuspendLayout();
            this.grpWarpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrollY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrollX)).BeginInit();
            this.grpInstanceSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpWarp
            // 
            this.grpWarp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.grpWarp.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.grpWarp.Controls.Add(this.grpWarpSettings);
            this.grpWarp.Controls.Add(this.grpInstanceSettings);
            this.grpWarp.Controls.Add(this.chkChangeInstance);
            this.grpWarp.Controls.Add(this.btnCancel);
            this.grpWarp.Controls.Add(this.btnSave);
            this.grpWarp.ForeColor = System.Drawing.Color.Gainsboro;
            this.grpWarp.Location = new System.Drawing.Point(3, 3);
            this.grpWarp.Name = "grpWarp";
            this.grpWarp.Size = new System.Drawing.Size(295, 375);
            this.grpWarp.TabIndex = 17;
            this.grpWarp.TabStop = false;
            this.grpWarp.Text = "Warp";
            // 
            // grpWarpSettings
            // 
            this.grpWarpSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.grpWarpSettings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.grpWarpSettings.Controls.Add(this.nudScrollY);
            this.grpWarpSettings.Controls.Add(this.btnVisual);
            this.grpWarpSettings.Controls.Add(this.rdoSpecificMap);
            this.grpWarpSettings.Controls.Add(this.lblY);
            this.grpWarpSettings.Controls.Add(this.lblX);
            this.grpWarpSettings.Controls.Add(this.rdoUserVariable);
            this.grpWarpSettings.Controls.Add(this.lblMap);
            this.grpWarpSettings.Controls.Add(this.rdoServerVariable);
            this.grpWarpSettings.Controls.Add(this.lblDir);
            this.grpWarpSettings.Controls.Add(this.rdoGuildVariable);
            this.grpWarpSettings.Controls.Add(this.cmbDirection);
            this.grpWarpSettings.Controls.Add(this.rdoPlayerVariable);
            this.grpWarpSettings.Controls.Add(this.cmbMap);
            this.grpWarpSettings.Controls.Add(this.nudScrollX);
            this.grpWarpSettings.Controls.Add(this.cmbScrollX);
            this.grpWarpSettings.Controls.Add(this.cmbScrollY);
            this.grpWarpSettings.ForeColor = System.Drawing.Color.Gainsboro;
            this.grpWarpSettings.Location = new System.Drawing.Point(6, 19);
            this.grpWarpSettings.Name = "grpWarpSettings";
            this.grpWarpSettings.Size = new System.Drawing.Size(283, 228);
            this.grpWarpSettings.TabIndex = 83;
            this.grpWarpSettings.TabStop = false;
            this.grpWarpSettings.Text = "Warp Settings";
            // 
            // nudScrollY
            // 
            this.nudScrollY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudScrollY.ForeColor = System.Drawing.Color.Gainsboro;
            this.nudScrollY.Location = new System.Drawing.Point(92, 140);
            this.nudScrollY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudScrollY.Name = "nudScrollY";
            this.nudScrollY.Size = new System.Drawing.Size(121, 20);
            this.nudScrollY.TabIndex = 89;
            this.nudScrollY.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // btnVisual
            // 
            this.btnVisual.Location = new System.Drawing.Point(58, 197);
            this.btnVisual.Name = "btnVisual";
            this.btnVisual.Padding = new System.Windows.Forms.Padding(5);
            this.btnVisual.Size = new System.Drawing.Size(155, 23);
            this.btnVisual.TabIndex = 80;
            this.btnVisual.Text = "Open Visual Interface";
            this.btnVisual.Click += new System.EventHandler(this.btnVisual_Click);
            // 
            // rdoSpecificMap
            // 
            this.rdoSpecificMap.AutoSize = true;
            this.rdoSpecificMap.Checked = true;
            this.rdoSpecificMap.Location = new System.Drawing.Point(29, 19);
            this.rdoSpecificMap.Name = "rdoSpecificMap";
            this.rdoSpecificMap.Size = new System.Drawing.Size(87, 17);
            this.rdoSpecificMap.TabIndex = 87;
            this.rdoSpecificMap.TabStop = true;
            this.rdoSpecificMap.Text = "Specific Map";
            this.rdoSpecificMap.CheckedChanged += new System.EventHandler(this.rdoSpecificMap_CheckedChanged);
            // 
            // lblY
            // 
            this.lblY.AutoSize = true;
            this.lblY.Location = new System.Drawing.Point(55, 142);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(17, 13);
            this.lblY.TabIndex = 74;
            this.lblY.Text = "Y:";
            // 
            // lblX
            // 
            this.lblX.AutoSize = true;
            this.lblX.Location = new System.Drawing.Point(55, 117);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(17, 13);
            this.lblX.TabIndex = 73;
            this.lblX.Text = "X:";
            // 
            // rdoUserVariable
            // 
            this.rdoUserVariable.AutoSize = true;
            this.rdoUserVariable.Location = new System.Drawing.Point(29, 65);
            this.rdoUserVariable.Name = "rdoUserVariable";
            this.rdoUserVariable.Size = new System.Drawing.Size(106, 17);
            this.rdoUserVariable.TabIndex = 86;
            this.rdoUserVariable.Text = "Account Variable";
            this.rdoUserVariable.CheckedChanged += new System.EventHandler(this.rdoUserVariable_CheckedChanged);
            // 
            // lblMap
            // 
            this.lblMap.AutoSize = true;
            this.lblMap.Location = new System.Drawing.Point(55, 92);
            this.lblMap.Name = "lblMap";
            this.lblMap.Size = new System.Drawing.Size(31, 13);
            this.lblMap.TabIndex = 72;
            this.lblMap.Text = "Map:";
            // 
            // rdoServerVariable
            // 
            this.rdoServerVariable.AutoSize = true;
            this.rdoServerVariable.Location = new System.Drawing.Point(146, 42);
            this.rdoServerVariable.Name = "rdoServerVariable";
            this.rdoServerVariable.Size = new System.Drawing.Size(96, 17);
            this.rdoServerVariable.TabIndex = 84;
            this.rdoServerVariable.Text = "Global Variable";
            this.rdoServerVariable.CheckedChanged += new System.EventHandler(this.rdoServerVariable_CheckedChanged);
            // 
            // lblDir
            // 
            this.lblDir.AutoSize = true;
            this.lblDir.Location = new System.Drawing.Point(55, 168);
            this.lblDir.Name = "lblDir";
            this.lblDir.Size = new System.Drawing.Size(23, 13);
            this.lblDir.TabIndex = 75;
            this.lblDir.Text = "Dir:";
            // 
            // rdoGuildVariable
            // 
            this.rdoGuildVariable.AutoSize = true;
            this.rdoGuildVariable.Location = new System.Drawing.Point(29, 42);
            this.rdoGuildVariable.Name = "rdoGuildVariable";
            this.rdoGuildVariable.Size = new System.Drawing.Size(90, 17);
            this.rdoGuildVariable.TabIndex = 85;
            this.rdoGuildVariable.Text = "Guild Variable";
            this.rdoGuildVariable.CheckedChanged += new System.EventHandler(this.rdoGuildVariable_CheckedChanged);
            // 
            // cmbDirection
            // 
            this.cmbDirection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.cmbDirection.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cmbDirection.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.cmbDirection.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.cmbDirection.DrawDropdownHoverOutline = false;
            this.cmbDirection.DrawFocusRectangle = false;
            this.cmbDirection.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDirection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDirection.ForeColor = System.Drawing.Color.Gainsboro;
            this.cmbDirection.FormattingEnabled = true;
            this.cmbDirection.Location = new System.Drawing.Point(92, 165);
            this.cmbDirection.Name = "cmbDirection";
            this.cmbDirection.Size = new System.Drawing.Size(121, 21);
            this.cmbDirection.TabIndex = 76;
            this.cmbDirection.Text = null;
            this.cmbDirection.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // rdoPlayerVariable
            // 
            this.rdoPlayerVariable.AutoSize = true;
            this.rdoPlayerVariable.Location = new System.Drawing.Point(146, 19);
            this.rdoPlayerVariable.Name = "rdoPlayerVariable";
            this.rdoPlayerVariable.Size = new System.Drawing.Size(95, 17);
            this.rdoPlayerVariable.TabIndex = 83;
            this.rdoPlayerVariable.Text = "Player Variable";
            this.rdoPlayerVariable.CheckedChanged += new System.EventHandler(this.rdoPlayerVariable_CheckedChanged);
            // 
            // cmbMap
            // 
            this.cmbMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.cmbMap.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cmbMap.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.cmbMap.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.cmbMap.DrawDropdownHoverOutline = false;
            this.cmbMap.DrawFocusRectangle = false;
            this.cmbMap.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbMap.ForeColor = System.Drawing.Color.Gainsboro;
            this.cmbMap.FormattingEnabled = true;
            this.cmbMap.Location = new System.Drawing.Point(92, 88);
            this.cmbMap.Name = "cmbMap";
            this.cmbMap.Size = new System.Drawing.Size(121, 21);
            this.cmbMap.TabIndex = 77;
            this.cmbMap.Text = null;
            this.cmbMap.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // nudScrollX
            // 
            this.nudScrollX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudScrollX.ForeColor = System.Drawing.Color.Gainsboro;
            this.nudScrollX.Location = new System.Drawing.Point(92, 115);
            this.nudScrollX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudScrollX.Name = "nudScrollX";
            this.nudScrollX.Size = new System.Drawing.Size(121, 20);
            this.nudScrollX.TabIndex = 88;
            this.nudScrollX.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // cmbScrollX
            // 
            this.cmbScrollX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.cmbScrollX.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cmbScrollX.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.cmbScrollX.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.cmbScrollX.DrawDropdownHoverOutline = false;
            this.cmbScrollX.DrawFocusRectangle = false;
            this.cmbScrollX.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbScrollX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScrollX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbScrollX.ForeColor = System.Drawing.Color.Gainsboro;
            this.cmbScrollX.FormattingEnabled = true;
            this.cmbScrollX.Location = new System.Drawing.Point(92, 115);
            this.cmbScrollX.Name = "cmbScrollX";
            this.cmbScrollX.Size = new System.Drawing.Size(121, 21);
            this.cmbScrollX.TabIndex = 90;
            this.cmbScrollX.Text = null;
            this.cmbScrollX.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // cmbScrollY
            // 
            this.cmbScrollY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.cmbScrollY.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cmbScrollY.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.cmbScrollY.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.cmbScrollY.DrawDropdownHoverOutline = false;
            this.cmbScrollY.DrawFocusRectangle = false;
            this.cmbScrollY.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbScrollY.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScrollY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbScrollY.ForeColor = System.Drawing.Color.Gainsboro;
            this.cmbScrollY.FormattingEnabled = true;
            this.cmbScrollY.Location = new System.Drawing.Point(92, 139);
            this.cmbScrollY.Name = "cmbScrollY";
            this.cmbScrollY.Size = new System.Drawing.Size(121, 21);
            this.cmbScrollY.TabIndex = 91;
            this.cmbScrollY.Text = null;
            this.cmbScrollY.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // grpInstanceSettings
            // 
            this.grpInstanceSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.grpInstanceSettings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.grpInstanceSettings.Controls.Add(this.cmbInstanceType);
            this.grpInstanceSettings.Controls.Add(this.lblInstanceType);
            this.grpInstanceSettings.ForeColor = System.Drawing.Color.Gainsboro;
            this.grpInstanceSettings.Location = new System.Drawing.Point(6, 276);
            this.grpInstanceSettings.Name = "grpInstanceSettings";
            this.grpInstanceSettings.Size = new System.Drawing.Size(283, 63);
            this.grpInstanceSettings.TabIndex = 82;
            this.grpInstanceSettings.TabStop = false;
            this.grpInstanceSettings.Text = "Instance Settings";
            this.grpInstanceSettings.Visible = false;
            // 
            // cmbInstanceType
            // 
            this.cmbInstanceType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.cmbInstanceType.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cmbInstanceType.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.cmbInstanceType.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.cmbInstanceType.DrawDropdownHoverOutline = false;
            this.cmbInstanceType.DrawFocusRectangle = false;
            this.cmbInstanceType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbInstanceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInstanceType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbInstanceType.ForeColor = System.Drawing.Color.Gainsboro;
            this.cmbInstanceType.FormattingEnabled = true;
            this.cmbInstanceType.Location = new System.Drawing.Point(44, 35);
            this.cmbInstanceType.Name = "cmbInstanceType";
            this.cmbInstanceType.Size = new System.Drawing.Size(169, 21);
            this.cmbInstanceType.TabIndex = 64;
            this.cmbInstanceType.Text = null;
            this.cmbInstanceType.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // lblInstanceType
            // 
            this.lblInstanceType.AutoSize = true;
            this.lblInstanceType.Location = new System.Drawing.Point(41, 19);
            this.lblInstanceType.Name = "lblInstanceType";
            this.lblInstanceType.Size = new System.Drawing.Size(78, 13);
            this.lblInstanceType.TabIndex = 64;
            this.lblInstanceType.Text = "Instance Type:";
            // 
            // chkChangeInstance
            // 
            this.chkChangeInstance.AutoSize = true;
            this.chkChangeInstance.Location = new System.Drawing.Point(177, 255);
            this.chkChangeInstance.Name = "chkChangeInstance";
            this.chkChangeInstance.Size = new System.Drawing.Size(112, 17);
            this.chkChangeInstance.TabIndex = 81;
            this.chkChangeInstance.Text = "Change instance?";
            this.chkChangeInstance.UseVisualStyleBackColor = true;
            this.chkChangeInstance.CheckedChanged += new System.EventHandler(this.chkChangeInstance_CheckedChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(214, 345);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(5);
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(133, 345);
            this.btnSave.Name = "btnSave";
            this.btnSave.Padding = new System.Windows.Forms.Padding(5);
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "Ok";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // EventCommandWarp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Controls.Add(this.grpWarp);
            this.Name = "EventCommandWarp";
            this.Size = new System.Drawing.Size(305, 384);
            this.grpWarp.ResumeLayout(false);
            this.grpWarp.PerformLayout();
            this.grpWarpSettings.ResumeLayout(false);
            this.grpWarpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrollY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrollX)).EndInit();
            this.grpInstanceSettings.ResumeLayout(false);
            this.grpInstanceSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkGroupBox grpWarp;
        private DarkButton btnCancel;
        private DarkButton btnSave;
        private DarkRadioButton rdoSpecificMap;
        private DarkGroupBox grpInstanceSettings;
        private DarkComboBox cmbInstanceType;
        private System.Windows.Forms.Label lblInstanceType;
        private DarkRadioButton rdoUserVariable;
        private System.Windows.Forms.CheckBox chkChangeInstance;
        private DarkRadioButton rdoServerVariable;
        private DarkButton btnVisual;
        private DarkRadioButton rdoGuildVariable;
        private DarkRadioButton rdoPlayerVariable;
        private DarkComboBox cmbMap;
        private DarkComboBox cmbDirection;
        private System.Windows.Forms.Label lblDir;
        private System.Windows.Forms.Label lblMap;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private DarkGroupBox grpWarpSettings;
        private DarkNumericUpDown nudScrollY;
        private DarkNumericUpDown nudScrollX;
        private DarkComboBox cmbScrollY;
        private DarkComboBox cmbScrollX;
    }
}
