using System;
using System.Windows.Forms;
using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Events.Commands;
using Intersect.GameObjects.Maps.MapList;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands
{

    public partial class EventCommandWarp : UserControl
    {

        private readonly FrmEvent mEventEditor;

        private WarpCommand mMyCommand;

        private WarpType mWarpType;

        public EventCommandWarp(WarpCommand refCommand, FrmEvent editor)
        {
            InitializeComponent();
            mMyCommand = refCommand;
            mEventEditor = editor;

            nudScrollX.Maximum = Options.MapWidth - 1;
            nudScrollY.Maximum = Options.MapHeight - 1;
            mWarpType = mMyCommand.WarpType;

            InitLocalization();
            UpdateFormItems();
        }

        private void InitLocalization()
        {
            grpWarp.Text = Strings.EventWarp.title;
            grpWarpSettings.Text = Strings.Warping.Title.ToString();
            rdoSpecificMap.Text = Strings.Warping.SpecificMap.ToString();
            rdoPlayerVariable.Text = Strings.Warping.PlayerVariable.ToString();
            rdoServerVariable.Text = Strings.Warping.ServerVariable.ToString();
            rdoGuildVariable.Text = Strings.Warping.GuildVariable.ToString();
            rdoUserVariable.Text = Strings.Warping.UserVariable.ToString();
            lblMap.Text = Strings.Warping.map.ToString("");
            lblX.Text = Strings.Warping.x.ToString();
            lblY.Text = Strings.Warping.y.ToString();
            lblDir.Text = Strings.Warping.direction.ToString("");
            btnVisual.Text = Strings.Warping.visual;
            cmbDirection.Items.Clear();
            for (var i = -1; i < 4; i++)
            {
                cmbDirection.Items.Add(Strings.Direction.dir[(Direction)i]);
            }

            chkChangeInstance.Text = Strings.Warping.ChangeInstance;
            grpInstanceSettings.Text = Strings.Warping.MapInstancingGroup;
            lblInstanceType.Text = Strings.Warping.InstanceType;
            chkReturnOverworld.Text = Strings.Warping.ReturnOverworld;

            btnSave.Text = Strings.EventWarp.okay;
            btnCancel.Text = Strings.EventWarp.cancel;
        }

        private void UpdateFormItems(bool resetVariables = false)
        {
            if(mMyCommand == null)
            {
                return;
            }

            resetForm();

            switch (mWarpType)
            {
                case WarpType.Specific:
                    {
                        rdoSpecificMap.Checked = true;
                        for (var i = 0; i < MapList.OrderedMaps.Count; i++)
                        {
                            cmbMap.Items.Add(MapList.OrderedMaps[i].Name);
                            if (MapList.OrderedMaps[i].MapId == mMyCommand.MapId)
                            {
                                cmbMap.SelectedIndex = i;
                            }
                        }

                        nudScrollX.Value = mMyCommand.X;
                        nudScrollY.Value = mMyCommand.Y;
                        chkChangeInstance.Checked = mMyCommand.ChangeInstance;

                        cmbInstanceType.Items.Clear();
                        // We do not want to iterate over the "NoChange" enum
                        foreach (MapInstanceType instanceType in Enum.GetValues(typeof(MapInstanceType)))
                        {
                            cmbInstanceType.Items.Add(instanceType.ToString());
                        }

                        cmbInstanceType.SelectedIndex = (int)mMyCommand.InstanceType;

                        if (cmbInstanceType.SelectedIndex == -1)
                        {
                            cmbInstanceType.SelectedIndex = 0;
                        }
                    }
                    break;

                case WarpType.PlayerVariable:
                    {
                        rdoPlayerVariable.Checked = true;
                        var VariableNames = PlayerVariableBase.GetNamesByType(VariableDataType.String);
                        if (VariableNames.Length == 0)
                        {
                            AlertNoVariableError();
                            return;
                        }

                        cmbMap.Items.AddRange(VariableNames);
                        cmbScrollX.Items.AddRange(VariableNames);
                        cmbScrollY.Items.AddRange(VariableNames);
                        cmbMap.SelectedIndex = PlayerVariableBase.ListIndex(mMyCommand.MapId, VariableDataType.String);
                        cmbScrollX.SelectedIndex = PlayerVariableBase.ListIndex(mMyCommand.VariableX, VariableDataType.String);
                        cmbScrollY.SelectedIndex = PlayerVariableBase.ListIndex(mMyCommand.VariableY, VariableDataType.String);
                    }
                    break;

                case WarpType.ServerVariable:
                    {
                        rdoServerVariable.Checked = true;
                        var VariableNames = ServerVariableBase.GetNamesByType(VariableDataType.String);
                        if (VariableNames.Length == 0)
                        {
                            AlertNoVariableError();
                            return;
                        }

                        cmbMap.Items.AddRange(VariableNames);
                        cmbScrollX.Items.AddRange(VariableNames);
                        cmbScrollY.Items.AddRange(VariableNames);
                        cmbMap.SelectedIndex = ServerVariableBase.ListIndex(mMyCommand.MapId, VariableDataType.String);
                        cmbScrollX.SelectedIndex = ServerVariableBase.ListIndex(mMyCommand.VariableX, VariableDataType.String);
                        cmbScrollY.SelectedIndex = ServerVariableBase.ListIndex(mMyCommand.VariableY, VariableDataType.String);
                    }
                    break;

                case WarpType.GuildVariable:
                    {
                        rdoGuildVariable.Checked = true;
                        var VariableNames = GuildVariableBase.GetNamesByType(VariableDataType.String);
                        if (VariableNames.Length == 0)
                        {
                            AlertNoVariableError();
                            return;
                        }

                        cmbMap.Items.AddRange(VariableNames);
                        cmbScrollX.Items.AddRange(VariableNames);
                        cmbScrollY.Items.AddRange(VariableNames);
                        cmbMap.SelectedIndex = GuildVariableBase.ListIndex(mMyCommand.MapId, VariableDataType.String);
                        cmbScrollX.SelectedIndex = GuildVariableBase.ListIndex(mMyCommand.VariableX, VariableDataType.String);
                        cmbScrollY.SelectedIndex = GuildVariableBase.ListIndex(mMyCommand.VariableY, VariableDataType.String);
                    }
                    break;

                case WarpType.UserVariable:
                    {
                        rdoUserVariable.Checked = true;
                        var VariableNames = UserVariableBase.GetNamesByType(VariableDataType.String);
                        if (VariableNames.Length == 0)
                        {
                            AlertNoVariableError();
                            return;
                        }

                        cmbMap.Items.AddRange(VariableNames);
                        cmbScrollX.Items.AddRange(VariableNames);
                        cmbScrollY.Items.AddRange(VariableNames);
                        cmbMap.SelectedIndex = UserVariableBase.ListIndex(mMyCommand.MapId, VariableDataType.String);
                        cmbScrollX.SelectedIndex = UserVariableBase.ListIndex(mMyCommand.VariableX, VariableDataType.String);
                        cmbScrollY.SelectedIndex = UserVariableBase.ListIndex(mMyCommand.VariableY, VariableDataType.String);
                    }
                    break;
            }

            if (cmbMap.Items.Count == 0)
            {
                AlertNoVariableError();
                return;
            }

            if (cmbMap.SelectedIndex == -1)
            {
                cmbMap.SelectedIndex = 0;
            }

            if (mWarpType != WarpType.Specific)
            {
                if(resetVariables)
                {
                    cmbMap.SelectedIndex = 0;
                    cmbScrollX.SelectedIndex = 0;
                    cmbScrollY.SelectedIndex = 0;
                }

                chkReturnOverworld.Checked = mMyCommand.ChangeInstance;
            }

            cmbScrollX.Visible = mWarpType != WarpType.Specific;
            cmbScrollY.Visible = mWarpType != WarpType.Specific;
            nudScrollX.Visible = mWarpType == WarpType.Specific;
            nudScrollY.Visible = mWarpType == WarpType.Specific;
            btnVisual.Visible = mWarpType == WarpType.Specific;
            chkChangeInstance.Visible = mWarpType == WarpType.Specific;
            grpInstanceSettings.Visible = mWarpType == WarpType.Specific && chkChangeInstance.Checked;
            chkReturnOverworld.Visible = mWarpType != WarpType.Specific;
            cmbDirection.SelectedIndex = (int) mMyCommand.Direction;
        }

        private void mapRadioButton_Changed()
        {
            var tmpWarpType = mWarpType;

            if(rdoSpecificMap.Checked)
            {
                mWarpType = WarpType.Specific;
            }

            if(rdoPlayerVariable.Checked)
            {
                mWarpType = WarpType.PlayerVariable;
            }

            if(rdoServerVariable.Checked)
            {
                mWarpType = WarpType.ServerVariable;
            }

            if(rdoGuildVariable.Checked)
            {
                mWarpType = WarpType.GuildVariable;
            }

            if (rdoUserVariable.Checked)
            {
                mWarpType = WarpType.UserVariable;
            }

            if(tmpWarpType != mWarpType)
            {
                UpdateFormItems(true);
            }
        }

        private void resetForm()
        {
            cmbMap.Items.Clear();
            cmbScrollX.Items.Clear();
            cmbScrollY.Items.Clear();
            nudScrollX.Value = 0;
            nudScrollY.Value = 0;
            cmbDirection.SelectedIndex = 0;
            chkChangeInstance.Checked = false;
            grpInstanceSettings.Visible = false;
            cmbInstanceType.Items.Clear();
            rdoSpecificMap.Checked = false;
            rdoPlayerVariable.Checked = false;
            rdoServerVariable.Checked = false;
            rdoGuildVariable.Checked = false;
            rdoUserVariable.Checked = false;
            chkReturnOverworld.Checked = false;
        }

        private void AlertNoVariableError()
        {
            MessageBox.Show(Strings.Warping.NoVariableError);
            rdoSpecificMap.Checked = true;
            mapRadioButton_Changed();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(mWarpType != WarpType.Specific &&
                (cmbMap.SelectedIndex < 0 || cmbScrollX.SelectedIndex < 0 || cmbScrollY.SelectedIndex < 0))
            {
                MessageBox.Show(Strings.Warping.SelectVariableError);
                return;
            }

            mMyCommand.WarpType = mWarpType;

            switch(mMyCommand.WarpType)
            {
                case WarpType.Specific:
                    mMyCommand.MapId = MapList.OrderedMaps[cmbMap.SelectedIndex].MapId;
                    mMyCommand.X = (byte) nudScrollX.Value;
                    mMyCommand.Y = (byte) nudScrollY.Value;
                    mMyCommand.ChangeInstance = chkChangeInstance.Checked;
                    mMyCommand.InstanceType = (MapInstanceType) cmbInstanceType.SelectedIndex;
                    break;
                case WarpType.PlayerVariable:
                    mMyCommand.MapId = PlayerVariableBase.IdFromList(cmbMap.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableX = PlayerVariableBase.IdFromList(cmbScrollX.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableY = PlayerVariableBase.IdFromList(cmbScrollY.SelectedIndex, VariableDataType.String);
                    break;
                case WarpType.ServerVariable:
                    mMyCommand.MapId = ServerVariableBase.IdFromList(cmbMap.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableX = ServerVariableBase.IdFromList(cmbScrollX.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableY = ServerVariableBase.IdFromList(cmbScrollY.SelectedIndex, VariableDataType.String);
                    break;
                case WarpType.GuildVariable:
                    mMyCommand.MapId = GuildVariableBase.IdFromList(cmbMap.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableX = GuildVariableBase.IdFromList(cmbScrollX.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableY = GuildVariableBase.IdFromList(cmbScrollY.SelectedIndex, VariableDataType.String);
                    break;
                case WarpType.UserVariable:
                    mMyCommand.MapId = UserVariableBase.IdFromList(cmbMap.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableX = UserVariableBase.IdFromList(cmbScrollX.SelectedIndex, VariableDataType.String);
                    mMyCommand.VariableY = UserVariableBase.IdFromList(cmbScrollY.SelectedIndex, VariableDataType.String);
                    break;
            }

            if(mMyCommand.WarpType != WarpType.Specific)
            {
                mMyCommand.ChangeInstance = chkReturnOverworld.Checked;
                mMyCommand.InstanceType = MapInstanceType.Overworld;
            }

            mMyCommand.Direction = (WarpDirection) cmbDirection.SelectedIndex;
            mEventEditor.FinishCommandEdit();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            mEventEditor.CancelCommandEdit();
        }

        private void btnVisual_Click(object sender, EventArgs e)
        {
            var frmWarpSelection = new FrmWarpSelection();
            frmWarpSelection.SelectTile(MapList.OrderedMaps[cmbMap.SelectedIndex].MapId, (int) nudScrollX.Value, (int) nudScrollY.Value);
            frmWarpSelection.ShowDialog();
            if (frmWarpSelection.GetResult())
            {
                for (var i = 0; i < MapList.OrderedMaps.Count; i++)
                {
                    if (MapList.OrderedMaps[i].MapId == frmWarpSelection.GetMap())
                    {
                        cmbMap.SelectedIndex = i;

                        break;
                    }
                }

                nudScrollY.Value = frmWarpSelection.GetX();
                nudScrollY.Value = frmWarpSelection.GetY();
            }
        }

        private void rdoSpecificMap_CheckedChanged(object sender, EventArgs e)
        {
            mapRadioButton_Changed();
        }

        private void rdoPlayerVariable_CheckedChanged(object sender, EventArgs e)
        {
            mapRadioButton_Changed();
        }

        private void rdoGuildVariable_CheckedChanged(object sender, EventArgs e)
        {
            mapRadioButton_Changed();
        }

        private void rdoServerVariable_CheckedChanged(object sender, EventArgs e)
        {
            mapRadioButton_Changed();
        }

        private void rdoUserVariable_CheckedChanged(object sender, EventArgs e)
        {
            mapRadioButton_Changed();
        }

        private void chkChangeInstance_CheckedChanged(object sender, EventArgs e)
        {
            grpInstanceSettings.Visible = chkChangeInstance.Checked && mWarpType == WarpType.Specific;
        }
    }

}
