using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Editor.Localization;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands
{
    public partial class EventCommandChangeSprite
    {
        private void ExtraLoad()
        {
            chkHidePaperdolls.Checked = mMyCommand.HideAllPaperdolls;
        }

        private void ExtraLocalization()
        {
            chkHidePaperdolls.Text = Strings.EventChangeSprite.HidePaperdolls;
        }
    }
}
