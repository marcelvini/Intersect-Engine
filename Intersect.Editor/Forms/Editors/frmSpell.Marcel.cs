using System;
using Intersect.Editor.Localization;
using Intersect.GameObjects;

namespace Intersect.Editor.Forms.Editors
{
    public partial class FrmSpell
    {
        private void ExtraLoad()
        {
            cmbChainSpell.Items.Clear();
            cmbChainSpell.Items.Add(Strings.General.None);
            cmbChainSpell.Items.AddRange(SpellBase.Names);
        }

        private void ExtraLocalization()
        {
            lblChainSpell.Text = Strings.SpellEditor.ChainSpell;
            lblChainSpellDelay.Text = Strings.SpellEditor.ChainSpellDelay;
        }

        private void ExtraUpdate()
        {
            cmbChainSpell.SelectedIndex = SpellBase.ListIndex(mEditorItem.Combat.ChainSpellId) + 1;
            nudChainSpellDelay.Value = mEditorItem.Combat.ChainSpellDelay;
        }

        private void cmbChainSpell_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Combat.ChainSpellId = SpellBase.IdFromList(cmbChainSpell.SelectedIndex - 1);
        }

        private void darkNumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Combat.ChainSpellDelay = (long) nudChainSpellDelay.Value;
        }
    }
}
